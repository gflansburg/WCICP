using FlightSim.DCS_BIOS;
using FlightSim.DCS_BIOS.ControlLocator;
using FlightSim.DCS_BIOS.EventArgs;
using FlightSim.DCS_BIOS.Interfaces;
using FlightSim.DCS_BIOS.Json;
using FlightSim.DCS_BIOS.Serialized;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace FlightSim
{
    public class DCSProvider : FlightSimProviderBase, IDcsBiosConnectionListener, IDcsBiosDataListener, IDCSBIOSStringListener, IDcsBiosBulkDataListener, IDisposable
    {
        public static readonly DCSProvider? Instance;

        public const string DCSBIOS_INCREMENT = "INC\n";
        public const string DCSBIOS_DECREMENT = "DEC\n";
        public const string DCSBIOS_TOGGLE = "TOGGLE\n";

        private IReadOnlyList<DCSCrossref> _crossref = Array.Empty<DCSCrossref>();

        private string DCSWorldAircraftName { get; set; } = string.Empty;
        private bool DCSWorldIsHelo { get; set; }
        private EngineType DCSWorldEngineType { get; set; }

        public override string AdfIdent => string.Empty;

        public override string AdfName => string.Empty;

        public override double AutopilotNav1CourseDegrees => 0d;

        public override double AutopilotNav2CourseDegrees => 0d;

        public override bool FuelPump1On => false;

        public override bool FuelPump2On => false;

        public override bool FuelPump3On => false;

        public override bool FuelPump4On => false;

        public override bool Propeller1DeIce => false;

        public override bool Propeller2DeIce => false;

        public override bool Propeller3DeIce => false;

        public override bool Propeller4DeIce => false;

        public override bool CarbHeatAntiIce1 => false;

        public override bool CarbHeatAntiIce2 => false;

        public override bool CarbHeatAntiIce3 => false;

        public override bool CarbHeatAntiIce4 => false;

        public override double WaypointLongitude => 0d;

        public override double WaypointLatitude => 0d;

        public override string WaypointIdent => string.Empty;

        public override string Nav1ActiveFrequencyIdent => string.Empty;

        public override string Nav2ActiveFrequencyIdent => string.Empty;

        public override double Engine1Rpm => 0d;

        public override double Engine2Rpm => 0d;

        public override double Engine3Rpm => 0d;

        public override double Engine4Rpm => 0d;

        public override double Torque1FootPounds => 0d;

        public override double Torque2FootPounds => 0d;

        public override double Torque3FootPounds => 0d;

        public override double Torque4FootPounds => 0d;

        public override double VerticalSpeedFpm => 0d;

        public override double PitchDegrees => 0d;

        public override double BankDegrees => 0d;

        public override bool AutopilotAltHold => false;

        public override bool AutopilotHeadingHold => false;

        public override bool AutopilotNavHold => false;

        public override bool AutopilotSpeedHold => false;

        public override bool AutopilotMaster => false;

        public override double AutopilotAltitudeTargetFeet => 0d;

        public override double AutopilotSpeedTargetKnots => 0d;

        private double _headingPointerRadians;
        public override double AutopilotHeadingBugDegrees => _headingPointerRadians * (180d / Math.PI);

        public override double ElevatorTrimPercent => 0d;

        public override double RudderTrimPercent => 0d;

        public override double AileronTrimPercent => 0d;

        public override double FlapsPercent => 0d;

        public override GearState GearState => GearState.Up;

        public override double SpoilersPercent => 0d;

        public override bool SpoilersArmed => false;

        public override bool ParkingBrakeOn => false;

        public override double FuelRemainingGallons => 0d;

        public override double FuelCapacityGallons => 0d;

        public override double Engine1OilPressurePsf => 0d;

        public override double Engine2OilPressurePsf => 0d;

        public override double Engine3OilPressurePsf => 0d;

        public override double Engine4OilPressurePsf => 0d;

        public override bool NavLightsOn => false;

        public override bool BeaconLightsOn => false;

        public override bool TaxiLightsOn => false;

        public override bool LandingLightsOn => false;

        public override bool StrobeLightsOn => false;

        public override bool WindowDefrostOn => false;

        public override double CowlFlaps1Percent => 0d;

        public override double CowlFlaps2Percent => 0d;

        public override double CowlFlaps3Percent => 0d;

        public override double CowlFlaps4Percent => 0d;

        public override double Throttle1Percent => 0d;

        public override double Throttle2Percent => 0d;

        public override double Throttle3Percent => 0d;

        public override double Throttle4Percent => 0d;

        public override double Mixture1Percent => 0d;

        public override double Mixture2Percent => 0d;

        public override double Mixture3Percent => 0d;

        public override double Mixture4Percent => 0d;

        public override double PropPitch1Degrees => 0d;

        public override double PropPitch2Degrees => 0d;

        public override double PropPitch3Degrees => 0d;

        public override double PropPitch4Degrees => 0d;

        public override bool Engine1Failed => false;

        public override bool Engine2Failed => false;

        public override bool Engine3Failed => false;

        public override bool Engine4Failed => false;

        public override int EngineCount => 1;

        public override bool Afterburner1On => false;

        public override bool Afterburner2On => false;

        public override bool Afterburner3On => false;

        public override bool Afterburner4On => false;

        public override bool Engine1Running => false;

        public override bool Engine2Running => false;

        public override bool Engine3Running => false;

        public override bool Engine4Running => false;

        public override double Engine1N1Percent => 0d;

        public override double Engine2N1Percent => 0d;

        public override double Engine3N1Percent => 0d;

        public override double Engine4N1Percent => 0d;

        public override double Engine1ManifoldPressureInchesMercury => 0d;

        public override double Engine2ManifoldPressureInchesMercury => 0d;

        public override double Engine3ManifoldPressureInchesMercury => 0d;

        public override double Engine4ManifoldPressureInchesMercury => 0d;

        public override double Nav1Frequency => 0;

        public override double Nav2Frequency => 0;

        public override double Nav1StandByFrequency => 0;

        public override double Nav2StandByFrequency => 0;

        public override bool Nav1Receive => false;

        public override bool Nav2Receive => false;

        public override string Name => "DCS World";

        private Dictionary<string, Aircraft> _traffic = new Dictionary<string, Aircraft>();
        public override Dictionary<string, Aircraft> Traffic => _traffic;

        private int _aircraftId;
        public override int AircraftId => _aircraftId;

        private string _aircraftName = string.Empty;
        public override string AircraftName => _aircraftName;

        public double _altitudeMSL;
        public override double AltitudeMSL => (_altitudeMSL * _kollsman) / _pressure;

        public double _altitudeAGL;
        public override double AltitudeAGL => _altitudeAGL;

        private double _headingMag;
        public override double HeadingMagneticDegrees => _headingMag;

        private double _headingTrue;
        public override double HeadingTrueDegrees => _headingTrue;

        public override double HeadingMagneticRadians => _headingMag * (Math.PI / 180);

        public override double HeadingTrueRadians => _headingTrue * (Math.PI / 180);

        private bool _isConnected = false;
        public override bool IsConnected => _isConnected;

        private string _atcIdentifier = string.Empty;
        public override string ATCIdentifier => _atcIdentifier;

        private string _atcModel = string.Empty;
        public override string ATCModel => _atcModel;

        private string _atcType = string.Empty;
        public override string ATCType => _atcType;

        private bool _isHeavy;
        public override bool IsHeavy => _isHeavy;

        public override bool IsGearFloats => false;

        private bool _isHelo;
        public override bool IsHelo => _isHelo;

        private EngineType _engineType;
        public override EngineType EngineType => _engineType;

        private bool _onGround;
        public override bool OnGround => _onGround;

        private double _groundSpeed;
        public override double GroundSpeedKnots => _groundSpeed;

        private double _airspeedIndicated;
        public override double AirSpeedIndicatedKnots => _airspeedIndicated;

        private double _airspeedTrue;
        public override double AirSpeedTrueKnots => _airspeedTrue;

        private double _ambientTemperatureCelsius;
        public override double AmbientTemperatureCelsius => _ambientTemperatureCelsius;

        private double _windDirection;
        public override double AmbientWindDirectionDegrees => _windDirection;

        private double _windSpeed;
        public override double AmbientWindSpeedKnots => _windSpeed * 1.943844d;

        private double _kollsman = 29.92;
        public override double KollsmanInchesMercury => _kollsman;

        public override double SecondaryKollsmanInchesMercury => _kollsman;

        private double _pressure = 29.92;
        public override double PressureInchesMercury => _pressure;

        private ReadyToFly _readyToFly = FlightSim.ReadyToFly.Loading;
        public override ReadyToFly IsReadyToFly => _readyToFly;

        private double _courseRadians;
        public override double GPSRequiredMagneticHeadingRadians => _courseRadians;

        private double _courseTrueRadians;
        public override double GPSRequiredTrueHeadingRadians => _courseTrueRadians;

        private bool _hasActiveWaypoint;
        public override bool HasActiveWaypoint => _hasActiveWaypoint;

        private double _gpsCrossTrackErrorMeters;
        public override double GPSCrossTrackErrorMeters => _gpsCrossTrackErrorMeters;

        private double _rmiRadians; 
        public override double Nav1Radial => _rmiRadians * (180d / Math.PI);

        public override double Nav2Radial => 0;

        private bool _rmiAvailable;
        public override bool Nav1Available => _rmiAvailable;

        public override bool Nav2Available => false;

        private double _adfRadians;
        public override double AdfRelativeBearing => _adfRadians * (180d / Math.PI);

        public override double AltitudeMSLFeet => _altitudeMSL;

        public override bool BatteryOn => false;

        public override bool AvionicsOn => false;

        public override uint Transponder => 0;

        public override TransponderMode TransponderMode => TransponderMode.Off;

        public override bool Com1Receive => false;

        public override bool Com2Receive => false;

        public override bool Com1Transmit => false;

        public override bool Com2Transmit => false;

        public override double Com1Frequency => 0;

        public override double Com2Frequency => 0;

        public override double Com1StandByFrequency => 0;

        public override double Com2StandByFrequency => 0;

        public override bool CabinLightsOn => false;

        public override bool PanelLightsOn => true;

        public override double CollectivePercent => 0d;

        public override bool RotorBrakeActive => false;

        public override double RotorBrakeHandlePercent => 0d;

        public override double MainRotorDiskBankPercent => 0d;

        public override double MainRotorDiskPitchPercent => 0d;

        public override double MainRotorDiskConingPercent => 0d;

        public override double MainRotorRotationAngleDegrees => 0d;

        public override double TailRotorDiskBankPercent => 0d;

        public override double TailRotorDiskPitchPercent => 0d;

        public override double TailRotorDiskConingPercent => 0d;

        public override double TailRotorRotationAngleDegrees => 0d;

        public override double MainRotorRpm => 0d;

        public override double TailRotorRpm => 0d;

        public override double MainRotorBladePitchPercent => 0d;

        public override double TailRotorBladePitchPercent => 0d;

        public override double RotorLateralTrimPercent => 0d;

        public override double RotorLongitudinalTrimPercent => 0d;

        public override double Engine1RotorRpmCommandPercent => 0d;

        public override double Engine2RotorRpmCommandPercent => 0d;

        public override double Engine1TorquePercent => 0d;

        public override double Engine2TorquePercent => 0d;

        public override bool RotorGovernor1Active => false;

        public override bool RotorGovernor2Active => false;


        private DCSBIOS? dcsBios;

        private string IPFrom { get; set; } = string.Empty;

        public string IPTo { get; set; } = string.Empty;

        public int FromPort { get; set; }

        public int ToPort { get; set; }

        private Dictionary<string, DCSBIOSControl>? OutputControls { get; set; }

        private DCSBIOSOutput? DCSBIOSOutputAltitudeMSL { get; set; }

        private DCSBIOSOutput? DCSBIOSOutputAltitudeAGL { get; set; }

        private DCSBIOSOutput? DCSBIOSOutputAirspeedIndicated { get; set; }

        private DCSBIOSOutput? DCSBIOSOutputAirspeedTrue { get; set; }

        private DCSBIOSOutput? DCSBIOSOutputHeadingTrue { get; set; }

        private DCSBIOSOutput? DCSBIOSOutputHeadingMag { get; set; }
        
        private DCSBIOSOutput? DCSBIOSOutputPilotName { get; set; }

        private DCSBIOSOutput? DCSBIOSOutputPosition { get; set; }

        private DCSBIOSOutput? DCSBIOSOutputKollsman { get; set; }

        private DCSBIOSOutput? DCSBIOSOutputCourse { get; set; }

        private DCSBIOSOutput? DCSBIOSOutputCourseTrue { get; set; }

        private DCSBIOSOutput? DCSBIOSOutputHeadingPointer { get; set; }

        private DCSBIOSOutput? DCSBIOSOutputADF { get; set; }

        private DCSBIOSOutput? DCSBIOSOutputRMI { get; set; }

        private DCSBIOSOutput? DCSBIOSOutputRMIAvailable { get; set; }

        private DCSBIOSOutput? DCSBIOSOutputWindSpeed { get; set; }

        private DCSBIOSOutput? DCSBIOSOutputWindDirection { get; set; }

        private DCSBIOSOutput? DCSBIOSOutputAircraftName { get; set; }

        private DCSBIOSOutput? DCSBIOSOutputGPSCrossTrackError { get; set; }

        private DCSBIOSOutput? DCSBIOSOutputAmbientTemperature { get; set; }

        private DCSBIOSOutput? DCSBIOSOutputOnGround { get; set; }

        private DCSBIOSOutput? DCSBIOSOutputEngineType { get; set; }

        private DCSBIOSOutput? DCSBIOSOutputIsHelicopter { get; set; }

        private DCSBIOSOutput? DCSBIOSOutputRMIAvialble { get; set; }

        private DCSBIOSOutput? DCSBIOSOutputGroundSpeed { get; set; }

        private DCSBIOSOutput? DCSBIOSOutputActiveWaypoint { get; set; }

        public override double AltitudeAGLFeet => AltitudeMSLFeet;

        public override double AltitudeTrueFeet => AltitudeMSLFeet;

        public override bool IdentActive => false;

        public override bool GeneratorOn => true;

        public override bool PitotHeatOn => true;

        public override ComStatus Com1Status => ComStatus.OK;

        public override ComStatus Com2Status => ComStatus.OK;

        public override string Com1ActiveFrequencyIdent => string.Empty;

        public override string Com2ActiveFrequencyIdent => string.Empty;

        public override TunedFacility Com1ActiveFrequencyType => TunedFacility.NONE;

        public override TunedFacility Com2ActiveFrequencyType => TunedFacility.NONE;

        public override double Nav1DmeDistanceNm => 0d;

        public override double Nav2DmeDistanceNm => 0d;

        public override double Nav1DmeSpeedKts => 0d;

        public override double Nav2DmeSpeedKts => 0d;

        public override bool GpsDrivesNav => false;

        public override bool FlightDirectorActive => false;

        public override bool AutopilotVerticalSpeedHold => false;

        public override double AutopilotVerticalSpeedTargetFpm => 0d;

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

        private int _aircraftLoadRequestId = 0;
        private int _aircraftLoadAppliedId = 0;
        private long _lastNotifyTicks;
        private const int NotifyMinIntervalMs = 50;
        private readonly object _stateLock = new object();

        static DCSProvider()
        {
            try
            {
                if (!DCSLuaBridgeInstaller.EnsureInstalled())
                {
                    return;
                }
                var dcsRoot = DCSLuaBridgeInstaller.GetPreferredDCSSavedGamesRoot();
                if (string.IsNullOrWhiteSpace(dcsRoot))
                { 
                    return;
                }
                string jsonPath = Path.Combine(dcsRoot, "Scripts", DCSLuaBridgeInstaller.BridgeFolderName, "doc", "json");
                // If the JSON directory isn't present, do not initialize
                if (!Directory.Exists(jsonPath))
                {
                    return;
                }
                Instance = new DCSProvider(jsonPath, "239.255.60.10", "127.0.0.1", 15010, 17778);
            }
            catch
            {
                // Absolutely do not allow provider init to crash the entire assembly
                Instance = null;
            }
        }

        DCSProvider(string DCSBiosJSONLocation, string ipFrom, string ipTo, int fromPort, int toPort)
        {
            Common.SetEmulationModes(EmulationMode.DCSBIOSInputEnabled | EmulationMode.DCSBIOSOutputEnabled);
            DCSAircraft.Init();
            BIOSEventHandler.AttachConnectionListener(this);
            BIOSEventHandler.AttachStringListener(this);
            BIOSEventHandler.AttachDataListener(this);
            BIOSEventHandler.AttachBulkDataListener(this);
            UpdateConnection(DCSBiosJSONLocation, ipFrom, ipTo, fromPort, toPort);
            LoadCommonData(DCSBiosJSONLocation);
            DCSBIOSOutputAltitudeMSL = GetDCSBIOSOutput("ALT_MSL_FT");
            DCSBIOSOutputAltitudeAGL = GetDCSBIOSOutput("ALT_AGL_FT");
            DCSBIOSOutputHeadingTrue = GetDCSBIOSOutput("HDG_DEG");
            DCSBIOSOutputHeadingMag = GetDCSBIOSOutput("HDG_DEG_MAG");
            DCSBIOSOutputPilotName = GetDCSBIOSOutput("PILOTNAME");
            DCSBIOSOutputAirspeedIndicated = GetDCSBIOSOutput("IAS_US_INT");
            DCSBIOSOutputAirspeedTrue = GetDCSBIOSOutput("TAS_US_INT");
            DCSBIOSOutputPosition = GetDCSBIOSOutput("POSITION");
            DCSBIOSOutputKollsman = GetDCSBIOSOutput("Kollsman");
            DCSBIOSOutputCourse = GetDCSBIOSOutput("COURSE_RAD");
            DCSBIOSOutputCourseTrue = GetDCSBIOSOutput("COURSE_TRUE_RAD");
            DCSBIOSOutputHeadingPointer = GetDCSBIOSOutput("HDG_PNT_RAD");
            DCSBIOSOutputADF = GetDCSBIOSOutput("ADF_RAD");
            DCSBIOSOutputRMI = GetDCSBIOSOutput("FRMI_RAD");
            DCSBIOSOutputWindSpeed = GetDCSBIOSOutput("WIND_SPEED");
            DCSBIOSOutputWindDirection = GetDCSBIOSOutput("WIND_DIR_DEG");
            DCSBIOSOutputAircraftName = GetDCSBIOSOutput("_ACFT_NAME");
            DCSBIOSOutputGPSCrossTrackError = GetDCSBIOSOutput("CRS_TRK_ERR");
            DCSBIOSOutputAmbientTemperature = GetDCSBIOSOutput("TEMPERATURE");
            DCSBIOSOutputOnGround = GetDCSBIOSOutput("ON_GROUND");
            DCSBIOSOutputEngineType = GetDCSBIOSOutput("ENGINE_TYPE");
            DCSBIOSOutputIsHelicopter = GetDCSBIOSOutput("IS_HELICOPTER");
            DCSBIOSOutputRMIAvailable = GetDCSBIOSOutput("RMI_AVAIL");
            DCSBIOSOutputGroundSpeed = GetDCSBIOSOutput("GRNDS_US");
            DCSBIOSOutputActiveWaypoint = GetDCSBIOSOutput("ACTIVE_WP");
            ThreadPool.QueueUserWorkItem(_ =>
            {
                var list = (IReadOnlyList<DCSCrossref>)DCSCrossref.GetDCSWorldCrossref();
                Volatile.Write(ref _crossref, list);

                var name = DCSWorldAircraftName;
                if (!string.IsNullOrWhiteSpace(name))
                    LoadAircraft(name);
            });
        }

        private DCSBIOSOutput? GetDCSBIOSOutput(string controlId)
        {
            try
            {
                if (OutputControls != null && OutputControls.ContainsKey(controlId))
                {
                    var control = OutputControls[controlId];
                    if (control != null && control.Outputs.Count > 0)
                    {
                        var dcsBIOSOutput = new DCSBIOSOutput();
                        dcsBIOSOutput.Consume(control, control.Outputs[0].OutputDataType);
                        return dcsBIOSOutput;
                    }
                }
            }
            catch (InvalidOperationException)
            {
            }
            return null;
        }

        private void LoadCommonData(string jsonDirectory)
        {
            try
            {
                OutputControls = DCSBIOSControlLocator.ReadControlsFromDocJson(jsonDirectory + $"\\{DCSAircraft.GetCommonDataJSONFilename()}").ToDictionary(o => o.Identifier, o => o);
                var meta = DCSBIOSControlLocator.ReadControlsFromDocJson(jsonDirectory + $"\\{DCSAircraft.GetMetaDataStartJSONFilename()}");
                foreach (var control in meta)
                {
                    OutputControls.Add(control.Identifier, control);
                }
            }
            catch (Exception)
            {
            }
        }

        public void UpdateConnection(string DCSBiosJSONLocation, string ipFrom, string ipTo, int fromPort, int toPort)
        {
            IPFrom = ipFrom;
            IPTo = ipTo;
            FromPort = fromPort;
            ToPort = toPort;
            DCSBIOSControlLocator.JSONDirectory = DCSBiosJSONLocation;
            DCSAircraft.FillModulesListFromDcsBios(DCSBiosJSONLocation, true);
            CreateDCSBIOS(true);
            StartupDCSBIOS(true);
        }

        private void CreateDCSBIOS(bool force = false)
        {
            if (dcsBios != null && !force)
            {
                return;
            }
            ShutdownDCSBIOS();
            dcsBios = new DCSBIOS(IPFrom, IPTo, FromPort, ToPort, DcsBiosNotificationMode.Parse);
        }

        private void StartupDCSBIOS(bool force = false)
        {
            if (dcsBios != null && dcsBios.IsRunning && !force)
            {
                return;
            }
            dcsBios?.Startup();
        }

        private void ShutdownDCSBIOS()
        {
            dcsBios?.Shutdown();
            dcsBios = null;
        }

        private void NotifyFlightDataThrottled()
        {
            long now = Stopwatch.GetTimestamp();
            long last = Interlocked.Read(ref _lastNotifyTicks);

            // Convert ms to ticks
            double ticksPerMs = (double)Stopwatch.Frequency / 1000.0;
            long minTicks = (long)(NotifyMinIntervalMs * ticksPerMs);

            if (now - last < minTicks)
                return;

            Interlocked.Exchange(ref _lastNotifyTicks, now);
            FlightDataReceived();
        }

        public override void SendControlToFS(string control, float value)
        {
            if (dcsBios != null)
            {
                DCSBIOS.Send(string.Format("{0} {1}\n", control, value));
            }
        }

        public override void SendCommandToFS(string command)
        {
            if (dcsBios != null)
            {
                DCSBIOS.Send(string.Format("{0}\n", command));
            }
        }

        public void DcsBiosConnectionActive(object sender, DCSBIOSConnectionEventArgs e)
        {
            if (!_isConnected || !HasConnected)
            {
                bool sendReadyToFly = false;
                _isConnected = true;
                if (_readyToFly != FlightSim.ReadyToFly.Ready)
                {
                    _readyToFly = FlightSim.ReadyToFly.Ready;
                    sendReadyToFly = true;
                }
                Connected();
                if (sendReadyToFly)
                {
                    ReadyToFly(_readyToFly);
                }
            }
        }

        private void WhoAmI(DCSBIOSStringDataEventArgs e)
        {
            if (DCSBIOSOutputPilotName?.Address == e.Address)
            {
                Debug.WriteLine("{0}: {1}", DCSBIOSOutputPilotName?.ControlId, e.StringData.Trim());
            }
            if (DCSBIOSOutputGPSCrossTrackError?.Address == e.Address)
            {
                Debug.WriteLine("{0}: {1}", DCSBIOSOutputGPSCrossTrackError?.ControlId, e.StringData.Trim());
            }
            if (DCSBIOSOutputKollsman?.Address == e.Address)
            {
                Debug.WriteLine("{0}: {1}", DCSBIOSOutputKollsman?.ControlId, e.StringData.Trim());
            }
            if (DCSBIOSOutputCourse?.Address == e.Address)
            {
                Debug.WriteLine("{0}: {1}", DCSBIOSOutputCourse?.ControlId, e.StringData.Trim());
            }
            if (DCSBIOSOutputCourseTrue?.Address == e.Address)
            {
                Debug.WriteLine("{0}: {1}", DCSBIOSOutputCourseTrue?.ControlId, e.StringData.Trim());
            }
            if (DCSBIOSOutputADF?.Address == e.Address)
            {
                Debug.WriteLine("{0}: {1}", DCSBIOSOutputADF?.ControlId, e.StringData.Trim());
            }
            if (DCSBIOSOutputRMI?.Address == e.Address)
            {
                Debug.WriteLine("{0}: {1}", DCSBIOSOutputRMI?.ControlId, e.StringData.Trim());
            }
            if (DCSBIOSOutputHeadingPointer?.Address == e.Address)
            {
                Debug.WriteLine("{0}: {1}", DCSBIOSOutputHeadingPointer?.ControlId, e.StringData.Trim());
            }
            if (DCSBIOSOutputWindSpeed?.Address == e.Address)
            {
                Debug.WriteLine("{0}: {1}", DCSBIOSOutputWindSpeed?.ControlId, e.StringData.Trim());
            }
            if (DCSBIOSOutputWindDirection?.Address == e.Address)
            {
                Debug.WriteLine("{0}: {1}", DCSBIOSOutputWindDirection?.ControlId, e.StringData.Trim());
            }
            if (DCSBIOSOutputAmbientTemperature?.Address == e.Address)
            {
                Debug.WriteLine("{0}: {1}", DCSBIOSOutputAmbientTemperature?.ControlId, e.StringData.Trim());
            }
            if (DCSBIOSOutputGroundSpeed?.Address == e.Address)
            {
                Debug.WriteLine("{0}: {1}", DCSBIOSOutputGroundSpeed?.ControlId, e.StringData.Trim());
            }
            if (DCSBIOSOutputEngineType?.Address == e.Address)
            {
                Debug.WriteLine("{0}: {1}", DCSBIOSOutputEngineType?.ControlId, e.StringData.Trim());
            }
            if (DCSBIOSOutputAircraftName?.Address == e.Address)
            {
                Debug.WriteLine("{0}: {1}", DCSBIOSOutputAircraftName?.ControlId, e.StringData.Trim());
            }
            if (DCSBIOSOutputPosition?.Address == e.Address)
            {
                Debug.WriteLine("{0}: {1}", DCSBIOSOutputPosition?.ControlId, e.StringData.Trim());
            }

        }
        public void DCSBIOSStringReceived(object sender, DCSBIOSStringDataEventArgs e)
        {
            WhoAmI(e);
            bool isDirty = false;
            lock (_stateLock)
            {
                if (DCSBIOSOutputPilotName?.StringValueHasChanged(e.Address, e.StringData.Trim()) == true)
                {
                    _atcIdentifier = DCSBIOSOutputPilotName.LastStringValue = e.StringData.Trim();
                    isDirty = true;
                }
                if (DCSBIOSOutputGPSCrossTrackError?.StringValueHasChanged(e.Address, e.StringData.Trim()) == true)
                {
                    _gpsCrossTrackErrorMeters = Convert.ToDouble(DCSBIOSOutputGPSCrossTrackError.LastStringValue = e.StringData.Trim());
                    isDirty = true;
                }
                if (DCSBIOSOutputKollsman?.StringValueHasChanged(e.Address, e.StringData.Trim()) == true)
                {
                    _pressure = Convert.ToDouble(DCSBIOSOutputKollsman.LastStringValue = e.StringData.Trim()) / 25.4d;
                    isDirty = true;
                }
                if (DCSBIOSOutputCourse?.StringValueHasChanged(e.Address, e.StringData.Trim()) == true)
                {
                    _courseRadians = Convert.ToDouble(DCSBIOSOutputCourse.LastStringValue = e.StringData.Trim());
                    isDirty = true;
                }
                if (DCSBIOSOutputCourseTrue?.StringValueHasChanged(e.Address, e.StringData.Trim()) == true)
                {
                    _courseTrueRadians = Convert.ToDouble(DCSBIOSOutputCourseTrue.LastStringValue = e.StringData.Trim());
                    isDirty = true;
                }
                if (DCSBIOSOutputADF?.StringValueHasChanged(e.Address, e.StringData.Trim()) == true)
                {
                    _adfRadians = Convert.ToDouble(DCSBIOSOutputADF.LastStringValue = e.StringData.Trim());
                    isDirty = true;
                }
                if (DCSBIOSOutputRMI?.StringValueHasChanged(e.Address, e.StringData.Trim()) == true)
                {
                    _rmiRadians = Convert.ToDouble(DCSBIOSOutputRMI.LastStringValue = e.StringData.Trim());
                    isDirty = true;
                }
                if (DCSBIOSOutputHeadingPointer?.StringValueHasChanged(e.Address, e.StringData.Trim()) == true)
                {
                    _headingPointerRadians = Convert.ToDouble(DCSBIOSOutputHeadingPointer.LastStringValue = e.StringData.Trim());
                    isDirty = true;
                }
                if (DCSBIOSOutputWindSpeed?.StringValueHasChanged(e.Address, e.StringData.Trim()) == true)
                {
                    _windSpeed = Convert.ToDouble(DCSBIOSOutputWindSpeed.LastStringValue = e.StringData.Trim());
                    isDirty = true;
                }
                if (DCSBIOSOutputWindDirection?.StringValueHasChanged(e.Address, e.StringData.Trim()) == true)
                {
                    _windDirection = Convert.ToDouble(DCSBIOSOutputWindDirection.LastStringValue = e.StringData.Trim());
                    isDirty = true;
                }
                if (DCSBIOSOutputAmbientTemperature?.StringValueHasChanged(e.Address, e.StringData.Trim()) == true)
                {
                    _ambientTemperatureCelsius = Convert.ToDouble(DCSBIOSOutputAmbientTemperature.LastStringValue = e.StringData.Trim());
                    isDirty = true;
                }
                if (DCSBIOSOutputGroundSpeed?.StringValueHasChanged(e.Address, e.StringData.Trim()) == true)
                {
                    _groundSpeed = Convert.ToDouble(DCSBIOSOutputGroundSpeed.LastStringValue = e.StringData.Trim());
                    isDirty = true;
                }
                if (DCSBIOSOutputEngineType?.StringValueHasChanged(e.Address, e.StringData.Trim()) == true)
                {
                    string engineType = DCSBIOSOutputEngineType.LastStringValue = e.StringData.Trim();
                    switch (engineType)
                    {
                        case "Jet":
                            DCSWorldEngineType = EngineType.Jet;
                            break;
                        case "Piston":
                            DCSWorldEngineType = _isHelo ? EngineType.Helo : EngineType.Piston;
                            break;
                        case "Turboprop":
                            DCSWorldEngineType = EngineType.Turboprop;
                            break;
                    }
                    if (AircraftId == 0)
                    {
                        _engineType = DCSWorldEngineType;
                    }
                    isDirty = true;
                }
                if (DCSBIOSOutputAircraftName?.StringValueHasChanged(e.Address, e.StringData.Trim()) == true)
                {
                    DCSWorldAircraftName = DCSBIOSOutputAircraftName.LastStringValue = e.StringData.Trim();
                    if (DCSWorldAircraftName.IndexOf("\0") != -1)
                    {
                        DCSWorldAircraftName = DCSWorldAircraftName.Substring(0, DCSWorldAircraftName.IndexOf("\0"));
                    }
                    _readyToFly = FlightSim.ReadyToFly.Ready;
                    ReadyToFly(IsReadyToFly);
                    LoadAircraft(DCSWorldAircraftName);
                }
                if (DCSBIOSOutputPosition?.StringValueHasChanged(e.Address, e.StringData.Trim()) == true)
                {
                    var position = DCSBIOSOutputPosition.LastStringValue = e.StringData.Trim();
                    if (!string.IsNullOrEmpty(position))
                    {
                        string[] pos = position.Split(',');
                        if (pos.Length > 1)
                        {
                            double lat = Latitude;
                            double lng = Longitude;
                            SetPosition(Convert.ToDouble(pos[0]), Convert.ToDouble(pos[1]));
                            double distance = Tools.DistanceTo(lat, lng, Latitude, Longitude);
                            //Have we moved more than 500M in a fraction of a second?
                            if (distance >= 500)
                            {
                                ReadyToFly(IsReadyToFly);
                            }
                            isDirty = true;
                        }
                    }
                }
            }
            if (isDirty)
            {
                NotifyFlightDataThrottled();
            }
        }

        public void Dispose()
        {
            ShutdownDCSBIOS();
        }

        public override void Deinitialize(int timeOut = 1000)
        {
            ShutdownDCSBIOS();
        }

        public void DcsBiosConnectionInActive(object sender, DCSBIOSConnectionEventArgs e)
        {
            if (_isConnected || !HasQuit)
            {
                _isConnected = false;
                if (_readyToFly != FlightSim.ReadyToFly.Loading)
                {
                    _readyToFly = FlightSim.ReadyToFly.Loading;
                    ReadyToFly(_readyToFly);
                }
                Quit();
            }
        }

        public void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e)
        {
            bool isDirty = false;
            lock (_stateLock)
            {
                if (DCSBIOSOutputAltitudeMSL?.UShortValueHasChanged(e.Address, e.Data) == true)
                {
                    _altitudeMSL = DCSBIOSOutputAltitudeMSL.GetUShortValue(e.Data);
                    isDirty = true;
                }
                if (DCSBIOSOutputAltitudeAGL?.UShortValueHasChanged(e.Address, e.Data) == true)
                {
                    _altitudeAGL = DCSBIOSOutputAltitudeAGL.GetUShortValue(e.Data);
                    isDirty = true;
                }
                if (DCSBIOSOutputHeadingTrue?.UShortValueHasChanged(e.Address, e.Data) == true)
                {
                    _headingTrue = DCSBIOSOutputHeadingTrue.GetUShortValue(e.Data);
                    isDirty = true;
                }
                if (DCSBIOSOutputHeadingMag?.UShortValueHasChanged(e.Address, e.Data) == true)
                {
                    _headingMag = DCSBIOSOutputHeadingMag.GetUShortValue(e.Data);
                    isDirty = true;
                }
                if (DCSBIOSOutputAirspeedIndicated?.UShortValueHasChanged(e.Address, e.Data) == true)
                {
                    _airspeedIndicated = DCSBIOSOutputAirspeedIndicated.GetUShortValue(e.Data);
                    isDirty = true;
                }
                if (DCSBIOSOutputAirspeedTrue?.UShortValueHasChanged(e.Address, e.Data) == true)
                {
                    _airspeedTrue = DCSBIOSOutputAirspeedTrue.GetUShortValue(e.Data);
                    isDirty = true;
                }
                if (DCSBIOSOutputOnGround?.UShortValueHasChanged(e.Address, e.Data) == true)
                {
                    _onGround = Convert.ToBoolean(DCSBIOSOutputOnGround.GetUShortValue(e.Data));
                    isDirty = true;
                }
                if (DCSBIOSOutputIsHelicopter?.UShortValueHasChanged(e.Address, e.Data) == true)
                {
                    DCSWorldIsHelo = Convert.ToBoolean(DCSBIOSOutputIsHelicopter.GetUShortValue(e.Data));
                    DCSWorldEngineType = _isHelo ? EngineType.Helo : _engineType;
                    if (DCSWorldIsHelo && AircraftId == 0)
                    {
                        _isHelo = DCSWorldIsHelo;
                    }
                    isDirty = true;
                }
                if (DCSBIOSOutputRMIAvailable?.UShortValueHasChanged(e.Address, e.Data) == true)
                {
                    _rmiAvailable = Convert.ToBoolean(DCSBIOSOutputRMIAvailable.GetUShortValue(e.Data));
                    isDirty = true;
                }
                if (DCSBIOSOutputActiveWaypoint?.UShortValueHasChanged(e.Address, e.Data) == true)
                {
                    _hasActiveWaypoint = Convert.ToBoolean(DCSBIOSOutputActiveWaypoint.GetUShortValue(e.Data));
                    isDirty = true;
                }
                if (isDirty)
                {
                    NotifyFlightDataThrottled();
                }
            }
        }

        public void IncKollsman()
        {
            _kollsman += .01;
            FlightDataReceived();
        }

        public void DecKollsman()
        {
            _kollsman -= .01;
            FlightDataReceived();
        }

        private void LoadAircraft(string aircraftName)
        {
            var crossrefSnapshot = Volatile.Read(ref _crossref);
            if (crossrefSnapshot.Count == 0)
                return;

            // Latest request wins
            int requestId = Interlocked.Increment(ref _aircraftLoadRequestId);

            // Normalize once
            string requestedName = (aircraftName ?? string.Empty).Trim();

            // Snapshot for fallback so the worker doesn't read moving targets
            bool fallbackIsHelo = DCSWorldIsHelo;
            EngineType fallbackEngine = DCSWorldEngineType;

            ThreadPool.QueueUserWorkItem(_ =>
            {
                // Bail fast if stale
                if (requestId != Volatile.Read(ref _aircraftLoadRequestId))
                    return;

                DCSCrossref? xref = null;
                try
                {
                    xref = crossrefSnapshot.FirstOrDefault(x =>
                        x.AircraftName.Equals(requestedName, StringComparison.OrdinalIgnoreCase));
                }
                catch
                {
                    xref = null;
                }

                int newId;
                string newName;
                string newType;
                string newModel;
                EngineType newEngine;
                bool newHeavy;
                bool newHelo;

                if (xref != null)
                {
                    var aircraft = Tools.LoadAircraft(xref.AircraftId);
                    newId = aircraft.AircraftId;
                    newName = aircraft.FriendlyName;
                    newType = aircraft.FriendlyType;
                    newModel = aircraft.FriendlyModel;
                    newEngine = aircraft.EngineType;
                    newHeavy = aircraft.Heavy;
                    newHelo = aircraft.Helo;
                }
                else
                {
                    // "Unknown" module fallback: keep whatever DCS tells us
                    newId = 0;
                    newName = requestedName;
                    newType = string.Empty;
                    newModel = string.Empty;
                    newHeavy = false;
                    newHelo = fallbackIsHelo;
                    newEngine = fallbackEngine;
                }

                // Don’t apply if stale
                if (requestId != Volatile.Read(ref _aircraftLoadRequestId))
                    return;

                _aircraftId = newId;
                _aircraftName = newName;
                _atcType = newType;
                _atcModel = newModel;
                _engineType = newEngine;
                _isHeavy = newHeavy;
                _isHelo = newHelo;

                Volatile.Write(ref _aircraftLoadAppliedId, requestId);

                AircraftChange(_aircraftId);
            });
        }

        public void DcsBiosBulkDataReceived(object sender, DCSBIOSBulkDataEventArgs e)
        {
            FlightDataReceived();
        }
    }
}
