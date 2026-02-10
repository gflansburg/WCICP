using Microsoft.FlightSimulator.SimConnect;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FlightSim
{
    public class SimConnect
    {
        public static readonly SimConnect Instance;

        private const string Provider = "SimConnect";

        static SimConnect()
        {
            Instance = new SimConnect();
        }

        SimConnect()
        {
            Events = new Dictionary<SimConnectEventId, SimConnectEvent>();
        }

        enum SYSTEM_EVENT : uint
        {
            SIM,
            CRASHED,
            PAUSE,
            FLIGHTLOADED,
            FLIGHTPLANACTIVATED,
            FLIGHTPLANDEACTIVATED
        }

        enum DATA_DEFINE_ID : uint
        {
            FLIGHTDATA,
            AIRCRAFT
        }

        enum DATA_REQUEST_ID : uint
        {
            NONSUBSCRIBE_REQ,
            SUBSCRIBE_REQ,
            AIRPORTS_REQ = 1001,
            WAYPOINTS_REQ = 1002,
            VORS_REQ = 1003,
            NDBS_REQ = 1004,
            TRAFFIC_REQ = 10000
        }

        enum SIMCONNECT_GROUP_PRIORITY : uint
        {
            HIGHEST = 1,
            HIGHEST_MASKABLE = 10000000,
            STANDARD = 1900000000,
            DEFAULT = 2000000000,
            LOWEST = 4000000000
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct FLIGHT_DATA
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string TITLE;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string ATC_AIRLINE;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string ATC_FLIGHT_NUMBER;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string ATC_MODEL;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string ATC_TYPE;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string ATC_IDENTIFIER;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string CATEGORY;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string GPS_WP_IDENT;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string COM1_ACTIVE_FREQ_TYPE;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string COM2_ACTIVE_FREQ_TYPE;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string COM1_ACTIVE_FREQ_IDENT;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string COM2_ACTIVE_FREQ_IDENT;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string NAV1_IDENT;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string NAV2_IDENT;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string ADF_IDENT;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string ADF_NAME;
            public double PLANE_LATITUDE;
            public double PLANE_LONGITUDE;
            public double PLANE_ALTITUDE;
            public double PRESSURE_ALTITUDE;
            public double ALTITUDE_AGL;
            public double AUTOPILOT_HEADING_LOCK_DIR;
            public double PLANE_HEADING_DEGREES_MAGNETIC;
            public double PLANE_HEADING_DEGREES_TRUE;
            public double PLANE_PITCH_DEGREES;
            public double PLANE_BANK_DEGREES;
            public double VERTICAL_SPEED;
            public double AIRSPEED_INDICATED;
            public double AIRSPEED_TRUE;
            public double GROUND_ALTITUDE;
            public double GROUND_VELOCITY;
            public double KOLLSMAN_SETTING_HG;
            public double SEC_KOLLSMAN_SETTING_HG;
            public double PRESSURE_IN_HG;
            public double AMBIENT_WIND_VELOCITY;
            public double AMBIENT_WIND_DIRECTION;
            public double AMBIENT_TEMPERATURE;
            public double ADF_RADIAL;
            public double NAV_RELATIVE_BEARING_TO_STATION_1;
            public double NAV_RELATIVE_BEARING_TO_STATION_2;
            public double NAV_DME_1;
            public double NAV_DME_2;
            public double NAV_DME_SPD_1;
            public double NAV_DME_SPD_2;
            public int GPS_IS_ACTIVE_WAY_POINT;
            public int GPS_DRIVES_NAV;
            public double GPS_WP_TRUE_REQ_HDG;
            public double GPS_WP_BEARING;
            public double GPS_WP_CROSS_TRK;
            public double GPS_WP_LAT;
            public double GPS_WP_LON;
            public double FUEL_CAPACITY_GAL;
            public double FUEL_LEFT_CAPACITY_GAL;
            public double FUEL_RIGHT_CAPACITY_GAL;
            public double FUEL_CENTER_CAPACITY_GAL;
            public double FUEL_TOTAL_REMAINING_GAL;
            public double FUEL_LEFT_REMAINING_GAL;
            public double FUEL_RIGHT_REMAINING_GAL;
            public double FUEL_CENTER_REMAINING_GAL;
            public double NAV1_FREQUENCY;
            public double NAV2_FREQUENCY;
            public double NAV1_STANDBY_FREQUENCY;
            public double NAV2_STANDBY_FREQUENCY;
            public double COM1_FREQUENCY;
            public double COM2_FREQUENCY;
            public double COM1_STANDBY_FREQUENCY;
            public double COM2_STANDBY_FREQUENCY;
            public int COM1_TRANSMIT;
            public int COM2_TRANSMIT;
            public int COM1_STATUS;
            public int COM2_STATUS;
            public int COM1_RECEIVE;
            public int COM2_RECEIVE;
            public int XPDR_CODE;
            public int XPDR_STATE;
            public int IDENT_ACTIVE;
            public double BATTERY_LOAD;
            public int BATTERY_MASTER;
            public int AVIONICS_MASTER;
            public int GENERATOR_MASTER;
            public int PITOT_HEAT;
            public int NAV1_AVAILABLE;
            public int NAV2_AVAILABLE;
            public int NAV1_SOUND;
            public int NAV2_SOUND;
            public int SIM_ON_GROUND;
            public int ENGINE_TYPE;
            public int ATC_HEAVY;
            public int IS_GEAR_FLOATS;
            public int ENGINE_COUNT;
            public double THROTTLE1_PCT;
            public double THROTTLE2_PCT;
            public double THROTTLE3_PCT;
            public double THROTTLE4_PCT;
            public double MIXTURE1_PCT;
            public double MIXTURE2_PCT;
            public double MIXTURE3_PCT;
            public double MIXTURE4_PCT;
            public double PROP_PITCH1_PCT;
            public double PROP_PITCH2_PCT;
            public double PROP_PITCH3_PCT;
            public double PROP_PITCH4_PCT;
            public double PROP_PITCH1_RAD;
            public double PROP_PITCH2_RAD;
            public double PROP_PITCH3_RAD;
            public double PROP_PITCH4_RAD;
            public int ENG1_RUNNING;
            public int ENG2_RUNNING;
            public int ENG3_RUNNING;
            public int ENG4_RUNNING;
            public double ENG1_N1_PCT;
            public double ENG2_N1_PCT;
            public double ENG3_N1_PCT;
            public double ENG4_N1_PCT;
            public double ENG1_MANIFOLD_INHG;
            public double ENG2_MANIFOLD_INHG;
            public double ENG3_MANIFOLD_INHG;
            public double ENG4_MANIFOLD_INHG;
            public int AB1_ON;
            public int AB2_ON;
            public int AB3_ON;
            public int AB4_ON;
            public double ENG1_RPM;
            public double ENG2_RPM;
            public double ENG3_RPM;
            public double ENG4_RPM;
            public double ENG1_TORQUE_FTLBS;
            public double ENG2_TORQUE_FTLBS;
            public double ENG3_TORQUE_FTLBS;
            public double ENG4_TORQUE_FTLBS;
            public double FLAPS_PCT;
            public double GEAR_HANDLE_POSITION;
            public double GEAR_CENTER_POSITION;
            public double GEAR_LEFT_POSITION;
            public double GEAR_RIGHT_POSITION;
            public double SPOILERS_PCT;
            public int SPOILERS_ARMED;
            public int PARKING_BRAKE;
            public int LIGHT_NAV;
            public int LIGHT_BEACON;
            public int LIGHT_STROBE;
            public int LIGHT_LANDING;
            public int LIGHT_TAXI;
            public int LIGHT_CABIN;
            public int LIGHT_PANEL;
            public int AP_MASTER;
            public int FLIGHT_DIRECTOR_ACTIVE;
            public int AP_HDG_HOLD;
            public int AP_ALT_HOLD;
            public int AP_NAV_HOLD;
            public int AP_SPD_HOLD;
            public int AP_VS_HOLD;
            public double AP_ALT_TARGET_FT;
            public double AP_SPD_TARGET_KTS;
            public double AP_VS_TARGET_FM;
            public double NAV1_OBS;
            public double NAV2_OBS;
            public double ELEVATOR_TRIM_PCT;
            public double AILERON_TRIM_PCT;
            public double RUDDER_TRIM_PCT;
            public int WINDOW_DEFROST;
            public double COWL_FLAPS1_OPEN;
            public double COWL_FLAPS2_OPEN;
            public double COWL_FLAPS3_OPEN;
            public double COWL_FLAPS4_OPEN;
            public int ENG1_FAILED;
            public int ENG2_FAILED;
            public int ENG3_FAILED;
            public int ENG4_FAILED;
            public double ENG1_OIL_PRESSURE;
            public double ENG2_OIL_PRESSURE;
            public double ENG3_OIL_PRESSURE;
            public double ENG4_OIL_PRESSURE;
            public int ENG1_ANTIICE;
            public int ENG2_ANTIICE;
            public int ENG3_ANTIICE;
            public int ENG4_ANTIICE;
            public int PROP1_DEICE;
            public int PROP2_DEICE;
            public int PROP3_DEICE;
            public int PROP4_DEICE;
            public int FUELPUMP1;
            public int FUELPUMP2;
            public int FUELPUMP3;
            public int FUELPUMP4;
            // Helicoptor
            public double COLLECTIVE_POSITION_PCT;
            public double DISK_BANK_PCT_1;
            public double DISK_BANK_PCT_2;
            public double DISK_CONING_PCT_1;
            public double DISK_CONING_PCT_2;
            public double DISK_PITCH_PCT_1;
            public double DISK_PITCH_PCT_2;
            public int ROTOR_BRAKE_ACTIVE;
            public double ROTOR_BRAKE_HANDLE_POS;
            public double ROTOR_COLLECTIVE_BLADE_PITCH_PCT;
            public double ROTOR_LATERAL_TRIM_PCT;
            public double ROTOR_LONGITUDINAL_TRIM_PCT;
            public double ROTOR_ROTATION_ANGLE_1;
            public double ROTOR_ROTATION_ANGLE_2;
            public int ROTOR_GOV_ACTIVE_1;
            public int ROTOR_GOV_ACTIVE_2;
            public double ROTOR_RPM_1;
            public double ROTOR_RPM_2;
            public double TAIL_ROTOR_BLADE_PITCH_PCT;
            public double ENG_ROTOR_RPM_1;
            public double ENG_ROTOR_RPM_2;
            public double ENG_TORQUE_PERCENT_1;
            public double ENG_TORQUE_PERCENT_2;
            // Flightplan
            public int FLIGHTPLAN_IS_ACTIVE_FLIGHTPLAN;
            public int FLIGHTPLAN_WAYPOINTS;
            public int FLIGHTPLAN_ACTIVE_WAYPOINT;
            public int FLIGHTPLAN_DIRECT_TO;
            public int FLIGHTPLAN_WAYPOINT_INDEX;
            public int FLIGHTPLAN_APPROACH_WAYPOINTS;
            public int FLIGHTPLAN_ACTIVE_APPROACH_WAYPOINT;
            public int FLIGHTPLAN_APPROACH_IS_WAYPOINT_RUNWAY;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string FLIGHTPLAN_APPROACH_ID;
            // Balloons
            public int BALLOON_AUTO_FILL_ACTIVE;          // BALLOON AUTO FILL ACTIVE (bool)
            public double BALLOON_FILL_AMOUNT;            // BALLOON FILL AMOUNT (percent)
            public double BALLOON_GAS_DENSITY;            // BALLOON GAS DENSITY
            public double BALLOON_GAS_TEMPERATURE;        // BALLOON GAS TEMPERATURE
            public double BALLOON_VENT_OPEN_VALUE;        // BALLOON VENT OPEN VALUE (percent)
            // Burners (indexed in SimVar; typically use :0 or “index 0” binding)
            public double BURNER_FUEL_FLOW_RATE;          // BURNER FUEL FLOW RATE (lbs/hour)
            public int BURNER_PILOT_LIGHT_ON;             // BURNER PILOT LIGHT ON (bool)
            public double BURNER_VALVE_OPEN_VALUE;        // BURNER VALVE OPEN VALUE (percent)
            // Airships (these are indexed compartments/fans; bind to :0)
            public int AIRSHIP_COMPARTMENT_GAS_TYPE;      // AIRSHIP COMPARTMENT GAS TYPE (enum/int)
            public double AIRSHIP_COMPARTMENT_PRESSURE;   // AIRSHIP COMPARTMENT PRESSURE (mb)
            public double AIRSHIP_COMPARTMENT_OVERPRESSURE; // AIRSHIP COMPARTMENT OVERPRESSURE (mb)
            public double AIRSHIP_COMPARTMENT_TEMPERATURE;  // AIRSHIP COMPARTMENT TEMPERATURE (C)
            public double AIRSHIP_COMPARTMENT_VOLUME;     // AIRSHIP COMPARTMENT VOLUME (m^3)
            public double AIRSHIP_COMPARTMENT_WEIGHT;     // AIRSHIP COMPARTMENT WEIGHT (lb)
            public double AIRSHIP_FAN_POWER_PCT;          // AIRSHIP FAN POWER PCT (percent)
            // Mast truck
            public int MAST_TRUCK_DEPLOYMENT;             // MAST TRUCK DEPLOYMENT (bool)
            public double MAST_TRUCK_EXTENSION;           // MAST TRUCK EXTENSION (percent)
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct AIRCRAFT_DATA
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string TITLE;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string ATC_MODEL;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string ATC_TYPE;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string ATC_IDENTIFIER;
            public double PLANE_LATITUDE;
            public double PLANE_LONGITUDE;
            public double PLANE_ALTITUDE;
            public double PLANE_HEADING_DEGREES_TRUE;
            public double AIRSPEED_INDICATED;
            public int ENGINE_TYPE;
            public int SIM_ON_GROUND;
            public double GROUND_VELOCITY;
            public int ENG_COMBUSTION;
        }

        public const int WM_USER_SIMCONNECT = 0x0402;

        public double SearchRadiusNm { get; set; } = 20;
        private uint SearchRadiusMeters => (uint)(SearchRadiusNm * 1852.0);

        private Microsoft.FlightSimulator.SimConnect.SimConnect? MicrosoftSimConnect = null;
        public Dictionary<string, Aircraft> Traffic = new Dictionary<string, Aircraft>();
        public AircraftData CurrentAircraft = new AircraftData();
        public Weather CurrentWeather = new Weather();
        private Timer? trafficTimer = null;
        private readonly object _trafficLock = new();

        public bool IsConnected { get; private set; }
        public bool IsRunning { get; private set; }
        public bool Error { get; private set; }
        public FlightSimProviderException? ErrorMessage { get; private set; } = null!;

        public delegate void SimConnectFlightDataEventHandler(FLIGHT_DATA data);
        public event SimConnectFlightDataEventHandler? OnFlightDataReceived = null;

        public delegate void SimConnectFacilitiesEventHandler(FacilityType type);
        public event SimConnectFacilitiesEventHandler? OnFacilitiesReceived = null;

        public delegate void SimConnectConnectEventHandler();
        public event SimConnectConnectEventHandler? OnConnected = null;

        public delegate void SimConnectErrorEventHandler(Exception error);
        public event SimConnectErrorEventHandler? OnError = null;

        public delegate void SimConnectQuitEventHandler();
        public event SimConnectQuitEventHandler? OnQuit = null;

        public delegate void SimConnectEventHandler(bool isRunning);
        public event SimConnectEventHandler? OnSim = null;

        public delegate void SimConnectCrashedEventHandler();
        public event SimConnectCrashedEventHandler? OnCrashed = null;

        public delegate void SimConnectPausedEventHandler(bool paused);
        public event SimConnectPausedEventHandler? OnPaused = null;

        public delegate void SimConnectFlightLoadedEventHandler(string? filename);
        public event SimConnectFlightLoadedEventHandler? OnFlightLoaded = null;

        public delegate void SimConnectFlightPlanActivatedEventHandler(string? filename);
        public event SimConnectFlightPlanActivatedEventHandler? OnFlightPlanActivated = null;

        public delegate void SimConnectFlightPlanDeactivatedEventHandler();
        public event SimConnectFlightPlanDeactivatedEventHandler? OnFlightPlanDeactivated = null;

        public delegate void SimConnectTrafficEventHandler(uint objectId, Aircraft? aircraft, TrafficEvent eventType);
        public event SimConnectTrafficEventHandler? OnTrafficReceived = null;

        private bool _requestTrafficInfo = false;
        public bool RequestTrafficInfo
        {
            get
            {
                return _requestTrafficInfo;
            }
            set
            {
                if (_requestTrafficInfo != value)
                {
                    _requestTrafficInfo = value;
                    trafficTimer?.Dispose();
                    lock (_trafficLock)
                    {
                        Traffic.Clear();
                    }
                    if (_requestTrafficInfo)
                    {
                        trafficTimer = new Timer(RequestTrafficData, null, 60000, 60000);
                    }
                }
            }
        }

        public Dictionary<SimConnectEventId, SimConnectEvent> Events { get; private set; }
        
        public FacilityIndex Facilities { get; private set; } = new();
        private readonly object _facLock = new();

        public void Subscribe(SimConnectEventId eventId, Action<SimConnectEvent, uint>? onchange = null)
        {
            SimConnectEvent evt = SimConnectEvents.Instance.Events[eventId];

            if (!Events.ContainsKey(evt.Id))
            {
                Events.Add(evt.Id, evt);
                if (IsConnected)
                {
                    MicrosoftSimConnect?.MapClientEventToSimEvent(evt.Id, evt.Command);
                }
            }

            if (onchange != null)
            {
                SimConnectEvent.NotifyChangeHandler changeHandler = (e, v) => onchange(e, v);
                evt.Delegates.Add(changeHandler);
                evt.OnValueChange += changeHandler;
            }
        }

        public void Unsubscribe(SimConnectEventId eventId)
        {
            SimConnectEvent evt = SimConnectEvents.Instance.Events[eventId];
            if (Events.ContainsKey(evt.Id))
            {
                foreach (SimConnectEvent.NotifyChangeHandler changeHandler in evt.Delegates)
                {
                    evt.OnValueChange -= changeHandler;
                }
                evt.Delegates.Clear();
                Events.Remove(evt.Id);
            }
        }

        public void SetValue(SimConnectEventId eventId, uint rawValue)
        {
            if (MicrosoftSimConnect == null || !IsConnected) return;

            SimConnectEvent? evt = SimConnectEvents.Instance.Events[eventId];
            if (evt == null) return;
            if (!Events.ContainsKey(evt.Id))
            {
                Events.Add(evt.Id, evt);
                MicrosoftSimConnect.MapClientEventToSimEvent(evt.Id, evt.Command);
            }
            MicrosoftSimConnect.TransmitClientEvent(Microsoft.FlightSimulator.SimConnect.SimConnect.SIMCONNECT_OBJECT_ID_USER, evt.Id, rawValue, SIMCONNECT_GROUP_PRIORITY.HIGHEST, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
        }

        // Packed helpers (opt-in)
        public void SetValue(SimConnectEventId eventId, double value, Tools.PackSpec spec)
            => SetValue(eventId, Tools.Pack(value, spec));

        public void SetValue(SimConnectEventId eventId, float value, Tools.PackSpec spec)
            => SetValue(eventId, Tools.Pack(value, spec));

        public void SetValue(SimConnectEventId eventId, int value, Tools.PackSpec spec)
            => SetValue(eventId, Tools.Pack(value, spec));

        public void SetValue(SimConnectEventId eventId, bool value, Tools.PackSpec spec)
            => SetValue(eventId, Tools.Pack(value, spec));

        public void SendCommand(SimConnectEventId eventId)
        {
            if (MicrosoftSimConnect == null || !IsConnected) return;
            SimConnectEvent? evt = SimConnectEvents.Instance.Events[eventId];
            if (evt == null) return;
            if (!Events.ContainsKey(evt.Id))
            {
                Events.Add(evt.Id, evt);
                MicrosoftSimConnect.MapClientEventToSimEvent(evt.Id, evt.Command);
            }
            MicrosoftSimConnect.TransmitClientEvent(Microsoft.FlightSimulator.SimConnect.SimConnect.SIMCONNECT_OBJECT_ID_USER, evt.Id, 0, SIMCONNECT_GROUP_PRIORITY.HIGHEST, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
        }

        private int _facilityGen = 0;

        public void Initialize(IntPtr hWnd)
        {
            try
            {
                Interlocked.Increment(ref _facilityGen);
                if (MicrosoftSimConnect != null)
                {
                    MicrosoftSimConnect.Dispose();
                    MicrosoftSimConnect = null!;
                    IsConnected = false;
                    IsRunning = false;
                }
                string appName = Process.GetCurrentProcess().ProcessName;
                MicrosoftSimConnect = new Microsoft.FlightSimulator.SimConnect.SimConnect(appName, hWnd, WM_USER_SIMCONNECT, null, 0);
                IsConnected = true;
                MicrosoftSimConnect.OnRecvOpen += MicrosoftSimConnect_OnRecvOpen;
                MicrosoftSimConnect.OnRecvQuit += MicrosoftSimConnect_OnRecvQuit;
                MicrosoftSimConnect.OnRecvSimobjectData += MicrosoftSimConnect_OnRecvSimobjectData;
                MicrosoftSimConnect.OnRecvSimobjectDataBytype += MicrosoftSimConnect_OnRecvSimobjectDataBytype;
                MicrosoftSimConnect.OnRecvAirportList += MicrosoftSimConnect_OnRecvAirportList;
                MicrosoftSimConnect.OnRecvWaypointList += MicrosoftSimConnect_OnRecvWaypointList;
                MicrosoftSimConnect.OnRecvVorList += MicrosoftSimConnect_OnRecvVorList;
                MicrosoftSimConnect.OnRecvNdbList += MicrosoftSimConnect_OnRecvNdbList;
                MicrosoftSimConnect.OnRecvException += MicrosoftSimConnect_OnRecvException;
                MicrosoftSimConnect.OnRecvSystemState += MicrosoftSimConnect_OnRecvSystemState;
                MicrosoftSimConnect.OnRecvEvent += MicrosoftSimConnect_OnRecvEvent; IsConnected = true;
                lock (_facLock)
                {
                    Facilities.Clear(FacilityType.Airport);
                    Facilities.Clear(FacilityType.Waypoint);
                    Facilities.Clear(FacilityType.Vor);
                    Facilities.Clear(FacilityType.Ndb);
                }
                OnConnected?.Invoke();
                FlightSimProviderBase.Log(Provider, "Connected");
            }
            catch (COMException)
            {
                // SimConnect Not Running
                IsConnected = false;
                IsRunning = false;
            }
            catch (Exception ex)
            {
                Fail(ex);
                IsConnected = false;
                IsRunning = false;
            }
        }

        private void MicrosoftSimConnect_OnRecvSystemState(Microsoft.FlightSimulator.SimConnect.SimConnect sender, SIMCONNECT_RECV_SYSTEM_STATE data)
        {
            FlightSimProviderBase.Log(Provider, $"Received System State: {data.szString}");
        }

        private static string Clean(string? s) => (s ?? "").Trim().TrimEnd('\0');

        private void AddFacility(FacilityType type, string ident, string region, double lat, double lon)
        {
            if (string.IsNullOrWhiteSpace(ident)) return;
            Facilities.Add(type, ident, region, lat, lon);
        }

        private readonly record struct FacilitySnap(string Ident, string Region, double Lat, double Lon);

        private void MicrosoftSimConnect_OnRecvWaypointList(Microsoft.FlightSimulator.SimConnect.SimConnect sender, SIMCONNECT_RECV_WAYPOINT_LIST data)
        {
            try
            {
                int gen = Volatile.Read(ref _facilityGen);
                var items = new List<FacilitySnap>((int)data.dwArraySize);
                for (int i = 0; i < data.dwArraySize; i++)
                {
                    if (data.rgData[i] is SIMCONNECT_DATA_FACILITY_WAYPOINT wp)
                    {
                        items.Add(new FacilitySnap(Clean(wp.Ident), Clean(wp.Region), wp.Latitude, wp.Longitude));
                    }
                }

                _ = Task.Run(() =>
                {
                    try
                    {
                        if (gen != Volatile.Read(ref _facilityGen)) return; // stale
                        lock (_facLock)
                        {
                            foreach (var it in items)
                            {
                                if (!IsConnected) return;
                                AddFacility(FacilityType.Waypoint, it.Ident, it.Region, it.Lat, it.Lon);
                            }
                        }

                        OnFacilitiesReceived?.Invoke(FacilityType.Waypoint);
                        FlightSimProviderBase.Log(Provider, "Waypoint Facilities Received");
                    }
                    catch (Exception ex) { Fail(ex); }
                });
            }
            catch (Exception ex) { Fail(ex); }
        }

        private void MicrosoftSimConnect_OnRecvAirportList(Microsoft.FlightSimulator.SimConnect.SimConnect sender, SIMCONNECT_RECV_AIRPORT_LIST data)
        {
            try
            {
                int gen = Volatile.Read(ref _facilityGen);
                var items = new List<FacilitySnap>((int)data.dwArraySize);
                for (int i = 0; i < data.dwArraySize; i++)
                {
                    if (data.rgData[i] is SIMCONNECT_DATA_FACILITY_AIRPORT ap)
                    {
                        items.Add(new FacilitySnap(Clean(ap.Ident), Clean(ap.Region), ap.Latitude, ap.Longitude));
                    }
                }

                _ = Task.Run(() =>
                {
                    try
                    {
                        if (gen != Volatile.Read(ref _facilityGen)) return; // stale
                        lock (_facLock)
                        {
                            foreach (var it in items)
                            {
                                if (!IsConnected) return;
                                AddFacility(FacilityType.Airport, it.Ident, it.Region, it.Lat, it.Lon);
                            }
                        }

                        OnFacilitiesReceived?.Invoke(FacilityType.Airport);
                        FlightSimProviderBase.Log(Provider, "Airport Facilities Received");
                    }
                    catch (Exception ex) { Fail(ex); }
                });
            }
            catch (Exception ex) { Fail(ex); }
        }

        private void MicrosoftSimConnect_OnRecvVorList(Microsoft.FlightSimulator.SimConnect.SimConnect sender, SIMCONNECT_RECV_VOR_LIST data)
        {
            try
            {
                int gen = Volatile.Read(ref _facilityGen);
                var items = new List<FacilitySnap>((int)data.dwArraySize);
                for (int i = 0; i < data.dwArraySize; i++)
                {
                    if (data.rgData[i] is SIMCONNECT_DATA_FACILITY_VOR vor)
                    {
                        items.Add(new FacilitySnap(Clean(vor.Ident), Clean(vor.Region), vor.Latitude, vor.Longitude));
                    }
                }

                _ = Task.Run(() =>
                {
                    try
                    {
                        if (gen != Volatile.Read(ref _facilityGen)) return; // stale
                        lock (_facLock)
                        {
                            foreach (var it in items)
                            {
                                if (!IsConnected) return;
                                AddFacility(FacilityType.Vor, it.Ident, it.Region, it.Lat, it.Lon);
                            }
                        }

                        OnFacilitiesReceived?.Invoke(FacilityType.Vor);
                        FlightSimProviderBase.Log(Provider, "VOR Facilities Received");
                    }
                    catch (Exception ex) { Fail(ex); }
                });
            }
            catch (Exception ex) { Fail(ex); }
        }

        private void MicrosoftSimConnect_OnRecvNdbList(Microsoft.FlightSimulator.SimConnect.SimConnect sender, SIMCONNECT_RECV_NDB_LIST data)
        {
            try
            {
                int gen = Volatile.Read(ref _facilityGen);
                var items = new List<FacilitySnap>((int)data.dwArraySize);
                for (int i = 0; i < data.dwArraySize; i++)
                {
                    if (data.rgData[i] is SIMCONNECT_DATA_FACILITY_NDB ndb)
                    {
                        items.Add(new FacilitySnap(Clean(ndb.Ident), Clean(ndb.Region), ndb.Latitude, ndb.Longitude));
                    }
                }

                _ = Task.Run(() =>
                {
                    try
                    {
                        if (gen != Volatile.Read(ref _facilityGen)) return; // stale
                        lock (_facLock)
                        {
                            foreach (var it in items)
                            {
                                if (!IsConnected) return;
                                AddFacility(FacilityType.Ndb, it.Ident, it.Region, it.Lat, it.Lon);
                            }
                        }

                        OnFacilitiesReceived?.Invoke(FacilityType.Ndb);
                        FlightSimProviderBase.Log(Provider, "NDB Facilities Received");
                    }
                    catch (Exception ex) { Fail(ex); }
                });
            }
            catch (Exception ex) { Fail(ex); }
        }

        private void Fail(Exception ex)
        {
            Error = true;
            ErrorMessage = WrapProviderError(ex);
            OnError?.Invoke(ex);
            FlightSimProviderBase.Log(Provider, ex);
        }

        private FlightSimProviderException WrapProviderError(Exception ex)
        {
            var dataBag = ex.Data.Count > 0 ? ex.Data.Cast<System.Collections.DictionaryEntry>().ToDictionary(e => e.Key.ToString()!, e => e.Value) : null;
            return new FlightSimProviderException(Provider, ex.Message, ex.GetType().Name, ex, dataBag);
        }

        public void ClearError()
        {
            Error = false;
            ErrorMessage = null;
        }

        private bool RemoveTraffic(uint objectId, out Aircraft? removed)
        {
            lock (_trafficLock)
            {
                if (Traffic.TryGetValue(objectId.ToString(), out removed))
                {
                    Traffic.Remove(objectId.ToString());
                    return true;
                }
            }
            removed = null;
            return false;
        }

        public void Deinitialize()
        {
            Interlocked.Increment(ref _facilityGen);
            if (MicrosoftSimConnect != null)
            {
                IsConnected = false;
                IsRunning = false;
                try
                {
                    MicrosoftSimConnect.Dispose();
                    MicrosoftSimConnect = null!;
                }
                catch (Exception)
                {
                }
                trafficTimer?.Dispose();
                trafficTimer = null;
                lock (_trafficLock)
                {
                    Traffic.Clear();
                }
            }
        }

        private void MicrosoftSimConnect_OnRecvEvent(Microsoft.FlightSimulator.SimConnect.SimConnect sender, SIMCONNECT_RECV_EVENT data)
        {
            if (data.uEventID == (uint)SYSTEM_EVENT.PAUSE)
            {
                bool paused = data.dwData != 0;
                OnPaused?.Invoke(paused);
                FlightSimProviderBase.Log(Provider, "Paused");
            }
            else if (data.uEventID == (uint)SYSTEM_EVENT.CRASHED)
            {
                OnCrashed?.Invoke();
                FlightSimProviderBase.Log(Provider, "Crashed");
            }
            else if (data.uEventID == (uint)SYSTEM_EVENT.FLIGHTLOADED)
            {
                OnFlightLoaded?.Invoke((data as SIMCONNECT_RECV_EVENT_FILENAME)?.szFileName);
                FlightSimProviderBase.Log(Provider, "Flight Loaded");
            }
            else if (data.uEventID == (uint)SYSTEM_EVENT.FLIGHTPLANACTIVATED)
            {
                OnFlightPlanActivated?.Invoke((data as SIMCONNECT_RECV_EVENT_FILENAME)?.szFileName);
                FlightSimProviderBase.Log(Provider, "Flightplan Activated");
            }
            else if (data.uEventID == (uint)SYSTEM_EVENT.FLIGHTPLANDEACTIVATED)
            {
                OnFlightPlanDeactivated?.Invoke();
                FlightSimProviderBase.Log(Provider, "Flightplan Deactivated");
            }
            else if (data.uEventID == (uint)SYSTEM_EVENT.SIM)
            {
                IsRunning = Convert.ToBoolean(data.dwData);
                if (IsRunning)
                {
                    RequestFlightData();
                    if (MicrosoftSimConnect != null)
                    {
                        try
                        {
                            MicrosoftSimConnect.RequestFacilitiesList(SIMCONNECT_FACILITY_LIST_TYPE.AIRPORT, DATA_REQUEST_ID.AIRPORTS_REQ);
                            MicrosoftSimConnect.RequestFacilitiesList(SIMCONNECT_FACILITY_LIST_TYPE.WAYPOINT, DATA_REQUEST_ID.WAYPOINTS_REQ);
                            MicrosoftSimConnect.RequestFacilitiesList(SIMCONNECT_FACILITY_LIST_TYPE.VOR, DATA_REQUEST_ID.VORS_REQ);
                            MicrosoftSimConnect.RequestFacilitiesList(SIMCONNECT_FACILITY_LIST_TYPE.NDB, DATA_REQUEST_ID.NDBS_REQ);

                            MicrosoftSimConnect.RequestDataOnSimObject(DATA_REQUEST_ID.NONSUBSCRIBE_REQ, DATA_DEFINE_ID.FLIGHTDATA, Microsoft.FlightSimulator.SimConnect.SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.SECOND, SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT, 0, 0, 0);

                            if (RequestTrafficInfo)
                            {
                                MicrosoftSimConnect.RequestDataOnSimObjectType(DATA_REQUEST_ID.TRAFFIC_REQ, DATA_DEFINE_ID.AIRCRAFT, SearchRadiusMeters, SIMCONNECT_SIMOBJECT_TYPE.AIRCRAFT);
                                MicrosoftSimConnect.RequestDataOnSimObjectType(DATA_REQUEST_ID.TRAFFIC_REQ, DATA_DEFINE_ID.AIRCRAFT, SearchRadiusMeters, SIMCONNECT_SIMOBJECT_TYPE.HELICOPTER);
                            }

                            trafficTimer?.Dispose();
                            lock (_trafficLock)
                            {
                                Traffic.Clear();
                            }
                            if (RequestTrafficInfo)
                            {
                                trafficTimer = new Timer(RequestTrafficData, null, 60000, 60000);
                            }
                        }
                        catch(Exception ex)
                        {
                            Fail(ex);
                        }
                    }
                }
                else
                {
                    MicrosoftSimConnect?.RequestDataOnSimObject(DATA_REQUEST_ID.NONSUBSCRIBE_REQ, DATA_DEFINE_ID.FLIGHTDATA, Microsoft.FlightSimulator.SimConnect.SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.NEVER, SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT, 0, 0, 0);
                    CurrentAircraft.Reset();
                    CurrentWeather.Reset();
                    trafficTimer?.Dispose();
                    trafficTimer = null;
                    lock (_trafficLock)
                    {
                        Traffic.Clear();
                    }
                }
                OnSim?.Invoke(IsRunning);
                FlightSimProviderBase.Log(Provider, $"Sim Running: {IsRunning}");
            }
            else if (Events.TryGetValue((SimConnectEventId)data.uEventID, out var evt))
            {
                evt.Update(data.dwData);
            }
        }

        private void MicrosoftSimConnect_OnRecvQuit(Microsoft.FlightSimulator.SimConnect.SimConnect sender, SIMCONNECT_RECV data)
        {
            trafficTimer?.Dispose();
            trafficTimer = null!;
            lock (_trafficLock)
            {
                Traffic.Clear();
            }
            IsConnected = false;
            IsRunning = false;
            CurrentAircraft.Reset();
            CurrentWeather.Reset();
            OnQuit?.Invoke();
            FlightSimProviderBase.Log(Provider, "Quit");
        }

        private void MicrosoftSimConnect_OnRecvException(Microsoft.FlightSimulator.SimConnect.SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            if (data.dwException == (uint)SIMCONNECT_EXCEPTION.UNRECOGNIZED_ID)
            {
                List<Aircraft> aircraftToRemove;
                lock (_trafficLock)
                {
                    aircraftToRemove = Traffic.Values.Where(a => (DateTime.Now - a.LastUpdateTime).TotalSeconds > 10).ToList();
                }
                foreach (Aircraft aircraft in aircraftToRemove)
                {
                    //Hasn't been updated in more than 10 seconds. Maybe this guy is the culprit.
                    try
                    {
                        RemoveTraffic(aircraft.Id, out var ac);
                        if (RequestTrafficInfo)
                        {
                            MicrosoftSimConnect?.RequestDataOnSimObject(DATA_REQUEST_ID.TRAFFIC_REQ, DATA_DEFINE_ID.AIRCRAFT, aircraft.Id, SIMCONNECT_PERIOD.NEVER, SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT, 0, 0, 0);
                        }
                        OnTrafficReceived?.Invoke(aircraft.Id, aircraft, TrafficEvent.Remove);
                    }
                    catch
                    {
                    }
                }
            }
            else
            {
                var scEx = (SIMCONNECT_EXCEPTION)data.dwException;
                var bag = new Dictionary<string, object?>
                {
                    ["dwException"] = data.dwException,
                    ["ExceptionName"] = scEx.ToString(),
                    ["dwSendID"] = data.dwSendID,
                    ["dwIndex"] = data.dwIndex
                };
                Fail(new FlightSimProviderException(
                    provider: Provider,
                    message: $"SimConnect exception: (SendId={data.dwSendID} Index={data.dwIndex})",
                    code: scEx.ToString(),
                    innerException: null,
                    dataBag: bag));
            }
        }

        private void MicrosoftSimConnect_OnRecvSimobjectDataBytype(Microsoft.FlightSimulator.SimConnect.SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            try
            {
                ClearError();
                if (data.dwDefineID == (uint)DATA_DEFINE_ID.FLIGHTDATA)
                {
                    FLIGHT_DATA flightData = (FLIGHT_DATA)data.dwData[0];
                    OnFlightDataReceived?.Invoke(flightData);
                    FlightSimProviderBase.Log(Provider, "Flight Data Received");
                }
                else if (data.dwRequestID == (uint)DATA_REQUEST_ID.TRAFFIC_REQ && data.dwDefineID == (uint)DATA_DEFINE_ID.AIRCRAFT && RequestTrafficInfo)
                {
                    if (data.dwObjectID <= 1) return;

                    Aircraft? aircraft;
                    bool added = false;
                    lock (_trafficLock)
                    {
                        if (!Traffic.TryGetValue(data.dwObjectID.ToString(), out aircraft))
                        {
                            aircraft = new Aircraft(data.dwObjectID);

                            Traffic.Add(data.dwObjectID.ToString(), aircraft);
                            added = true;
                        }
                    }
                    if (aircraft != null)
                    {
                        if (added)
                        {
                            OnTrafficReceived?.Invoke(data.dwObjectID, aircraft, TrafficEvent.Add);
                        }
                        var aircraftData = (AIRCRAFT_DATA)data.dwData[0];
                        aircraft.UpdateAircraft(aircraftData);
                        OnTrafficReceived?.Invoke(data.dwObjectID, aircraft, TrafficEvent.Update);
                        FlightSimProviderBase.Log(Provider, $"Traffic Received: {aircraft.AircraftName} {aircraft.Callsign}");
                    }
                }
            }
            catch (Exception ex)
            {
                Fail(ex);
            }
        }

        private void MicrosoftSimConnect_OnRecvSimobjectData(Microsoft.FlightSimulator.SimConnect.SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            try
            {
                ClearError();
                if (data.dwDefineID == (uint)DATA_DEFINE_ID.FLIGHTDATA)
                {
                    FLIGHT_DATA flightData = (FLIGHT_DATA)data.dwData[0];
                    CurrentAircraft.UpdateData(flightData);
                    CurrentWeather.UpdateWeather(flightData);
                    OnFlightDataReceived?.Invoke(flightData);
                    FlightSimProviderBase.Log(Provider, "Flight Data Received");
                    return;
                }

                // Traffic updates (only relevant if you are doing per-aircraft RequestDataOnSimObject calls).
                // If you're purely polling via RequestDataOnSimObjectType (every 60s), you can ignore this handler for traffic.
                if (data.dwDefineID == (uint)DATA_DEFINE_ID.AIRCRAFT)
                {
                    // If we aren't tracking this object, ignore the packet.
                    Aircraft? aircraft;
                    lock (_trafficLock)
                    {
                        Traffic.TryGetValue(data.dwObjectID.ToString(), out aircraft);
                    }
                    if (aircraft == null)
                    {
                        OnTrafficReceived?.Invoke(data.dwObjectID, null, TrafficEvent.Remove);
                        return;
                    }
                    AIRCRAFT_DATA aircraftData = (AIRCRAFT_DATA)data.dwData[0];
                    bool onGround = Convert.ToBoolean(aircraftData.SIM_ON_GROUND);
                    bool engCombustion = Convert.ToBoolean(aircraftData.ENG_COMBUSTION);
                    var myLat = CurrentAircraft.Position.Latitude;
                    var myLon = CurrentAircraft.Position.Longitude;
                    double distanceNm = Tools.DistanceTo(myLat.Value, myLon.Value, aircraftData.PLANE_LATITUDE, aircraftData.PLANE_LONGITUDE, DistanceUnit.NauticalMiles);
                    // Remove if outside radius OR parked/dead on ground.
                    if (myLat != null && myLon != null)
                    {
                        if (distanceNm > SearchRadiusNm || (onGround && !engCombustion))
                        {
                            if (MicrosoftSimConnect != null)
                            {
                                var reqId = (DATA_REQUEST_ID)((uint)DATA_REQUEST_ID.TRAFFIC_REQ + (uint)data.dwObjectID);

                                MicrosoftSimConnect.RequestDataOnSimObject(reqId, DATA_DEFINE_ID.AIRCRAFT, data.dwObjectID, SIMCONNECT_PERIOD.NEVER, SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT, 0, 0, 0);
                            }
                            RemoveTraffic(data.dwObjectID, out var ac);
                            OnTrafficReceived?.Invoke(data.dwObjectID, aircraft, TrafficEvent.Remove);
                            FlightSimProviderBase.Log(Provider, $"Traffic Removed: {aircraft.AircraftName} {aircraft.Callsign}");
                            return;
                        }
                    }
                    aircraft.UpdateAircraft(aircraftData);
                    OnTrafficReceived?.Invoke(data.dwObjectID, aircraft, TrafficEvent.Update);
                    FlightSimProviderBase.Log(Provider, $"Traffic Received: {aircraft.AircraftName} {aircraft.Callsign}");
                }
            }
            catch (Exception ex)
            {
                Fail(ex);
            }
        }

        private void AddDef(DATA_DEFINE_ID def, string name, string? units, SIMCONNECT_DATATYPE type)
        {
            try
            {
                FlightSimProviderBase.Log(Provider, $"DEF + {name} [{units}] ({type})");
                MicrosoftSimConnect?.AddToDataDefinition(def, name, units, type, 0, Microsoft.FlightSimulator.SimConnect.SimConnect.SIMCONNECT_UNUSED);
            }
            catch (Exception ex)
            {
                FlightSimProviderBase.Log(Provider, ex, $"AddToDataDefinition threw for {name}");
                throw;
            }
        }

        private void MapEvt(SimConnectEventId id, string evtName)
        {
            FlightSimProviderBase.Log(Provider, $"MAP + {evtName}");
            MicrosoftSimConnect?.MapClientEventToSimEvent(id, evtName);

        }

        private void SubscribeToSystemEvent(SYSTEM_EVENT id, string evtName)
        {
            FlightSimProviderBase.Log(Provider, $"SUBSCRIBE + {evtName}");
            MicrosoftSimConnect?.SubscribeToSystemEvent(id, evtName);
        }

        private void MicrosoftSimConnect_OnRecvOpen(Microsoft.FlightSimulator.SimConnect.SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            try
            {
                try
                {
                    ClearError();
                    if (MicrosoftSimConnect != null)
                    {
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "TITLE", null, SIMCONNECT_DATATYPE.STRING256);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ATC AIRLINE", null, SIMCONNECT_DATATYPE.STRING256);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ATC FLIGHT NUMBER", null, SIMCONNECT_DATATYPE.STRING256);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ATC MODEL", null, SIMCONNECT_DATATYPE.STRING256);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ATC TYPE", null, SIMCONNECT_DATATYPE.STRING256);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ATC ID", null, SIMCONNECT_DATATYPE.STRING256);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "CATEGORY", null, SIMCONNECT_DATATYPE.STRING256);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GPS WP NEXT ID", null, SIMCONNECT_DATATYPE.STRING256);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "COM ACTIVE FREQ TYPE:1", null, SIMCONNECT_DATATYPE.STRING256);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "COM ACTIVE FREQ TYPE:2", null, SIMCONNECT_DATATYPE.STRING256);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "COM ACTIVE FREQ IDENT:1", null, SIMCONNECT_DATATYPE.STRING256);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "COM ACTIVE FREQ IDENT:2", null, SIMCONNECT_DATATYPE.STRING256);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "NAV IDENT:1", null, SIMCONNECT_DATATYPE.STRING256);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "NAV IDENT:2", null, SIMCONNECT_DATATYPE.STRING256);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ADF IDENT", null, SIMCONNECT_DATATYPE.STRING256);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ADF NAME:1", null, SIMCONNECT_DATATYPE.STRING256);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "PLANE LATITUDE", "degrees", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "PLANE LONGITUDE", "degrees", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "PLANE ALTITUDE", "feet", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "PRESSURE ALTITUDE", "feet", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "PLANE ALT ABOVE GROUND", "feet", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "AUTOPILOT HEADING LOCK DIR", "degrees", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "PLANE HEADING DEGREES MAGNETIC", "degrees", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "PLANE HEADING DEGREES TRUE", "degrees", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "PLANE PITCH DEGREES", "degrees", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "PLANE BANK DEGREES", "degrees", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "VERTICAL SPEED", "feet/minute", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "AIRSPEED INDICATED", "knots", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "AIRSPEED TRUE", "knots", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GROUND ALTITUDE", "feet", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GROUND VELOCITY", "knots", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "KOHLSMAN SETTING HG:1", "inHg", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "KOHLSMAN SETTING HG:2", "inHg", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "SEA LEVEL PRESSURE", "inHg", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "AMBIENT WIND VELOCITY", "knots", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "AMBIENT WIND DIRECTION", "degrees", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "AMBIENT TEMPERATURE", "celsius", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ADF RADIAL MAG:1", "degrees", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "NAV RELATIVE BEARING TO STATION:1", "degrees", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "NAV RELATIVE BEARING TO STATION:2", "degrees", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "NAV DME:1", "nautical miles", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "NAV DME:2", "nautical miles", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "NAV DMESPEED:1", "knots", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "NAV DMESPEED:2", "knots", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GPS IS ACTIVE WAY POINT", "bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GPS DRIVES NAV1", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GPS WP TRUE REQ HDG", "radians", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GPS WP BEARING", "radians", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GPS WP CROSS TRK", "meters", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GPS WP NEXT LAT", "degrees", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GPS WP NEXT LON", "degrees", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "FUEL TOTAL CAPACITY", "gallons", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "FUEL TANK LEFT MAIN CAPACITY", "gallons", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "FUEL TANK RIGHT MAIN CAPACITY", "gallons", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "FUEL TANK CENTER CAPACITY", "gallons", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "FUEL TOTAL QUANTITY", "gallons", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "FUEL TANK LEFT MAIN QUANTITY", "gallons", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "FUEL TANK RIGHT MAIN QUANTITY", "gallons", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "FUEL TANK CENTER QUANTITY", "gallons", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "NAV ACTIVE FREQUENCY:1", "Khz", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "NAV ACTIVE FREQUENCY:2", "Khz", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "NAV STANDBY FREQUENCY:1", "Khz", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "NAV STANDBY FREQUENCY:2", "Khz", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "COM ACTIVE FREQUENCY:1", "Khz", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "COM ACTIVE FREQUENCY:2", "Khz", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "COM STANDBY FREQUENCY:1", "Khz", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "COM STANDBY FREQUENCY:2", "Khz", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "COM TRANSMIT:1", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "COM TRANSMIT:2", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "COM STATUS:1", "Enum", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "COM STATUS:2", "Enum", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "COM RECEIVE:1", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "COM RECEIVE:2", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "TRANSPONDER CODE:1", "Bco16", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "TRANSPONDER STATE:1", "Enum", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "TRANSPONDER IDENT", "bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ELECTRICAL BATTERY LOAD", "Amperes", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ELECTRICAL MASTER BATTERY:1", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "AVIONICS MASTER SWITCH:1", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GENERAL ENG MASTER ALTERNATOR:1", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "PITOT HEAT SWITCH:1", "Enum", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "NAV AVAILABLE:1", "bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "NAV AVAILABLE:2", "bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "NAV SOUND:1", "bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "NAV SOUND:2", "bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "SIM ON GROUND", "bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ENGINE TYPE", "Enum", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ATC HEAVY", "bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "IS GEAR FLOATS", "bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "NUMBER OF ENGINES", "Number", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GENERAL ENG THROTTLE LEVER POSITION:1", "Percent", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GENERAL ENG THROTTLE LEVER POSITION:2", "Percent", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GENERAL ENG THROTTLE LEVER POSITION:3", "Percent", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GENERAL ENG THROTTLE LEVER POSITION:4", "Percent", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GENERAL ENG MIXTURE LEVER POSITION:1", "Percent", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GENERAL ENG MIXTURE LEVER POSITION:2", "Percent", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GENERAL ENG MIXTURE LEVER POSITION:3", "Percent", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GENERAL ENG MIXTURE LEVER POSITION:4", "Percent", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GENERAL ENG PROPELLER LEVER POSITION:1", "Percent", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GENERAL ENG PROPELLER LEVER POSITION:2", "Percent", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GENERAL ENG PROPELLER LEVER POSITION:3", "Percent", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GENERAL ENG PROPELLER LEVER POSITION:4", "Percent", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "PROP BETA:1", "Radians", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "PROP BETA:2", "Radians", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "PROP BETA:3", "Radians", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "PROP BETA:4", "Radians", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GENERAL ENG COMBUSTION:1", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GENERAL ENG COMBUSTION:2", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GENERAL ENG COMBUSTION:3", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GENERAL ENG COMBUSTION:4", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "TURB ENG COMMANDED N1:1", "Percent", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "TURB ENG COMMANDED N1:2", "Percent", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "TURB ENG COMMANDED N1:3", "Percent", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "TURB ENG COMMANDED N1:4", "Percent", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ENG MANIFOLD PRESSURE:1", "inHg", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ENG MANIFOLD PRESSURE:2", "inHg", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ENG MANIFOLD PRESSURE:3", "inHg", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ENG MANIFOLD PRESSURE:4", "inHg", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "TURB ENG AFTERBURNER:1", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "TURB ENG AFTERBURNER:2", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "TURB ENG AFTERBURNER:3", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "TURB ENG AFTERBURNER:4", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GENERAL ENG RPM:1", "RPM", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GENERAL ENG RPM:2", "RPM", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GENERAL ENG RPM:3", "RPM", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GENERAL ENG RPM:4", "RPM", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ENG TORQUE:1", "Foot pounds", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ENG TORQUE:2", "Foot pounds", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ENG TORQUE:3", "Foot pounds", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ENG TORQUE:4", "Foot pounds", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "FLAPS HANDLE PERCENT", "Percent", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GEAR HANDLE POSITION", "Percent", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GEAR CENTER POSITION", "Percent Over 100", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GEAR LEFT POSITION", "Percent Over 100", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GEAR LEFT POSITION", "Percent Over 100", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "SPOILERS HANDLE POSITION", "Percent", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "SPOILERS ARMED", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "BRAKE PARKING POSITION", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "LIGHT NAV", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "LIGHT BEACON", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "LIGHT STROBE", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "LIGHT LANDING", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "LIGHT TAXI", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "LIGHT CABIN", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "LIGHT PANEL", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "AUTOPILOT MASTER", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "AUTOPILOT FLIGHT DIRECTOR ACTIVE", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "AUTOPILOT HEADING LOCK", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "AUTOPILOT ALTITUDE LOCK", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "AUTOPILOT NAV1 LOCK", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "AUTOPILOT AIRSPEED HOLD", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "AUTOPILOT VERTICAL HOLD", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "AUTOPILOT ALTITUDE LOCK VAR", "feet", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "AUTOPILOT AIRSPEED HOLD VAR", "knots", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "AUTOPILOT VERTICAL HOLD VAR", "feet per minute", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "NAV OBS:1", "degrees", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "NAV OBS:2", "degrees", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ELEVATOR TRIM PCT", "percent", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "AILERON TRIM PCT", "percent", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "RUDDER TRIM PCT", "percent", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "WINDSHIELD DEICE SWITCH", "bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "RECIP ENG COWL FLAP POSITION:1", "percent", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "RECIP ENG COWL FLAP POSITION:2", "percent", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "RECIP ENG COWL FLAP POSITION:3", "percent", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "RECIP ENG COWL FLAP POSITION:4", "percent", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ENG FAILED:1", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ENG FAILED:2", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ENG FAILED:3", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ENG FAILED:4", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ENG OIL PRESSURE:1", "psf", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ENG OIL PRESSURE:2", "psf", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ENG OIL PRESSURE:3", "psf", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ENG OIL PRESSURE:4", "psf", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ENG ANTI ICE:1", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ENG ANTI ICE:2", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ENG ANTI ICE:3", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ENG ANTI ICE:4", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "PROP DEICE SWITCH:1", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "PROP DEICE SWITCH:2", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "PROP DEICE SWITCH:3", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "PROP DEICE SWITCH:4", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GENERAL ENG FUEL PUMP SWITCH:1", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GENERAL ENG FUEL PUMP SWITCH:2", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GENERAL ENG FUEL PUMP SWITCH:3", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GENERAL ENG FUEL PUMP SWITCH:4", "Bool", SIMCONNECT_DATATYPE.INT32);

                        // Helicoptor
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "COLLECTIVE POSITION", "Percent Over 100", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "DISK BANK PCT:1", "Percent Over 100", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "DISK BANK PCT:2", "Percent Over 100", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "DISK CONING PCT:1", "Percent Over 100", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "DISK CONING PCT:2", "Percent Over 100", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "DISK PITCH PCT:1", "Percent Over 100", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "DISK PITCH PCT:2", "Percent Over 100", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ROTOR BRAKE ACTIVE", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ROTOR BRAKE HANDLE POS", "Percent Over 100", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ROTOR COLLECTIVE BLADE PITCH PCT", "Percent Over 100", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ROTOR LATERAL TRIM PCT", "Percent Over 100", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ROTOR LONGITUDINAL TRIM PCT", "Percent Over 100", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ROTOR ROTATION ANGLE:1", "Radians", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ROTOR ROTATION ANGLE:2", "Radians", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ROTOR GOV ACTIVE:1", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ROTOR GOV ACTIVE:2", "Bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ROTOR RPM:1", "RPM", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ROTOR RPM:2", "RPM", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "TAIL ROTOR BLADE PITCH PCT", "Percent Over 100", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ENG ROTOR RPM:1", "Percent", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ENG ROTOR RPM:2", "Percent", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ENG TORQUE PERCENT:1", "Percent", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "ENG TORQUE PERCENT:2", "Percent", SIMCONNECT_DATATYPE.FLOAT64);

                        // Flightplan
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GPS IS ACTIVE FLIGHT PLAN", "bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GPS FLIGHT PLAN WP COUNT", "number", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GPS FLIGHT PLAN WP INDEX", "number", SIMCONNECT_DATATYPE.INT32);   // current active WP index
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GPS IS DIRECTTO FLIGHTPLAN", "bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GPS FLIGHT PLAN WP INDEX", "number", SIMCONNECT_DATATYPE.INT32); // set/read
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GPS APPROACH WP COUNT", "number", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GPS APPROACH WP INDEX", "number", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GPS APPROACH IS WP RUNWAY", "bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "GPS APPROACH AIRPORT ID", null, SIMCONNECT_DATATYPE.STRING256);
                        // Balloons
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "BALLOON AUTO FILL ACTIVE", "bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "BALLOON FILL AMOUNT", "percent", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "BALLOON GAS DENSITY", "kilograms per cubic meter", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "BALLOON GAS TEMPERATURE", "celsius", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "BALLOON VENT OPEN VALUE", "percent", SIMCONNECT_DATATYPE.FLOAT64);
                        // Burners (use index 0)
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "BURNER FUEL FLOW RATE:0", "pounds per hour", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "BURNER PILOT LIGHT ON:0", "bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "BURNER VALVE OPEN VALUE:0", "percent", SIMCONNECT_DATATYPE.FLOAT64);
                        // Airships (compartment 0 / fan 0)
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "AIRSHIP COMPARTMENT GAS TYPE:0", "enum", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "AIRSHIP COMPARTMENT PRESSURE:0", "millibars", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "AIRSHIP COMPARTMENT OVERPRESSURE:0", "millibars", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "AIRSHIP COMPARTMENT TEMPERATURE:0", "celsius", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "AIRSHIP COMPARTMENT VOLUME:0", "cubic meters", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "AIRSHIP COMPARTMENT WEIGHT:0", "pounds", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "AIRSHIP FAN POWER PCT:0", "percent", SIMCONNECT_DATATYPE.FLOAT64);
                        // Mast truck
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "MAST TRUCK DEPLOYMENT", "bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.FLIGHTDATA, "MAST TRUCK EXTENSION", "percent", SIMCONNECT_DATATYPE.FLOAT64);

                        MicrosoftSimConnect.RegisterDataDefineStruct<FLIGHT_DATA>(DATA_DEFINE_ID.FLIGHTDATA);

                        AddDef(DATA_DEFINE_ID.AIRCRAFT, "TITLE", null, SIMCONNECT_DATATYPE.STRING256);
                        AddDef(DATA_DEFINE_ID.AIRCRAFT, "ATC MODEL", null, SIMCONNECT_DATATYPE.STRING256);
                        AddDef(DATA_DEFINE_ID.AIRCRAFT, "ATC TYPE", null, SIMCONNECT_DATATYPE.STRING256);
                        AddDef(DATA_DEFINE_ID.AIRCRAFT, "ATC ID", null, SIMCONNECT_DATATYPE.STRING256);
                        AddDef(DATA_DEFINE_ID.AIRCRAFT, "PLANE LATITUDE", "degrees", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.AIRCRAFT, "PLANE LONGITUDE", "degrees", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.AIRCRAFT, "PLANE ALTITUDE", "feet", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.AIRCRAFT, "PLANE HEADING DEGREES TRUE", "degrees", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.AIRCRAFT, "AIRSPEED INDICATED", "knots", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.AIRCRAFT, "ENGINE TYPE", "Enum", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.AIRCRAFT, "SIM ON GROUND", "bool", SIMCONNECT_DATATYPE.INT32);
                        AddDef(DATA_DEFINE_ID.AIRCRAFT, "GROUND VELOCITY", "knots", SIMCONNECT_DATATYPE.FLOAT64);
                        AddDef(DATA_DEFINE_ID.AIRCRAFT, "ENG COMBUSTION:1", "bool", SIMCONNECT_DATATYPE.INT32);

                        MicrosoftSimConnect.RegisterDataDefineStruct<AIRCRAFT_DATA>(DATA_DEFINE_ID.AIRCRAFT);

                        foreach (SimConnectEvent evt in Events.Values)
                        {
                            MapEvt(evt.Id, evt.Command);
                        }

                        SubscribeToSystemEvent(SYSTEM_EVENT.SIM, "Sim");
                        SubscribeToSystemEvent(SYSTEM_EVENT.CRASHED, "Crashed");
                        SubscribeToSystemEvent(SYSTEM_EVENT.PAUSE, "Pause");
                        SubscribeToSystemEvent(SYSTEM_EVENT.FLIGHTLOADED, "FlightLoaded");
                        SubscribeToSystemEvent(SYSTEM_EVENT.FLIGHTPLANACTIVATED, "FlightPlanActivated");
                        SubscribeToSystemEvent(SYSTEM_EVENT.FLIGHTPLANDEACTIVATED, "FlightPlanDeactivated");
                    }
                }
                catch (Exception ex)
                {
                    Fail(ex);
                }
            }
            catch (Exception ex) 
            { 
                Fail(ex); 
            }
        }

        private void RequestTrafficData(object? state)
        {
            if (MicrosoftSimConnect == null || !IsConnected || !IsRunning) return;

            if (MicrosoftSimConnect == null || !IsConnected || !IsRunning) return;

            MicrosoftSimConnect.RequestDataOnSimObjectType(DATA_REQUEST_ID.TRAFFIC_REQ, DATA_DEFINE_ID.AIRCRAFT, SearchRadiusMeters, SIMCONNECT_SIMOBJECT_TYPE.AIRCRAFT);
            MicrosoftSimConnect.RequestDataOnSimObjectType(DATA_REQUEST_ID.TRAFFIC_REQ, DATA_DEFINE_ID.AIRCRAFT, SearchRadiusMeters, SIMCONNECT_SIMOBJECT_TYPE.HELICOPTER);
            MicrosoftSimConnect.RequestDataOnSimObject(DATA_REQUEST_ID.NONSUBSCRIBE_REQ, DATA_DEFINE_ID.FLIGHTDATA, Microsoft.FlightSimulator.SimConnect.SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.ONCE, SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT, 0, 0, 0);
        }

        public void RequestFlightData()
        {
            if (MicrosoftSimConnect != null && IsConnected && IsRunning)
            {
                MicrosoftSimConnect.RequestDataOnSimObject(DATA_REQUEST_ID.NONSUBSCRIBE_REQ, DATA_DEFINE_ID.FLIGHTDATA, Microsoft.FlightSimulator.SimConnect.SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.ONCE, SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT, 0, 0, 0);
            }
        }

        public void ReceiveMessage()
        {
            if (MicrosoftSimConnect != null)
            {
                ClearError();
                try
                {
                    MicrosoftSimConnect.ReceiveMessage();
                }
                catch (Exception ex)
                {
                    Fail(ex);
                }
            }
        }
    }
}
