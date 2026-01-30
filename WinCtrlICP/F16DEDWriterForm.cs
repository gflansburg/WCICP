using FlightSim;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;

namespace WinCtrlICP
{
    public partial class F16DEDWriterForm : Form
    {
        private F16DedBridge bridge;

        private const string MSFSKEY = "WinCtrl IPC";

        private string[] waiting = { string.Empty, "         WAITING", "           TO", "         CONNECT", string.Empty };
 
        private bool _handleInitialized;

        private List<UserIcpDisplay> UserIcpDisplays { get; set; } = new List<UserIcpDisplay>();

        public static F16DEDWriterForm Instance { get; private set; } = null!;

        public Joysticks Joysticks { get; private set; } = null!;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CaptureJoystickEvents { get; set; } = true;

        private readonly object _linesLock = new object();

        // last lines requested by UpdateScreen (always stored)
        private string[]? _lastRequestedLines;

        // last lines actually rendered to icpDisplayControl1
        private string[]? _lastUiLines;

        // last lines actually sent to the bridge
        private string[]? _lastSentLines;

        private Page ActivePage
        {
            get
            {
                return Enum.IsDefined(typeof(Page), Properties.Settings.Default.ActivePage) ? (Page)Properties.Settings.Default.ActivePage : Page.CNI;
            }
            set
            {
                Properties.Settings.Default.ActivePage = (int)value;
                Properties.Settings.Default.Save();
            }
        }

        private bool waitForFlightSim = true;

        private Task? _bridgeInitTask;
        private volatile bool _bridgeReady;

        public F16DEDWriterForm()
        {
            Instance = this;
            FlightSimProviders.XPlane.OnFlightDataReceived += XPlane_OnFlightDataReceived;
            FlightSimProviders.XPlane.OnConnected += XPlane_OnConnected;
            FlightSimProviders.XPlane.OnQuit += XPlane_OnQuit;
            FlightSimProviders.XPlane.OnReadyToFly += XPlane_OnReadyToFly;
            FlightSimProviders.XPlane.OnAircraftChange += XPlane_OnAircraftChange;
            FlightSimProviders.SimConnect.OnFlightDataReceived += SimConnect_OnFlightDataReceived;
            FlightSimProviders.SimConnect.OnConnected += SimConnect_OnConnected;
            FlightSimProviders.SimConnect.OnQuit += SimConnect_OnQuit;
            FlightSimProviders.SimConnect.OnReadyToFly += SimConnect_OnReadyToFly;
            FlightSimProviders.SimConnect.OnAircraftChange += SimConnect_OnAircraftChange;
            FlightSimProviders.FSUIPC.OnFlightDataReceived += FSUIPC_OnFlightDataReceived;
            FlightSimProviders.FSUIPC.OnConnected += FSUIPC_OnConnected;
            FlightSimProviders.FSUIPC.OnQuit += FSUIPC_OnQuit;
            FlightSimProviders.FSUIPC.OnReadyToFly += FSUIPC_OnReadyToFly;
            FlightSimProviders.FSUIPC.OnAircraftChange += FSUIPC_OnAircraftChange;
#if DEBUG
            FlightSimProviders.Preview.OnFlightDataReceived += Preview_OnFlightDataReceived;
#endif
            FlightSimProviderBase.Units = Properties.Settings.Default.UnitSystem == 1 ? UnitSystem.Metric : UnitSystem.Aviation;
            InitializeComponent();
            VisibleChanged += (_, __) => FlushUiIfNeeded();
            Resize += (_, __) => FlushUiIfNeeded();
            Joysticks = new Joysticks();
            Joysticks.JoystickEvent += Joysticks_JoystickEvent;
            UserIcpDisplays = UserIcpDisplayStore.Load();
            bridge = new F16DedBridge();
            bridge.OnBridgeFatal += Bridge_OnBridgeFatal;
            TryStartBridge();
            HandleCreated += F16DEDWriterForm_HandleCreated;
        }

        private string Center(string text, int width)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty.PadRight(width);

            if (text.Length >= width)
                return text.Substring(0, width);

            int padding = width - text.Length;
            int padLeft = padding / 2 + text.Length;
            return text.PadLeft(padLeft).PadRight(width);
        }

        private string[] Connected(FlightSimProviderBase sender)
        {
            return new string[]
            {
                string.Empty,
                Center(sender.Name, 25),
                Center("CONNECTED", 25),
                string.Empty,
                string.Empty
            };
        }

        private string[] ReadyToFly(FlightSimProviderBase sender)
        {
            return new string[]
            {
                string.Empty,
                Center(sender.Name, 25),
                Center(sender.IsReadyToFly.ToString(), 25),
                string.Empty,
                string.Empty
            };
        }

        private void TryStartBridge()
        {
            if (_bridgeInitTask == null || _bridgeInitTask.IsCompleted)
                _bridgeInitTask = InitializeAsync();
        }

        private void Bridge_OnBridgeFatal(string msg)
        {
            // Bridge is dead for this run; prevent any further pipe sends.
            _bridgeReady = false;
            lock (_linesLock)
            {
                _lastSentLines = null;
            }
        }

#if DEBUG
        private void Preview_OnFlightDataReceived(FlightSimProviderBase sender)
        {
            if (FlightSimProviders.SimConnect.IsConnected || FlightSimProviders.FSUIPC.IsConnected || FlightSimProviders.XPlane.IsConnected)
            {
                return;
            }
            UpdateScreen(sender);
        }
#endif

        private void FSUIPC_OnAircraftChange(FlightSimProviderBase sender, int aircraftId)
        {
            UpdateScreen(sender);
        }

        private void SimConnect_OnAircraftChange(FlightSimProviderBase sender, int aircraftId)
        {
            UpdateScreen(sender);
        }

        private void XPlane_OnAircraftChange(FlightSimProviderBase sender, int aircraftId)
        {
            UpdateScreen(sender);
        }

        private void F16DEDWriterForm_HandleCreated(object? sender, EventArgs e)
        {
            if (_handleInitialized)
            {
                return;
            }
            _handleInitialized = true;
            if (_simConnectInitQueued)
            {
                BeginInvoke(new Action(() =>
                {
                    FlightSimProviders.SimConnect.MainWindowHandle = Handle;
                }));
            }
            if (_lastRequestedLines != null)
            {
                SetLines(_lastRequestedLines);
            }
            else
            {
                SetLines(waiting);
            }
        }

        private async Task InitializeAsync()
        {
            await bridge.ConnectOrLaunchAsync(connectTimeoutMs: 1200, launchWaitMs: 8000).ConfigureAwait(false);
            _bridgeReady = true;
            string[]? lines;
            lock (_linesLock)
            {
                lines = _lastRequestedLines;
            }

            if (lines != null)
            {
                await bridge.DrawIcpAsync(lines).ConfigureAwait(false);
            }
        }

        private void SetLines(string[] lines)
        {
            bool doUi = false;
            bool doSend = false;

            lock (_linesLock)
            {
                // Requirement #2: always remember what the last lines were
                _lastRequestedLines = lines;

                // Requirement #1: only update UI if we're actually visible + not minimized
                if (ShouldRenderUiNow() && !LinesEqual(_lastUiLines, lines))
                {
                    _lastUiLines = lines;
                    doUi = true;
                }

                // Requirement #3: only SendLines if lines actually changed
                if (!LinesEqual(_lastSentLines, lines))
                {
                    _lastSentLines = lines;
                    doSend = true;
                }
            }

            if (doUi)
            {
                Ui(() => icpDisplayControl1.SetLines(lines));
            }

            // Only send to the pipe when the bridge is ready AND the lines changed.
            if (doSend)
            {
                if (_bridgeReady)
                {
                    _ = bridge.DrawIcpAsync(lines);
                }
            }
        }

        private void FlushUiIfNeeded()
        {
            if (!ShouldRenderUiNow()) return;

            string[]? linesToRender = null;

            lock (_linesLock)
            {
                if (_lastRequestedLines != null && !LinesEqual(_lastUiLines, _lastRequestedLines))
                {
                    _lastUiLines = _lastRequestedLines;
                    linesToRender = _lastRequestedLines;
                }
            }

            if (linesToRender != null)
            {
                Ui(() => icpDisplayControl1.SetLines(linesToRender));
            }
        }

        private bool ShouldRenderUiNow()
        {
            // "Hidden" in your app means Visible == false (tray mode), and minimized is WindowState == Minimized.
            // Also don't waste UI work if the form handle isn't ready.
            return IsHandleCreated
                   && Visible
                   && WindowState != FormWindowState.Minimized;
        }

        private static bool LinesEqual(string[]? a, string[]? b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null) return false;
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (!string.Equals(a[i], b[i], StringComparison.Ordinal)) return false;
            }
            return true;
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
            SetLines(Connected(sender));
        }

        private void XPlane_OnQuit(FlightSimProviderBase sender)
        {
            SetLines(waiting);
        }

        private void XPlane_OnReadyToFly(FlightSimProviderBase sender, ReadyToFly readyToFly)
        {
            if (readyToFly == FlightSim.ReadyToFly.Ready)
            {
                UpdateScreen(sender);
            }
            else
            {
                SetLines(FlightSimProviders.XPlane.IsConnected ? Connected(sender) : waiting);
            }
        }

        private void SimConnect_OnConnected(FlightSimProviderBase sender)
        {
            if (FlightSimProviders.FSUIPC.IsConnected)
            {
                return;
            }
            SetLines(Connected(sender));
        }

        private void SimConnect_OnQuit(FlightSimProviderBase sender)
        {
            if (FlightSimProviders.FSUIPC.IsConnected)
            {
                return;
            }
            SetLines(waiting);
        }

        private void SimConnect_OnReadyToFly(FlightSimProviderBase sender, ReadyToFly readyToFly)
        {
            if (FlightSimProviders.FSUIPC.IsConnected)
            {
                return;
            }
            if (readyToFly == FlightSim.ReadyToFly.Ready)
            {
                UpdateScreen(sender);
            }
            else
            {

                SetLines(FlightSimProviders.SimConnect.IsConnected ? Connected(sender) : waiting);
            }
        }

        private void FSUIPC_OnConnected(FlightSimProviderBase sender)
        {
            SetLines(Connected(sender));
        }

        private void FSUIPC_OnQuit(FlightSimProviderBase sender)
        {
            SetLines(waiting);
        }

        private void FSUIPC_OnReadyToFly(FlightSimProviderBase sender, ReadyToFly readyToFly)
        {
            if (readyToFly == FlightSim.ReadyToFly.Ready)
            {
                UpdateScreen(sender);
            }
            else
            {
                SetLines(FlightSimProviders.FSUIPC.IsConnected ? Connected(sender) : waiting);
            }
        }

        public FlightSimProviderBase? IsConnected
        {
            get
            {
                if (FlightSimProviders.FSUIPC.IsConnected)
                {
                    return FlightSimProviders.FSUIPC;
                }
                else if (FlightSimProviders.SimConnect.IsConnected)
                {
                    return FlightSimProviders.SimConnect;
                }
                else if (FlightSimProviders.XPlane.IsConnected)
                {
                    return FlightSimProviders.XPlane;
                }
#if DEBUG
                else if (FlightSimProviders.Preview.IsConnected)
                {
                    return FlightSimProviders.Preview;
                }
#endif
                return null;
            }
        }

        public FlightSimProviderBase? IsReadyToFly
        {
            get
            {
                if (FlightSimProviders.FSUIPC.IsConnected && FlightSimProviders.FSUIPC.IsReadyToFly == FlightSim.ReadyToFly.Ready)
                {
                    return FlightSimProviders.FSUIPC;
                }
                else if (FlightSimProviders.SimConnect.IsConnected && FlightSimProviders.SimConnect.IsReadyToFly == FlightSim.ReadyToFly.Ready)
                {
                    return FlightSimProviders.SimConnect;
                }
                else if (FlightSimProviders.XPlane.IsConnected && FlightSimProviders.XPlane.IsReadyToFly == FlightSim.ReadyToFly.Ready)
                {
                    return FlightSimProviders.XPlane;
                }
#if DEBUG
                else if (FlightSimProviders.Preview.IsConnected && FlightSimProviders.Preview.IsReadyToFly == FlightSim.ReadyToFly.Ready)
                {
                    return FlightSimProviders.Preview;
                }
#endif
                return null;
            }
        }

        private void UpdateScreen()
        {
            FlightSimProviderBase? sender = IsReadyToFly;
            if (sender != null)
            {
                UpdateScreen(sender);
            }
        }

        private void UpdateScreen(FlightSimProviderBase sender)
        {
            if (!sender.IsConnected)
            {
                return;
            }
#if !DEBUG
            if (sender.IsReadyToFly != FlightSim.ReadyToFly.Ready)
            {
               SetLines(ReadyToFly(sender));
               return;
            }
#else
            if (sender is PreviewFlightSimProvider && IsConnected != FlightSimProviders.Preview)
            {
                return;
            }
#endif
            switch (ActivePage)
            {
                case Page.COM_1:
                    SetLines(UserIcpDisplay.BuildIcpLines(sender, SystemDisplays.COM1));
                    break;
                case Page.COM_2:
                    SetLines(UserIcpDisplay.BuildIcpLines(sender, SystemDisplays.COM2));
                    break;
                case Page.IFF:
                    SetLines(UserIcpDisplay.BuildIcpLines(sender, SystemDisplays.IFF));
                    break;
                case Page.NAV_1:
                    SetLines(UserIcpDisplay.BuildIcpLines(sender, SystemDisplays.NAV1));
                    break;
                case Page.NAV_2:
                    SetLines(UserIcpDisplay.BuildIcpLines(sender, SystemDisplays.NAV2));
                    break;
                case Page.GPS:
                    SetLines(UserIcpDisplay.BuildIcpLines(sender, SystemDisplays.GPS));
                    break;
                case Page.FLPN:
                    SetLines(UserIcpDisplay.BuildIcpLines(sender, SystemDisplays.FLPN));
                    break;
                case Page.AP:
                    SetLines(UserIcpDisplay.BuildIcpLines(sender, SystemDisplays.AP));
                    break;
                case Page.SWCH:
                    SetLines(UserIcpDisplay.BuildIcpLines(sender, SystemDisplays.SWCH));
                    break;
                case Page.LGHT:
                    SetLines(UserIcpDisplay.BuildIcpLines(sender, SystemDisplays.LGHT));
                    break;
                case Page.WX:
                    SetLines(UserIcpDisplay.BuildIcpLines(sender, SystemDisplays.WX));
                    break;
                case Page.TIME:
                    SetLines(UserIcpDisplay.BuildIcpLines(sender, SystemDisplays.TIME));
                    break;
                case Page.INFO:
                    SetLines(UserIcpDisplay.BuildIcpLines(sender, SystemDisplays.INFO));
                    break;
                case Page.HROT:
                    SetLines(UserIcpDisplay.BuildIcpLines(sender, SystemDisplays.HROT));
                    break;
                case Page.HDIS:
                    SetLines(UserIcpDisplay.BuildIcpLines(sender, SystemDisplays.HDIS));
                    break;
                case Page.HCTL:
                    SetLines(UserIcpDisplay.BuildIcpLines(sender, SystemDisplays.HCTL));
                    break;
                case Page.Custom:
                    {
                        UserIcpDisplay? userIcpDisplay = UserIcpDisplays.FirstOrDefault(i => i.Id == Properties.Settings.Default.CustomDisplayId);
                        if (userIcpDisplay != null)
                        {
                            SetLines(userIcpDisplay.BuildIcpLines(sender));
                        }
                    }
                    break;
                case Page.CNI:
                default:
                    SetLines(UserIcpDisplay.BuildIcpLines(sender, SystemDisplays.CNI));
                    break;
            }
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
            Show();
            BringToFront();
            Focus();
            Activate();
            FlushUiIfNeeded();
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
            startWithMSFSToolStripMenuItem.Checked =
                   (Tools.IsPluginInstalled(MSFSEdition.Microsoft2020, MSFSKEY) == PluginInstalled.PluginInstalled || Tools.IsPluginInstalled(MSFSEdition.Microsoft2020, MSFSKEY) == PluginInstalled.MSFSNotInstalled)
                && (Tools.IsPluginInstalled(MSFSEdition.Steam2020, MSFSKEY) == PluginInstalled.PluginInstalled || Tools.IsPluginInstalled(MSFSEdition.Steam2020, MSFSKEY) == PluginInstalled.MSFSNotInstalled)
                && (Tools.IsPluginInstalled(MSFSEdition.Microsoft2024, MSFSKEY) == PluginInstalled.PluginInstalled || Tools.IsPluginInstalled(MSFSEdition.Microsoft2024, MSFSKEY) == PluginInstalled.MSFSNotInstalled)
                && (Tools.IsPluginInstalled(MSFSEdition.Steam2024, MSFSKEY) == PluginInstalled.PluginInstalled || Tools.IsPluginInstalled(MSFSEdition.Steam2024, MSFSKEY) == PluginInstalled.MSFSNotInstalled);
            cNIToolStripMenuItem.Checked = ActivePage == Page.CNI;
            cOM1ToolStripMenuItem.Checked = ActivePage == Page.COM_1;
            cOM2ToolStripMenuItem.Checked = ActivePage == Page.COM_2;
            iFFToolStripMenuItem.Checked = ActivePage == Page.IFF;
            nAV1ToolStripMenuItem.Checked = ActivePage == Page.NAV_1;
            nAV2ToolStripMenuItem.Checked = ActivePage == Page.NAV_2;
            gPSToolStripMenuItem.Checked = ActivePage == Page.GPS;
            fLPNToolStripMenuItem.Checked = ActivePage == Page.FLPN;
            aPToolStripMenuItem.Checked = ActivePage == Page.AP;
            sWCHToolStripMenuItem.Checked = ActivePage == Page.SWCH;
            lGHTToolStripMenuItem.Checked = ActivePage == Page.LGHT;
            wXToolStripMenuItem.Checked = ActivePage == Page.WX;
            tIMEToolStripMenuItem.Checked = ActivePage == Page.TIME;
            iNFOToolStripMenuItem.Checked = ActivePage == Page.INFO;
            HROTToolStripMenuItem.Checked = ActivePage == Page.HROT;
            HDISToolStripMenuItem.Checked = ActivePage == Page.HDIS;
            HCTLToolStripMenuItem.Checked = ActivePage == Page.HCTL;
            aviationUSICAOToolStripMenuItem.Checked = Properties.Settings.Default.UnitSystem == 0;
            metricSIToolStripMenuItem.Checked = Properties.Settings.Default.UnitSystem != 0;
            foreach (ToolStripItem item in customToolStripMenuItem.DropDownItems)
            {
                if (item is ToolStripMenuItem tsm && tsm.Tag is Guid)
                {
                    tsm.Checked = false;
                }
            }
            if (ActivePage == Page.Custom)
            {
                Guid id = Properties.Settings.Default.CustomDisplayId;
                if (id != Guid.Empty)
                {
                    foreach (ToolStripItem item in customToolStripMenuItem.DropDownItems)
                    {
                        if (item is ToolStripMenuItem tsm && tsm.Tag is Guid tagId && tagId == id)
                        {
                            tsm.Checked = true;
                            break;
                        }
                    }
                }
            }
        }

        private void cNIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetPage(Page.CNI);
        }

        private void cOM1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetPage(Page.COM_1);
        }

        private void cOM2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetPage(Page.COM_2);
        }

        private void iFFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetPage(Page.IFF);
        }

        private void nAV1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetPage(Page.NAV_1);
        }

        private void nAV2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetPage(Page.NAV_2);
        }

        private void gPSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetPage(Page.GPS);
        }

        private void fLPNToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetPage(Page.FLPN);
        }

        private void aPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetPage(Page.AP);
        }

        private void sWCHToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetPage(Page.SWCH);
        }

        private void lGHTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetPage(Page.LGHT);
        }

        private void tIMEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetPage(Page.TIME);
        }

        private void wXToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetPage(Page.WX);
        }

        private void iNFOToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetPage(Page.INFO);
        }

        private void HROTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetPage(Page.HROT);
        }

        private void HDISToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetPage(Page.HDIS);
        }

        private void HCTLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetPage(Page.HCTL);
        }

        private void SetPage(Page page)
        {
            ActivePage = page;
            UpdateChecked();
            UpdateScreen();
        }

        private void customDisplayManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CustomDisplayManagerForm form = new CustomDisplayManagerForm(UserIcpDisplays);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                if (form.IsDirty)
                {
                    UserIcpDisplays = form.UserIcpDisplays;
                    UserIcpDisplayStore.Save(UserIcpDisplays);
                    RebuildCustomDisplaysMenu();
                    if (Properties.Settings.Default.CustomDisplayId != Guid.Empty)
                    {
                        if (UserIcpDisplays.FirstOrDefault(i => i.Id == Properties.Settings.Default.CustomDisplayId) == null)
                        {
                            Properties.Settings.Default.CustomDisplayId = Guid.Empty;
                            if (ActivePage == Page.Custom)
                            {
                                ActivePage = Page.CNI;
                            }
                        }
                    }
                    UpdateChecked();
                    UpdateScreen();
                }
            }
        }

        private void RebuildCustomDisplaysMenu()
        {
            // Capture the manager item so we can keep it at the bottom
            var managerItem = customDisplayManagerToolStripMenuItem;
            // Remove all items except the manager item
            for (int i = customToolStripMenuItem.DropDownItems.Count - 1; i >= 0; i--)
            {
                var item = customToolStripMenuItem.DropDownItems[i];
                if (!ReferenceEquals(item, managerItem))
                {
                    customToolStripMenuItem.DropDownItems.RemoveAt(i);
                }
            }
            // Ensure manager item is still present (defensive)
            if (!customToolStripMenuItem.DropDownItems.Contains(managerItem))
            {
                customToolStripMenuItem.DropDownItems.Add(managerItem);
            }
            // Insert dynamic display items ABOVE the manager item
            int insertIndex = customToolStripMenuItem.DropDownItems.IndexOf(managerItem);
            var displays = UserIcpDisplays.OrderBy(d => d.DisplayName).ToList();
            foreach (var display in displays)
            {
                var mi = new ToolStripMenuItem(display.DisplayName)
                {
                    Tag = display.Id // store guid here
                };
                mi.Click += CustomDisplayMenuItem_Click;
                customToolStripMenuItem.DropDownItems.Insert(insertIndex, mi);
                insertIndex++; // keep stacking above manager item
            }
            // Insert separator ONLY if we added at least one display
            if (displays.Count > 0)
            {
                customToolStripMenuItem.DropDownItems.Insert(insertIndex, new ToolStripSeparator());
            }
        }

        private void CustomDisplayMenuItem_Click(object? sender, EventArgs e)
        {
            if (sender is not ToolStripMenuItem mi)
            {
                return;
            }
            if (mi.Tag is not Guid id)
            {
                return;
            }
            ActivePage = Page.Custom;
            Properties.Settings.Default.CustomDisplayId = id;
            Properties.Settings.Default.Save();
            UpdateChecked();
            UpdateScreen();
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public JoystickBinding? KollsmanBinding
        {
            get
            {
                return (!string.IsNullOrEmpty(Properties.Settings.Default.KollsmanBinding) ? JsonConvert.DeserializeObject<JoystickBinding>(Properties.Settings.Default.KollsmanBinding) : null);
            }
            set
            {
                if (value == null)
                {
                    Properties.Settings.Default.KollsmanBinding = null;
                }
                else
                {
                    Properties.Settings.Default.KollsmanBinding = JsonConvert.SerializeObject(value);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public JoystickBinding? BUGBinding
        {
            get
            {
                return (!string.IsNullOrEmpty(Properties.Settings.Default.BUGBinding) ? JsonConvert.DeserializeObject<JoystickBinding>(Properties.Settings.Default.BUGBinding) : null);
            }
            set
            {
                if (value == null)
                {
                    Properties.Settings.Default.BUGBinding = null;
                }
                else
                {
                    Properties.Settings.Default.BUGBinding = JsonConvert.SerializeObject(value);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public JoystickBinding? CNIBinding
        {
            get
            {
                return (!string.IsNullOrEmpty(Properties.Settings.Default.CNIBinding) ? JsonConvert.DeserializeObject<JoystickBinding>(Properties.Settings.Default.CNIBinding) : null);
            }
            set
            {
                if (value == null)
                {
                    Properties.Settings.Default.CNIBinding = null;
                }
                else
                {
                    Properties.Settings.Default.CNIBinding = JsonConvert.SerializeObject(value);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public JoystickBinding? COM1Binding
        {
            get
            {
                return (!string.IsNullOrEmpty(Properties.Settings.Default.COM1Binding) ? JsonConvert.DeserializeObject<JoystickBinding>(Properties.Settings.Default.COM1Binding) : null);
            }
            set
            {
                if (value == null)
                {
                    Properties.Settings.Default.COM1Binding = null;
                }
                else
                {
                    Properties.Settings.Default.COM1Binding = JsonConvert.SerializeObject(value);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public JoystickBinding? COM2Binding
        {
            get
            {
                return (!string.IsNullOrEmpty(Properties.Settings.Default.COM2Binding) ? JsonConvert.DeserializeObject<JoystickBinding>(Properties.Settings.Default.COM2Binding) : null);
            }
            set
            {
                if (value == null)
                {
                    Properties.Settings.Default.COM2Binding = null;
                }
                else
                {
                    Properties.Settings.Default.COM2Binding = JsonConvert.SerializeObject(value);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public JoystickBinding? IFFBinding
        {
            get
            {
                return (!string.IsNullOrEmpty(Properties.Settings.Default.IFFBinding) ? JsonConvert.DeserializeObject<JoystickBinding>(Properties.Settings.Default.IFFBinding) : null);
            }
            set
            {
                if (value == null)
                {
                    Properties.Settings.Default.IFFBinding = null;
                }
                else
                {
                    Properties.Settings.Default.IFFBinding = JsonConvert.SerializeObject(value);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public JoystickBinding? NAV1Binding
        {
            get
            {
                return (!string.IsNullOrEmpty(Properties.Settings.Default.NAV1Binding) ? JsonConvert.DeserializeObject<JoystickBinding>(Properties.Settings.Default.NAV1Binding) : null);
            }
            set
            {
                if (value == null)
                {
                    Properties.Settings.Default.NAV1Binding = null;
                }
                else
                {
                    Properties.Settings.Default.NAV1Binding = JsonConvert.SerializeObject(value);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public JoystickBinding? NAV2Binding
        {
            get
            {
                return (!string.IsNullOrEmpty(Properties.Settings.Default.NAV2Binding) ? JsonConvert.DeserializeObject<JoystickBinding>(Properties.Settings.Default.NAV2Binding) : null);
            }
            set
            {
                if (value == null)
                {
                    Properties.Settings.Default.NAV2Binding = null;
                }
                else
                {
                    Properties.Settings.Default.NAV2Binding = JsonConvert.SerializeObject(value);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public JoystickBinding? GPSBinding
        {
            get
            {
                return (!string.IsNullOrEmpty(Properties.Settings.Default.GPSBinding) ? JsonConvert.DeserializeObject<JoystickBinding>(Properties.Settings.Default.GPSBinding) : null);
            }
            set
            {
                if (value == null)
                {
                    Properties.Settings.Default.GPSBinding = null;
                }
                else
                {
                    Properties.Settings.Default.GPSBinding = JsonConvert.SerializeObject(value);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public JoystickBinding? FLPNBinding
        {
            get
            {
                return (!string.IsNullOrEmpty(Properties.Settings.Default.FLPNBinding) ? JsonConvert.DeserializeObject<JoystickBinding>(Properties.Settings.Default.FLPNBinding) : null);
            }
            set
            {
                if (value == null)
                {
                    Properties.Settings.Default.FLPNBinding = null;
                }
                else
                {
                    Properties.Settings.Default.FLPNBinding = JsonConvert.SerializeObject(value);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public JoystickBinding? APBinding
        {
            get
            {
                return (!string.IsNullOrEmpty(Properties.Settings.Default.APBinding) ? JsonConvert.DeserializeObject<JoystickBinding>(Properties.Settings.Default.APBinding) : null);
            }
            set
            {
                if (value == null)
                {
                    Properties.Settings.Default.APBinding = null;
                }
                else
                {
                    Properties.Settings.Default.APBinding = JsonConvert.SerializeObject(value);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public JoystickBinding? SWCHBinding
        {
            get
            {
                return (!string.IsNullOrEmpty(Properties.Settings.Default.SWCHBinding) ? JsonConvert.DeserializeObject<JoystickBinding>(Properties.Settings.Default.SWCHBinding) : null);
            }
            set
            {
                if (value == null)
                {
                    Properties.Settings.Default.SWCHBinding = null;
                }
                else
                {
                    Properties.Settings.Default.SWCHBinding = JsonConvert.SerializeObject(value);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public JoystickBinding? LGHTBinding
        {
            get
            {
                return (!string.IsNullOrEmpty(Properties.Settings.Default.LGHTBinding) ? JsonConvert.DeserializeObject<JoystickBinding>(Properties.Settings.Default.LGHTBinding) : null);
            }
            set
            {
                if (value == null)
                {
                    Properties.Settings.Default.LGHTBinding = null;
                }
                else
                {
                    Properties.Settings.Default.LGHTBinding = JsonConvert.SerializeObject(value);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public JoystickBinding? WXBinding
        {
            get
            {
                return (!string.IsNullOrEmpty(Properties.Settings.Default.WXBinding) ? JsonConvert.DeserializeObject<JoystickBinding>(Properties.Settings.Default.WXBinding) : null);
            }
            set
            {
                if (value == null)
                {
                    Properties.Settings.Default.WXBinding = null;
                }
                else
                {
                    Properties.Settings.Default.WXBinding = JsonConvert.SerializeObject(value);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public JoystickBinding? TIMEBinding
        {
            get
            {
                return (!string.IsNullOrEmpty(Properties.Settings.Default.TIMEBinding) ? JsonConvert.DeserializeObject<JoystickBinding>(Properties.Settings.Default.TIMEBinding) : null);
            }
            set
            {
                if (value == null)
                {
                    Properties.Settings.Default.TIMEBinding = null;
                }
                else
                {
                    Properties.Settings.Default.TIMEBinding = JsonConvert.SerializeObject(value);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public JoystickBinding? INFOBinding
        {
            get
            {
                return (!string.IsNullOrEmpty(Properties.Settings.Default.INFOBinding) ? JsonConvert.DeserializeObject<JoystickBinding>(Properties.Settings.Default.INFOBinding) : null);
            }
            set
            {
                if (value == null)
                {
                    Properties.Settings.Default.INFOBinding = null;
                }
                else
                {
                    Properties.Settings.Default.INFOBinding = JsonConvert.SerializeObject(value);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public JoystickBinding? HROTBinding
        {
            get
            {
                return (!string.IsNullOrEmpty(Properties.Settings.Default.HROTBinding) ? JsonConvert.DeserializeObject<JoystickBinding>(Properties.Settings.Default.HROTBinding) : null);
            }
            set
            {
                if (value == null)
                {
                    Properties.Settings.Default.HROTBinding = null;
                }
                else
                {
                    Properties.Settings.Default.HROTBinding = JsonConvert.SerializeObject(value);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public JoystickBinding? HDISBinding
        {
            get
            {
                return (!string.IsNullOrEmpty(Properties.Settings.Default.HDISBinding) ? JsonConvert.DeserializeObject<JoystickBinding>(Properties.Settings.Default.HDISBinding) : null);
            }
            set
            {
                if (value == null)
                {
                    Properties.Settings.Default.HDISBinding = null;
                }
                else
                {
                    Properties.Settings.Default.HDISBinding = JsonConvert.SerializeObject(value);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public JoystickBinding? HCTLBinding
        {
            get
            {
                return (!string.IsNullOrEmpty(Properties.Settings.Default.HCTLBinding) ? JsonConvert.DeserializeObject<JoystickBinding>(Properties.Settings.Default.HCTLBinding) : null);
            }
            set
            {
                if (value == null)
                {
                    Properties.Settings.Default.HCTLBinding = null;
                }
                else
                {
                    Properties.Settings.Default.HCTLBinding = JsonConvert.SerializeObject(value);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public JoystickBinding? CycleSystemUpBinding
        {
            get
            {
                return (!string.IsNullOrEmpty(Properties.Settings.Default.CycleSystemUpBinding) ? JsonConvert.DeserializeObject<JoystickBinding>(Properties.Settings.Default.CycleSystemUpBinding) : null);
            }
            set
            {
                if (value == null)
                {
                    Properties.Settings.Default.CycleSystemUpBinding = null;
                }
                else
                {
                    Properties.Settings.Default.CycleSystemUpBinding = JsonConvert.SerializeObject(value);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public JoystickBinding? CycleSystemDownBinding
        {
            get
            {
                return (!string.IsNullOrEmpty(Properties.Settings.Default.CycleSystemDownBinding) ? JsonConvert.DeserializeObject<JoystickBinding>(Properties.Settings.Default.CycleSystemDownBinding) : null);
            }
            set
            {
                if (value == null)
                {
                    Properties.Settings.Default.CycleSystemDownBinding = null;
                }
                else
                {
                    Properties.Settings.Default.CycleSystemDownBinding = JsonConvert.SerializeObject(value);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public JoystickBinding? CycleCustomUpBinding
        {
            get
            {
                return (!string.IsNullOrEmpty(Properties.Settings.Default.CycleCustomUpBinding) ? JsonConvert.DeserializeObject<JoystickBinding>(Properties.Settings.Default.CycleCustomUpBinding) : null);
            }
            set
            {
                if (value == null)
                {
                    Properties.Settings.Default.CycleCustomUpBinding = null;
                }
                else
                {
                    Properties.Settings.Default.CycleCustomUpBinding = JsonConvert.SerializeObject(value);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public JoystickBinding? CycleCustomDownBinding
        {
            get
            {
                return (!string.IsNullOrEmpty(Properties.Settings.Default.CycleCustomDownBinding) ? JsonConvert.DeserializeObject<JoystickBinding>(Properties.Settings.Default.CycleCustomDownBinding) : null);
            }
            set
            {
                if (value == null)
                {
                    Properties.Settings.Default.CycleCustomDownBinding = null;
                }
                else
                {
                    Properties.Settings.Default.CycleCustomDownBinding = JsonConvert.SerializeObject(value);
                }
            }
        }

        private bool IsJoystickButton(JoystickBinding binding, JoystickEventArgs e)
        {
            if (binding != null && (binding.DeviceType == BindingDeviceType.JoystickButton || binding.DeviceType == BindingDeviceType.JoystickPov) && binding.ButtonOrKey == e.ButtonOrAxis && binding.JoystickGuid == e.Joystick.Information.InstanceGuid && binding.Direction == e.Direction)
            {
                return true;
            }
            return false;
        }

        private static readonly Page[] CycleOrder =
        {
            Page.CNI,
            Page.COM_1,
            Page.COM_2,
            Page.IFF,
            Page.NAV_1,
            Page.NAV_2,
            Page.GPS,
            Page.FLPN,
            Page.AP,
            Page.SWCH,
            Page.LGHT,
            Page.WX,
            Page.TIME,
            Page.INFO,
            Page.HROT,
            Page.HDIS,
            Page.HCTL,
            Page.Custom
        };

        private void CyclePage(int direction) // +1 = up, -1 = down
        {
            int idx = Array.IndexOf(CycleOrder, ActivePage);
            if (idx < 0) idx = 0;

            int Step(int i, int dir)
            {
                int n = (i + dir) % CycleOrder.Length;
                if (n < 0) n += CycleOrder.Length;
                return n;
            }
            int nextIdx = Step(idx, direction);
            // Rule:
            // - If we're currently on Custom: move normally (so Custom can go to its neighbors).
            // - If we're NOT on Custom and the next target would be Custom: skip it.
            if (ActivePage != Page.Custom && CycleOrder[nextIdx] == Page.Custom)
            {
                nextIdx = Step(nextIdx, direction);
            }
            ActivePage = CycleOrder[nextIdx];
            UpdateScreen();
        }

        private void CycleCustomDisplay(int direction) // +1 = up, -1 = down
        {
            // No customs? Do nothing.
            if (UserIcpDisplays == null || UserIcpDisplays.Count == 0)
                return;

            // Keep ordering consistent with RebuildCustomDisplaysMenu()
            var displays = UserIcpDisplays
                .OrderBy(d => d.DisplayName)
                .ToList();

            Guid currentId = Properties.Settings.Default.CustomDisplayId;

            int idx = displays.FindIndex(d => d.Id == currentId);

            // If currentId isn't valid, pick a sensible starting point:
            // - Up: go to first item
            // - Down: go to last item
            if (idx < 0)
                idx = (direction >= 0) ? -1 : 0;

            int next = idx + direction;

            if (next >= displays.Count) next = 0;
            if (next < 0) next = displays.Count - 1;

            ActivePage = Page.Custom;
            Properties.Settings.Default.CustomDisplayId = displays[next].Id;
            Properties.Settings.Default.Save();

            UpdateChecked();   // keep menu state correct
            UpdateScreen();
        }

        private void Joysticks_JoystickEvent(object sender, JoystickEventArgs e)
        {
            Ui(() =>
            {
                if (CaptureJoystickEvents)
                {
                    if (e.EventType == JoystickEventType.ButtonDown || e.EventType == JoystickEventType.PovDown || e.EventType == JoystickEventType.PovLeft || e.EventType == JoystickEventType.PovRight || e.EventType == JoystickEventType.PovUp)
                    {
                        foreach (var userIcpDisplay in UserIcpDisplays)
                        {
                            if (userIcpDisplay.Binding != null)
                            {
                                if (IsJoystickButton(userIcpDisplay.Binding, e))
                                {
                                    ActivePage = Page.Custom;
                                    Properties.Settings.Default.CustomDisplayId = userIcpDisplay.Id;
                                    Properties.Settings.Default.Save();
                                    UpdateScreen();
                                    break;
                                }
                            }
                        }
                        if (CNIBinding != null && IsJoystickButton(CNIBinding, e))
                        {
                            SetPage(Page.CNI);
                        }
                        else if (COM1Binding != null && IsJoystickButton(COM1Binding, e))
                        {
                            SetPage(Page.COM_1);
                        }
                        else if (COM2Binding != null && IsJoystickButton(COM2Binding, e))
                        {
                            SetPage(Page.COM_2);
                        }
                        else if (IFFBinding != null && IsJoystickButton(IFFBinding, e))
                        {
                            SetPage(Page.IFF);
                        }
                        else if (NAV1Binding != null && IsJoystickButton(NAV1Binding, e))
                        {
                            SetPage(Page.NAV_1);
                        }
                        else if (NAV2Binding != null && IsJoystickButton(NAV2Binding, e))
                        {
                            SetPage(Page.NAV_2);
                        }
                        else if (GPSBinding != null && IsJoystickButton(GPSBinding, e))
                        {
                            SetPage(Page.GPS);
                        }
                        else if (FLPNBinding != null && IsJoystickButton(FLPNBinding, e))
                        {
                            SetPage(Page.FLPN);
                        }
                        else if (APBinding != null && IsJoystickButton(APBinding, e))
                        {
                            SetPage(Page.AP);
                        }
                        else if (SWCHBinding != null && IsJoystickButton(SWCHBinding, e))
                        {
                            SetPage(Page.SWCH);
                        }
                        else if (LGHTBinding != null && IsJoystickButton(LGHTBinding, e))
                        {
                            SetPage(Page.LGHT);
                        }
                        else if (TIMEBinding != null && IsJoystickButton(TIMEBinding, e))
                        {
                            SetPage(Page.TIME);
                        }
                        else if (WXBinding != null && IsJoystickButton(WXBinding, e))
                        {
                            SetPage(Page.WX);
                            UpdateScreen();
                        }
                        else if (INFOBinding != null && IsJoystickButton(INFOBinding, e))
                        {
                            SetPage(Page.INFO);
                        }
                        else if (HROTBinding != null && IsJoystickButton(HROTBinding, e))
                        {
                            SetPage(Page.HROT);
                        }
                        else if (HDISBinding != null && IsJoystickButton(HDISBinding, e))
                        {
                            SetPage(Page.HDIS);
                        }
                        else if (HCTLBinding != null && IsJoystickButton(HCTLBinding, e))
                        {
                            SetPage(Page.HCTL);
                        }
                        else if (CycleSystemUpBinding != null && IsJoystickButton(CycleSystemUpBinding, e))
                        {
                            CyclePage(1);
                        }
                        else if (CycleSystemDownBinding != null && IsJoystickButton(CycleSystemDownBinding, e))
                        {
                            CyclePage(-1);
                        }
                        else if (CycleCustomUpBinding != null && IsJoystickButton(CycleCustomUpBinding, e))
                        {
                            CycleCustomDisplay(1);
                        }
                        else if (CycleCustomDownBinding != null && IsJoystickButton(CycleCustomDownBinding, e))
                        {
                            CycleCustomDisplay(-1);
                        }
                        else if (BUGBinding != null && IsJoystickButton(BUGBinding, e))
                        {
                            SyncHeadingBug();
                        }
                        else if (KollsmanBinding != null && IsJoystickButton(KollsmanBinding, e))
                        {
                            SyncKollsman();
                        }
                    }
                }
            });
        }

        private void SyncHeadingBug()
        {
            try
            {
                if (FlightSimProviders.FSUIPC.IsConnected)
                {
                    FlightSimProviders.FSUIPC.SendControlToFS(FSUIPC.FsControl.HEADING_BUG_SET.ToString(), Tools.Pack(FlightSimProviders.FSUIPC.HeadingMagneticDegrees, new Tools.PackSpec(Scale: 1.0)));
                }
                else
                {
                    SimConnect.Instance.SetValue(SimConnectEventId.HeadingBugSet, Tools.Pack(FlightSimProviders.SimConnect.HeadingMagneticDegrees, new Tools.PackSpec(Scale: 1.0)));
                }
                FlightSimProviders.XPlane.SendControlToFS(FlightSim.XPlaneConnect.DataRefId.CockpitAutopilotHeadingMag.ToString(), (float)Math.Round(FlightSimProviders.XPlane.HeadingMagneticDegrees));
            }
            catch (Exception)
            {
            }
        }

        private void SyncKollsman()
        {
            try
            {
                const float StdPressureInHg = 29.92f;
                const float StdPressureHpa = 1013.25f;
                const int TransitionAltitudeFt = 18000;
                if (FlightSimProviders.FSUIPC.IsConnected)
                {
                    FlightSimProviders.FSUIPC.SendControlToFS(FSUIPC.FsControl.KOHLSMAN_SET.ToString(), Tools.Pack(FlightSimProviders.FSUIPC.AltitudeMSL < TransitionAltitudeFt ? FlightSimProviders.FSUIPC.PressureHectoPascals : StdPressureHpa, new Tools.PackSpec(Scale: 16.0)));
                }
                else
                {
                    SimConnect.Instance.SetValue(SimConnectEventId.KollsmanSet, Tools.Pack(FlightSimProviders.SimConnect.AltitudeMSL < TransitionAltitudeFt ? FlightSimProviders.SimConnect.PressureHectoPascals : StdPressureHpa, new Tools.PackSpec(Scale: 16.0)));
                }
                SimConnect.Instance.SetValue(SimConnectEventId.SecondaryKollsmanSet, Tools.Pack(FlightSimProviders.SimConnect.AltitudeMSL < TransitionAltitudeFt ? FlightSimProviders.SimConnect.PressureHectoPascals : StdPressureHpa, new Tools.PackSpec(Scale: 16.0)));
                FlightSimProviders.XPlane.SendControlToFS(FlightSim.XPlaneConnect.DataRefId.CockpitMiscBarometerSetting.ToString(), FlightSimProviders.XPlane.AltitudeMSL < TransitionAltitudeFt ? (float)FlightSimProviders.XPlane.PressureInchesMercury : StdPressureInHg);
                FlightSimProviders.XPlane.SendControlToFS(FlightSim.XPlaneConnect.DataRefId.CockpitMiscBarometerSetting2.ToString(), FlightSimProviders.XPlane.AltitudeMSL < TransitionAltitudeFt ? (float)FlightSimProviders.XPlane.PressureInchesMercury : StdPressureInHg);
            }
            catch (Exception)
            {
            }
        }

        private void joystickBindingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BindingsForm form = new BindingsForm(UserIcpDisplays);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                Properties.Settings.Default.Save();
            }
        }

        private bool _simConnectInitQueued;

        private void F16DEDWriterForm_Shown(object sender, EventArgs e)
        {
            RebuildCustomDisplaysMenu();
            UpdateChecked();
            HideWindow();

            if (!_simConnectInitQueued)
            {
                _simConnectInitQueued = true;
                // Delay until the message loop has processed show/minimize.
                BeginInvoke(new Action(() =>
                {
                    if (!IsHandleCreated) return;
                    FlightSimProviders.SimConnect.MainWindowHandle = Handle;
                }));
            }

#if DEBUG
            UpdateScreen(FlightSimProviders.Preview);
#endif
        }

        private void F16DEDWriterForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (bridge != null)
            {
                bridge.ShutdownAsync().GetAwaiter().GetResult();
            }
            FlightSimProviders.XPlane.Deinitialize();
            FlightSimProviders.SimConnect.Deinitialize();
            FlightSimProviders.FSUIPC.Deinitialize();
            // Event though we are not using them:
            FlightSimProviders.DCS?.Deinitialize();
            FlightSimProviders.FalcomBMS.Deinitialize();
        }

        private void startWithMSFSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            startWithMSFSToolStripMenuItem.Checked = !startWithMSFSToolStripMenuItem.Checked;
            if (startWithMSFSToolStripMenuItem.Checked)
            {
                if (Tools.IsPluginInstalled(MSFSEdition.Microsoft2020, MSFSKEY) == PluginInstalled.PluginNotInstalled
                    || Tools.IsPluginInstalled(MSFSEdition.Steam2020, MSFSKEY) == PluginInstalled.PluginNotInstalled
                    || Tools.IsPluginInstalled(MSFSEdition.Microsoft2024, MSFSKEY) == PluginInstalled.PluginNotInstalled
                    || Tools.IsPluginInstalled(MSFSEdition.Steam2024, MSFSKEY) == PluginInstalled.PluginNotInstalled)
                {
                    Tools.InstallToMSFS(MSFSKEY);
                }
            }
            else
            {
                if (Tools.IsPluginInstalled(MSFSEdition.Microsoft2020, MSFSKEY) == PluginInstalled.PluginInstalled
                    || Tools.IsPluginInstalled(MSFSEdition.Steam2020, MSFSKEY) == PluginInstalled.PluginInstalled
                    || Tools.IsPluginInstalled(MSFSEdition.Microsoft2024, MSFSKEY) == PluginInstalled.PluginInstalled
                    || Tools.IsPluginInstalled(MSFSEdition.Steam2024, MSFSKEY) == PluginInstalled.PluginInstalled)
                {
                    Tools.UninstallFromMSFS(MSFSKEY);
                }
            }
        }

        private void aviationUSICAOToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.UnitSystem = 0;
            Properties.Settings.Default.Save();
            FlightSimProviderBase.Units = UnitSystem.Aviation;
            UpdateChecked();
            UpdateScreen();
        }

        private void metricSIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.UnitSystem = 1;
            Properties.Settings.Default.Save();
            FlightSimProviderBase.Units = UnitSystem.Metric;
            UpdateChecked();
            UpdateScreen();
        }
    }
}
