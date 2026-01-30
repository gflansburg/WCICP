using System.Diagnostics;
using FlightSim.XPlaneConnect;

namespace FlightSim
{
    public class XPlaneProvider : FlightSimProviderBase, IDisposable
    {
        public static readonly XPlaneProvider Instance;

        private const int Frequency = 5;

        private List<XPlaneCrossref> Crossref = new List<XPlaneCrossref>();

        private XPlaneConnector connector { get; set; } = null!;

        public enum XPLANE_ENGINETYPE
        {
            RecipCarb = 0,
            RecipInjected = 1,
            Electric = 3,
            SingleSpoolJet = 5,
            Rocket = 6,
            MultiSpoolJet = 7,
            FreeTurborpop = 9,
            FixedTurboprop = 10
        }

        public VSpeed VSpeed { get; private set; } = VSpeed.DefaultVSpeed();
        public Guid XPlaneAircraftId { get; private set; }
        public bool IsXPlaneAirline { get; private set; }
        public bool IsXPlaneGlider { get; private set; }
        public bool IsXPlaneHelo { get; private set; }
        public bool IsXPlaneMilitary { get; private set; }
        public bool IsXPlaneGeneralAviation { get; private set; }
        public string XPlaneAircraftName { get; private set; } = null!;
        public string XPlaneTailnumber { get; private set; } = null!;
        public XPLANE_ENGINETYPE XPlaneEngineType { get; private set; }

        private bool _com1Active;
        public bool Com1Active => _com1Active;

        private bool _com2Active;
        public bool Com2Active => _com2Active;

        private double _nav1Frequency;
        public override double Nav1Frequency => _nav1Frequency;

        private double _nav2Frequency;
        public override double Nav2Frequency => _nav2Frequency;

        private double _nav1StandByFrequency;
        public override double Nav1StandByFrequency => _nav1StandByFrequency;

        private double _nav2StandByFrequency;
        public override double Nav2StandByFrequency => _nav2StandByFrequency;

        public override string Name => "X-Plane";

        private Dictionary<string, Aircraft> _traffic = new Dictionary<string, Aircraft>();
        public override Dictionary<string, Aircraft> Traffic => _traffic;

        // Strings (from StringDataRefElement byte[150] datarefs)
        private string _adfIdent = string.Empty;
        private string _wpIdent = string.Empty;
        private string _nav1Ident = string.Empty;
        private string _nav2Ident = string.Empty;
        private double _wpLon;
        private double _wpLat;

        private double _nav1Dme;
        private double _nav2Dme;
        private double _nav1DmeSpeed;
        private double _nav2DmeSpeed;

        // NAV courses (OBS)
        private double _apNav1CourseDeg;
        private double _apNav2CourseDeg;

        // Fuel pumps (per engine)
        private readonly bool[] _fuelPumpOn = new bool[4];

        // Prop de-ice (prop heat per engine)
        private readonly bool[] _propDeiceOn = new bool[4];

        // Engine inlet heat (engine anti-ice) per engine
        private readonly bool[] _engineInletHeatOn = new bool[4];

        // Engine arrays
        private readonly double[] _engineRpm = new double[4];
        private readonly double[] _torqueFtLb = new double[4];
        private readonly double[] _oilPressurePsf = new double[4];

        // Flight model / attitude / vertical speed
        private double _verticalSpeedFpm;
        private double _pitchDeg;
        private double _bankDeg;

        // Autopilot
        private bool _apMaster;
        private int _apFlightDirectorMode;
        private bool _apGpsDrivesNav;
        private bool _apHdgHold;
        private bool _apAltHold;
        private bool _apNavHold;
        private bool _apSpdHold;
        private bool _apVsHold;
        private double _apAltTargetFt;
        private double _apSpdTargetKts;
        private double _apHdgButTargetDeg;
        private double _apVsTargetFm;

        // Controls / surfaces
        private double _flapsPct;
        private double _spoilersPct;
        private bool _spoilersArmed;
        private bool _parkingBrake;
        private double _elevatorTrimPercent;
        private double _rudderTrimPercent;
        private double _aileronTrimPercent;

        // Gear
        private double _gearAvgDeploy;
        private bool _gearHandleDown;

        // Fuel totals
        private double _fuelRemainingGallons;
        private double _fuelCapacityGallons;

        // Lights
        private bool _navLights;
        private bool _beaconLights;
        private bool _taxiLights;
        private bool _landingLights;
        private bool _strobeLights;
        private bool _cabinLights;
        private bool _panelLights;

        // Window defrost
        private bool _windowDefrost;

        // Cowl flaps (ratio per engine)
        private readonly double[] _cowlFlapsPct = new double[4];

        // Engine controls (ratio per engine)
        private readonly double[] _throttlePct = new double[4];
        private readonly double[] _mixturePct = new double[4];
        private readonly double[] _propPitchDeg = new double[4];

        // Engine state (per engine)
        private readonly bool[] _engFailed = new bool[4];
        private readonly bool[] _engRunning = new bool[4];
        private readonly double[] _n1Pct = new double[4]; // depends on what you pick; see subscription section
        private readonly double[] _manifoldInHg = new double[4]; // piston only; see note

        private int _engineCount = 1;

        // Afterburners
        private readonly bool[] _afterburnerOn = new bool[4];

        public override string Com1ActiveFrequencyIdent => string.Empty;

        public override string Com2ActiveFrequencyIdent => string.Empty;

        public override string AdfIdent => _adfIdent;
        public override string AdfName => string.Empty;

        public override double AutopilotNav1CourseDegrees => _apNav1CourseDeg;
        public override double AutopilotNav2CourseDegrees => _apNav2CourseDeg;

        public override bool FuelPump1On => _fuelPumpOn[0];
        public override bool FuelPump2On => _fuelPumpOn[1];
        public override bool FuelPump3On => _fuelPumpOn[2];
        public override bool FuelPump4On => _fuelPumpOn[3];

        public override bool Propeller1DeIce => _propDeiceOn[0];
        public override bool Propeller2DeIce => _propDeiceOn[1];
        public override bool Propeller3DeIce => _propDeiceOn[2];
        public override bool Propeller4DeIce => _propDeiceOn[3];

        public override bool CarbHeatAntiIce1 => _engineInletHeatOn[0];
        public override bool CarbHeatAntiIce2 => _engineInletHeatOn[1];
        public override bool CarbHeatAntiIce3 => _engineInletHeatOn[2];
        public override bool CarbHeatAntiIce4 => _engineInletHeatOn[3];

        public override double WaypointLongitude => _wpLon;
        public override double WaypointLatitude => _wpLat;
        public override string WaypointIdent => _wpIdent;

        public override string Nav1ActiveFrequencyIdent => _nav1Ident;
        public override string Nav2ActiveFrequencyIdent => _nav2Ident;

        public override double Engine1Rpm => _engineRpm[0];
        public override double Engine2Rpm => _engineRpm[1];
        public override double Engine3Rpm => _engineRpm[2];
        public override double Engine4Rpm => _engineRpm[3];

        public override double Torque1FootPounds => _torqueFtLb[0];
        public override double Torque2FootPounds => _torqueFtLb[1];
        public override double Torque3FootPounds => _torqueFtLb[2];
        public override double Torque4FootPounds => _torqueFtLb[3];

        public override double VerticalSpeedFpm => _verticalSpeedFpm;
        public override double PitchDegrees => _pitchDeg;
        public override double BankDegrees => _bankDeg;

        public override bool AutopilotAltHold => _apAltHold;
        public override bool AutopilotHeadingHold => _apHdgHold;
        public override bool AutopilotNavHold => _apNavHold;
        public override bool AutopilotSpeedHold => _apSpdHold;
        public override bool AutopilotMaster => _apMaster;

        public override double AutopilotAltitudeTargetFeet => _apAltTargetFt;
        public override double AutopilotSpeedTargetKnots => _apSpdTargetKts;
        public override double AutopilotHeadingBugDegrees => _apHdgButTargetDeg;

        public override double ElevatorTrimPercent => _elevatorTrimPercent;
        public override double RudderTrimPercent => _rudderTrimPercent;
        public override double AileronTrimPercent => _aileronTrimPercent;

        public override double FlapsPercent => _flapsPct;

        // ==========================
        // Helicopter – backing fields
        // ==========================
        private double _collectivePct;

        private bool _rotorBrakeActive;
        private double _rotorBrakeHandlePct;

        // Disk “attitude” (we’ll map cyclic tilt to percent of +/-10 deg => +/-100%)
        private double _mainDiskBankPct;
        private double _mainDiskPitchPct;
        private double _mainDiskConingPct;

        private double _tailDiskBankPct;
        private double _tailDiskPitchPct;
        private double _tailDiskConingPct;

        // Rotation angle (deg)
        private double _mainRotorRotationDeg;
        private double _tailRotorRotationDeg;

        // Rotor RPM (from rad/sec -> RPM)
        private double _mainRotorRpm;
        private double _tailRotorRpm;

        // Blade pitch (we’ll map prop angle degrees to 0..90 => 0..100%)
        private double _mainBladePitchPct;
        private double _tailBladePitchPct;

        // Trim (X-Plane gives one rotor trim; we’ll treat it as lateral trim)
        private double _rotorTrimPct;

        // Engine/rotor coupling
        private readonly double[] _engineRotorCmdPct = new double[2];   // 0..100-ish
        private readonly double[] _engineTorquePct = new double[2];   // 0..100

        private readonly bool[] _rotorGovernorActive = new bool[2];

        // For torque percent calc (need both to compute percent)
        private readonly double[] _drivTrqNm = new double[2];
        private readonly double[] _maxTrqNm = new double[2];

        public override GearState GearState
        {
            get
            {
                // If handle down but not fully deployed -> transit.
                if (_gearHandleDown)
                    return (_gearAvgDeploy >= 0.99) ? GearState.Down : GearState.Transit;

                // If handle up but not fully retracted -> transit.
                return (_gearAvgDeploy <= 0.01) ? GearState.Up : GearState.Transit;
            }
        }

        public override double SpoilersPercent => _spoilersPct;
        public override bool SpoilersArmed => _spoilersArmed;

        public override bool ParkingBrakeOn => _parkingBrake;

        public override double FuelRemainingGallons => _fuelRemainingGallons;
        public override double FuelCapacityGallons => _fuelCapacityGallons;

        public override double Engine1OilPressurePsf => _oilPressurePsf[0];
        public override double Engine2OilPressurePsf => _oilPressurePsf[1];
        public override double Engine3OilPressurePsf => _oilPressurePsf[2];
        public override double Engine4OilPressurePsf => _oilPressurePsf[3];

        public override bool NavLightsOn => _navLights;
        public override bool BeaconLightsOn => _beaconLights;
        public override bool TaxiLightsOn => _taxiLights;
        public override bool LandingLightsOn => _landingLights;
        public override bool StrobeLightsOn => _strobeLights;
        public override bool CabinLightsOn => _cabinLights;
        public override bool PanelLightsOn => _panelLights;

        public override bool WindowDefrostOn => _windowDefrost;

        public override double CowlFlaps1Percent => _cowlFlapsPct[0];
        public override double CowlFlaps2Percent => _cowlFlapsPct[1];
        public override double CowlFlaps3Percent => _cowlFlapsPct[2];
        public override double CowlFlaps4Percent => _cowlFlapsPct[3];

        public override double Throttle1Percent => _throttlePct[0];
        public override double Throttle2Percent => _throttlePct[1];
        public override double Throttle3Percent => _throttlePct[2];
        public override double Throttle4Percent => _throttlePct[3];

        public override double Mixture1Percent => _mixturePct[0];
        public override double Mixture2Percent => _mixturePct[1];
        public override double Mixture3Percent => _mixturePct[2];
        public override double Mixture4Percent => _mixturePct[3];

        public override double PropPitch1Degrees => _propPitchDeg[0];
        public override double PropPitch2Degrees => _propPitchDeg[1];
        public override double PropPitch3Degrees => _propPitchDeg[2];
        public override double PropPitch4Degrees => _propPitchDeg[3];

        public override bool Engine1Failed => _engFailed[0];
        public override bool Engine2Failed => _engFailed[1];
        public override bool Engine3Failed => _engFailed[2];
        public override bool Engine4Failed => _engFailed[3];

        public override int EngineCount => _engineCount;

        public override bool Afterburner1On => _afterburnerOn[0];
        public override bool Afterburner2On => _afterburnerOn[1];
        public override bool Afterburner3On => _afterburnerOn[2];
        public override bool Afterburner4On => _afterburnerOn[3];

        public override bool Engine1Running => _engRunning[0];
        public override bool Engine2Running => _engRunning[1];
        public override bool Engine3Running => _engRunning[2];
        public override bool Engine4Running => _engRunning[3];

        public override double Engine1N1Percent => _n1Pct[0];
        public override double Engine2N1Percent => _n1Pct[1];
        public override double Engine3N1Percent => _n1Pct[2];
        public override double Engine4N1Percent => _n1Pct[3];

        public override double Engine1ManifoldPressureInchesMercury => _manifoldInHg[0];
        public override double Engine2ManifoldPressureInchesMercury => _manifoldInHg[1];
        public override double Engine3ManifoldPressureInchesMercury => _manifoldInHg[2];
        public override double Engine4ManifoldPressureInchesMercury => _manifoldInHg[3];

        private int _aircraftId = 0;
        public override int AircraftId => _aircraftId;

        private string _aircraftName = string.Empty;
        public override string AircraftName => _aircraftName;

        public double _altitudeMSLFeet;
        public override double AltitudeMSLFeet => _altitudeMSLFeet;

        public double _altitudeAGLFeet;
        public override double AltitudeAGLFeet => _altitudeAGLFeet;

        private double _headingMagneticDegrees;
        public override double HeadingMagneticDegrees => _headingMagneticDegrees;

        private double _headingTrueDegrees;
        public override double HeadingTrueDegrees => _headingTrueDegrees;

        public override double HeadingMagneticRadians => _headingMagneticDegrees * (Math.PI / 180);

        public override double HeadingTrueRadians => _headingTrueDegrees * (Math.PI / 180);

        private bool _isConnected;
        public override bool IsConnected => _isConnected;

        private string _atcIdentifier = null!;
        public override string ATCIdentifier => _atcIdentifier;

        private string _atcModel = null!;
        public override string ATCModel => _atcModel;

        private string _atcType = null!;
        public override string ATCType => _atcType;

        private bool _isHeavy;
        public override bool IsHeavy => _isHeavy;

        private bool _isGearFloats;
        public override bool IsGearFloats => _isGearFloats;

        private bool _isHelo;
        public override bool IsHelo => _isHelo;

        private EngineType _engineType;
        public override EngineType EngineType => _engineType;

        private bool _onGround;
        public override bool OnGround => _onGround;

        private double _groundSpeedKnots;
        public override double GroundSpeedKnots => _groundSpeedKnots;

        private double _airspeedIndicatedKnots;
        public override double AirSpeedIndicatedKnots => _airspeedIndicatedKnots;

        private double _airspeedTrueKnots;
        public override double AirSpeedTrueKnots => _airspeedTrueKnots;

        private double _ambientTemperatureCelsius;
        public override double AmbientTemperatureCelsius => _ambientTemperatureCelsius;

        private double _ambientWindDirectionDegrees;
        public override double AmbientWindDirectionDegrees => _ambientWindDirectionDegrees;

        private double _ambientWindSpeedKnots;
        public override double AmbientWindSpeedKnots => _ambientWindSpeedKnots;

        private double _kollsmanInchesMercury = 29.92;
        public override double KollsmanInchesMercury => _kollsmanInchesMercury;

        private double _secondaryKollsmanInchesMercury = 29.92;
        public override double SecondaryKollsmanInchesMercury => _secondaryKollsmanInchesMercury;

        private double _pressureInchesMercury = 29.92;
        public override double PressureInchesMercury => _pressureInchesMercury;

        private ReadyToFly _isReadyToFly;
        public override ReadyToFly IsReadyToFly => _isReadyToFly;

        private double _gpsRequiredMagneticHeadingRadians;
        public override double GPSRequiredMagneticHeadingRadians => _gpsRequiredMagneticHeadingRadians;

        private double _gpsRequiredTrueHeadingRadians;
        public override double GPSRequiredTrueHeadingRadians => _gpsRequiredTrueHeadingRadians;

        private bool _hasActiveWaypoint;
        public override bool HasActiveWaypoint => _hasActiveWaypoint;

        private double _gpsCrossTrackErrorMeters;
        public override double GPSCrossTrackErrorMeters => _gpsCrossTrackErrorMeters;

        private double _nav1Radial;
        public override double Nav1Radial => _nav1Radial;

        private double _nav2Radial;
        public override double Nav2Radial => _nav2Radial;

        public override bool Nav1Available => Nav1Frequency != 0;

        public override bool Nav2Available => Nav2Frequency != 0;

        private bool _nav1Sound;
        public override bool Nav1Receive => _nav1Sound;

        private bool _nav2Sound;
        public override bool Nav2Receive => _nav2Sound;

        private double _adfRelativeBearing;
        public override double AdfRelativeBearing => _adfRelativeBearing;

        public double _altitudeTrueFeet;
        public override double AltitudeTrueFeet => _altitudeTrueFeet;

        private bool _batteryOn;
        public override bool BatteryOn => _batteryOn;

        private bool _avionicsOn;
        public override bool AvionicsOn => _avionicsOn;

        private bool _generatorOn;
        public override bool GeneratorOn => _generatorOn;

        private bool _pitotHeatOn;
        public override bool PitotHeatOn => _pitotHeatOn;

        private uint _transponder;
        public override uint Transponder => _transponder;

        private uint _transponderMode;
        public override TransponderMode TransponderMode => MapFromXPlane((XPlaneTransponderMode)_transponderMode);

        private bool _identActive;
        public override bool IdentActive => _identActive;

        private bool _com1Power;
        private Failures _com1Failure;
        public override ComStatus Com1Status => !_com1Power ? ComStatus.NoElectricity : (_com1Failure == Failures.Inoperative ? ComStatus.Failed : ComStatus.OK);

        private bool _com2Power;
        private Failures _com2Failure;
        public override ComStatus Com2Status => !_com2Power ? ComStatus.NoElectricity : (_com2Failure == Failures.Inoperative ? ComStatus.Failed : ComStatus.OK);

        private bool _com1Receive;
        public override bool Com1Receive => _com1Receive;

        private bool _com2Receive;
        public override bool Com2Receive => _com2Receive;

        private bool _com1Transmit;
        public override bool Com1Transmit => _com1Transmit;

        private bool _com2Transmit;
        public override bool Com2Transmit => _com2Transmit;

        private TunedFacility _com1ActiveFrequencyType;
        public override TunedFacility Com1ActiveFrequencyType => _com1ActiveFrequencyType;

        private TunedFacility _com2ActiveFrequencyType;
        public override TunedFacility Com2ActiveFrequencyType => _com2ActiveFrequencyType;

        private double _com1Frequency;
        public override double Com1Frequency => _com1Frequency;

        private double _com2Frequency;
        public override double Com2Frequency => _com2Frequency;

        private double _com1StandByFrequency;
        public override double Com1StandByFrequency => _com1StandByFrequency;

        private double _com2StandByFrequency;
        public override double Com2StandByFrequency => _com2StandByFrequency;

        private AbortableTaskRunner? _timerConnection = null;
        private bool initialized = false;
        private bool stop = false;
        private DateTime? _lastReceiveTime;

        private static TransponderMode MapFromXPlane(XPlaneTransponderMode xp)
        {
            return xp switch
            {
                XPlaneTransponderMode.Off => TransponderMode.Off,

                XPlaneTransponderMode.Standby => TransponderMode.Standby,

                XPlaneTransponderMode.Test => TransponderMode.Test,

                XPlaneTransponderMode.On_Mode_A => TransponderMode.On_Mode_A,

                XPlaneTransponderMode.Alt_Mode_C => TransponderMode.Alt_Mode_C,

                XPlaneTransponderMode.Ground_Mode_S => TransponderMode.Ground_Mode_S,

                // Mode S with traffic still means ALT for pilot-facing displays
                XPlaneTransponderMode.TA_Only_Mode_S => TransponderMode.Alt_Mode_C,
                XPlaneTransponderMode.TA_RA_Mode_S => TransponderMode.Alt_Mode_C,

                _ => TransponderMode.Standby
            };
        }

        public string XPlaneListenerIPAddress 
        { 
            get
            {
                return connector.XPlaneEP.Address.ToString();
            }
            set
            {
                if (connector != null && !connector.XPlaneEP.Address.ToString().Equals(value))
                {
                    connector = new XPlaneConnector(value, XPlaneListenerPort);
                    connector.OnDataRefUpdated += Connector_OnDataRefUpdated;
                    connector.OnRawReceive += Connector_OnRawReceive;
                    connector.Start();
                }
            }
        }
        public int XPlaneListenerPort
        {
            get
            {
                return connector.XPlaneEP.Port;
            }
            set
            {
                if (connector != null && connector.XPlaneEP.Port != value)
                {
                    connector = new XPlaneConnector(XPlaneListenerIPAddress, value);
                    connector.OnDataRefUpdated += Connector_OnDataRefUpdated;
                    connector.OnRawReceive += Connector_OnRawReceive;
                    connector.OnConnectionChanged += Connector_OnConnectionChanged;
                    connector.Start();
                }
            }
        }

        public override double CollectivePercent => _collectivePct;

        public override bool RotorBrakeActive => _rotorBrakeActive;

        public override double RotorBrakeHandlePercent => _rotorBrakeHandlePct;

        public override double MainRotorDiskBankPercent => _mainDiskBankPct;
        public override double MainRotorDiskPitchPercent => _mainDiskPitchPct;
        public override double MainRotorDiskConingPercent => _mainDiskConingPct;
        public override double MainRotorRotationAngleDegrees => _mainRotorRotationDeg;

        public override double TailRotorDiskBankPercent => _tailDiskBankPct;
        public override double TailRotorDiskPitchPercent => _tailDiskPitchPct;
        public override double TailRotorDiskConingPercent => _tailDiskConingPct;
        public override double TailRotorRotationAngleDegrees => _tailRotorRotationDeg;

        public override double MainRotorRpm => _mainRotorRpm;
        public override double TailRotorRpm => _tailRotorRpm;

        public override double MainRotorBladePitchPercent => _mainBladePitchPct;
        public override double TailRotorBladePitchPercent => _tailBladePitchPct;

        public override double RotorLateralTrimPercent => _rotorTrimPct;
        public override double RotorLongitudinalTrimPercent => 0; // no true separate long-trim dataref we can trust

        public override double Engine1RotorRpmCommandPercent => _engineRotorCmdPct[0];
        public override double Engine2RotorRpmCommandPercent => _engineRotorCmdPct[1];

        public override double Engine1TorquePercent => _engineTorquePct[0];
        public override double Engine2TorquePercent => _engineTorquePct[1];

        public override bool RotorGovernor1Active => _rotorGovernorActive[0];
        public override bool RotorGovernor2Active => _rotorGovernorActive[1];

        public override double Nav1DmeDistanceNm => _nav1Dme;
        public override double Nav2DmeDistanceNm => _nav2Dme;
        public override double Nav1DmeSpeedKts => _nav1DmeSpeed;
        public override double Nav2DmeSpeedKts => _nav2DmeSpeed;

        public override bool GpsDrivesNav => _apGpsDrivesNav;

        public override bool FlightDirectorActive => _apFlightDirectorMode != 0;

        public override bool AutopilotVerticalSpeedHold => _apVsHold;

        public override double AutopilotVerticalSpeedTargetFpm => _apVsTargetFm;

        public override bool FlightPlanIsActiveFlightPlan => false;

        public override int FlightPlanWaypointsNumber => 0;

        public override int FlightPlanActiveWaypoint => 0;

        public override bool FlightPlanIsDirectTo => false;

        public override string FlightPlanApproachIdent => string.Empty;

        public override int FlightPlanWaypointIndex => 0;

        public override string FlightPlanWaypointIdent => string.Empty;

        public override int FlightPlanApproachWaypointsNumber => 0;

        public override int FlightPlanActiveApproachWaypoint => 0;

        public override bool FlightPlanApproachIsWaypointRunway => false;

        private void Connector_OnConnectionChanged(bool connected)
        {
            if (connected)
            {
                Connected();
            }
            else
            { 
                Quit();
            }
        }

        static XPlaneProvider()
        {
            Instance = new XPlaneProvider();
        }

        XPlaneProvider(string ip = "127.0.0.1", int xplanePort = 49000)
        {
            UpdateConnection(ip, xplanePort);
            Initialize();
        }

        public void UpdateConnection(string ip, int xplanePort)
        {
            connector = new XPlaneConnector(ip, xplanePort);
            connector.OnDataRefUpdated += Connector_OnDataRefUpdated;
            connector.OnRawReceive += Connector_OnRawReceive;
            connector.Start();
        }

        private void Connector_OnRawReceive(string raw)
        {
            _lastReceiveTime = DateTime.Now;
        }

        bool _isSendingFlightData;
        private void Connector_OnDataRefUpdated(List<DataRefElement> dataRefs)
        {
            _lastReceiveTime = DateTime.Now;
            if (dataRefs.Count > 0)
            {
                if (!_isSendingFlightData)
                {
                    _isSendingFlightData = true;
                    FlightDataReceived();
                    _isSendingFlightData = false;
                }
            }
            if (_isReadyToFly == FlightSim.ReadyToFly.Loading)
            {
                _isReadyToFly = FlightSim.ReadyToFly.Ready;
                ReadyToFly(IsReadyToFly);
            }
        }

        private void Initialize()
        {
            if (!initialized)
            {
                initialized = true;
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    Crossref = XPlaneCrossref.GetXPlaneCrossref();
                    if (!string.IsNullOrEmpty(XPlaneAircraftName))
                    {
                        LoadAircraft(XPlaneAircraftName);
                    }
                });
                if (_timerConnection == null)
                {
                    _timerConnection = new AbortableTaskRunner();
                    _timerConnection.Start(timerConnection_DoWorkAsync);
                }
            }
        }

        public override void Deinitialize(int timeOut = 1000)
        {
            StopTimerConnectionAsync(timeOut).GetAwaiter().GetResult();
        }

        public async Task StopTimerConnectionAsync(int timeOut = 1000)
        {
            stop = true;
            if (_timerConnection != null && _timerConnection.IsRunning)
            {
                await _timerConnection.StopAsync(timeOut).ConfigureAwait(false);
            }
            _timerConnection?.Dispose();
            _timerConnection = null;
        }

        private async Task timerConnection_DoWorkAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested && !stop)
            {
                try
                {
                    if (!connector.IsConnected && IsConnected)
                    {
                        //connector.Stop();
                        SetLoading();
                        _lastReceiveTime = null;
                        _isConnected = false;
                        Quit();
                    }
                    else if (!connector.IsConnected && !IsConnected)
                    {
                        _isConnected = connector.IsConnected;
                        if (IsConnected)
                        {
                            Connected();
                            SetLoading();
                            Subscribe();
                        }
                    }
                    else if (connector.IsConnected && !IsConnected)
                    {
                        _isConnected = true;
                        Connected();
                        SetLoading();
                        Subscribe();
                    }
                    else if (connector.IsConnected && IsConnected && _lastReceiveTime.HasValue && (DateTime.Now - _lastReceiveTime.Value) > connector.MaxDataRefAge)
                    {
                        // Are we loading a new flight?
                        SetLoading();
                    }
                    else if (connector.IsConnected && IsConnected && _isReadyToFly == FlightSim.ReadyToFly.Loading && !string.IsNullOrEmpty(XPlaneAircraftName))
                    {
                        _lastReceiveTime = DateTime.Now;
                        _isReadyToFly = FlightSim.ReadyToFly.Ready;
                        ReadyToFly(IsReadyToFly);
                    }
                }
                catch (Exception)
                {
                }
                finally
                {
                    await Task.Delay(1000, token);
                }
            }
        }

        private void SetLoading()
        {
            _aircraftId = 0;
            _atcIdentifier = string.Empty;
            _aircraftName = string.Empty;
            XPlaneAircraftName = string.Empty;
            XPlaneAircraftId = Guid.Empty;
            XPlaneTailnumber = string.Empty;
            StringDataRefElement? acfName = connector != null ? connector.StringDataRefs.FirstOrDefault(d => d.DataRef.Equals(XPlaneStructs.DataRefs.DataRefList[DataRefId.AircraftViewAcfUiName].DataRef, StringComparison.OrdinalIgnoreCase)) : null;
            if (acfName != null)
            {
                acfName.Value = string.Empty;
                acfName.ForceUpdate = true;
            }
            StringDataRefElement? tailnumber = connector != null ? connector.StringDataRefs.FirstOrDefault(d => d.DataRef.Equals(XPlaneStructs.DataRefs.DataRefList[DataRefId.AircraftViewAcfTailnum].DataRef, StringComparison.OrdinalIgnoreCase)) : null;
            if (tailnumber != null)
            {
                tailnumber.Value = string.Empty;
                tailnumber.ForceUpdate = true;
            }
            StringDataRefElement? acfAcraftId = connector != null ? connector.StringDataRefs.FirstOrDefault(d => d.DataRef.Equals(XPlaneStructs.DataRefs.DataRefList[DataRefId.AircraftViewAcfModesId].DataRef, StringComparison.OrdinalIgnoreCase)) : null;
            if (acfAcraftId != null)
            {
                acfAcraftId.Value = string.Empty;
                acfAcraftId.ForceUpdate = true;
            }
            if (_isReadyToFly != FlightSim.ReadyToFly.Loading)
            {
                _isReadyToFly = FlightSim.ReadyToFly.Loading;
                ReadyToFly(IsReadyToFly);
            }
        }

        public void Dispose()
        {
            if (connector.IsConnected)
            {
                Unsubscribe();
            }
        }

        private void SubscribeEngineArray(DataRefId id, Action<int, float> set, int count = 4)
        {
            var baseDr = XPlaneStructs.DataRefs.DataRefList[id] as DataRefElement 
                ?? throw new InvalidOperationException($"DataRef {id} not found or wrong type.");

            for (int i = 0; i < count; i++)
            {
                // clone with Index set (don’t mutate the shared DataRefList instance)
                var dr = new DataRefElement
                {
                    DataRef = baseDr.DataRef,
                    Units = baseDr.Units,
                    Description = baseDr.Description,
                    Frequency = baseDr.Frequency,
                    Name = baseDr.Name,
                    Id = baseDr.Id,
                    Writable = baseDr.Writable,
                    DataType = baseDr.DataType,
                    Index = i
                };

                connector.Subscribe(dr, Frequency, (element, value) => set(i, value));
            }
        }

        private TimeSpan? _flightTime = null;
        private int _subSinglesRunning = 0;

        private void Subscribe()
        {
            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.TimeTotalFlightTimeSec], 1, (element, value) =>
            {
                TimeSpan flightTime = TimeSpan.FromSeconds(value);
                Debug.WriteLine($"Total Flight Time: {flightTime.ToReadableString()}");

                bool isFirst = !_flightTime.HasValue;

                // Require a meaningful drop (avoid tiny jitter)
                bool dropped = _flightTime.HasValue && (flightTime + TimeSpan.FromSeconds(1)) < _flightTime.Value;

                if (isFirst || dropped)
                {
                    if (Interlocked.Exchange(ref _subSinglesRunning, 1) == 0)
                    {
                        try 
                        { 
                            SubscribeToSingles();
                            if (!_isConnected)
                            {
                                _isConnected = true;
                                Connected();
                            }
                            _isReadyToFly = FlightSim.ReadyToFly.Ready;
                            ReadyToFly(IsReadyToFly);
                        }
                        finally 
                        { 
                            Interlocked.Exchange(ref _subSinglesRunning, 0); 
                        }
                    }
                }
                _flightTime = flightTime;
            });


            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.FlightmodelPositionLatitude], Frequency, (element, value) =>
            {
                Latitude = value;
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.FlightmodelPositionLongitude], Frequency, (element, value) =>
            {
                Longitude = value;
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.FlightmodelPositionIndicatedAirspeed], Frequency, (element, value) =>
            {
                _airspeedIndicatedKnots = value;
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.FlightmodelPositionTrueAirspeed], Frequency, (element, value) =>
            {
                _airspeedTrueKnots = value;
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.CockpitMiscBarometerSetting], Frequency, (element, value) =>
            {
                _kollsmanInchesMercury = value;
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.CockpitMiscBarometerSetting2], Frequency, (element, value) =>
            {
                _secondaryKollsmanInchesMercury = value;
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.WeatherBarometerCurrentInhg], Frequency, (element, value) =>
            {
                _pressureInchesMercury = value;
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.WeatherWindSpeedKt], Frequency, (element, value) =>
            {
                _ambientWindSpeedKnots = value;
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.WeatherWindDirectionDegt], Frequency, (element, value) =>
            {
                _ambientWindDirectionDegrees = Tools.NormalizeDegrees(value);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Flightmodel2GearOnGround], Frequency, (element, value) =>
            {
                _onGround = Convert.ToBoolean(value);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.FlightmodelPositionGroundspeed], Frequency, (element, value) =>
            {
                _groundSpeedKnots = value;
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.FlightmodelPositionTruePsi], Frequency, (element, value) =>
            {
                _headingTrueDegrees = Tools.NormalizeDegrees(value);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.FlightmodelPositionMagPsi], Frequency, (element, value) =>
            {
                _headingMagneticDegrees = Tools.NormalizeDegrees(value);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.WeatherTemperatureAmbientC], Frequency, (element, value) =>
            {
                _ambientTemperatureCelsius = value;
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.CockpitRadiosNav1DirDegt], Frequency, (element, value) =>
            {
                _nav1Radial = Tools.NormalizeDegrees(value);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.CockpitRadiosNav2DirDegt], Frequency, (element, value) =>
            {
                _nav2Radial = Tools.NormalizeDegrees(value);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.CockpitRadiosAdf1DirDegt], Frequency, (element, value) =>
            {
                _adfRelativeBearing = Tools.NormalizeDegrees(value);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.CockpitAutopilotHeading], Frequency, (element, value) =>
            {
                _apHdgButTargetDeg = Tools.NormalizeDegrees(value);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Cockpit2RadiosIndicatorsGpsRelativeBearingDeg], Frequency, (element, value) =>
            {
                _gpsRequiredTrueHeadingRadians = Tools.DegToRad(value);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Cockpit2RadiosIndicatorsGpsBearingDegMag], Frequency, (element, value) =>
            {
                _gpsRequiredMagneticHeadingRadians = Tools.DegToRad(value);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Cockpit2RadiosIndicatorsGpsBearingError], Frequency, (element, value) =>
            {
                _gpsCrossTrackErrorMeters = value;
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.CockpitGpsDestinationIndex], Frequency, (element, value) =>
            {
                _hasActiveWaypoint = value != -1;
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Cockpit2GaugesIndicatorsAltitudeFtPilot], Frequency, (element, value) =>
            {
                _altitudeMSLFeet = value;
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.FlightmodelPositionYAgl], Frequency, (element, value) =>
            {
                _altitudeAGLFeet = value;
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Flightmodel2PositionPressureAltitude], Frequency, (element, value) =>
            {
                _altitudeTrueFeet = value;
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.CockpitRadiosTransponderCode], Frequency, (element, value) =>
            {
                _transponder = Tools.Bcd2Dec((uint)Convert.ToInt16(value));
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.CockpitRadiosTransponderMode], Frequency, (element, value) =>
            {
                _transponderMode = (uint)Convert.ToInt16(value);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Cockpit2RadiosIndicatorsTransponderId], Frequency, (element, value) =>
            {
                _identActive = Convert.ToBoolean(value);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.CockpitElectricalBatteryOn], Frequency, (element, value) =>
            {
                _batteryOn = Convert.ToBoolean(value);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.CockpitElectricalAvionicsOn], Frequency, (element, value) =>
            {
                _avionicsOn = Convert.ToBoolean(value);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.CockpitElectricalGeneratorOn], Frequency, (element, value) =>
            {
                _generatorOn = Convert.ToBoolean(value);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.CockpitSwitchesPitotHeatOn], Frequency, (element, value) =>
            {
                _pitotHeatOn = Convert.ToBoolean(value);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.AtcCom1Active], Frequency, (element, value) =>
            {
                _com1Active = Convert.ToBoolean(value);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.AtcCom2Active], Frequency, (element, value) =>
            {
                _com2Active = Convert.ToBoolean(value);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Cockpit2RadiosActuatorsCom1Power], Frequency, (element, value) =>
            {
                _com1Power = Convert.ToBoolean(value);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Cockpit2RadiosActuatorsCom2Power], Frequency, (element, value) =>
            {
                _com2Power = Convert.ToBoolean(value);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.OperationFailuresRelCom1], Frequency, (element, value) =>
            {
                _com1Failure = Enum.IsDefined(typeof(Failures), value) ? (Failures)value : Failures.AlwaysWorking;
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.OperationFailuresRelCom2], Frequency, (element, value) =>
            {
                _com2Failure = Enum.IsDefined(typeof(Failures), value) ? (Failures)value : Failures.AlwaysWorking;
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.AtcCom1Rx], Frequency, (element, value) =>
            {
                _com1Receive = Convert.ToBoolean(value);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.AtcCom2Rx], Frequency, (element, value) =>
            {
                _com2Receive = Convert.ToBoolean(value);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.AtcCom1Tx], Frequency, (element, value) =>
            {
                _com1Transmit = Convert.ToBoolean(value);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.AtcCom2Tx], Frequency, (element, value) =>
            {
                _com2Transmit = Convert.ToBoolean(value);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Cockpit2RadiosActuatorsAudioSelectionNav1], Frequency, (element, value) =>
            {
                _nav1Sound = Convert.ToBoolean(value);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Cockpit2RadiosActuatorsAudioSelectionNav2], Frequency, (element, value) =>
            {
                _nav2Sound = Convert.ToBoolean(value);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Cockpit2RadiosIndicatorsNav1DmeDistanceNm], Frequency, (element, value) =>
            {
                _nav1Dme = value;
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Cockpit2RadiosIndicatorsNav2DmeDistanceNm], Frequency, (element, value) =>
            {
                _nav2Dme = value;
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Cockpit2RadiosIndicatorsNav1DmeSpeedKts], Frequency, (element, value) => 
            { 
                _nav1DmeSpeed = value; 
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Cockpit2RadiosIndicatorsNav2DmeSpeedKts], Frequency, (element, value) => 
            { 
                _nav2DmeSpeed = value; 
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.CockpitRadiosNav1FreqHz], Frequency, (element, value) =>
            {
                _nav1Frequency = value / 100.0d;
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.CockpitRadiosNav2FreqHz], Frequency, (element, value) =>
            {
                _nav2Frequency = value / 100.0d;
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.CockpitRadiosNav1StdbyFreqHz], Frequency, (element, value) =>
            {
                _nav1StandByFrequency = value / 100.0d;
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.CockpitRadiosNav2StdbyFreqHz], Frequency, (element, value) =>
            {
                _nav2StandByFrequency = value / 100.0d;
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.AtcCom1TunedFacility], Frequency, (element, value) =>
            {
                _com1ActiveFrequencyType = Enum.IsDefined(typeof(TunedFacility), value) ? (TunedFacility)value : TunedFacility.NONE;
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.AtcCom2TunedFacility], Frequency, (element, value) =>
            {
                _com2ActiveFrequencyType = Enum.IsDefined(typeof(TunedFacility), value) ? (TunedFacility)value : TunedFacility.NONE;
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Cockpit2RadiosActuatorsCom1FrequencyHz833], Frequency, (element, value) =>
            {
                _com1Frequency = value / 1000.0d;
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Cockpit2RadiosActuatorsCom2FrequencyHz833], Frequency, (element, value) =>
            {
                _com2Frequency = value / 1000.0d;
            });
            
            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Cockpit2RadiosActuatorsCom1StandbyFrequencyHz833], Frequency, (element, value) =>
            {
                _com1StandByFrequency = value / 1000.0d;
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Cockpit2RadiosActuatorsCom2StandbyFrequencyHz833], Frequency, (element, value) =>
            {
                _com2StandByFrequency = value / 1000.0d;
            });




            // ---------------------------
            // NEW: String idents (byte[150] StringDataRefElement datarefs)
            // ---------------------------

            var drAdfIdent = XPlaneStructs.DataRefs.DataRefList[DataRefId.Cockpit2RadiosIndicatorsAdf1NavId] as StringDataRefElement
                ?? throw new InvalidOperationException("DataRef Cockpit2RadiosIndicatorsAdf1NavId not found or wrong type.");

            connector.Subscribe(drAdfIdent, 1,
                (element, value) => { _adfIdent = value; },
                (element, value) => { _adfIdent = value; });

            var drWpIdent = XPlaneStructs.DataRefs.DataRefList[DataRefId.Cockpit2RadiosIndicatorsGpsNavId] as StringDataRefElement
                ?? throw new InvalidOperationException("DataRef Cockpit2RadiosIndicatorsGpsNavId not found or wrong type.");

            connector.Subscribe(drWpIdent, 1,
                (element, value) => { _wpIdent = value; },
                (element, value) => { _wpIdent = value; });

            var drNav1Ident = XPlaneStructs.DataRefs.DataRefList[DataRefId.Cockpit2RadiosIndicatorsNav1NavId] as StringDataRefElement
                ?? throw new InvalidOperationException("DataRef Cockpit2RadiosIndicatorsNav1NavId not found or wrong type.");

            connector.Subscribe(drNav1Ident, 1,
                (element, value) => { _nav1Ident = value; },
                (element, value) => { _nav1Ident = value; });

            var drNav2Ident = XPlaneStructs.DataRefs.DataRefList[DataRefId.Cockpit2RadiosIndicatorsNav2NavId] as StringDataRefElement
                ?? throw new InvalidOperationException("DataRef Cockpit2RadiosIndicatorsNav2NavId not found or wrong type.");

            connector.Subscribe(drNav2Ident, 1,
                (element, value) => { _nav2Ident = value; },
                (element, value) => { _nav2Ident = value; });


            // ---------------------------
            // NEW: NAV OBS courses (AutopilotNav1CourseDegrees / AutopilotNav2CourseDegrees)
            // ---------------------------

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.CockpitRadiosNav1ObsDegt], Frequency, (element, value) =>
            {
                _apNav1CourseDeg = Tools.NormalizeDegrees(value);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.CockpitRadiosNav2ObsDegt], Frequency, (element, value) =>
            {
                _apNav2CourseDeg = Tools.NormalizeDegrees(value);
            });


            // ---------------------------
            // NEW: Engine count
            // ---------------------------

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.AircraftEngineAcfNumEngines], 1, (element, value) =>
            {
                _engineCount = Math.Max(1, Math.Min(4, (int)value));
            });


            // ---------------------------
            // NEW: Fuel pumps (per engine), prop de-ice (prop heat), carb heat (ratio)
            // ---------------------------

            SubscribeEngineArray(DataRefId.Cockpit2EngineActuatorsFuelPumpOn, (i, v) =>
            {
                if (i < 4) _fuelPumpOn[i] = v > 0.5f;
            });

            SubscribeEngineArray(DataRefId.Cockpit2IceIcePropHeatOn, (i, v) =>
            {
                if (i < 4) _propDeiceOn[i] = v > 0.5f;
            });

            SubscribeEngineArray(DataRefId.Cockpit2IceIceInletHeatOnPerEngine, (i, v) =>
            {
                if (i < 4) _engineInletHeatOn[i] = v > 0.5f;
            });

            // ---------------------------
            // NEW: Engine RPM, torque, oil pressure
            //   - torque is N·m -> ft·lb
            //   - oil pressure is PSI -> PSF
            // ---------------------------

            SubscribeEngineArray(DataRefId.Cockpit2EngineIndicatorsEngineSpeedRpm, (i, v) =>
            {
                if (i < 4) _engineRpm[i] = v;
            });

            SubscribeEngineArray(DataRefId.Cockpit2EngineIndicatorsTorqueNMtr, (i, v) =>
            {
                const double NmToFtLb = 0.737562149;
                if (i < 4) _torqueFtLb[i] = v * NmToFtLb;
            });

            SubscribeEngineArray(DataRefId.Cockpit2EngineIndicatorsOilPressurePsi, (i, v) =>
            {
                if (i < 4) _oilPressurePsf[i] = v * 144.0;
            });


            // ---------------------------
            // NEW: Vertical speed, pitch, bank
            // ---------------------------

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Cockpit2GaugesIndicatorsVviFpmPilot], Frequency, (element, value) =>
            {
                _verticalSpeedFpm = value;
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.FlightmodelPositionTheta], Frequency, (element, value) =>
            {
                _pitchDeg = value;
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.FlightmodelPositionPhi], Frequency, (element, value) =>
            {
                _bankDeg = value;
            });


            // ---------------------------
            // NEW: Autopilot master/modes + targets
            // ---------------------------

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Cockpit2AutopilotAutopilotOn], Frequency, (element, value) =>
            {
                _apMaster = Convert.ToBoolean(value);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.CockpitAutopilotAutopilotMode], Frequency, (element, value) =>
            {
                _apFlightDirectorMode = Convert.ToInt32(value);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Cockpit2RadiosActuatorsHSISourceSelectPilot], Frequency, (element, value) =>
            {
                int src = Convert.ToInt32(value);
                _apGpsDrivesNav = (src == 2 || src == 3);   // GPS1 or GPS2
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.CockpitAutopilotHeadingMode], Frequency, (element, value) =>
            {
                int mode = Convert.ToInt32(value);
                _apHdgHold =
                    mode == 1   // heading select
                    || mode == 14; // heading hold

                _apNavHold =
                    mode == 2   // VOR / LOC
                    || mode == 13  // GPSS
                    || mode == 18; // track (optional, but defensible)
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.CockpitAutopilotAltitudeMode], Frequency, (element, value) =>
            {
                int mode = Convert.ToInt32(value);
                _apAltHold = mode == 6;
                _apVsHold = mode == 4;
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Cockpit2AutopilotAutothrottleEnabled], Frequency, (element, value) =>
            {
                int mode = Convert.ToInt32(value);
                _apSpdHold = mode == 1;
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.CockpitAutopilotVerticalVelocity], Frequency, (element, value) =>
            {
                _apVsTargetFm = value;
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.CockpitAutopilotAltitude], Frequency, (element, value) =>
            {
                _apAltTargetFt = value;
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.CockpitAutopilotAirspeed], Frequency, (element, value) =>
            {
                _apSpdTargetKts = value;
            });


            // ---------------------------
            // NEW: Trim percents (-1..1 -> -100..100)
            // ---------------------------

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.FlightmodelControlsElvTrim], Frequency, (element, value) =>
            {
                _elevatorTrimPercent = Math.Clamp(value, -1f, 1f) * 100.0;
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.FlightmodelControlsAilTrim], Frequency, (element, value) =>
            {
                _aileronTrimPercent = Math.Clamp(value, -1f, 1f) * 100.0;
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.FlightmodelControlsRudTrim], Frequency, (element, value) =>
            {
                _rudderTrimPercent = Math.Clamp(value, -1f, 1f) * 100.0;
            });


            // ---------------------------
            // NEW: Flaps, spoilers, parking brake
            // ---------------------------

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Cockpit2ControlsFlapRatio], Frequency, (element, value) =>
            {
                _flapsPct = Math.Clamp(value, 0f, 1f) * 100.0;
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Cockpit2ControlsSpeedbrakeRatio], Frequency, (element, value) =>
            {
                _spoilersPct = Math.Clamp(value, 0f, 1f) * 100.0;
                _spoilersArmed = _spoilersPct > 0.5;
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Cockpit2ControlsParkingBrakeRatio], Frequency, (element, value) =>
            {
                _parkingBrake = value > 0.5f;
            });


            // ---------------------------
            // NEW: Gear (handle + deploy ratio)
            // ---------------------------

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Cockpit2ControlsGearHandleDown], Frequency, (element, value) =>
            {
                _gearHandleDown = value > 0.5f;
            });

            // If you later want a better average, store an array and average all gear legs.
            // For now, index 0 is “good enough” for a simple Up/Down/Transit presentation.
            SubscribeEngineArray(DataRefId.Flightmodel2GearDeployRatio, (i, v) =>
            {
                if (i == 0) _gearAvgDeploy = Math.Clamp(v, 0f, 1f);
            }, count: 1);


            // ---------------------------
            // NEW: Fuel totals (kg -> gallons, using a pragmatic default)
            // ---------------------------

            const double KgPerGallon_AVGAS = 2.721554; // 6.0 lb/gal * 0.45359237 kg/lb

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.AircraftWeightAcfMFuelTot], 1, (element, value) =>
            {
                _fuelCapacityGallons = Math.Max(0, value / KgPerGallon_AVGAS);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.FlightmodelWeightMFuelTotal], Frequency, (element, value) =>
            {
                _fuelRemainingGallons = Math.Max(0, value / KgPerGallon_AVGAS);
            });


            // ---------------------------
            // NEW: Lights
            // ---------------------------

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.CockpitElectricalNavLightsOn], Frequency, (element, value) =>
            {
                _navLights = Convert.ToBoolean(value);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.CockpitElectricalBeaconLightsOn], Frequency, (element, value) =>
            {
                _beaconLights = Convert.ToBoolean(value);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.CockpitElectricalCockpitLightsOn], Frequency, (element, value) =>
            {
                _cabinLights = Convert.ToBoolean(value);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Cockpit2ElectricalPanelBrightnessRatio], Frequency, (element, value) =>
            {
                _panelLights = value > 0.01f;
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.CockpitElectricalTaxiLightOn], Frequency, (element, value) =>
            {
                _taxiLights = Convert.ToBoolean(value);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.CockpitElectricalLandingLightsOn], Frequency, (element, value) =>
            {
                _landingLights = Convert.ToBoolean(value);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.CockpitElectricalStrobeLightsOn], Frequency, (element, value) =>
            {
                _strobeLights = Convert.ToBoolean(value);
            });


            // ---------------------------
            // NEW: Window defrost / heat
            // ---------------------------

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.CockpitSwitchesAntiIceWindowHeat], Frequency, (element, value) =>
            {
                _windowDefrost = Convert.ToBoolean(value);
            });


            // ---------------------------
            // NEW: Cowl flaps (ratio per engine -> percent)
            // ---------------------------

            SubscribeEngineArray(DataRefId.Cockpit2EngineActuatorsCowlFlapRatio, (i, v) =>
            {
                if (i < 4) _cowlFlapsPct[i] = Math.Clamp(v, 0f, 1f) * 100.0;
            });


            // ---------------------------
            // NEW: Throttle / Mixture / Prop pitch (ratio per engine -> percent)
            // ---------------------------

            SubscribeEngineArray(DataRefId.Cockpit2EngineActuatorsThrottleRatio, (i, v) =>
            {
                if (i < 4) _throttlePct[i] = Math.Clamp(v, 0f, 1f) * 100.0;
            });

            SubscribeEngineArray(DataRefId.Cockpit2EngineActuatorsMixtureRatio, (i, v) =>
            {
                if (i < 4) _mixturePct[i] = Math.Clamp(v, 0f, 1f) * 100.0;
            });

            SubscribeEngineArray(DataRefId.Cockpit2EngineActuatorsPropPitchDeg, (i, v) =>
            {
                // If your list uses a different prop pitch ratio DataRefId, swap it here.
                // This one exists in a lot of exports, but if you have an actual prop_pitch_ratio id, use that instead.
                if (i < 4) _propPitchDeg[i] = Math.Clamp(v, 0f, 1f) * 100.0;
            });


            // ---------------------------
            // NEW: Engine failed / running / N1 / manifold / afterburner
            // ---------------------------

            SubscribeEngineArray(DataRefId.OperationFailuresRelEngfai0, (i, v) =>
            {
                if (i < 4) _engFailed[i] = v > 0.5f;
            });

            SubscribeEngineArray(DataRefId.Flightmodel2EnginesEngineIsBurningFuel, (i, v) =>
            {
                if (i < 4) _engRunning[i] = v > 0.5f;
            });

            SubscribeEngineArray(DataRefId.Cockpit2EngineIndicatorsN1Percent, (i, v) =>
            {
                if (i < 4) _n1Pct[i] = Math.Max(0, v);
            });

            SubscribeEngineArray(DataRefId.Cockpit2EngineIndicatorsMPRInHg, (i, v) =>
            {
                if (i < 4) _manifoldInHg[i] = Math.Max(0, v);
            });

            SubscribeEngineArray(DataRefId.Flightmodel2EnginesAfterburnerRatio, (i, v) =>
            {
                if (i < 4) _afterburnerOn[i] = v > 0.1f;
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Cockpit2RadiosIndicatorsGpsTargetLonDeg], Frequency, (element, value) =>
            {
                _wpLon = value;
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Cockpit2RadiosIndicatorsGpsTargetLatDeg], Frequency, (element, value) =>
            {
                _wpLat = value;
            });

            // ==========================
            // Helicopter – subscriptions
            // ==========================

            // Collective (ratio -> percent). Use index 0 as “main”.
            SubscribeEngineArray(DataRefId.Cockpit2EngineActuatorsPropRatio, (i, v) =>
            {
                if (i == 0) _collectivePct = Tools.Clamp(v * 100.0, 0.0, 100.0);
            }, count: 1);

            // Rotor brake switch
            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Cockpit2SwitchesRotorBrake], Frequency, (el, v) =>
            {
                _rotorBrakeActive = Convert.ToBoolean(v);
            });

            // Rotor brake “handle” proxy (strength ratio -> percent)
            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Cockpit2SwitchesRotorBrakeRatio], Frequency, (el, v) =>
            {
                _rotorBrakeHandlePct = Tools.Clamp(v * 100.0, 0.0, 100.0);
            });

            // Disk bank/pitch proxies from cyclic tilt (deg -> percent, +/-10deg => +/-100%)
            SubscribeEngineArray(DataRefId.Flightmodel2EnginesRotorCyclicAileronTiltDeg, (i, v) =>
            {
                double pct = Tools.DegToPct10(v);
                if (i == 0) _mainDiskBankPct = pct;
                else if (i == 1) _tailDiskBankPct = pct;
            }, count: 2);

            SubscribeEngineArray(DataRefId.Flightmodel2EnginesRotorCyclicElevatorTiltDeg, (i, v) =>
            {
                double pct = Tools.DegToPct10(v);
                if (i == 0) _mainDiskPitchPct = pct;
                else if (i == 1) _tailDiskPitchPct = pct;
            }, count: 2);

            // Coning proxy: prop_cone_angle_rad[] (rad -> deg -> percent-ish using +/-10deg scale)
            SubscribeEngineArray(DataRefId.Flightmodel2EnginesPropConeAngleRad, (i, v) =>
            {
                double deg = v * (180.0 / Math.PI);
                double pct = Tools.DegToPct10(deg);
                if (i == 0) _mainDiskConingPct = pct;
                else if (i == 1) _tailDiskConingPct = pct;
            }, count: 2);

            // Rotation angle (deg)
            SubscribeEngineArray(DataRefId.Flightmodel2EnginesPropRotationAngleDeg, (i, v) =>
            {
                double deg = Tools.NormalizeDegrees(v);
                if (i == 0) _mainRotorRotationDeg = deg;
                else if (i == 1) _tailRotorRotationDeg = deg;
            }, count: 2);

            // Rotor speed (rad/sec -> RPM)
            SubscribeEngineArray(DataRefId.Flightmodel2EnginesPropRotationSpeedRadSec, (i, v) =>
            {
                double rpm = Tools.Clamp(Tools.RadSecToRpm(v), 0.0, 99999.0);
                if (i == 0) _mainRotorRpm = rpm;
                else if (i == 1) _tailRotorRpm = rpm;
            }, count: 2);

            // Blade pitch proxy: actuator prop angle degrees (0..90 => 0..100)
            SubscribeEngineArray(DataRefId.Cockpit2EngineActuatorsPropAngleDegrees, (i, v) =>
            {
                double pct = Tools.DegToPct90(v);
                if (i == 0) _mainBladePitchPct = pct;
                else if (i == 1) _tailBladePitchPct = pct;
            }, count: 2);

            // Rotor trim (single) -> treat as lateral trim percent
            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Flightmodel2ControlsRotorTrim], Frequency, (el, v) =>
            {
                _rotorTrimPct = Tools.Clamp(v * 100.0, -100.0, 100.0);
            });

            // Governor active (per engine)
            SubscribeEngineArray(DataRefId.Cockpit2EngineActuatorsGovernorOn, (i, v) =>
            {
                if (i < 2) _rotorGovernorActive[i] = Convert.ToBoolean(v);
            }, count: 2);

            // Rotor RPM “command percent” proxy:
            // cmd rad/sec from cockpit2 actuator / actual rad/sec from flightmodel2
            // We’ll store cmdRadSec first, then compute percent when we have actual.
            double[] cmdRadSec = new double[2];
            double[] actRadSec = new double[2];

            SubscribeEngineArray(DataRefId.Cockpit2EngineActuatorsPropRotationSpeedRadSec, (i, v) =>
            {
                if (i < 2)
                {
                    cmdRadSec[i] = v;
                    _engineRotorCmdPct[i] = (actRadSec[i] > 0.0001) ? Tools.Clamp((cmdRadSec[i] / actRadSec[i]) * 100.0, 0.0, 999.0) : 0.0;
                }
            }, count: 2);

            SubscribeEngineArray(DataRefId.Flightmodel2EnginesPropRotationSpeedRadSec, (i, v) =>
            {
                if (i < 2)
                {
                    actRadSec[i] = v;
                    _engineRotorCmdPct[i] = (actRadSec[i] > 0.0001) ? Tools.Clamp((cmdRadSec[i] / actRadSec[i]) * 100.0, 0.0, 999.0) : 0.0;
                }
            }, count: 2);

            // Torque percent: driv_TRQ / max_TRQ (both N*m)
            SubscribeEngineArray(DataRefId.FlightmodelEnginePOINTDrivTRQ, (i, v) =>
            {
                if (i < 2)
                {
                    _drivTrqNm[i] = v;
                    RecalcTorquePct(i);
                }
            }, count: 2);

            SubscribeEngineArray(DataRefId.FlightmodelEnginePOINTMaxTRQ, (i, v) =>
            {
                if (i < 2)
                {
                    _maxTrqNm[i] = v;
                    RecalcTorquePct(i);
                }
            }, count: 2);
        }

        private void SubscribeToSingles()
        {
            //UnsubscribeSingles();
            //IsXPlaneAirline = false;
            //IsXPlaneHelo = false;
            //IsXPlaneGlider = false;
            //IsXPlaneMilitary = false;
            //IsXPlaneGeneralAviation = true;
            //XPlaneEngineType = XPLANE_ENGINETYPE.RecipCarb;
            //XPlaneAircraftId = Guid.Empty;
            //XPlaneTailnumber = string.Empty;
            VSpeed = VSpeed.DefaultVSpeed();
            //_isGearFloats = false;
            _aircraftId = 0;
            _aircraftName = VSpeed.AircraftName = string.Empty;
            _atcType = string.Empty;
            _atcModel = string.Empty;
            _engineType = EngineType.Piston;
            _isHeavy = false;
            _isHelo = false;
            AircraftChange(_aircraftId);

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Aircraft2MetadataIsAirliner], 1, (element, value) =>
            {
                IsXPlaneAirline = Convert.ToBoolean(value);
                //connector.Unsubscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Aircraft2MetadataIsAirliner]);
                if (IsXPlaneAirline && AircraftId == 0)
                {
                    _isHeavy = Convert.ToBoolean(value);
                }
                AircraftChange(_aircraftId);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Aircraft2MetadataIsSeaplane], 1, (element, value) =>
            {
                _isGearFloats = Convert.ToBoolean(value);
                //connector.Unsubscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Aircraft2MetadataIsSeaplane]);
                AircraftChange(_aircraftId);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Aircraft2MetadataIsHelicopter], 1, (element, value) =>
            {
                IsXPlaneHelo = Convert.ToBoolean(value);
                //connector.Unsubscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Aircraft2MetadataIsHelicopter]);
                _isHelo = IsXPlaneHelo;
                if (IsXPlaneHelo && AircraftId == 0)
                {
                    _engineType = EngineType.Helo;
                }
                AircraftChange(_aircraftId);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Aircraft2MetadataIsGlider], 1, (element, value) =>
            {
                IsXPlaneGlider = Convert.ToBoolean(value);
                //connector.Unsubscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Aircraft2MetadataIsGlider]);
                if (IsXPlaneGlider && AircraftId == 0)
                {
                    _engineType = EngineType.Sailplane;
                }
                AircraftChange(_aircraftId);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Aircraft2MetadataIsMilitary], 1, (element, value) =>
            {
                IsXPlaneMilitary = Convert.ToBoolean(value);
                //connector.Unsubscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Aircraft2MetadataIsMilitary]);
                AircraftChange(_aircraftId);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Aircraft2MetadataIsGeneralAviation], 1, (element, value) =>
            {
                IsXPlaneGeneralAviation = Convert.ToBoolean(value);
                //connector.Unsubscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Aircraft2MetadataIsGeneralAviation]);
                AircraftChange(_aircraftId);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.AircraftPropAcfEnType], 1, (element, value) =>
            {
                XPlaneEngineType = (XPLANE_ENGINETYPE)Convert.ToInt32(value);
                //connector.Unsubscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.AircraftPropAcfEnType]);
                if (!IsHelo && !IsXPlaneGlider && AircraftId == 0)
                {
                    switch (XPlaneEngineType)
                    {
                        case XPLANE_ENGINETYPE.FixedTurboprop:
                        case XPLANE_ENGINETYPE.FreeTurborpop:
                            _engineType = EngineType.Turboprop;
                            break;
                        case XPLANE_ENGINETYPE.MultiSpoolJet:
                        case XPLANE_ENGINETYPE.SingleSpoolJet:
                            _engineType = EngineType.Jet;
                            break;
                        case XPLANE_ENGINETYPE.RecipCarb:
                        case XPLANE_ENGINETYPE.RecipInjected:
                            _engineType = EngineType.Piston;
                            break;
                        case XPLANE_ENGINETYPE.Electric:
                            _engineType = EngineType.Electric;
                            break;
                        case XPLANE_ENGINETYPE.Rocket:
                            _engineType = EngineType.Rocket;
                            break;
                    }
                }
                AircraftChange(_aircraftId);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.AircraftViewAcfModesId], 1, (element, value) =>
            {
                XPlaneAircraftId = Convert.ToInt32(value).ToGuid();
                //connector.Unsubscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.AircraftViewAcfModesId]);
                _isReadyToFly = FlightSim.ReadyToFly.Ready;
                AircraftChange(_aircraftId);
            });

            var drAircraftViewAcfTailnum = XPlaneStructs.DataRefs.DataRefList[DataRefId.AircraftViewAcfTailnum] as StringDataRefElement
                     ?? throw new InvalidOperationException("DataRef AircraftViewAcfTailnum not found or wrong type.");

            connector.Subscribe(drAircraftViewAcfTailnum, 1, (element, value) =>
            {
                XPlaneTailnumber = _atcIdentifier = value;
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }
                Debug.WriteLine($"drAircraftViewAcfTailnum: {value}");
                AircraftChange(_aircraftId);
            }, (element, value) =>
            {
                //connector.Unsubscribe(drAircraftViewAcfTailnum);
                Debug.WriteLine($"drAircraftViewAcfTailnum: Complete: {value}");
                AircraftChange(_aircraftId);
            });

            var drAircraftViewAcfUiName = XPlaneStructs.DataRefs.DataRefList[DataRefId.AircraftViewAcfUiName] as StringDataRefElement
                     ?? throw new InvalidOperationException("DataRef AircraftViewAcfUiName not found or wrong type.");

            connector.Subscribe(drAircraftViewAcfUiName, 1, (element, value) =>
            {
                VSpeed.AircraftName = XPlaneAircraftName = value;
                if (_aircraftId == 0)
                {
                    _aircraftName = XPlaneAircraftName;
                }
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }
                Debug.WriteLine($"AircraftViewAcfUiName: {value}");
                AircraftChange(_aircraftId);
            }, (element, value) =>
            {
                Debug.WriteLine($"AircraftViewAcfUiName: Complete: {value}");
                //connector.Unsubscribe(drAircraftViewAcfUiName);
                LoadAircraft(XPlaneAircraftName);
                AircraftChange(_aircraftId);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.AircraftViewAcfVso], Frequency, (element, value) =>
            {
                VSpeed.WhiteStart = (int)value;
                VSpeed.TickStart = (int)value;
                //connector.Unsubscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.AircraftViewAcfVso]);
                AircraftChange(_aircraftId);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.AircraftViewAcfVno], Frequency, (element, value) =>
            {
                VSpeed.YellowStart = (int)value;
                VSpeed.GreenEnd = (int)value;
                //connector.Unsubscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.AircraftViewAcfVno]);
                AircraftChange(_aircraftId);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.AircraftViewAcfVne], Frequency, (element, value) =>
            {
                VSpeed.MaxSpeed = (int)value + (((int)value) / 10);
                VSpeed.HighLimit = (int)value + 10;
                VSpeed.RedEnd = (int)value + 10;
                VSpeed.YellowEnd = (int)value;
                VSpeed.RedStart = (int)value;
                VSpeed.TickEnd = (int)value + 10;
                VSpeed.TickSpan = (((int)value) / 10);
                //connector.Unsubscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.AircraftViewAcfVne]);
                AircraftChange(_aircraftId);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.AircraftViewAcfVs], Frequency, (element, value) =>
            {
                VSpeed.GreenStart = (int)value;
                //connector.Unsubscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.AircraftViewAcfVs]);
                AircraftChange(_aircraftId);
            });

            connector.Subscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.AircraftViewAcfVfe], Frequency, (element, value) =>
            {
                VSpeed.WhiteEnd = (int)value;
                //connector.Unsubscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.AircraftViewAcfVfe]);
                AircraftChange(_aircraftId);
            });
        }

        private void UnsubscribeSingles()
        {
            connector.Unsubscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Aircraft2MetadataIsAirliner]);
            connector.Unsubscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Aircraft2MetadataIsSeaplane]);
            connector.Unsubscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Aircraft2MetadataIsHelicopter]);
            connector.Unsubscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Aircraft2MetadataIsGlider]);
            connector.Unsubscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Aircraft2MetadataIsMilitary]);
            connector.Unsubscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.Aircraft2MetadataIsGeneralAviation]);
            connector.Unsubscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.AircraftPropAcfEnType]);
            connector.Unsubscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.AircraftViewAcfModesId]);
            var drAircraftViewAcfTailnum = XPlaneStructs.DataRefs.DataRefList[DataRefId.AircraftViewAcfTailnum] as StringDataRefElement
                     ?? throw new InvalidOperationException("DataRef AircraftViewAcfTailnum not found or wrong type.");
            connector.Unsubscribe(drAircraftViewAcfTailnum);
            var drAircraftViewAcfUiName = XPlaneStructs.DataRefs.DataRefList[DataRefId.AircraftViewAcfUiName] as StringDataRefElement
                     ?? throw new InvalidOperationException("DataRef AircraftViewAcfUiName not found or wrong type.");
            connector.Unsubscribe(drAircraftViewAcfUiName);
            connector.Unsubscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.AircraftViewAcfVso]);
            connector.Unsubscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.AircraftViewAcfVno]);
            connector.Unsubscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.AircraftViewAcfVne]);
            connector.Unsubscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.AircraftViewAcfVs]);
            connector.Unsubscribe(XPlaneStructs.DataRefs.DataRefList[DataRefId.AircraftViewAcfVfe]);
        }

        private void Unsubscribe()
        {
            connector.Stop();
        }

        private void LoadAircraft(string aircraftName)
        {
            if (Crossref != null)
            {
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    XPlaneCrossref? crossref = Crossref.FirstOrDefault(x => x.AircraftName.Equals(aircraftName ?? string.Empty, StringComparison.OrdinalIgnoreCase));
                    if (crossref != null)
                    {
                        AircraftData aircraft = Tools.LoadAircraft(crossref.AircraftId);
                        _aircraftId = aircraft.AircraftId;
                        _aircraftName = VSpeed.AircraftName = aircraft.FriendlyName;
                        _atcType = aircraft.FriendlyType;
                        _atcModel = aircraft.FriendlyModel;
                        _engineType = aircraft.EngineType;
                        _isHeavy = aircraft.Heavy;
                        _isHelo = aircraft.Helo;
                        AircraftChange(_aircraftId);
                    }
                    else
                    {
                        _aircraftId = 0;
                        _aircraftName = VSpeed.AircraftName = XPlaneAircraftName;
                        _atcType = string.Empty;
                        _atcModel = string.Empty;
                        _isHeavy = IsXPlaneAirline;
                        _isHelo = IsXPlaneHelo;
                        switch (XPlaneEngineType)
                        {
                            case XPLANE_ENGINETYPE.FixedTurboprop:
                            case XPLANE_ENGINETYPE.FreeTurborpop:
                                _engineType = EngineType.Turboprop;
                                break;
                            case XPLANE_ENGINETYPE.MultiSpoolJet:
                            case XPLANE_ENGINETYPE.SingleSpoolJet:
                                _engineType = EngineType.Jet;
                                break;
                            case XPLANE_ENGINETYPE.RecipCarb:
                            case XPLANE_ENGINETYPE.RecipInjected:
                                _engineType = EngineType.Piston;
                                break;
                            case XPLANE_ENGINETYPE.Electric:
                                _engineType = EngineType.Electric;
                                break;
                            case XPLANE_ENGINETYPE.Rocket:
                                _engineType = EngineType.Rocket;
                                break;
                        }
                        if (IsXPlaneGlider)
                        {
                            _engineType = EngineType.Sailplane;
                        }
                        AircraftChange(_aircraftId);
                    }
                });
            }
        }

        private void RecalcTorquePct(int i)
        {
            double max = _maxTrqNm[i];
            double drv = _drivTrqNm[i];
            _engineTorquePct[i] = (max > 0.0001) ? Tools.Clamp((drv / max) * 100.0, 0.0, 999.0) : 0.0;
        }

        public override void SendControlToFS(string control, float value)
        {
            if (connector != null)
            {
                DataRefId xpControl;
                if (Enum.TryParse(control, out xpControl))
                {
                    DataRefElement dataRef = DataRefs.Instance.DataRefList[xpControl];
                    if (dataRef != null)
                    {
                        connector.SetDataRefValue(dataRef.DataRef, value);
                    }
                }
            }
        }

        public override void SendCommandToFS(string command)
        {
            if (connector != null)
            {
                XPlaneCommands xpCommand;
                if (Enum.TryParse(command, out xpCommand))
                {
                    XPlaneCommand dataRef = Commands.Instance.CommandList[xpCommand];
                    if (dataRef != null)
                    {
                        connector.SendCommand(dataRef);
                    }
                }
            }
        }
    }
}
