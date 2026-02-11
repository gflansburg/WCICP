using System.Windows.Forms;

namespace FlightSim
{
    public class FlightSimForm : Form
    {
        public SimConnectProvider SimConnect => FlightSimProviders.SimConnect;
        public FSUIPCProvider FSUIPC => FlightSimProviders.FSUIPC;
        public XPlaneProvider XPlane => FlightSimProviders.XPlane;
        public FalconBMSProvider FalconBMS => FlightSimProviders.FalconBMS;
        public DCSProvider? DCS => FlightSimProviders.DCS;
        public PreviewFlightSimProvider Preview => FlightSimProviders.Preview;

        private bool _eventsWired;
        private bool _disposing;

        public FlightSimForm() : base()
        {
        }

        protected IEnumerable<FlightSimProviderBase> GetProviders()
        {
            yield return SimConnect;
            yield return FSUIPC;
            yield return XPlane;
            yield return FalconBMS;
            if (DCS is not null) yield return DCS;
            yield return Preview;
        }

        private void WireProviderEvents(FlightSimProviderBase p)
        {
            p.OnConnected += OnConnected;
            p.OnFlightDataReceived += OnFlightDataReceived;
            p.OnReadyToFly += OnReadyToFly;
            p.OnAircraftChange += OnAircraftChange;
            p.OnQuit += OnQuit;
            p.OnTrafficReceived += OnTrafficReceived;
            p.OnError += OnError;
        }

        private void WireSimConnectOnlyEvents()
        {
            var sc = FlightSim.SimConnect.Instance;
            sc.OnCrashed += OnCrashed;
            sc.OnPaused += OnPaused;
            sc.OnFacilitiesReceived += OnFacilitiesReceived;
            sc.OnFlightLoaded += OnFlightLoaded;
            sc.OnFlightPlanActivated += OnFlightPlanActivated;
            sc.OnFlightPlanDeactivated += OnFlightPlanDeactivated;
        }

        private void UnwireProviderEvents(FlightSimProviderBase p)
        {
            p.OnConnected -= OnConnected;
            p.OnFlightDataReceived -= OnFlightDataReceived;
            p.OnReadyToFly -= OnReadyToFly;
            p.OnAircraftChange -= OnAircraftChange;
            p.OnQuit -= OnQuit;
            p.OnTrafficReceived -= OnTrafficReceived;
            p.OnError -= OnError;
        }

        private void UnwireSimConnectOnlyEvents()
        {
            var sc = FlightSim.SimConnect.Instance;
            sc.OnCrashed -= OnCrashed;
            sc.OnPaused -= OnPaused;
            sc.OnFacilitiesReceived -= OnFacilitiesReceived;
            sc.OnFlightLoaded -= OnFlightLoaded;
            sc.OnFlightPlanActivated -= OnFlightPlanActivated;
            sc.OnFlightPlanDeactivated -= OnFlightPlanDeactivated;
        }

        protected virtual void OnFlightPlanDeactivated()
        {
        }

        protected virtual void OnFlightPlanActivated(string? filename)
        {
        }

        protected virtual void OnFlightLoaded(string? filename)
        {
        }

        protected virtual void OnFacilitiesReceived(FacilityType type)
        {
        }

        protected virtual void OnPaused(bool paused)
        {
        }

        protected virtual void OnCrashed()
        {
        }

        protected virtual void OnError(FlightSimProviderBase sender, FlightSimProviderException ex)
        {
        }

        protected virtual void OnTrafficReceived(FlightSimProviderBase sender, string callsign, Aircraft? aircraft, TrafficEvent eventType)
        {
        }

        protected virtual void OnQuit(FlightSimProviderBase sender)
        {
        }

        protected virtual void OnAircraftChange(FlightSimProviderBase sender, int aircraftId)
        {
        }

        protected virtual void OnReadyToFly(FlightSimProviderBase sender, ReadyToFly readyToFly)
        {
        }

        protected virtual void OnFlightDataReceived(FlightSimProviderBase sender)
        {
        }

        protected virtual void OnConnected(FlightSimProviderBase sender)
        {
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == FlightSim.SimConnect.WM_USER_SIMCONNECT)
            {
                SimConnect.ReceiveMessage();
                return;
            }
            base.WndProc(ref m);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            SimConnect.MainWindowHandle = Handle;
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (!_disposing)
            {
                SimConnect.MainWindowHandle = IntPtr.Zero;
            }
            base.OnHandleDestroyed(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (_eventsWired)
            {
                return;
            }
            _eventsWired = true;
            foreach (var p in GetProviders())
            {
                WireProviderEvents(p);
            }
            WireSimConnectOnlyEvents();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposing)
            {
                _disposing = true;
                if (_eventsWired)
                {
                    foreach (var p in GetProviders())
                    {
                        UnwireProviderEvents(p);
                    }
                    UnwireSimConnectOnlyEvents();
                    _eventsWired = false;
                }
                XPlane.Deinitialize();
                SimConnect.Deinitialize();
                FSUIPC.Deinitialize();
                DCS?.Deinitialize();
                FalconBMS.Deinitialize();
                Preview.Deinitialize();
            }
            base.Dispose(disposing);
        }
    }
}
