using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FlightSim.XPlaneConnect
{
    public class XPlaneConnector : IDisposable
    {
        private enum ConnectionState
        {
            Disconnected = 0,
            Connected = 1,
            Stale = 2
        }

        private const int CheckInterval_ms = 1000;
        public TimeSpan MaxDataRefAge { get; private set; } = TimeSpan.FromSeconds(5);

        private CultureInfo EnCulture = new CultureInfo("en-US");

        private UdpClient? server;
        private UdpClient? client;

        private volatile bool _needResubscribe;

        IPEndPoint? remoteEP = null;

        public IPEndPoint XPlaneEP { get; private set; }
        private CancellationTokenSource? ts;
        private Task? serverTask;
        private Task? observerTask;
        private volatile bool _everReceived;

        public delegate void RawReceiveHandler(string raw);
        public event RawReceiveHandler? OnRawReceive;

        public delegate void DataRefReceived(DataRefElement dataRef);
        public event DataRefReceived? OnDataRefReceived;

        public delegate void DataRefUpdated(List<DataRefElement> dataRefs);
        public event DataRefUpdated? OnDataRefUpdated;

        public delegate void LogHandler(string message);
        public event LogHandler? OnLog;

        public List<DataRefElement> DataRefs { get; private set; }
        private List<DataRefElement> UpdatedDataRefs = new List<DataRefElement>();
        public List<StringDataRefElement> StringDataRefs { get; private set; }
        
        private ConnectionState connectionState;
        private bool _isConnected;

        private readonly object _dataRefsLock = new();
        private readonly object _stringDataRefsLock = new();
        private readonly object _clientLock = new();
        private readonly object _serverLock = new();

        public bool IsConnected
        {
            get
            {
                return connectionState == ConnectionState.Connected;
            }
        }

        public bool IsStopping { get; private set; }

        public DateTime LastReceive { get; internal set; }

        public TimeSpan MaxReceiveSilence { get; set; } = TimeSpan.FromSeconds(3);

        public delegate void ConnectionChangedHandler(bool connected);

        public event ConnectionChangedHandler? OnConnectionChanged;

        public bool IsUdpAlive => _isConnected && (DateTime.UtcNow - LastReceive) <= MaxReceiveSilence;

        private void SetConnected(ConnectionState state)
        {
            bool isConnected = state == ConnectionState.Connected;
            if (connectionState == state) return;
            if (IsConnected == isConnected) return;
            LastReceive = DateTime.UtcNow;
            connectionState = state;
            if (IsConnected != isConnected)
            {
                OnConnectionChanged?.Invoke(isConnected);
            }
            _isConnected = state != ConnectionState.Disconnected;
            OnLog?.Invoke(IsConnected? "X-Plane UDP is active" : "X-Plane UDP went stale");
        }

        public IPEndPoint? LocalEP
        {
            get
            {
                return client?.Client.LocalEndPoint as IPEndPoint;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ip">IP of the machine running X-Plane, default 127.0.0.1 (localhost)</param>
        /// <param name="xplanePort">Port the machine running X-Plane is listening for, default 49000</param>
        public XPlaneConnector(string ip = "127.0.0.1", int xplanePort = 49000)
        {
            XPlaneEP = new IPEndPoint(IPAddress.Parse(ip), xplanePort);
            DataRefs = new List<DataRefElement>();
            StringDataRefs = new List<StringDataRefElement>();
        }

        private IPEndPoint? StartClient()
        {
            lock (_clientLock)
            {
                client?.Dispose();
                remoteEP = null;
                client = new UdpClient();
                client.Client.ReceiveTimeout = 100;
                client.Connect(XPlaneEP.Address, XPlaneEP.Port);
                byte[] handshake = new byte[1];
                try
                {
                    client.Send(handshake, 1);
                    client.Receive(ref remoteEP);
                    SetConnected(ConnectionState.Connected);
                    StartServer();
                }
                catch (SocketException ex)
                {
                    SetConnected(ex.ErrorCode != 10054 ? ConnectionState.Connected : ConnectionState.Disconnected);
                    if (connectionState != ConnectionState.Connected)
                    {
                        client?.Dispose();
                        client = null;
                        remoteEP = null;
                    }
                    else
                    {
                        StartServer();
                    }
                }
            }
            return remoteEP;
        }

        private void StartServer()
        {
            if (LocalEP != null)
            {
                lock (_serverLock)
                {
                    server?.Close();
                    server?.Dispose();
                    server = new UdpClient(LocalEP);
                    server.Client.ReceiveTimeout = 1;
                }
            }
        }

        /// <summary>
        /// Initialize the communication with X-Plane machine and starts listening for DataRefs
        /// </summary>
        public void Start()
        {
            Stop();
            StartClient();
            ts = new CancellationTokenSource();
            StartObserver(ts.Token);
            serverTask = Task.Run(() =>
            {
                while (!ts.Token.IsCancellationRequested && !IsStopping)
                {
                    try
                    {
                        UdpClient? localServer;
                        IPEndPoint? localRemote;
                        lock (_serverLock)
                        {
                            localServer = server;
                            localRemote = remoteEP;
                        }
                        if (localServer == null)
                        {
                            Thread.Sleep(50);
                            continue;
                        }
                        // BLOCK until a packet arrives, or until the socket is closed in Stop()
                        byte[] data = localServer.Receive(ref localRemote);
                        lock (_serverLock)
                        {
                            remoteEP = localRemote;
                        }
                        var raw = Encoding.UTF8.GetString(data);
                        LastReceive = DateTime.UtcNow;
                        _everReceived = true;
                        _needResubscribe = false;
                        SetConnected(ConnectionState.Connected);
                        OnRawReceive?.Invoke(raw);
                        // Parse on threadpool (fine), but do NOT spawn unbounded work if packets flood.
                        ThreadPool.QueueUserWorkItem(_ => ParseResponse(data));
                    }
                    catch (ObjectDisposedException)
                    {
                        break; // server socket closed during Stop/Dispose
                    }
                    catch (SocketException ex) when (ex.ErrorCode == 10004 /* interrupted */)
                    {
                        break;
                    }
                    catch (SocketException ex)
                    {
                        // timeout shouldn't happen if ReceiveTimeout=0, but keep your behavior
                        SetConnected(ex.ErrorCode != 10054 ? ConnectionState.Connected : connectionState);
                    }
                    catch
                    {
                        // optional log
                    }
                }
                OnLog?.Invoke("Stopping server");
            }, ts.Token);
        }

        private void StartObserver(CancellationToken token)
        {
            if ((observerTask == null || observerTask.IsCompleted) && !token.IsCancellationRequested && !IsStopping)
            {
                observerTask?.Dispose();

                observerTask = Task.Run(async () =>
                {
                    try
                    {
                        while (!token.IsCancellationRequested && !IsStopping)
                        {
                            if (connectionState == ConnectionState.Connected && _everReceived &&
                                (DateTime.UtcNow - LastReceive) > MaxReceiveSilence)
                            {
                                SetConnected(ConnectionState.Stale);
                                _needResubscribe = true;

                                lock (_clientLock)
                                {
                                    client?.Dispose();
                                    client = null;
                                    remoteEP = null;
                                }

                                foreach (var dr in DataRefs)
                                {
                                    if (dr == null) continue;
                                    dr.IsSubscribed = false;
                                }
                            }

                            if ((_needResubscribe || server == null || client == null) && !IsStopping)
                                StartClient();

                            DataRefElement?[] snapshot;
                            lock (_dataRefsLock)
                                snapshot = DataRefs.ToArray();

                            foreach (var dr in snapshot)
                            {
                                if (IsStopping) break;
                                if (dr == null) continue;

                                if (_needResubscribe || !dr.IsSubscribed || (dr.Age > MaxDataRefAge && client != null))
                                {
                                    dr.IsSubscribed = true;
                                    RequestDataRef(dr);
                                }
                            }
                            await Task.Delay(CheckInterval_ms, token);
                        }
                    }
                    catch (OperationCanceledException) when (token.IsCancellationRequested)
                    {
                        // expected on shutdown
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Observer task faulted: {ex}");
                    }
                    finally
                    {
                        Debug.WriteLine("Observer Task Complete");
                    }
                }, token);
            }
        }

        /// <summary>
        /// Stops the comunications with the X-Plane machine
        /// </summary>
        /// <param name="timeout"></param>
        public void Stop(int timeout = 5000)
        {
            IsStopping = true;
            UnsubscribeAll();
            lock (_serverLock)
            {
                server?.Close();
                server?.Dispose();
                server = null;
            }
            lock (_clientLock)
            {
                client?.Dispose();
                client = null;
            }
            try
            {
                ts?.Cancel();
            }
            catch
            {
                // ignore
            }
            var allTasks = Task.WhenAll(
                serverTask ?? Task.CompletedTask,
                observerTask ?? Task.CompletedTask
            );
            // Wait for both OR timeout
            bool completed = allTasks.Wait(timeout);
            // If they completed, observe exceptions (cancellation will often surface here)
            if (completed)
            {
                try
                {
                    allTasks.GetAwaiter().GetResult();
                }
                catch
                {
                    // swallow or log — expected during cancellation
                }
            }
            ts?.Dispose();
            // Only dispose tasks once they are completed (or you accept the timeout risk)
            if (serverTask?.IsCompleted == true) serverTask?.Dispose();
            if (observerTask?.IsCompleted == true) observerTask?.Dispose();
            serverTask = null;
            observerTask = null;
            ts = null;
            remoteEP = null;
            IsStopping = false;
        }

        private void ParseResponse(byte[] buffer)
        {
            var pos = 0;
            var header = Encoding.UTF8.GetString(buffer, pos, 4);
            pos += 5; // Including tailing 0

            if (header == "RREF") // Ignore other messages
            {
                bool updated = false;
                UpdatedDataRefs.Clear();
                while (pos < buffer.Length)
                {
                    var id = BitConverter.ToInt32(buffer, pos);
                    pos += 4;

                    try
                    {
                        var value = BitConverter.ToSingle(buffer, pos);
                        pos += 4;
                        DataRefElement?[] localDataRefs;
                        lock (_dataRefsLock)
                        {
                            localDataRefs = DataRefs.ToArray();
                        }
                        foreach (var dr in localDataRefs)
                        {
                            if (dr != null)
                            {
                                bool dataRefUpdated = dr.Update(id, value);
                                updated = dataRefUpdated ? true : updated;
                                if (dataRefUpdated)
                                {
                                    UpdatedDataRefs.Add(dr);
                                    OnDataRefReceived?.Invoke(dr);
                                }
                            }
                        }
                    }
                    catch (ArgumentException)
                    {

                    }
                    catch (Exception ex)
                    {
                        var error = ex.Message;
                    }
                }
                if (updated)
                    OnDataRefUpdated?.Invoke(UpdatedDataRefs);
            }
        }

        /// <summary>
        /// Sends a command
        /// </summary>
        /// <param name="command">Command to send</param>
        public void SendCommand(XPlaneCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var dg = new XPDatagram();
            dg.Add("CMND");
            dg.Add(command.Command);
            if (client != null)
                client.Send(dg.Get(), dg.Len);
        }

        /// <summary>
        /// Sends a command continously. Use return parameter to cancel the send cycle
        /// </summary>
        /// <param name="command">Command to send</param>
        /// <returns>Token to cancel the executing</returns>
        public CancellationTokenSource StartCommand(XPlaneCommand command)
        {
            var tokenSource = new CancellationTokenSource();

            Task.Run(() =>
            {
                while (!tokenSource.IsCancellationRequested)
                {
                    SendCommand(command);
                }
            }, tokenSource.Token);

            return tokenSource;
        }

        public void StopCommand(CancellationTokenSource token)
        {
            token.Cancel();
        }

        /// <summary>
        /// Subscribe to a DataRef, notification will be sent every time the value changes
        /// </summary>
        /// <param name="dataref">DataRef to subscribe to</param>
        /// <param name="frequency">Times per seconds X-Plane will be seding this value</param>
        /// <param name="onchange">Callback invoked every time a change in the value is detected</param>
        public void Subscribe(DataRefElement dataref, int frequency = -1, Action<DataRefElement, float> onchange = null!)
        {
            if (dataref == null)
                throw new ArgumentNullException(nameof(dataref));

            lock (_dataRefsLock)
            {
                var existing = DataRefs.FirstOrDefault(d => d != null && d.Id == dataref.Id && d.Index == dataref.Index);

                if (existing == null)
                {
                    dataref.ForceUpdate = true;

                    if (frequency > 0)
                        dataref.Frequency = frequency;

                    if (onchange != null)
                    {
                        dataref.Subscribed = true;

                        DataRefElement.NotifyChangeHandler changeHandler = (e, v) => { onchange(e, v); };
                        dataref.Delegates.Add(changeHandler);
                        dataref.OnValueChange += changeHandler;
                    }

                    //dataref.IsSubscribed = true;
                    //RequestDataRef(dataref);
                    DataRefs.Add(dataref);
                    return;
                }

                // Update existing
                existing.ForceUpdate = true;
                existing.Value = 0;

                // Don't stomp existing frequency with -1/0
                if (frequency > 0)
                    existing.Frequency = frequency;

                if (onchange != null)
                {
                    // Already subscribed: add handler only if we don't already have an equivalent one
                    if (!existing.Subscribed)
                    {
                        existing.Subscribed = true;
                    }

                    bool alreadyWired = existing.Delegates.Any(d =>
                        d.Method == onchange.Method && Equals(d.Target, onchange.Target));

                    if (!alreadyWired)
                    {
                        DataRefElement.NotifyChangeHandler changeHandler = (e, v) => { onchange(e, v); };
                        existing.Delegates.Add(changeHandler);
                        existing.OnValueChange += changeHandler;
                    }
                }
                else
                {
                    // Caller wants no notifications
                    if (existing.Subscribed)
                    {
                        foreach (var dh in existing.Delegates)
                            existing.OnValueChange -= dh;

                        existing.Delegates.Clear();
                        existing.Subscribed = false;
                    }
                }
            }
        }

        /// <summary>
        /// Subscribe to a DataRef, notification will be sent every time the value changes
        /// </summary>
        /// <param name="dataref">DataRef to subscribe to</param>
        /// <param name="frequency">Times per seconds X-Plane will be seding this value</param>
        /// <param name="onchange">Callback invoked every time a change in the value is detected</param>
        /// <param name="oncomplete">Callback invoked every time the value has finished its initial population</param>
        public void Subscribe(StringDataRefElement dataref, int frequency = -1, Action<StringDataRefElement, string> onchange = null!, Action<StringDataRefElement, string> oncomplete = null!)
        {
            if (dataref == null)
                throw new ArgumentNullException(nameof(dataref));

            // Normalize frequency (avoid 0/-1 accidentally disabling streaming)
            if (frequency <= 0)
                frequency = 1;

            lock (_stringDataRefsLock)
            {

                // Canonical (stored) string element for this Id, if any
                StringDataRefElement? stored = StringDataRefs.FirstOrDefault(d => d.Id == dataref.Id);

                // If this is the first time we've seen this string Id, the passed-in element becomes canonical
                if (stored == null)
                {
                    dataref.ForceUpdate = true;
                    dataref.Frequency = frequency;

                    // Wire change notifications on the canonical instance
                    if (onchange != null)
                    {
                        dataref.Subscribed = true;

                        StringDataRefElement.NotifyChangeHandler changeHandler = (e, v) => { onchange(e, v); };
                        dataref.ValueChangedDelegates.Add(changeHandler);
                        dataref.OnValueChange += changeHandler;
                    }

                    if (oncomplete != null)
                    {
                        dataref.Subscribed = true;

                        StringDataRefElement.NotifyChangeHandler completeHandler = (e, v) => { oncomplete(e, v); };
                        dataref.ValueCompleteDelegates.Add(completeHandler);
                        dataref.OnValueComplete += completeHandler;
                    }

                    // Ensure per-character datarefs exist and are subscribed
                    for (int c = 0; c < dataref.StringLength; c++)
                    {
                        int currentIndex = c;

                        lock (_dataRefsLock)
                        {
                            var charRef = DataRefs.FirstOrDefault(d => d != null && d.Id == dataref.Id && d.Index == c);
                            if (charRef != null)
                            {
                                charRef.Frequency = frequency;
                                charRef.ForceUpdate = true;

                                // NOTE:
                                // If you need to guarantee callbacks are reattached after stale/restart,
                                // you should remove+recreate the char refs on stale rather than relying on existing ones.
                            }
                            else
                            {
                                var arrayElementDataRef = new DataRefElement
                                {
                                    Id = dataref.Id,
                                    DataRef = $"{dataref.DataRef}[{c}]",
                                    Frequency = frequency,
                                    Units = "String",
                                    Description = dataref.Description,
                                    Index = c,
                                    ForceUpdate = true
                                };

                                Subscribe(arrayElementDataRef, frequency, (e, v) =>
                                {
                                    var character = Convert.ToChar(Convert.ToInt32(v));
                                    dataref.Update(currentIndex, character); // canonical instance
                                });
                            }
                        }
                    }

                    StringDataRefs.Add(dataref);
                    return;
                }

                // Existing string: update canonical instance fields
                stored.Frequency = frequency;
                stored.ForceUpdate = true;
                stored.Value = string.Empty;
                stored.CharactersInitialized = 0;

                // Ensure per-character datarefs exist and are subscribed (and update the CANONICAL instance!)
                for (int c = 0; c < stored.StringLength; c++)
                {
                    int currentIndex = c;

                    lock (_dataRefsLock)
                    { 
                    var charRef = DataRefs.FirstOrDefault(d => d != null && d.Id == stored.Id && d.Index == c);
                        if (charRef != null)
                        {
                            charRef.Frequency = frequency;
                            charRef.ForceUpdate = true;

                            // NOTE:
                            // We do not attempt to rewire per-char callbacks here if the DataRefElement already exists.
                            // If you need guaranteed rewire, do it in the stale handler by removing+recreating Units=="String" refs.
                        }
                        else
                        {
                            var arrayElementDataRef = new DataRefElement
                            {
                                Id = stored.Id,
                                DataRef = $"{stored.DataRef}[{c}]",
                                Frequency = frequency,
                                Units = "String",
                                Description = stored.Description,
                                Index = c,
                                ForceUpdate = true
                            };

                            Subscribe(arrayElementDataRef, frequency, (e, v) =>
                            {
                                var character = Convert.ToChar(Convert.ToInt32(v));
                                stored.Update(currentIndex, character); // <-- FIX: update canonical instance, not parameter
                            });
                        }
                    }
                }

                // Manage change notification subscription on the canonical instance
                if (onchange != null)
                {
                    if (!stored.Subscribed)
                    {
                        stored.Subscribed = true;
                        StringDataRefElement.NotifyChangeHandler changeHandler = (e, v) => { onchange(e, v); };
                        stored.ValueChangedDelegates.Add(changeHandler);
                        stored.OnValueChange += changeHandler;
                    }
                    else
                    {
                        // Avoid duplicate handlers: compare by delegate Target/Method if possible
                        bool alreadyWired = stored.ValueChangedDelegates.Any(d => d.Method == onchange.Method && Equals(d.Target, onchange.Target));
                        if (!alreadyWired)
                        {
                            StringDataRefElement.NotifyChangeHandler changeHandler = (e, v) => { onchange(e, v); };
                            stored.ValueChangedDelegates.Add(changeHandler);
                            stored.OnValueChange += changeHandler;
                        }
                    }
                }
                else
                {
                    // Caller wants no notifications
                    if (stored.Subscribed)
                    {
                        foreach (var dh in stored.ValueChangedDelegates)
                            stored.OnValueChange -= dh;
                        stored.ValueChangedDelegates.Clear();
                        stored.Subscribed = oncomplete == null;
                    }
                }
                if (oncomplete != null)
                {
                    if (!stored.Subscribed)
                    {
                        stored.Subscribed = true;
                        StringDataRefElement.NotifyChangeHandler completeHandler = (e, v) => { oncomplete(e, v); };
                        stored.ValueCompleteDelegates.Add(completeHandler);
                        stored.OnValueComplete += completeHandler;
                    }
                    else
                    {
                        // Avoid duplicate handlers: compare by delegate Target/Method if possible
                        bool alreadyWired = stored.ValueCompleteDelegates.Any(d => d.Method == oncomplete.Method && Equals(d.Target, oncomplete.Target));
                        if (!alreadyWired)
                        {
                            StringDataRefElement.NotifyChangeHandler completeHandler = (e, v) => { oncomplete(e, v); };
                            stored.ValueCompleteDelegates.Add(completeHandler);
                            stored.OnValueComplete += completeHandler;
                        }
                    }
                }
                else
                {
                    // Caller wants no notifications
                    if (stored.Subscribed)
                    {
                        foreach (var dh in stored.ValueCompleteDelegates)
                            stored.OnValueChange -= dh;
                        stored.ValueCompleteDelegates.Clear();
                        stored.Subscribed = onchange == null;
                    }
                }
            }
        }

        private void RequestDataRef(DataRefElement element)
        {
            var dg = new XPDatagram();
            dg.Add("RREF");
            dg.Add(element.Frequency);
            dg.Add(element.DataGramId);
            dg.Add(element.DataRef);
            dg.FillTo(413);
            lock (_clientLock)
            {
                if (client != null)
                {
                    int i = client.Send(dg.Get(), dg.Len);
                    OnLog?.Invoke($"Requested {element.DataRef}@{element.Frequency}Hz with Id:{element.DataGramId}");
                }
            }
        }

        /// <summary>
        /// Checks if X-Plane is sending data for this DataRef
        /// </summary>
        /// <param name="dataref">DataRef to check</param>
        public bool IsSubscribedToDataRef(DataRefElement dataRef)
        {
            lock (_dataRefsLock)
            {
                return DataRefs.FirstOrDefault(d => d.Id == dataRef.Id) != null;
            }
        }

        /// <summary>
        /// Checks if X-Plane is sending data for this DataRef
        /// </summary>
        /// <param name="dataref">DataRef to check</param>
        public bool IsSubscribedToDataRef(StringDataRefElement stringDataRef)
        {
            lock (_stringDataRefsLock)
            {
                return StringDataRefs.FirstOrDefault(d => d.Id == stringDataRef.Id) != null;
            }
        }

        /// <summary>
        /// Informs X-Plane to stop sending all DataRefs
        /// </summary>
        public void UnsubscribeAll()
        {

            DataRefElement?[] localStringDataRefs;
            lock (_stringDataRefsLock)
            {
                localStringDataRefs = StringDataRefs.ToArray();
            }
            foreach (var dr in localStringDataRefs)
            {
                if (dr != null)
                {
                    Unsubscribe(dr);
                }
            }
            DataRefElement?[] localDataRefs;
            lock (_dataRefsLock)
            {
                localDataRefs = DataRefs.ToArray();
            }
            foreach (var dr in localDataRefs)
            {
                if (dr != null)
                {
                    Unsubscribe(dr);
                }
            }
        }

        /// <summary>
        /// Informs X-Plane to stop sending this DataRef
        /// </summary>
        /// <param name="dataref">DataRef to unsubscribe to</param>
        public void Unsubscribe(DataRefElement dataref)
        {
            DataRefElement? dr = DataRefs.FirstOrDefault(d => d != null && d.Id == dataref.Id && d.Index == dataref.Index);
            if (dr != null)
            {
                var dg = new XPDatagram();
                dg.Add("RREF");
                dg.Add(dr.DataGramId);
                dg.Add(0);
                dg.Add(dr.DataRef);
                dg.FillTo(413);
                lock (_clientLock)
                {
                    if (client != null)
                        client.Send(dg.Get(), dg.Len);
                }
                lock (_dataRefsLock)
                {
                    DataRefs.Remove(dr);
                }
                OnLog?.Invoke($"Unsubscribed from {dr.DataRef}");
            }
        }

        /// <summary>
        /// Informs X-Plane to stop sending this DataRef
        /// </summary>
        /// <param name="dataref">DataRef to unsubscribe to</param>
        public void Unsubscribe(StringDataRefElement stringDataRef)
        {
            StringDataRefElement? dr = StringDataRefs.FirstOrDefault(d => d != null && d.Id == stringDataRef.Id);
            if (dr != null)
            {
                for (var c = 0; c < dr.StringLength; c++)
                {
                    var currentIndex = c;
                    string dataRef = $"{dr.DataRef}[{c}]";
                    DataRefElement? dataref = DataRefs.FirstOrDefault(d => d != null && d.Id == stringDataRef.Id && d.Index == currentIndex);
                    if (dataref != null)
                    {
                        var dg = new XPDatagram();
                        dg.Add("RREF");
                        dg.Add(dataref.DataGramId);
                        dg.Add(0);
                        dg.Add(dataref.DataRef);
                        dg.FillTo(413);
                        lock (_clientLock)
                        {
                            if (client != null)
                                client.Send(dg.Get(), dg.Len);
                        }
                        lock (_dataRefsLock)
                        {
                            DataRefs.Remove(dataref);
                        }
                    }
                }
                lock (_stringDataRefsLock)
                {
                    StringDataRefs.Remove(dr);
                }
                OnLog?.Invoke($"Unsubscribed from {stringDataRef.DataRef}");
            }
        }

        private void ResetStringCharDataRefsForResubscribe()
        {
            foreach (var sdr in StringDataRefs.ToArray())
            {
                if (sdr == null) continue;
                lock (_dataRefsLock)
                {
                    DataRefs.RemoveAll(d => d != null && d.Id == sdr.Id && d.Units == "String");
                }
            }
            lock (_stringDataRefsLock)
            {
                StringDataRefs.Clear();
            }
        }

        /// <summary>
        /// Informs X-Plane to change the value of the DataRef
        /// </summary>
        /// <param name="dataref">DataRef that will be changed</param>
        /// <param name="value">New value of the DataRef</param>
        public void SetDataRefValue(DataRefElement dataref, float value)
        {
            if (dataref == null)
                throw new ArgumentNullException(nameof(dataref));

            SetDataRefValue(dataref.DataRef, value);
        }

        /// <summary>
        /// Informs X-Plane to change the value of the DataRef
        /// </summary>
        /// <param name="dataref">DataRef that will be changed</param>
        /// <param name="value">New value of the DataRef</param>
        public void SetDataRefValue(string dataref, float value)
        {
            var dg = new XPDatagram();
            dg.Add("DREF");
            dg.Add(value);
            dg.Add(dataref);
            dg.FillTo(509);
            lock (_clientLock)
            {
                if (client != null)
                    client.Send(dg.Get(), dg.Len);
            }
        }
        /// <summary>
        /// Informs X-Plane to change the value of the DataRef
        /// </summary>
        /// <param name="dataref">DataRef that will be changed</param>
        /// <param name="value">New value of the DataRef</param>
        public void SetDataRefValue(string dataref, string value)
        {
            var dg = new XPDatagram();
            dg.Add("DREF");
            dg.Add(value);
            dg.Add(dataref);
            dg.FillTo(509);
            lock (_clientLock)
            {
                client?.Send(dg.Get(), dg.Len);
            }
        }

        /// <summary>
        /// Request X-Plane to close, a notification message will appear
        /// </summary>
        public void QuitXPlane()
        {
            var dg = new XPDatagram();
            dg.Add("QUIT");
            lock (_clientLock)
            {
                if (client != null)
                    client.Send(dg.Get(), dg.Len);
            }
        }

        /// <summary>
        /// Inform X-Plane that a system is failed
        /// </summary>
        /// <param name="system">Integer value representing the system to fail</param>
        public void Fail(int system)
        {
            var dg = new XPDatagram();
            dg.Add("FAIL");

            dg.Add(system.ToString(EnCulture));
            lock (_clientLock)
            {
                if (client != null)
                    client.Send(dg.Get(), dg.Len);
            }
        }

        /// <summary>
        /// Inform X-Plane that a system is back to normal functioning
        /// </summary>
        /// <param name="system">Integer value representing the system to recover</param>
        public void Recover(int system)
        {
            var dg = new XPDatagram();
            dg.Add("RECO");

            dg.Add(system.ToString(EnCulture));
            lock (_clientLock)
            {
                if (client != null)
                    client.Send(dg.Get(), dg.Len);
            }
        }

        protected virtual void Dispose(bool a)
        {
            Stop();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
