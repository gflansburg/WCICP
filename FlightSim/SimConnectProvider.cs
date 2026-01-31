using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using static FlightSim.SimConnect;

namespace FlightSim
{
    public class SimConnectProvider : FlightSimProviderBase
    {
        public static readonly SimConnectProvider Instance;

        public override string Name => "SimConnect";

        public bool RequestTrafficInfo
        {
            get
            {
                return SimConnect.Instance.RequestTrafficInfo;
            }
            set
            {
                SimConnect.Instance.RequestTrafficInfo = value;
            }
        }

        private FLIGHT_DATA FlightData { get; set; } = new FLIGHT_DATA();

        public override double WaypointLongitude => FlightData.GPS_WP_LON;

        public override double WaypointLatitude => FlightData.GPS_WP_LAT;

        public override string WaypointIdent
        {
            get
            {
                string? ident = SimConnect.Instance.Facilities.FindNearestWaypointIdent(WaypointLatitude, WaypointLongitude, maxNm: 3.0) ?? string.Empty;
                if (string.IsNullOrEmpty(ident))
                {
                    ident = SimConnect.Instance.Facilities.FindNearestAirportIdent(WaypointLatitude, WaypointLongitude, maxNm: 5.0) ?? string.Empty;
                }
                if (string.IsNullOrEmpty(ident))
                {
                    ident = FlightData.GPS_WP_IDENT;
                }
                return ident ?? string.Empty;
            }
        }

        public override string Nav1ActiveFrequencyIdent => FlightData.NAV1_IDENT;

        public override string Nav2ActiveFrequencyIdent => FlightData.NAV2_IDENT;

        public override string AdfIdent => FlightData.ADF_IDENT;

        public override string AdfName => FlightData.ADF_NAME;

        public override double Engine1Rpm => FlightData.ENG1_RPM;

        public override double Engine2Rpm => FlightData.ENG2_RPM;

        public override double Engine3Rpm => FlightData.ENG3_RPM;

        public override double Engine4Rpm => FlightData.ENG4_RPM;

        public override double Torque1FootPounds => FlightData.ENG1_TORQUE_FTLBS;

        public override double Torque2FootPounds => FlightData.ENG2_TORQUE_FTLBS;

        public override double Torque3FootPounds => FlightData.ENG3_TORQUE_FTLBS;

        public override double Torque4FootPounds => FlightData.ENG4_TORQUE_FTLBS;

        public override bool Propeller1DeIce => Convert.ToBoolean(FlightData.PROP1_DEICE);

        public override bool Propeller2DeIce => Convert.ToBoolean(FlightData.PROP2_DEICE);

        public override bool Propeller3DeIce => Convert.ToBoolean(FlightData.PROP3_DEICE);

        public override bool Propeller4DeIce => Convert.ToBoolean(FlightData.PROP4_DEICE);

        public override bool FuelPump1On => Convert.ToBoolean(FlightData.FUELPUMP1);

        public override bool FuelPump2On => Convert.ToBoolean(FlightData.FUELPUMP2);

        public override bool FuelPump3On => Convert.ToBoolean(FlightData.FUELPUMP3);

        public override bool FuelPump4On => Convert.ToBoolean(FlightData.FUELPUMP4);

        public override double VerticalSpeedFpm => FlightData.VERTICAL_SPEED;

        public override double PitchDegrees => FlightData.PLANE_PITCH_DEGREES;

        public override double BankDegrees => FlightData.PLANE_BANK_DEGREES;

        public override bool AutopilotAltHold => Convert.ToBoolean(FlightData.AP_ALT_HOLD);

        public override bool AutopilotHeadingHold => Convert.ToBoolean(FlightData.AP_HDG_HOLD);

        public override bool AutopilotNavHold => Convert.ToBoolean(FlightData.AP_NAV_HOLD);

        public override bool AutopilotSpeedHold => Convert.ToBoolean(FlightData.AP_SPD_HOLD);

        public override bool AutopilotMaster => Convert.ToBoolean(FlightData.AP_MASTER);

        public override double AutopilotAltitudeTargetFeet => FlightData.AP_ALT_TARGET_FT;

        public override double AutopilotSpeedTargetKnots => FlightData.AP_SPD_TARGET_KTS;

        public override double AutopilotHeadingBugDegrees => Tools.NormalizeDegrees(FlightData.AUTOPILOT_HEADING_LOCK_DIR);

        public override double AutopilotNav1CourseDegrees => Tools.NormalizeDegrees(FlightData.NAV1_OBS);

        public override double AutopilotNav2CourseDegrees => Tools.NormalizeDegrees(FlightData.NAV2_OBS);

        public override double ElevatorTrimPercent => FlightData.ELEVATOR_TRIM_PCT;

        public override double RudderTrimPercent => FlightData.RUDDER_TRIM_PCT;

        public override double AileronTrimPercent => FlightData.AILERON_TRIM_PCT;

        public override double FlapsPercent => FlightData.FLAPS_PCT;

        public override GearState GearState
        {
            get
            {
                double handle = FlightData.GEAR_HANDLE_POSITION;   // 0..1
                double pos = FlightData.GEAR_CENTER_POSITION;   // 0..1

                const double eps = 0.02;

                bool handleUp = handle <= eps;
                bool handleDown = handle >= 1.0 - eps;

                if (pos <= eps && handleUp) return GearState.Up;
                if (pos >= 1.0 - eps && handleDown) return GearState.Down;

                return GearState.Transit;
            }
        }
        public override double SpoilersPercent => FlightData.SPOILERS_PCT;

        public override bool SpoilersArmed => Convert.ToBoolean(FlightData.SPOILERS_ARMED);

        public override bool ParkingBrakeOn => Convert.ToBoolean(FlightData.PARKING_BRAKE);

        public override double FuelRemainingGallons => FlightData.FUEL_TOTAL_REMAINING_GAL > 0 ? FlightData.FUEL_TOTAL_REMAINING_GAL : (FlightData.FUEL_LEFT_REMAINING_GAL + FlightData.FUEL_RIGHT_REMAINING_GAL + FlightData.FUEL_CENTER_REMAINING_GAL);

        public override double FuelCapacityGallons => FlightData.FUEL_CAPACITY_GAL > 0 ? FlightData.FUEL_CAPACITY_GAL : (FlightData.FUEL_LEFT_CAPACITY_GAL + FlightData.FUEL_RIGHT_CAPACITY_GAL + FlightData.FUEL_CENTER_CAPACITY_GAL);

        public override double Engine1OilPressurePsf => FlightData.ENG1_OIL_PRESSURE;

        public override double Engine2OilPressurePsf => FlightData.ENG2_OIL_PRESSURE;

        public override double Engine3OilPressurePsf => FlightData.ENG3_OIL_PRESSURE;

        public override double Engine4OilPressurePsf => FlightData.ENG4_OIL_PRESSURE;

        public override bool NavLightsOn => Convert.ToBoolean(FlightData.LIGHT_NAV);

        public override bool BeaconLightsOn => Convert.ToBoolean(FlightData.LIGHT_BEACON);

        public override bool CabinLightsOn => Convert.ToBoolean(FlightData.LIGHT_CABIN);

        public override bool TaxiLightsOn => Convert.ToBoolean(FlightData.LIGHT_TAXI);

        public override bool LandingLightsOn => Convert.ToBoolean(FlightData.LIGHT_LANDING);

        public override bool StrobeLightsOn => Convert.ToBoolean(FlightData.LIGHT_STROBE);

        public override bool WindowDefrostOn => Convert.ToBoolean(FlightData.WINDOW_DEFROST);

        public override double CowlFlaps1Percent => FlightData.COWL_FLAPS1_OPEN;

        public override double CowlFlaps2Percent => FlightData.COWL_FLAPS2_OPEN;

        public override double CowlFlaps3Percent => FlightData.COWL_FLAPS3_OPEN;

        public override double CowlFlaps4Percent => FlightData.COWL_FLAPS4_OPEN;

        public override double Throttle1Percent => FlightData.THROTTLE1_PCT;

        public override double Throttle2Percent => FlightData.THROTTLE2_PCT;

        public override double Throttle3Percent => FlightData.THROTTLE3_PCT;

        public override double Throttle4Percent => FlightData.THROTTLE4_PCT;

        public override double Mixture1Percent => FlightData.MIXTURE1_PCT;

        public override double Mixture2Percent => FlightData.MIXTURE2_PCT;

        public override double Mixture3Percent => FlightData.MIXTURE3_PCT;

        public override double Mixture4Percent => FlightData.MIXTURE4_PCT;

        public override double PropPitch1Degrees => Tools.NormalizeDegrees(Tools.RadToDeg(FlightData.PROP_PITCH1_RAD));

        public override double PropPitch2Degrees => Tools.NormalizeDegrees(Tools.RadToDeg(FlightData.PROP_PITCH2_RAD));

        public override double PropPitch3Degrees => Tools.NormalizeDegrees(Tools.RadToDeg(FlightData.PROP_PITCH3_RAD));

        public override double PropPitch4Degrees => Tools.NormalizeDegrees(Tools.RadToDeg(FlightData.PROP_PITCH4_RAD));

        public double PropPitch1Percent => FlightData.PROP_PITCH1_PCT;

        public double PropPitch2Percent => FlightData.PROP_PITCH2_PCT;

        public double PropPitch3Percent => FlightData.PROP_PITCH3_PCT;

        public double PropPitch4Percent => FlightData.PROP_PITCH4_PCT;

        public override bool Engine1Failed => Convert.ToBoolean(FlightData.ENG1_FAILED);

        public override bool Engine2Failed => Convert.ToBoolean(FlightData.ENG2_FAILED);

        public override bool Engine3Failed => Convert.ToBoolean(FlightData.ENG3_FAILED);

        public override bool Engine4Failed => Convert.ToBoolean(FlightData.ENG4_FAILED);

        public override int EngineCount => FlightData.ENGINE_COUNT;

        public override bool Afterburner1On => Convert.ToBoolean(FlightData.AB1_ON);

        public override bool Afterburner2On => Convert.ToBoolean(FlightData.AB2_ON);

        public override bool Afterburner3On => Convert.ToBoolean(FlightData.AB3_ON);

        public override bool Afterburner4On => Convert.ToBoolean(FlightData.AB4_ON);

        public override bool Engine1Running => Convert.ToBoolean(FlightData.ENG1_RUNNING);

        public override bool Engine2Running => Convert.ToBoolean(FlightData.ENG2_RUNNING);

        public override bool Engine3Running => Convert.ToBoolean(FlightData.ENG3_RUNNING);

        public override bool Engine4Running => Convert.ToBoolean(FlightData.ENG4_RUNNING);

        public override bool CarbHeatAntiIce1 => Convert.ToBoolean(FlightData.ENG2_ANTIICE);

        public override bool CarbHeatAntiIce2 => Convert.ToBoolean(FlightData.ENG2_ANTIICE);

        public override bool CarbHeatAntiIce3 => Convert.ToBoolean(FlightData.ENG3_ANTIICE);

        public override bool CarbHeatAntiIce4 => Convert.ToBoolean(FlightData.ENG4_ANTIICE);

        public override double Engine1N1Percent => FlightData.ENG1_N1_PCT;

        public override double Engine2N1Percent => FlightData.ENG2_N1_PCT;

        public override double Engine3N1Percent => FlightData.ENG3_N1_PCT;

        public override double Engine4N1Percent => FlightData.ENG4_N1_PCT;

        public override double Engine1ManifoldPressureInchesMercury => FlightData.ENG1_MANIFOLD_INHG;

        public override double Engine2ManifoldPressureInchesMercury => FlightData.ENG2_MANIFOLD_INHG;

        public override double Engine3ManifoldPressureInchesMercury => FlightData.ENG3_MANIFOLD_INHG;

        public override double Engine4ManifoldPressureInchesMercury => FlightData.ENG4_MANIFOLD_INHG;

        public override Dictionary<string, Aircraft> Traffic => SimConnect.Instance.Traffic;

        public override double AltitudeMSLFeet => FlightData.PLANE_ALTITUDE;

        public override double AltitudeAGLFeet => FlightData.ALTITUDE_AGL;

        public override double AltitudeTrueFeet => FlightData.PRESSURE_ALTITUDE;

        public override double HeadingMagneticDegrees => Tools.NormalizeDegrees(FlightData.PLANE_HEADING_DEGREES_MAGNETIC);

        public override double HeadingTrueDegrees => Tools.NormalizeDegrees(FlightData.PLANE_HEADING_DEGREES_TRUE);

        public override double HeadingMagneticRadians => Tools.DegToRad(HeadingMagneticDegrees);

        public override double HeadingTrueRadians => Tools.DegToRad(HeadingTrueDegrees);

        public override bool OnGround => Convert.ToBoolean(FlightData.SIM_ON_GROUND);

        public override double GroundSpeedKnots => FlightData.AIRSPEED_TRUE;

        public override double AirSpeedIndicatedKnots => FlightData.AIRSPEED_INDICATED;

        public override double AirSpeedTrueKnots => FlightData.AIRSPEED_TRUE;

        public override double AmbientTemperatureCelsius => FlightData.AMBIENT_TEMPERATURE;

        public override double AmbientWindDirectionDegrees => FlightData.AMBIENT_WIND_DIRECTION;

        public override double AmbientWindSpeedKnots => FlightData.AMBIENT_WIND_VELOCITY;

        public override double KollsmanInchesMercury => FlightData.KOLLSMAN_SETTING_HG;

        public override double SecondaryKollsmanInchesMercury => FlightData.SEC_KOLLSMAN_SETTING_HG;

        public override double PressureInchesMercury => FlightData.PRESSURE_IN_HG;

        public override ReadyToFly IsReadyToFly => IsRunning && !Location.IsEmpty() ? FlightSim.ReadyToFly.Ready : FlightSim.ReadyToFly.Loading;

        public override double GPSRequiredMagneticHeadingRadians => (AircraftId == 50 ? FlightData.GPS_WP_BEARING + Math.PI : FlightData.GPS_WP_BEARING);

        public override double GPSRequiredTrueHeadingRadians => FlightData.GPS_WP_TRUE_REQ_HDG;

        public override bool HasActiveWaypoint => Convert.ToBoolean(FlightData.GPS_IS_ACTIVE_WAY_POINT);

        public override double GPSCrossTrackErrorMeters => FlightData.GPS_WP_CROSS_TRK;

        public override double Nav1Radial => Tools.NormalizeDegrees(FlightData.NAV_RELATIVE_BEARING_TO_STATION_1);

        public override double Nav2Radial => Tools.NormalizeDegrees(FlightData.NAV_RELATIVE_BEARING_TO_STATION_2);

        public override bool Nav1Available => Convert.ToBoolean(FlightData.NAV1_AVAILABLE);

        public override bool Nav2Available => Convert.ToBoolean(FlightData.NAV2_AVAILABLE);

        public override bool Nav1Receive => Convert.ToBoolean(FlightData.NAV1_SOUND);

        public override bool Nav2Receive => Convert.ToBoolean(FlightData.NAV2_SOUND);

        public override double Nav1Frequency => FlightData.NAV1_FREQUENCY / 1000.0d;

        public override double Nav2Frequency => FlightData.NAV2_FREQUENCY / 1000.0d;

        public override double Nav1StandByFrequency => FlightData.NAV1_STANDBY_FREQUENCY / 1000.0d;

        public override double Nav2StandByFrequency => FlightData.NAV2_STANDBY_FREQUENCY / 1000.0d;

        public override double AdfRelativeBearing => Tools.NormalizeDegrees(FlightData.ADF_RADIAL);

        public override TunedFacility Com1ActiveFrequencyType => ParseTunedFacility(FlightData.COM1_ACTIVE_FREQ_TYPE);

        public override TunedFacility Com2ActiveFrequencyType => ParseTunedFacility(FlightData.COM2_ACTIVE_FREQ_TYPE);

        public override double Com1Frequency => FlightData.COM1_FREQUENCY / 1000.0d;

        public override double Com2Frequency => FlightData.COM2_FREQUENCY / 1000.0d;

        public override double Com1StandByFrequency => FlightData.COM1_STANDBY_FREQUENCY / 1000.0d;

        public override double Com2StandByFrequency => FlightData.COM2_STANDBY_FREQUENCY / 1000.0d;

        public override ComStatus Com1Status => Enum.IsDefined(typeof(ComStatus), FlightData.COM1_STATUS) ? (ComStatus)FlightData.COM1_STATUS : ComStatus.Invalid;

        public override ComStatus Com2Status => Enum.IsDefined(typeof(ComStatus), FlightData.COM2_STATUS) ? (ComStatus)FlightData.COM2_STATUS : ComStatus.Invalid;

        public override string Com1ActiveFrequencyIdent => FlightData.COM1_ACTIVE_FREQ_IDENT;

        public override string Com2ActiveFrequencyIdent => FlightData.COM2_ACTIVE_FREQ_IDENT;

        public override bool Com1Receive => Convert.ToBoolean(FlightData.COM1_RECEIVE);

        public override bool Com2Receive => Convert.ToBoolean(FlightData.COM2_RECEIVE);

        public override bool Com1Transmit => Convert.ToBoolean(FlightData.COM1_TRANSMIT);

        public override bool Com2Transmit => Convert.ToBoolean(FlightData.COM2_TRANSMIT);

        public override bool AvionicsOn => Convert.ToBoolean(FlightData.AVIONICS_MASTER);

        public override bool BatteryOn => Convert.ToBoolean(FlightData.BATTERY_MASTER);

        public override bool GeneratorOn => Convert.ToBoolean(FlightData.GENERATOR_MASTER);

        public override bool PitotHeatOn => ((PitotHeatSwitchState)FlightData.PITOT_HEAT) != PitotHeatSwitchState.Off;

        public override uint Transponder => Tools.Bcd2Dec((uint)FlightData.XPDR_CODE);

        public override bool IdentActive => Convert.ToBoolean(FlightData.IDENT_ACTIVE);
        
        public override TransponderMode TransponderMode => (TransponderMode)FlightData.XPDR_STATE;

        private IntPtr _mainWindowHandle;
        
        public IntPtr MainWindowHandle 
        { 
            get
            {
                return _mainWindowHandle;
            }
            set
            {
                _mainWindowHandle = value;
                if (_mainWindowHandle != IntPtr.Zero)
                {
                    Initialize();
                }
            }
        }

        private int _aircraftId = 0;

        public override int AircraftId
        {
            get
            {
                return _aircraftId;
            }
        }

        private bool _isConnected;
        public override bool IsConnected
        {
            get
            {
                return _isConnected;
            }
        }

        public bool IsRunning { get; private set; }

        private EngineType _engineType;
        public override EngineType EngineType
        {
            get
            {
                return _engineType;
            }
        }

        private bool _isGearFloats;
        public override bool IsGearFloats
        {
            get
            {
                return _isGearFloats;
            }
        }

        private bool _isHelo;
        public override bool IsHelo
        {
            get
            {
                return _isHelo;
            }
        }

        private bool _isHeavy;
        public override bool IsHeavy
        {
            get
            {
                return _isHeavy;
            }
        }

        private string _aircraftName = null!;
        public override string AircraftName
        {
            get
            {
                return _aircraftName;
            }
        }

        private string _atcModel = null!;
        public override string ATCModel
        {
            get
            {
                return _atcModel;
            }
        }

        private string _atcType = null!;
        public override string ATCType
        {
            get
            {
                return _atcType;
            }
        }

        private string _atcIdentifier = null!;
        public override string ATCIdentifier
        {
            get
            {
                return _atcIdentifier;
            }
        }

        public override double CollectivePercent => FlightData.COLLECTIVE_POSITION_PCT;

        public override bool RotorBrakeActive => Convert.ToBoolean(FlightData.ROTOR_BRAKE_ACTIVE);

        public override double RotorBrakeHandlePercent => FlightData.ROTOR_BRAKE_HANDLE_POS;

        public override double MainRotorDiskBankPercent =>  FlightData.DISK_BANK_PCT_1;

        public override double MainRotorDiskPitchPercent => FlightData.DISK_PITCH_PCT_1;

        public override double MainRotorDiskConingPercent => FlightData.DISK_CONING_PCT_1;

        public override double MainRotorRotationAngleDegrees => Tools.NormalizeDegrees(Tools.RadToDeg(FlightData.ROTOR_ROTATION_ANGLE_1));

        public override double TailRotorDiskBankPercent => FlightData.DISK_BANK_PCT_2;

        public override double TailRotorDiskPitchPercent => FlightData.DISK_PITCH_PCT_2;

        public override double TailRotorDiskConingPercent => FlightData.DISK_CONING_PCT_2;

        public override double TailRotorRotationAngleDegrees => Tools.NormalizeDegrees(Tools.RadToDeg(FlightData.ROTOR_ROTATION_ANGLE_2));

        public override double MainRotorRpm => FlightData.ROTOR_RPM_1;

        public override double TailRotorRpm => FlightData.ROTOR_RPM_2;

        public override double MainRotorBladePitchPercent => FlightData.ROTOR_COLLECTIVE_BLADE_PITCH_PCT;

        public override double TailRotorBladePitchPercent => FlightData.TAIL_ROTOR_BLADE_PITCH_PCT;

        public override double RotorLateralTrimPercent => FlightData.ROTOR_LATERAL_TRIM_PCT;

        public override double RotorLongitudinalTrimPercent => FlightData.ROTOR_LONGITUDINAL_TRIM_PCT;

        public override double Engine1RotorRpmCommandPercent => FlightData.ENG_ROTOR_RPM_1;

        public override double Engine2RotorRpmCommandPercent => FlightData.ENG_ROTOR_RPM_2;

        public override double Engine1TorquePercent => Tools.Scalar16KToPercent(FlightData.ENG_TORQUE_PERCENT_1);

        public override double Engine2TorquePercent => Tools.Scalar16KToPercent(FlightData.ENG_TORQUE_PERCENT_2);
        
        public override bool RotorGovernor1Active => Convert.ToBoolean(FlightData.ROTOR_GOV_ACTIVE_1);

        public override bool RotorGovernor2Active => Convert.ToBoolean(FlightData.ROTOR_GOV_ACTIVE_2);

        public override double Nav1DmeDistanceNm => FlightData.NAV_DME_1;

        public override double Nav2DmeDistanceNm => FlightData.NAV_DME_2;

        public override double Nav1DmeSpeedKts => FlightData.NAV_DME_SPD_1;

        public override double Nav2DmeSpeedKts => FlightData.NAV_DME_SPD_2;

        public override bool GpsDrivesNav => Convert.ToBoolean(FlightData.GPS_DRIVES_NAV);

        public override bool FlightDirectorActive => Convert.ToBoolean(FlightData.FLIGHT_DIRECTOR_ACTIVE);

        public override bool AutopilotVerticalSpeedHold => Convert.ToBoolean(FlightData.AP_VS_HOLD);

        public override double AutopilotVerticalSpeedTargetFpm => FlightData.AP_VS_TARGET_FM;

        public override bool PanelLightsOn => Convert.ToBoolean(FlightData.LIGHT_PANEL);

        public override bool FlightPlanIsActiveFlightPlan => Convert.ToBoolean(FlightData.FLIGHTPLAN_IS_ACTIVE_FLIGHTPLAN);

        public override int FlightPlanWaypointsNumber => FlightData.FLIGHTPLAN_WAYPOINTS;

        public override int FlightPlanActiveWaypoint => FlightData.FLIGHTPLAN_ACTIVE_WAYPOINT;

        public override bool FlightPlanIsDirectTo => Convert.ToBoolean(FlightData.FLIGHTPLAN_DIRECT_TO);

        public override string FlightPlanApproachIdent => FlightData.FLIGHTPLAN_APPROACH_ID ?? string.Empty;

        public override int FlightPlanWaypointIndex => FlightData.FLIGHTPLAN_WAYPOINT_INDEX;

        public override string FlightPlanWaypointIdent
        {
            get
            {
                bool wpValid = !(WaypointLatitude == 0 && WaypointLongitude == 0);

                if (!wpValid)
                    return string.Empty;

                double lat = WaypointLatitude;
                double lon = WaypointLongitude;

                if (FlightPlanActiveApproachWaypoint == FlightPlanWaypointIndex && FlightPlanApproachIsWaypointRunway)
                {
                    return SimConnect.Instance.Facilities.FindNearestAirportIdent(lat, lon, maxNm: 5.0) ?? string.Empty;
                }
                // Airports first – big targets, rare false positives
                var ap = SimConnect.Instance.Facilities.FindNearestAirportIdent(lat, lon, maxNm: 5.0);
                if (ap != null) return ap;

                // VORs – medium radius
                var vor = SimConnect.Instance.Facilities.FindNearestVorIdent(lat, lon, maxNm: 10.0);
                if (vor != null) return vor;

                // NDBs – largest radius
                var ndb = SimConnect.Instance.Facilities.FindNearestNdbIdent(lat, lon, maxNm: 15.0);
                if (ndb != null) return ndb;

                // Fixes/intersections – tight
                return SimConnect.Instance.Facilities.FindNearestWaypointIdent(lat, lon, maxNm: 3.0) ?? WaypointIdent;
            }
        }

        public override int FlightPlanApproachWaypointsNumber => FlightData.FLIGHTPLAN_APPROACH_WAYPOINTS;

        public override int FlightPlanActiveApproachWaypoint => FlightData.FLIGHTPLAN_ACTIVE_APPROACH_WAYPOINT;

        public override bool FlightPlanApproachIsWaypointRunway => Convert.ToBoolean(FlightData.FLIGHTPLAN_APPROACH_IS_WAYPOINT_RUNWAY);

        public override bool BalloonAutoFillActive => Convert.ToBoolean(FlightData.BALLOON_AUTO_FILL_ACTIVE);

        public override double BalloonFillAmountPercent => FlightData.BALLOON_FILL_AMOUNT;

        public override double BalloonGasDensity => FlightData.BALLOON_GAS_DENSITY;

        public override double BalloonGasTemperatureCelsius => FlightData.BALLOON_GAS_TEMPERATURE;

        public override double BalloonVentOpenPercent => FlightData.BALLOON_VENT_OPEN_VALUE;

        public override double BalloonBurnerFuelFlowRatePounds => FlightData.BURNER_FUEL_FLOW_RATE;

        public override bool BalloonBurnerPilotLightOn => Convert.ToBoolean(FlightData.BURNER_PILOT_LIGHT_ON);

        public override double BalloonBurnerValveOpenPercent => FlightData.BURNER_VALVE_OPEN_VALUE;

        public override AirshipGasType AirshipCompartmentGasType => Enum.IsDefined(typeof(AirshipGasType), FlightData.AIRSHIP_COMPARTMENT_GAS_TYPE) ? (AirshipGasType)FlightData.AIRSHIP_COMPARTMENT_GAS_TYPE: AirshipGasType.Other;

        public override double AirshipCompartmentPressureHectoPascals => FlightData.AIRSHIP_COMPARTMENT_PRESSURE;

        public override double AirshipCompartmentOverPressureHectoPascals => FlightData.AIRSHIP_COMPARTMENT_OVERPRESSURE;

        public override double AirshipCompartmentTemperatureCelsius => FlightData.AIRSHIP_COMPARTMENT_TEMPERATURE;

        public override double AirshipCompartmentVolumeCubicMeters => FlightData.AIRSHIP_COMPARTMENT_VOLUME;

        public override double AirshipCompartmentWeightPounds => FlightData.AIRSHIP_COMPARTMENT_WEIGHT;

        public override double AirshipFanPowerPercent => FlightData.AIRSHIP_FAN_POWER_PCT;

        public override bool AirshipMastTruckDeployment => Convert.ToBoolean(FlightData.MAST_TRUCK_DEPLOYMENT);

        public override double AirshipMastTruckExtensionPercent => FlightData.MAST_TRUCK_EXTENSION;

        protected AbortableTaskRunner? _timerConnection = null;
        private bool _stop = false;

        static SimConnectProvider()
        {
            Instance = new SimConnectProvider();
        }
        
        SimConnectProvider()
        {
            //Initialize();
            SimConnect.Instance.OnError += SimConnect_OnError;
            SimConnect.Instance.OnConnected += SimConnect_OnConnected;
            SimConnect.Instance.OnQuit += SimConnect_OnQuit;
            SimConnect.Instance.OnFlightDataReceived += SimConnect_OnFlightDataReceived;
            SimConnect.Instance.OnTrafficReceived += SimConnect_OnTrafficReceived;
            SimConnect.Instance.OnSim += SimConnect_OnSim;
            SimConnect.Instance.OnFacilitiesReceived += Instance_OnFacilitiesReceived;
        }

        private void Instance_OnFacilitiesReceived(FacilityType type)
        {
            FlightDataReceived();
        }

        private TunedFacility ParseTunedFacility(string? simValue)
        {
            if (string.IsNullOrWhiteSpace(simValue))
                return TunedFacility.NONE;

            switch (simValue.Trim().ToUpperInvariant())
            {
                case "DEL":
                    return TunedFacility.DEL;

                case "GND":
                    return TunedFacility.GND;

                case "TWR":
                    return TunedFacility.TWR;

                case "ATIS":
                    return TunedFacility.ATIS;

                case "UNI":
                    return TunedFacility.UNI;

                case "CTAF":
                    return TunedFacility.CTAF;

                case "CLR":
                    return TunedFacility.CLR;

                case "APPR":
                case "APP":
                    return TunedFacility.APPR;

                case "DEP":
                    return TunedFacility.DEP;

                case "FSS":
                    return TunedFacility.FSS;

                case "AWS":
                case "AWOS":
                    return TunedFacility.AWS;

                default:
                    return TunedFacility.NONE;
            }
        }

        protected void SimConnect_OnTrafficReceived(uint objectId, Aircraft? aircraft, TrafficEvent eventType)
        {
            TrafficReceived(objectId.ToString(), aircraft, eventType);
        }

        private async Task ConnectionTimer_DoWorkAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested && !IsConnected && !_stop)
            {
                try
                {
                    if (!SimConnect.Instance.IsConnected && MainWindowHandle != IntPtr.Zero)
                    {
                        SimConnect.Instance.Initialize(MainWindowHandle);
                    }
                }
                catch
                {
                }
                finally
                {
                    await Task.Delay(1000, token);
                }
            }
        }

        protected void SimConnect_OnSim(bool isRunning)
        {
            IsRunning = isRunning;
            SetLeds();
            ReadyToFly(IsReadyToFly);
        }

        protected void SimConnect_OnQuit()
        {
            _isConnected = false;
            StopTimer();
            if (_timerConnection != null && !_timerConnection.IsRunning)
            {
                _timerConnection.Start(ConnectionTimer_DoWorkAsync);
            }
            SetLeds();
            Quit();
        }

        protected void SimConnect_OnConnected()
        {
            _isConnected = true;
            UdatePage();
            SetLeds();
            Connected();
        }

        protected void SimConnect_OnError(Exception ex)
        {
            Error(WrapProviderError(ex));
        }

        private string _currentATCType = string.Empty;
        private string _currentATCModel = string.Empty;
        private object _lock = new object();
        private readonly Stopwatch _positionTimer = Stopwatch.StartNew();
        private long _nextPositionUpdateMs = 0;

        protected void SimConnect_OnFlightDataReceived(FLIGHT_DATA data)
        {
            lock (_lock)
            {
                ReadyToFly readyToFly = IsReadyToFly;
                double lat = Latitude;
                double lng = Longitude;
                FlightData = data;
                long now = _positionTimer.ElapsedMilliseconds;
                // Always set immediately on first fix, otherwise throttle to 1 Hz
                if (Location.IsEmpty() || now >= _nextPositionUpdateMs)
                {
                    SetPosition(FlightData.PLANE_LATITUDE, FlightData.PLANE_LONGITUDE);
                    _nextPositionUpdateMs = now + 1000;
                }
                double distance = Tools.DistanceTo(lat, lng, Latitude, Longitude);
                //Have we moved more than 500M in 1 millisecond?
                if (readyToFly != IsReadyToFly || distance >= 500)
                {
                    ReadyToFly(IsReadyToFly);
                }
            }
            if (!data.ATC_TYPE.Equals(_currentATCType, StringComparison.OrdinalIgnoreCase) || !data.ATC_MODEL.Equals(_currentATCModel, StringComparison.OrdinalIgnoreCase))
            {
                _currentATCType = data.ATC_TYPE;
                _currentATCModel = data.ATC_MODEL;
                AircraftData aircraftData = Tools.LoadAircraft(data.ATC_TYPE, data.ATC_MODEL);
                if (aircraftData == null)
                {
                    aircraftData = Tools.GetDefaultAircraft(data.ATC_TYPE, data.ATC_MODEL);
                    aircraftData.EngineType = (EngineType)data.ENGINE_TYPE;
                    aircraftData.Heavy = Convert.ToBoolean(data.ATC_HEAVY);
                    aircraftData.Helo = _engineType == EngineType.Helo;
                    aircraftData.IsGearFloats = Convert.ToBoolean(data.IS_GEAR_FLOATS);
                }
                _aircraftName = aircraftData.FriendlyName;
                _aircraftId = aircraftData.AircraftId;
                _engineType = aircraftData.EngineType;
                _isHeavy = aircraftData.Heavy;
                _isHelo = aircraftData.Helo;
                _atcModel = aircraftData.FriendlyModel;
                _atcType = aircraftData.FriendlyType;
                _atcIdentifier = aircraftData.ATCIdentifier;
                _isGearFloats = aircraftData.IsGearFloats;
                aircraftData.ATCIdentifier = data.ATC_IDENTIFIER;
                if (data.ATC_MODEL.Equals("Airbus-H135") || data.ATC_MODEL.Equals("EC135P3H"))
                {
                    _engineType = EngineType.Helo;
                    _isHelo = true;
                    _isGearFloats = false;
                }
                AircraftChange(_aircraftId);
            }
            FlightDataReceived();
        }

        public void ReceiveMessage()
        {
            SimConnect.Instance.ReceiveMessage();
        }

        public void Initialize()
        {
            if (MainWindowHandle != IntPtr.Zero)
            {
                SimConnect.Instance.Initialize(MainWindowHandle);
            }
            if (_timerConnection == null)
            {
                _timerConnection = new AbortableTaskRunner();
                _timerConnection.Start(ConnectionTimer_DoWorkAsync);
            }
        }

        public override void Deinitialize(int timeOut = 1000)
        {
            StopTimerConnectionAsync(timeOut).GetAwaiter().GetResult();
            try
            {
                SimConnect.Instance.Deinitialize();
            }
            catch (Exception)
            {
            }
        }

        public async Task StopTimerConnectionAsync(int timeOut = 1000)
        {
            _stop = true;
            if (_timerConnection != null && _timerConnection.IsRunning)
            {
                await _timerConnection.StopAsync(timeOut).ConfigureAwait(false);
            }
            _timerConnection?.Dispose();
            _timerConnection = null!;
        }

        public override void SendControlToFS(string control, float value)
        {
            if (!Enum.TryParse(control, out SimConnectEventId id))
                return;
            SimConnect.Instance.SetValue(id, (uint)Math.Round(value, MidpointRounding.AwayFromZero));
        }

        public override void SendCommandToFS(string command)
        {
            SimConnectEventId id;
            if (Enum.TryParse(command, out id))
            {
                SimConnect.Instance.SendCommand(id);
            }
        }

        public void Subscribe(SimConnectEventId eventId, Action<SimConnectEvent, uint> onchange = null!)
        {
            SimConnect.Instance.Subscribe(eventId, onchange);
        }

    }
}
