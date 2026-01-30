using FlightSim;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinCtrlICP
{
    public partial class SimAppProForm : Form
    {
        private UdpClient? _udp;
        private bool _sapHandshakeSent = false;

        private string[] waiting = { string.Empty, "         WAITING", "           TO", "         CONNECT", string.Empty };
        private string[] connected = { string.Empty, string.Empty, "       CONNECTED", string.Empty, string.Empty };

        private Page ActivePage
        {
            get
            {
                return (Page)Properties.Settings.Default.ActivePage;
            }
            set
            {
                Properties.Settings.Default.ActivePage = (int)value;
                Properties.Settings.Default.Save();
            }
        }

        private bool waitForFlightSim = true;

        private IPEndPoint _any = new IPEndPoint(IPAddress.Any, 0);

        private volatile bool _running;
        private Dictionary<string, string> _commonRequests = new(); // key -> identifyMethod (lua string)

        private const string ModuleKey = "F-16C_50";

        public SimAppProForm()
        {
            InitializeComponent();
            SetLines(waiting);
            StartUdp();
            HandleCreated += SimAppProForm_HandleCreated;
        }

        private void SetLines(string[] lines)
        {
            Ui(() =>
            {
                icpDisplayControl1.SetLines(lines);
            });

        }

        private void SimAppProForm_HandleCreated(object? sender, EventArgs e)
        {
            FlightSimProviders.XPlane.OnFlightDataReceived += XPlane_OnFlightDataReceived;
            FlightSimProviders.XPlane.OnConnected += XPlane_OnConnected;
            FlightSimProviders.XPlane.OnQuit += XPlane_OnQuit;
            FlightSimProviders.XPlane.OnReadyToFly += XPlane_OnReadyToFly;
            FlightSimProviders.SimConnect.MainWindowHandle = this.Handle;
            FlightSimProviders.SimConnect.OnFlightDataReceived += SimConnect_OnFlightDataReceived;
            FlightSimProviders.SimConnect.OnConnected += SimConnect_OnConnected;
            FlightSimProviders.SimConnect.OnQuit += SimConnect_OnQuit;
            FlightSimProviders.SimConnect.OnReadyToFly += SimConnect_OnReadyToFly;
            FlightSimProviders.FSUIPC.OnFlightDataReceived += FSUIPC_OnFlightDataReceived;
            FlightSimProviders.FSUIPC.OnConnected += FSUIPC_OnConnected;
            FlightSimProviders.FSUIPC.OnQuit += FSUIPC_OnQuit;
            FlightSimProviders.FSUIPC.OnReadyToFly += FSUIPC_OnReadyToFly;
        }

        private void Ui(Action action)
        {
            if (InvokeRequired)
            {
                BeginInvoke(action);
            }
            else
            {
                action();
            }
        }

        private void XPlane_OnConnected(FlightSimProviderBase sender)
        {
            _sapHandshakeSent = false;
            SetLines(connected);
        }

        private void XPlane_OnQuit(FlightSimProviderBase sender)
        {
            if (_sapHandshakeSent)
            {
                SendUdp(new { func = "mission", msg = "stop" });
                _sapHandshakeSent = false;
            }
            SetLines(waiting);
        }

        private void XPlane_OnReadyToFly(FlightSimProviderBase sender, ReadyToFly readyToFly)
        {
            if (readyToFly == ReadyToFly.Ready)
            {
                EnsureSimAppProHandshake();
                UpdateScreen(sender);
            }
            else
            {
                if (_sapHandshakeSent)
                {
                    SendUdp(new { func = "mission", msg = "stop" });
                    _sapHandshakeSent = false;
                }
                SetLines(FlightSimProviders.XPlane.IsConnected ? connected : waiting);
            }
        }

        private void SimConnect_OnConnected(FlightSimProviderBase sender)
        {
            if (FlightSimProviders.FSUIPC.IsConnected)
            {
                return;
            }
            _sapHandshakeSent = false;
            SetLines(connected);
        }

        private void SimConnect_OnQuit(FlightSimProviderBase sender)
        {
            if (FlightSimProviders.FSUIPC.IsConnected)
            {
                return;
            }
            if (_sapHandshakeSent)
            {
                SendUdp(new { func = "mission", msg = "stop" });
                _sapHandshakeSent = false;
            }
            SetLines(waiting);
        }

        private void SimConnect_OnReadyToFly(FlightSimProviderBase sender, ReadyToFly readyToFly)
        {
            if (FlightSimProviders.FSUIPC.IsConnected)
            {
                return;
            }
            if (readyToFly == ReadyToFly.Ready)
            {
                EnsureSimAppProHandshake();
                UpdateScreen(sender);
            }
            else
            {
                if (_sapHandshakeSent)
                {
                    SendUdp(new { func = "mission", msg = "stop" });
                    _sapHandshakeSent = false;
                }
                SetLines(FlightSimProviders.SimConnect.IsConnected ? connected : waiting);
            }
        }

        private void FSUIPC_OnConnected(FlightSimProviderBase sender)
        {
            _sapHandshakeSent = false;
            SetLines(connected);
        }

        private void FSUIPC_OnQuit(FlightSimProviderBase sender)
        {
            if (_sapHandshakeSent)
            {
                SendUdp(new { func = "mission", msg = "stop" });
                _sapHandshakeSent = false;
            }
            SetLines(waiting);
        }

        private void FSUIPC_OnReadyToFly(FlightSimProviderBase sender, ReadyToFly readyToFly)
        {
            if (readyToFly == ReadyToFly.Ready)
            {
                EnsureSimAppProHandshake();
                UpdateScreen(sender);
            }
            else
            {
                if (_sapHandshakeSent)
                {
                    SendUdp(new { func = "mission", msg = "stop" });
                    _sapHandshakeSent = false;
                }
                SetLines(FlightSimProviders.FSUIPC.IsConnected ? connected : waiting);
            }
        }

        private void EnsureSimAppProHandshake()
        {
            if (_sapHandshakeSent)
            {
                return;
            }
            SendUdp(new { func = "net", msg = "ready" });
            SendUdp(new { func = "mission", msg = "ready" });
            SendUdp(new { func = "mission", msg = "start" });
            SendUdp(new { func = "mod", msg = ModuleKey });
            _sapHandshakeSent = true;
        }

        private void UpdateScreen()
        {
            if (FlightSimProviders.SimConnect.IsReadyToFly == ReadyToFly.Ready)
            {
                if (!FlightSimProviders.SimConnect.IsConnected)
                {
                    UpdateScreen(FlightSimProviders.SimConnect);
                }
            }
            if (FlightSimProviders.FSUIPC.IsReadyToFly == ReadyToFly.Ready)
            {
                UpdateScreen(FlightSimProviders.FSUIPC);
            }
            if (FlightSimProviders.XPlane.IsReadyToFly == ReadyToFly.Ready)
            {
                UpdateScreen(FlightSimProviders.XPlane);
            }
        }

        private void UpdateScreen(FlightSimProviderBase sender)
        {
            if (!sender.IsConnected)
            {
                return;
            }

            bool com2Active = sender.Com2Transmit;
            string comLabel = com2Active ? "COM2" : "COM1";

            double comAct = com2Active ? sender.Com2Frequency : sender.Com1Frequency;
            double comStb = com2Active ? sender.Com2StandByFrequency : sender.Com1StandByFrequency;

            bool nav2Active = sender.Nav2Receive;
            string navLabel = nav2Active ? "NAV2" : "NAV1";
            bool navActive = sender.Nav2Receive || sender.Nav1Receive;

            double navAct = nav2Active ? sender.Nav2Frequency : sender.Nav1Frequency;
            double navStb = nav2Active ? sender.Nav2StandByFrequency : sender.Nav1StandByFrequency;

            string xpdrMode;
            switch (sender.TransponderMode)
            {
                case TransponderMode.Off: xpdrMode = " OFF"; break;
                case TransponderMode.Standby:
                case TransponderMode.Test:
                case TransponderMode.Ground_Mode_S: xpdrMode = "STBY"; break;
                case TransponderMode.On_Mode_A: xpdrMode = "  ON"; break;
                case TransponderMode.Alt_Mode_C: xpdrMode = " ALT"; break;
                default: xpdrMode = "STBY"; break;
            }

            int hdg = (int)Math.Round(sender.HeadingMagneticDegrees) % 360;
            int alt = (int)Math.Round(sender.AltitudeMSLFeet);
            int ias = (int)Math.Round(sender.AirSpeedIndicatedKnots);
            int gs = (int)Math.Round(sender.GroundSpeedKnots);

            string comActS = comAct.ToString("000.00", CultureInfo.InvariantCulture);
            string comStbS = comStb.ToString("000.00", CultureInfo.InvariantCulture);
            string navActS = navAct.ToString("000.00", CultureInfo.InvariantCulture);
            string navStbS = navStb.ToString("000.00", CultureInfo.InvariantCulture);

            string band = navAct < 112.0 ? "Y" : "X";
            int channel = (int)Math.Round(navAct * 10) % 126;
            if (channel == 0) channel = 1;

            string mode3 = sender.Transponder.ToString("0000", CultureInfo.InvariantCulture);

            //string l1 = $"{comLabel} {comActS} STBY {comStbS}";
            //string l2 = $"{navLabel} {navActS} STBY {navStbS}";
            //string l3 = $"HDG  {hdg:000}     ALT  {alt:00000}";
            //string l4 = $"ASPD {ias:000}     GSPD {gs:000}";
            //string l5 = $"XPDR {sender.Transponder:0000}    {xpdrMode}";

            string l1 = string.Empty;
            string l2 = string.Empty;
            string l3 = string.Empty;
            string l4 = string.Empty;
            string l5 = string.Empty;
            switch(ActivePage)
            {
                case Page.COM_1:
                    (l1, l2, l3, l4, l5) = BuildComLines(1, sender.Com1Frequency, sender.Com1StandByFrequency, sender.Com1Transmit, sender.Com2Transmit);
                    break;
                case Page.COM_2:
                    (l1, l2, l3, l4, l5) = BuildComLines(2, sender.Com2Frequency, sender.Com2StandByFrequency, sender.Com1Transmit, sender.Com2Transmit);
                    break;
                case Page.IFF:
                    (l1, l2, l3, l4, l5) = BuildIffLines(mode3, xpdrMode, sender.IdentActive);
                    break;
                case Page.NAV_1:
                    (l1, l2, l3, l4, l5) = BuildNavLines(nav2Active ? 2 : 1, navAct, navStb, navActive);
                    break;
                case Page.CNI:
                default:
                    (l1, l2, l3, l4, l5) = BuildCniLines(comAct, navAct, mode3, comLabel, navLabel, xpdrMode, channel, band, hdg);
                    break;
            }
            
            // ✅ Always update local ICP
            var dedLines = new[] { l1, l2, l3, l4, l5 };
            SetLines(dedLines);

            // ✅ Only send to SimAppPro if it has subscribed to DED
            bool wantsDed;
            lock (_commonLock)
                wantsDed = _commonKeys.Contains("DED");

            if (!wantsDed || !_sapHandshakeSent)
                return;

            // Build the SimAppPro-compatible DED string
            string ded = string.Empty;
            
            switch(ActivePage)
            {
                case Page.COM_1:
                    ded = BuildDedCom(1, sender.Com1Frequency, sender.Com1StandByFrequency, sender.Com1Receive, sender.Com2Receive);
                    break;
                case Page.COM_2:
                    ded = BuildDedCom(2, sender.Com2Frequency, sender.Com2StandByFrequency, sender.Com1Receive, sender.Com2Receive);
                    break;
                case Page.IFF:
                    ded = BuildDedIff(mode3, xpdrMode, sender.IdentActive);
                    break;
                case Page.NAV_1:
                    ded = BuildDedNav(nav2Active ? 2 : 1, navAct, navStb, navActive);
                    break;
                case Page.CNI:
                default:
                    ded = BuildDedCni(comAct, navAct, mode3, comLabel, navLabel, xpdrMode,channel, band, hdg);
                    break;

            }

            // Build args only for keys SimAppPro requested
            Dictionary<string, string> outArgs;
            lock (_commonLock)
            {
                outArgs = new Dictionary<string, string>(_commonKeys.Count, StringComparer.OrdinalIgnoreCase);
                foreach (var key in _commonKeys)
                    outArgs[key] = key.Equals("DED", StringComparison.OrdinalIgnoreCase) ? ded : "";
            }

            // If somehow we got here without keys, don't spam anyway—just send DED.
            if (outArgs.Count == 0)
                outArgs["DED"] = ded;

            SendUdp(new
            {
                func = "addCommon",
                args = outArgs,
                timestamp = 1.2
            });
        }

        private static string Inv(string s) => $"⟦{s}⟧";

        private static (string l1, string l2, string l3, string l4, string l5) BuildCniLines(
            double comAct,
            double navAct,
            string mode3,
            string comLabel,
            string navLabel,
            string xpdrMode,
            int channel,
            string band,
            int hdg)
        {
            string l1 = $"{comLabel} {comAct.ToString("000.00", CultureInfo.InvariantCulture)}     HDG {hdg:000}";
            string l2 = string.Empty;
            string l3 = $"{navLabel} {navAct.ToString("000.00", CultureInfo.InvariantCulture)}    {DateTime.Now.ToString("HH:mm:ss")}";
            string l4 = string.Empty;
            string l5 = $"XPDR {mode3} {xpdrMode}     T{channel}{band}";

            return (l1, l2, l3, l4, l5);
        }

        private static (string l1, string l2, string l3, string l4, string l5) BuildComLines(
            int com,
            double comActive,
            double comStandby,
            bool com1Receive,
            bool com2Receive,
            bool invertStandby = true)   // invert standby like the boxed scratchpad
        {
            string receiverMode = (com1Receive && com2Receive) ? "BOTH" : "MAIN";

            string act = comActive.ToString("000.00", CultureInfo.InvariantCulture);
            string stb = comStandby.ToString("000.00", CultureInfo.InvariantCulture);

            // Inversion markup (see section 2)
            string stbMarked = invertStandby ? Inv(stb) : stb;

            string l1 = $"     COM{com}     {receiverMode}";
            string l2 = $"  {act}";
            string l3 = $"              ⟦*⟧{stb}⟦*⟧";
            string l4 = $"  PRE   1 ↕       TOD";
            string l5 = $"     {stb}        NB";

            return (l1, l2, l3, l4, l5);
        }

        private static (string l1, string l2, string l3, string l4, string l5) BuildNavLines(
            int nav,
            double navActive,
            double navStandby,
            bool isOn,
            bool invertStandby = true)   // invert standby like the boxed scratchpad
        {
            string receiverMode = (isOn) ? " ON" : "OFF";

            string act = navActive.ToString("000.00", CultureInfo.InvariantCulture);
            string stb = navStandby.ToString("000.00", CultureInfo.InvariantCulture);

            // Inversion markup (see section 2)
            string stbMarked = invertStandby ? Inv(stb) : stb;

            string l1 = $"     NAV{nav}     {receiverMode}";
            string l2 = $"  {act}";
            string l3 = $"              ⟦*⟧{stb}⟦*⟧";
            string l4 = $"  PRE   1 ↕       TOD";
            string l5 = $"     {stb}        NB";

            return (l1, l2, l3, l4, l5);
        }

        private static string BuildDedCni(
            double uhfFreq,
            double vhfFreq,
            string mode3,
            string comLabel,
            string navLabel,
            string xpdrMode,
            int channel,
            string band,
            int steerpoint)
        {
            const string sep = "-----------------------------------------";

            return string.Join("\n", new[]
            {
                // UHF label block
                sep, "UHF Mode Rotary_placeholder", "",
                "", "children are {",
                sep, "UHF Mode Rotary", comLabel,
                "}",

                // UHF freq
                sep, "Selected UHF Frequency", uhfFreq.ToString("000.00", CultureInfo.InvariantCulture),

                // ★ Upper-right steerpoint cluster ★
                sep, "Steerpoint Use", "  HDG",
                //sep, "WPT IncDecSymbol", "↕",
                sep, "Selected Steerpoint", steerpoint.ToString(),

                // VHF label block
                sep, "VHF Label_placeholder", "",
                "", "children are {",
                sep, "VHF Label", navLabel,
                "}",

                // VHF freq
                sep, "Selected VHF Frequency", vhfFreq.ToString("000.00", CultureInfo.InvariantCulture),

                // IFF / XPDR block (left bottom)
                sep, "DED CNI IFF PH", " ",
                "", "children are {",
                sep, "IFF Modes Label", "XPDR " + mode3 + " " + xpdrMode,
                "}",

                // TACAN block (right bottom)
                sep, "DED CNI TACAN PH", "",
                "", "children are {",
                sep, "TACAN Label", "T",
                sep, "TACAN Channel", channel.ToString(),
                sep, "TACAN Band", band,
                "}",

                // System time (far right)
                sep, "System Time", DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture),

                ""
            });
        }

        private static (string l1, string l2, string l3, string l4, string l5) BuildIffLines(
            string xpdrCode,            // "1200"
            string xpdrModeLabel,     // "OFF" / "STBY" / "ON" / "ALT" (your label)
            bool identActive)          // if you want a brief marker
        {
            string eventOccured = identActive ? Inv("ID") : string.Empty;

            string l1 = $" IFF {xpdrModeLabel}   STAT {eventOccured}";
            string l2 = string.Empty;
            string l3 = $" M3:{xpdrCode}";
            string l4 = string.Empty;
            string l5 = string.Empty;

            return (l1, l2, l3, l4, l5);
        }

        private static string BuildDedCom(
            int com,
            double comActiveFreq,
            double comStandbyFreq,
            bool com1Receive,
            bool com2Receive)
        {
            const string sep = "-----------------------------------------";

            // Receiver mode (cosmetic but meaningful)
            string receiverMode = (com1Receive && com2Receive) ? "BOTH" : "MAIN";

            // Frequency formatting
            string act = comActiveFreq.ToString("000.00", CultureInfo.InvariantCulture);
            string stb = comStandbyFreq.ToString("000.00", CultureInfo.InvariantCulture);

            return string.Join("\n", new[]
            {
                // --- Top labels ---
                //sep, "Secure Voice", "SEC",
                sep, $"COM {com} Mode", $"COM{com}",
                sep, "Receiver Mode", receiverMode,

                // --- Active frequency ---
                sep, "Active Frequency or Channel", act,

                // --- Scratchpad (standby freq) ---
                sep, "Scratchpad", stb,
                sep, "Asterisks on Scratchpad_lhs", "*",
                sep, "Asterisks on Scratchpad_rhs", "*",

                // --- Preset block (cosmetic in Phase 1) ---
                sep, "Preset Label", "PRE",
                
                sep, "Preset Number", "1",
                sep, "Preset Frequency", stb,

                // --- TOD / Bandwidth ---
                sep, "TOD Label", "TOD",
                sep, "Bandwidth", "NB",

                ""
            });
        }

        private static string BuildDedNav(
            int nav,
            double navActiveFreq,
            double navStandbyFreq,
            bool isOn)
        {
            const string sep = "-----------------------------------------";

            // Receiver mode (cosmetic but meaningful)
            string receiverMode = (isOn) ? " ON" : "OFF";

            // Frequency formatting
            string act = navActiveFreq.ToString("000.00", CultureInfo.InvariantCulture);
            string stb = navStandbyFreq.ToString("000.00", CultureInfo.InvariantCulture);

            return string.Join("\n", new[]
            {
                // --- Top labels ---
                //sep, "Secure Voice", "SEC",
                sep, $"COM {nav} Mode", $"NAV{nav}",
                sep, "Receiver Mode", receiverMode,

                // --- Active frequency ---
                sep, "Active Frequency or Channel", act,

                // --- Scratchpad (standby freq) ---
                sep, "Scratchpad", stb,
                sep, "Asterisks on Scratchpad_lhs", "*",
                sep, "Asterisks on Scratchpad_rhs", "*",

                // --- Preset block (cosmetic in Phase 1) ---
                sep, "Preset Label", "PRE",

                sep, "Preset Number", "1",
                sep, "Preset Frequency", stb,

                // --- TOD / Bandwidth ---
                sep, "TOD Label", "TOD",
                sep, "Bandwidth", "NB",

                ""
            });
        }

        private static string BuildDedIff(
            string xpdrCode,            // "1200"
            string xpdrModeLabel,     // "OFF" / "STBY" / "ON" / "ALT" (your label)
            bool identActive          // if you want a brief marker
        )
        {
            const string sep = "-----------------------------------------";

            // The sheet shows "Event Occured" example values like "P/T".
            // We can repurpose this as IDENT feedback if desired.
            string eventOccured = identActive ? "ID" : "";

            return string.Join("\n", new[]
            {
                // Header row
                sep, "IFF label", "IFF",
                sep, "IFF Power Status", xpdrModeLabel,
                sep, "Mode label", "STAT",
                sep, "Event Occured", eventOccured,

                // M1 label (use the non-inv placeholder/child)
                sep, "M1 Mode_placeholder", "",
                "", "children are {",
                sep, "M1 Mode", " M3",
                "}",
                sep, "M1 Lockout Status", ":",
                sep, "M1 Code", xpdrCode,

                ""
            });
        }


        private void XPlane_OnFlightDataReceived(FlightSimProviderBase sender)
        {
            UpdateScreen(sender);
        }

        private void SimConnect_OnFlightDataReceived(FlightSimProviderBase sender)
        {
            if (FlightSimProviders.FSUIPC.IsConnected)
            {
                return;
            }
            UpdateScreen(sender);
        }

        private void FSUIPC_OnFlightDataReceived(FlightSimProviderBase sender)
        {
            UpdateScreen(sender);
        }

        private void SimAppProForm_Load(object sender, EventArgs e)
        {
            UpdateChecked();
            HideWindow();
        }

        private void SimAppProForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_sapHandshakeSent)
            {
                SendUdp(new { func = "mission", msg = "stop" });
                _sapHandshakeSent = false;
            }
            _running = false;
            FlightSimProviders.XPlane.Deinitialize();
            FlightSimProviders.SimConnect.Deinitialize();
            FlightSimProviders.FSUIPC.Deinitialize();
        }

        public const uint SC_MINIMIZE = 0xf020;
        public const uint SC_CLOSE = 0xF060;
        public const uint WM_SYSCOMMAND = 0x0112;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == SimConnect.WM_USER_SIMCONNECT)
            {
                FlightSimProviders.SimConnect.ReceiveMessage();
                return;
            }
            else if (m.Msg == WM_SYSCOMMAND)
            {
                if (m.WParam.ToInt32() == SC_MINIMIZE)
                {
                    // Caption bar minimize button
                    if (Properties.Settings.Default.MinimizeToSystemTray)
                    {
                        m.Result = IntPtr.Zero;
                        ShowInTaskbar = false;
                        Visible = false;
                        return;
                    }
                }
                else if (m.WParam.ToInt32() == SC_CLOSE)
                {
                    // Caption bar close button
                    if (Properties.Settings.Default.MinimizeToSystemTray)
                    {
                        m.Result = IntPtr.Zero;
                        ShowInTaskbar = false;
                        Visible = false;
                        return;
                    }
                }
            }
            base.WndProc(ref m);
        }

        private void StartUdp()
        {
            // Bind to an ephemeral local port; SimAppPro will reply to it.
            _udp = new UdpClient(0);
            _udp.Client.ReceiveTimeout = 0;

            _running = true;
            Task.Run(ReceiveLoop);
        }

        private readonly object _commonLock = new();
        private readonly HashSet<string> _commonKeys = new(StringComparer.OrdinalIgnoreCase);

        private async Task ReceiveLoop()
        {
            if (_udp == null) return;

            while (_running)
            {
                UdpReceiveResult res;
                try { res = await _udp.ReceiveAsync(); }
                catch { continue; }

                var json = Encoding.UTF8.GetString(res.Buffer);
                Debug.WriteLine($"UDP IN ({res.RemoteEndPoint}): {json}");

                var jobj = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(json);
                var func = (string?)jobj?["func"];

                switch (func)
                {
                    case "clearCommon":
                        lock (_commonLock) _commonKeys.Clear();
                        // optional: also clear local display
                        SetLines(new[] { "", "", "", "", "" });
                        break;

                    case "addCommon":
                        // SimAppPro sends args = { "DED": "<identifyMethod lua...>", ... }
                        var args = jobj?["args"] as Newtonsoft.Json.Linq.JObject;
                        if (args != null)
                        {
                            lock (_commonLock)
                            {
                                foreach (var prop in args.Properties())
                                    _commonKeys.Add(prop.Name);
                            }

                            Debug.WriteLine("SimAppPro common keys: " +
                                string.Join(", ", args.Properties().Select(p => p.Name)));
                        }
                        break;

                    case "clearOutput":
                    case "addOutput":
                        // For now, you can ignore these unless you’re also driving SimAppPro “outputs”.
                        // Logging is enough.
                        break;
                }
            }
        }

        private void SendUdp(object payload)
        {
            if (_udp != null)
            {
                var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                _udp.Send(bytes, bytes.Length, Properties.Settings.Default.SapHost, Properties.Settings.Default.SapPort);
            }
        }

        private void SendUdp(object payload, string host, int port)
        {
            if (_udp != null)
            {
                var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                _udp.Send(bytes, bytes.Length, host, port);
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ShowWindow();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowWindow();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void startMinimizedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.StartMinimized = !Properties.Settings.Default.StartMinimized;
            startMinimizedToolStripMenuItem.Checked = Properties.Settings.Default.StartMinimized;
            Properties.Settings.Default.Save();
        }

        private void licenseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LicenseDlg dlg = new LicenseDlg();
            dlg.ShowDialog(this);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutDlg dlg = new AboutDlg();
            dlg.ShowDialog(this);
        }

        public void HideWindow()
        {
            if (Properties.Settings.Default.StartMinimized)
            {
                if (Properties.Settings.Default.MinimizeToSystemTray)
                {
                    Visible = false;
                    ShowInTaskbar = false;
                }
                else
                {
                    WindowState = FormWindowState.Minimized;
                }
            }
        }

        public void ShowWindow()
        {
            WindowState = FormWindowState.Normal;
            ShowInTaskbar = true;
            this.Show();
            this.BringToFront();
            this.Focus();
            this.Activate();
        }

        public void ActivateFromExternalSignal()
        {
            ShowWindow();
        }

        private void minimizeToSystemTrayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.MinimizeToSystemTray = !Properties.Settings.Default.MinimizeToSystemTray;
            minimizeToSystemTrayToolStripMenuItem.Checked = Properties.Settings.Default.MinimizeToSystemTray;
            Properties.Settings.Default.Save();
        }

        private void simAppProSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string sapHost = Properties.Settings.Default.SapHost;
            int sapPort = Properties.Settings.Default.SapPort;
            SimAppProSettings settings = new SimAppProSettings();
            if (settings.ShowDialog(this) == DialogResult.OK)
            {
                if (!sapHost.Equals(Properties.Settings.Default.SapHost) || sapPort != Properties.Settings.Default.SapPort)
                {
                    if (FlightSimProviders.SimConnect.IsReadyToFly == ReadyToFly.Ready && _sapHandshakeSent)
                    {
                        if (!FlightSimProviders.FSUIPC.IsConnected)
                        {
                            SendUdp(new { func = "mission", msg = "stop" }, sapHost, sapPort);
                        }
                    }
                    if (FlightSimProviders.FSUIPC.IsReadyToFly == ReadyToFly.Ready && _sapHandshakeSent)
                    {
                        SendUdp(new { func = "mission", msg = "stop" }, sapHost, sapPort);
                    }
                    if (FlightSimProviders.XPlane.IsReadyToFly == ReadyToFly.Ready && _sapHandshakeSent)
                    {
                        SendUdp(new { func = "mission", msg = "stop" }, sapHost, sapPort);
                    }
                    _sapHandshakeSent = false;
                    if (FlightSimProviders.SimConnect.IsReadyToFly == ReadyToFly.Ready)
                    {
                        SimConnect_OnReadyToFly(FlightSimProviders.SimConnect, ReadyToFly.Ready);
                    }
                    if (FlightSimProviders.FSUIPC.IsReadyToFly == ReadyToFly.Ready)
                    {
                        FSUIPC_OnReadyToFly(FlightSimProviders.FSUIPC, ReadyToFly.Ready);
                    }
                    if (FlightSimProviders.XPlane.IsReadyToFly == ReadyToFly.Ready)
                    {
                        XPlane_OnReadyToFly(FlightSimProviders.XPlane, ReadyToFly.Ready);
                    }
                }
            }
        }

        private void xPlaneSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            XPlaneSettings settings = new XPlaneSettings();
            if (settings.ShowDialog(this) == DialogResult.OK)
            {
                FlightSimProviders.XPlane.UpdateConnection(Properties.Settings.Default.XPlaneIPAddress, Properties.Settings.Default.XPlanePort);
            }
        }

        private void shutdownWithFlightSimToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.ShutdownWithFS = !Properties.Settings.Default.ShutdownWithFS;
            shutdownWithFlightSimToolStripMenuItem.Checked = Properties.Settings.Default.ShutdownWithFS;
            Properties.Settings.Default.Save();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            List<Process> flightSim = Process.GetProcessesByName("FlightSimulator").ToList();
            Process[] flightSim2024 = Process.GetProcessesByName("FlightSimulator2024");
            if (flightSim2024 != null)
            {
                flightSim.AddRange(flightSim2024);
            }
            Process[] x_plane = Process.GetProcessesByName("X-Plane");
            if (x_plane != null)
            {
                flightSim.AddRange(x_plane);
            }
            if ((flightSim == null || flightSim.Count == 0) && !waitForFlightSim && Properties.Settings.Default.ShutdownWithFS)
            {
                Close();
                return;
            }
            else if (flightSim != null && flightSim.Count > 0 && waitForFlightSim)
            {
                waitForFlightSim = false;
            }
            timer1.Start();
        }

        private void UpdateChecked()
        {
            startMinimizedToolStripMenuItem.Checked = Properties.Settings.Default.StartMinimized;
            minimizeToSystemTrayToolStripMenuItem.Checked = Properties.Settings.Default.MinimizeToSystemTray;
            shutdownWithFlightSimToolStripMenuItem.Checked = Properties.Settings.Default.ShutdownWithFS;
            cNIToolStripMenuItem.Checked = Properties.Settings.Default.ActivePage == (int)Page.CNI;
            cOM1ToolStripMenuItem.Checked = Properties.Settings.Default.ActivePage == (int)Page.COM_1;
            cOM2ToolStripMenuItem.Checked = Properties.Settings.Default.ActivePage == (int)Page.COM_2;
            iFFToolStripMenuItem.Checked = Properties.Settings.Default.ActivePage == (int)Page.IFF;
            nAVToolStripMenuItem.Checked = Properties.Settings.Default.ActivePage == (int)Page.NAV_1;
        }

        private void cNIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ActivePage = Page.CNI;
            UpdateChecked();
            UpdateScreen();
        }

        private void cOM1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ActivePage = Page.COM_1;
            UpdateChecked();
            UpdateScreen();
        }

        private void cOM2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ActivePage = Page.COM_2;
            UpdateChecked();
            UpdateScreen();
        }

        private void iFFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ActivePage = Page.IFF;
            UpdateChecked();
            UpdateScreen();
        }

        private void nAVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ActivePage = Page.NAV_1;
            UpdateChecked();
            UpdateScreen();
        }
    }
}
