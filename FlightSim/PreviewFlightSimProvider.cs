using System;
using System.Collections.Generic;

namespace FlightSim
{
    public sealed class PreviewFlightSimProvider : FlightSimProviderBase, IDisposable
    {
        public static readonly PreviewFlightSimProvider Instance;

        public PreviewFlightSimProvider()
        {
            SetPosition(35.2456, -97.4721);
            Initialize();
        }

        public override void SendControlToFS(string control, float value) { }

        public override void SendCommandToFS(string command) { }

        public override string Name => "PREVIEW PROVIDER";

        public override bool IsConnected => true;

        public override double WaypointLatitude => 35.4147;

        public override double WaypointLongitude => -97.3866;

        public override string WaypointIdent => "KTIK";

        public override string Nav1ActiveFrequencyIdent => "ILS";

        public override string Nav2ActiveFrequencyIdent => "VOR";

        public override double VerticalSpeedFpm => 0;

        public override double PitchDegrees => 2.5;

        public override double BankDegrees => 0.0;

        public override bool AutopilotMaster => true;

        public override bool AutopilotAltHold => true;

        public override bool AutopilotHeadingHold => true;

        public override bool AutopilotNavHold => false;

        public override bool AutopilotSpeedHold => true;

        public override double AutopilotAltitudeTargetFeet => 6500;

        public override double AutopilotSpeedTargetKnots => 120;

        public override double AutopilotHeadingBugDegrees => 270;

        public override double ElevatorTrimPercent => 0;

        public override double AileronTrimPercent => 0;

        public override double RudderTrimPercent => 0;

        public override double FlapsPercent => 0;
        
        public override double SpoilersPercent => 0;
        
        public override bool SpoilersArmed => false;
        
        public override bool ParkingBrakeOn => false;
        
        public override GearState GearState => GearState.Up;

        public override double FuelRemainingGallons => 42;

        public override double FuelCapacityGallons => 60;

        public override bool NavLightsOn => true;

        public override bool BeaconLightsOn => true;

        public override bool TaxiLightsOn => false;

        public override bool LandingLightsOn => false;

        public override bool StrobeLightsOn => true;

        public override int EngineCount => 1;

        public override bool Engine1Running => true;

        public override bool Engine2Running => false;

        public override bool Engine3Running => false;

        public override bool Engine4Running => false;

        public override double Engine1Rpm => 2350;

        public override double Engine2Rpm => 0;

        public override double Engine3Rpm => 0;

        public override double Engine4Rpm => 0;

        public override double Throttle1Percent => 65;

        public override double Throttle2Percent => 0;

        public override double Throttle3Percent => 0;

        public override double Throttle4Percent => 0;

        public override double Mixture1Percent => 75;

        public override double Mixture2Percent => 0;

        public override double Mixture3Percent => 0;

        public override double Mixture4Percent => 0;

        public double PropPitch1Percent => 85;

        public double PropPitch2Percent => 0;

        public double PropPitch3Percent => 0;

        public double PropPitch4Percent => 0;

        public override double PropPitch1Degrees => 0;

        public override double PropPitch2Degrees => 0;

        public override double PropPitch3Degrees => 0;

        public override double PropPitch4Degrees => 0;

        public override double Engine1N1Percent => 0;

        public override double Engine2N1Percent => 0;

        public override double Engine3N1Percent => 0;

        public override double Engine4N1Percent => 0;

        public override double Engine1ManifoldPressureInchesMercury => 23.5;

        public override double Engine2ManifoldPressureInchesMercury => 0;

        public override double Engine3ManifoldPressureInchesMercury => 0;

        public override double Engine4ManifoldPressureInchesMercury => 0;

        public override double Torque1FootPounds => 0;

        public override double Torque2FootPounds => 0;

        public override double Torque3FootPounds => 0;

        public override double Torque4FootPounds => 0;

        public override bool Afterburner1On => false;

        public override bool Afterburner2On => false;

        public override bool Afterburner3On => false;

        public override bool Afterburner4On => false;

        public override bool Engine1Failed => false;

        public override bool Engine2Failed => false;

        public override bool Engine3Failed => false;

        public override bool Engine4Failed => false;

        public override bool WindowDefrostOn => false;

        public override double CowlFlaps1Percent => 0;

        public override double CowlFlaps2Percent => 0;

        public override double CowlFlaps3Percent => 0;

        public override double CowlFlaps4Percent => 0;

        public override Dictionary<string, Aircraft> Traffic => new Dictionary<string, Aircraft>();

        public override int AircraftId => 1;

        public override string AircraftName => "F-16 Falcon";

        public override string ATCIdentifier => "VIPER11";

        public override string ATCModel => "F16";

        public override string ATCType => "GENERALDYNAMICS";

        public override bool IsHeavy => false;

        public override bool IsGearFloats => false;

        public override bool IsHelo => false;

        public override bool OnGround => false;

        public override bool IdentActive => true;

        public override EngineType EngineType => EngineType.Jet;

        public override double AltitudeMSLFeet => 18250;

        public override double AltitudeAGLFeet => 17070;

        public override double AltitudeTrueFeet => 18150;

        public override double HeadingMagneticDegrees => 273;

        public override double HeadingTrueDegrees => 270;

        public override double HeadingMagneticRadians => Tools.DegToRad(Tools.NormalizeDegrees(HeadingMagneticDegrees));

        public override double HeadingTrueRadians => Tools.DegToRad(Tools.NormalizeDegrees(HeadingTrueDegrees));

        public override double GroundSpeedKnots => 420;

        public override double AirSpeedIndicatedKnots => 395;

        public override double AirSpeedTrueKnots => 430;

        public override double AmbientTemperatureCelsius => -32;

        public override double AmbientWindDirectionDegrees => 310;

        public override double AmbientWindSpeedKnots => 18;

        public override double KollsmanInchesMercury => 29.92;

        public override double SecondaryKollsmanInchesMercury => 29.92;

        public override double PressureInchesMercury => 30.02;

        public override ReadyToFly IsReadyToFly => FlightSim.ReadyToFly.Ready;

        public override bool HasActiveWaypoint => true;

        public override double GPSRequiredMagneticHeadingRadians => Tools.DegToRad(275);

        public override double GPSRequiredTrueHeadingRadians => Tools.DegToRad(270);

        public override double GPSCrossTrackErrorMeters => -12.5;

        public override double Nav1Radial => AutopilotNav1CourseDegrees;

        public override double Nav2Radial => AutopilotNav2CourseDegrees;

        public override bool Nav1Available => true;

        public override bool Nav2Available => true;

        public override bool Nav1Receive => true;

        public override bool Nav2Receive => false;

        public override double Nav1Frequency => 113.90;

        public override double Nav2Frequency => 113.40;

        public override double Nav1StandByFrequency => 112.30;

        public override double Nav2StandByFrequency => 114.80;

        public override double AdfRelativeBearing => 045;

        public override bool AvionicsOn => true;

        public override bool BatteryOn => true;

        public override bool GeneratorOn => true;

        public override bool PitotHeatOn => false;

        public override uint Transponder => 1200;

        public override TransponderMode TransponderMode => TransponderMode.Alt_Mode_C;

        public override ComStatus Com1Status => ComStatus.OK;

        public override ComStatus Com2Status => ComStatus.OK;

        public override bool Com1Receive => true;

        public override bool Com2Receive => true;

        public override bool Com1Transmit => true;

        public override bool Com2Transmit => false;

        public override TunedFacility Com1ActiveFrequencyType => TunedFacility.TWR;

        public override TunedFacility Com2ActiveFrequencyType => TunedFacility.NONE;

        public override double Com1Frequency => 118.60;

        public override double Com2Frequency => 121.50;

        public override double Com1StandByFrequency => 122.80;

        public override double Com2StandByFrequency => 121.50;

        public override string Com1ActiveFrequencyIdent => "KOUN";

        public override string Com2ActiveFrequencyIdent => "OFF";

        public override double Engine1OilPressurePsf => 60;
        
        public override double Engine2OilPressurePsf => 0;
        
        public override double Engine3OilPressurePsf => 0;
        public override double Engine4OilPressurePsf => 0;

        public override double AutopilotNav1CourseDegrees => 90;

        public override double AutopilotNav2CourseDegrees => 330;

        public override bool CarbHeatAntiIce1 => true;

        public override bool CarbHeatAntiIce2 => false;

        public override bool CarbHeatAntiIce3 => false;

        public override bool CarbHeatAntiIce4 => false;

        public override bool Propeller1DeIce => false;

        public override bool Propeller2DeIce => false;

        public override bool Propeller3DeIce => false;

        public override bool Propeller4DeIce => false;

        public override bool FuelPump1On => true;

        public override bool FuelPump2On => false;

        public override bool FuelPump3On => false;

        public override bool FuelPump4On => false;

        public override string AdfIdent => "OUN";

        public override string AdfName => "Norman NDB";

        public override bool PanelLightsOn => true;

        public override bool CabinLightsOn => false;

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

        public override double Nav1DmeDistanceNm => Tools.DistanceTo(Latitude, Longitude, WaypointLatitude, WaypointLongitude, 'N');

        public override double Nav2DmeDistanceNm => Tools.DistanceTo(Latitude, Longitude, 35.3931, -97.6007, 'N');

        public override double Nav1DmeSpeedKts => 180;

        public override double Nav2DmeSpeedKts => 380;

        public override bool GpsDrivesNav => true;

        public override bool FlightDirectorActive => false;

        public override bool AutopilotVerticalSpeedHold => false;

        public override double AutopilotVerticalSpeedTargetFpm => 0d;

        public override bool FlightPlanIsActiveFlightPlan => true;

        public override int FlightPlanWaypointsNumber => 1;

        public override int FlightPlanActiveWaypoint => 1;

        public override bool FlightPlanIsDirectTo => true;

        public override string FlightPlanApproachIdent => "KTIK";

        public override int FlightPlanWaypointIndex => 1;

        public override string FlightPlanWaypointIdent => "KTIK";

        public override int FlightPlanApproachWaypointsNumber => 1;

        public override int FlightPlanActiveApproachWaypoint => 1;

        public override bool FlightPlanApproachIsWaypointRunway => true;

        public override bool BalloonAutoFillActive => false;

        public override double BalloonFillAmountPercent => 0d;

        public override double BalloonGasDensity => 0d;

        public override double BalloonGasTemperatureCelsius => 0d;

        public override double BalloonVentOpenPercent => 0d;

        public override double BalloonBurnerFuelFlowRatePounds => 0d;

        public override bool BalloonBurnerPilotLightOn => false;

        public override double BalloonBurnerValveOpenPercent => 0d;

        public override AirshipGasType AirshipCompartmentGasType => AirshipGasType.Other;

        public override double AirshipCompartmentPressureHectoPascals => 0d;

        public override double AirshipCompartmentOverPressureHectoPascals => 0d;

        public override double AirshipCompartmentTemperatureCelsius => 0d;

        public override double AirshipCompartmentVolumeCubicMeters => 0d;

        public override double AirshipCompartmentWeightPounds => 0d;

        public override double AirshipFanPowerPercent => 0d;

        public override bool AirshipMastTruckDeployment => false;

        public override double AirshipMastTruckExtensionPercent => 0d;

        static PreviewFlightSimProvider()
        {
            Instance = new PreviewFlightSimProvider();
        }

        private System.Timers.Timer? _previewTimer;
        
        public void Initialize()
        {
            _previewTimer = new System.Timers.Timer(1000);
            _previewTimer.AutoReset = true;
            _previewTimer.Elapsed += (_, __) =>
            {
                FlightDataReceived();
            };
            _previewTimer.Start();
        }

        public override void Deinitialize(int timeOut = 1000)
        {
            _previewTimer?.Stop();
            _previewTimer?.Dispose();
            _previewTimer = null;
        }

        public void Dispose()
        {
            Deinitialize();
        }
    }
}
