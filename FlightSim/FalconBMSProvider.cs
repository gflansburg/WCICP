using FlightSim.F4SharedMem;
using FlightSim.F4SharedMem.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FlightSim
{
    public class FalconBMSProvider : FlightSimProviderBase
    {
        public static readonly FalconBMSProvider Instance;

        private IReadOnlyList<FalconBMSCrossref> _crossref = Array.Empty<FalconBMSCrossref>();

        private string FalconBMSAircraftName => GetAircraftName(_lastFlightData.vehicleACD);
        
        public int FalconBMSAircraftID => _lastFlightData.vehicleACD;

        private const double NominalOilPressurePsi = 80.0;

        public override string Name => "Falcon BMS";

        public override string AdfIdent => string.Empty;

        public override string AdfName => string.Empty;

        public override double AutopilotNav1CourseDegrees => Tools.NormalizeDegrees(_lastFlightData.courseState);

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

        public override double Engine1Rpm => _lastFlightData.rpm;

        public override double Engine2Rpm => 0d;

        public override double Engine3Rpm => 0d;

        public override double Engine4Rpm => 0d;

        public override double Torque1FootPounds => 0d;

        public override double Torque2FootPounds => 0d;

        public override double Torque3FootPounds => 0d;

        public override double Torque4FootPounds => 0d;

        public override double VerticalSpeedFpm => (-_lastFlightData.zDot) * 60.0;

        public override double PitchDegrees => Tools.RadToDeg(_lastFlightData.pitch);

        public override double BankDegrees => Tools.RadToDeg(_lastFlightData.roll);

        public override bool AutopilotAltHold => false;

        public override bool AutopilotHeadingHold => false;

        public override bool AutopilotNavHold => false;

        public override bool AutopilotSpeedHold => false;

        public override bool AutopilotMaster => false;

        public override double AutopilotAltitudeTargetFeet => 0d;

        public override double AutopilotSpeedTargetKnots => 0d;

        public override double AutopilotHeadingBugDegrees => Tools.NormalizeDegrees(_lastFlightData.headingState);

        public override double ElevatorTrimPercent => Math.Clamp(_lastFlightData.TrimPitch, -0.5f, 0.5f) * 200.0;

        public override double AileronTrimPercent => Math.Clamp(_lastFlightData.TrimRoll, -0.5f, 0.5f) * 200.0;

        public override double RudderTrimPercent => Math.Clamp(_lastFlightData.TrimYaw, -0.5f, 0.5f) * 200.0;

        public override double FlapsPercent => Math.Clamp(_lastFlightData.tefPos, 0.0f, 1.0f) * 100.0;

        private static GearState FromGearPos(float pos)
        {
            if (pos >= 0.95f) return GearState.Down;
            if (pos <= 0.05f) return GearState.Up;
            return GearState.Transit;
        }

        static bool AnyLampOn(FlightData fd, LightBits bit)
        {
            uint m = (uint)bit;
            return ((fd.lightBits & m) != 0); // LightBits lives here by definition
        }

        static bool AnyLampOn(FlightData fd, LightBits2 bit)
        {
            uint m = (uint)bit;
            return ((fd.lightBits2 & m) != 0); // LightBits2 lives here by definition
        }

        static bool AnyLampOn(FlightData fd, LightBits3 bit)
        {
            uint m = (uint)bit;
            return ((fd.lightBits3 & m) != 0); // LightBits3 lives here by definition
        }

        public override GearState NoseGearState => FromGearPos(_lastFlightData.NoseGearPos);

        public override GearState LeftGearState => FromGearPos(_lastFlightData.LeftGearPos);

        public override GearState RightGearState => FromGearPos(_lastFlightData.RightGearPos);

        public bool NoseGearDown => NoseGearState == GearState.Down;

        public bool LeftGearDown => LeftGearState == GearState.Down;

        public bool RightGearDown => RightGearState == GearState.Down;

        public override GearState GearState
        {
            get
            {
                // Prefer per-wheel states when BMS provides them
                var nose = NoseGearState;
                var left = LeftGearState;
                var right = RightGearState;

                // All three down => Down
                if (nose == GearState.Down && left == GearState.Down && right == GearState.Down)
                    return GearState.Down;

                // All three up => Up
                if (nose == GearState.Up && left == GearState.Up && right == GearState.Up)
                    return GearState.Up;

                // Otherwise transit (covers mixed states too)
                return GearState.Transit;
            }
        }

        public override double SpoilersPercent => Math.Clamp(_lastFlightData.speedBrake, 0.0f, 1.0f) * 100.0;

        public override bool SpoilersArmed => SpoilersPercent > 0.5;

        public override bool ParkingBrakeOn => false;

        private const double LbsPerGallonJetA = 6.7;

        public override double FuelRemainingGallons => Math.Max(0.0, (_lastFlightData.internalFuel + +_lastFlightData.externalFuel) / LbsPerGallonJetA);
        
        public override double FuelCapacityGallons => 0d;

        public override double Engine1OilPressurePsf => Tools.PsiToPsf(Math.Clamp(_lastFlightData.oilPressure, 0.0, 120.0) / 100.0 * NominalOilPressurePsi);

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

        public override double Throttle1Percent => (Math.Clamp(_lastFlightData.rpm, 0.0f, 103.0f) / 103.0) * 100.0;

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

        public override bool Afterburner1On => _lastFlightData.rpm > 100f;

        public override bool Afterburner2On => false;

        public override bool Afterburner3On => false;

        public override bool Afterburner4On => false;

        public override bool Engine1Running => _lastFlightData.rpm > 0.05f;

        public override bool Engine2Running => false;

        public override bool Engine3Running => false;

        public override bool Engine4Running => false;

        public override double Engine1N1Percent => Math.Clamp(_lastFlightData.rpm, 0.0, 103.0);

        public override double Engine2N1Percent => 0d;

        public override double Engine3N1Percent => 0d;

        public override double Engine4N1Percent => 0d;

        public override double Engine1ManifoldPressureInchesMercury => 0d;

        public override double Engine2ManifoldPressureInchesMercury => 0d;

        public override double Engine3ManifoldPressureInchesMercury => 0d;

        public override double Engine4ManifoldPressureInchesMercury => 0d;

        private int _aircraftId;
        public override int AircraftId => _aircraftId;

        private string _aircraftName = null!;
        public override string AircraftName => _aircraftName;

        public override double AltitudeMSLFeet => Math.Abs(_lastFlightData.aauz);

        public override double AltitudeAGLFeet => Math.Abs(_lastFlightData.RALT);

        public override double AltitudeTrueFeet => CalculatePressureAltitude(_lastFlightData.aauz, KollsmanInchesMercury);

        public override double HeadingMagneticDegrees => Tools.NormalizeDegrees(_lastFlightData.currentHeading);
        
        public override double HeadingTrueDegrees => Tools.NormalizeDegrees(_lastFlightData.currentHeading + _lastFlightData.magDeviationReal);
        
        public override double HeadingMagneticRadians => Tools.DegToRad(_lastFlightData.currentHeading);
        
        public override double HeadingTrueRadians => (_lastFlightData.currentHeading + _lastFlightData.magDeviationReal) * (Math.PI / 180);
        
        private bool _isConnected;
        public override bool IsConnected => _isConnected;

        public override string ATCIdentifier => string.Empty;

        private string _atcModel = null!;
        public override string ATCModel => _atcModel;

        private string _atcType = null!;
        public override string ATCType => _atcType;

        private bool _isHeavy;
        public override bool IsHeavy => _isHeavy;
        
        public override bool IsGearFloats => false;

        private bool _isHelo;
        public override bool IsHelo => _isHelo;
        
        private EngineType _engineType;
        public override EngineType EngineType => _engineType;

        public override bool OnGround => (_lastFlightData.hsiBits & (uint)HsiBits.Flying) == 0;
        
        public override double GroundSpeedKnots => _lastFlightData.gs;
        
        public override double AirSpeedIndicatedKnots => _lastFlightData.kias;
        
        public override double AirSpeedTrueKnots => _lastFlightData.vt;
        
        public override double AmbientTemperatureCelsius => 0;

        public override double AmbientWindDirectionDegrees => 0;
        
        public override double AmbientWindSpeedKnots => 0;

        public override double KollsmanInchesMercury
        {
            get
            {
                // Determine if the calibration is in inHg or hPa
                bool isInHg = (_lastFlightData.altBits & 0x01) != 0;

                // Convert the AltCalReading to inHg if necessary
                if (isInHg)
                {
                    return _lastFlightData.AltCalReading / 100.0; // Already in inHg
                }
                else
                {
                    // Convert from hPa to inHg (1 hPa = 0.02953 inHg)
                    return (_lastFlightData.AltCalReading / 100.0) * 0.02953;
                }
            }
        }

        public override double SecondaryKollsmanInchesMercury => KollsmanInchesMercury;

        public override double PressureInchesMercury => CalculateAtmosphericPressure();
        
        public override ReadyToFly IsReadyToFly => (_lastFlightData != null && _lastFlightData.currentTime > 0 ? FlightSim.ReadyToFly.Ready : FlightSim.ReadyToFly.Loading);
        
        public override double GPSRequiredMagneticHeadingRadians => Tools.DegToRad(_lastFlightData.desiredHeading);
        
        public override double GPSRequiredTrueHeadingRadians => Tools.DegToRad(_lastFlightData.desiredHeading + _lastFlightData.magDeviationReal);
        
        public override bool HasActiveWaypoint => CheckActiveWaypoint(_lastFlightData);
        
        public override double GPSCrossTrackErrorMeters => _lastFlightData.courseDeviation * 1000;
        
        public override double Nav1Radial => _lastFlightData.bearingToBeacon;
        
        public override double Nav2Radial => 0;

        public override bool Nav1Available => _lastFlightData.navMode == (byte)NavModes.NAV || _lastFlightData.navMode == (byte)NavModes.ILS_NAV;
        
        public override bool Nav2Available => false;
        
        public override double Nav1Frequency => _lastFlightData.UFCTChan * 0.001;
        
        public override double Nav2Frequency => 0;

        public override double Nav1StandByFrequency => _lastFlightData.UFCTChan * 0.001;

        public override double Nav2StandByFrequency => 0;

        public override bool Nav1Receive => true;

        public override bool Nav2Receive => false;

        public override double AdfRelativeBearing => Tools.NormalizeDegrees(_lastFlightData.bearingToBeacon);
        
        public override bool AvionicsOn => (_lastFlightData.powerBits & (uint)(PowerBits.BusPowerEssential | PowerBits.MainGenerator)) != 0;

        public override double BatteryLoadAmps => 0d;

        public (int symbol, float bearing, float lethality)? RwrTopThreat
        {
            get
            {
                int n = Math.Min(_lastFlightData.RwrObjectCount, FlightData.MAX_RWR_OBJECTS);
                if (n <= 0) return null;

                var sym = _lastFlightData.RWRsymbol;
                var brg = _lastFlightData.bearing;
                var leth = _lastFlightData.lethality;
                if (sym == null || brg == null || leth == null) return null;

                int bestIdx = -1;
                float bestL = float.MinValue;

                for (int i = 0; i < n && i < sym.Length && i < brg.Length && i < leth.Length; i++)
                {
                    if (leth[i] > bestL)
                    {
                        bestL = leth[i];
                        bestIdx = i;
                    }
                }

                if (bestIdx < 0) return null;
                return (sym[bestIdx], brg[bestIdx], leth[bestIdx]);
            }
        }

        public bool GearWarnLamp => AnyLampOn(_lastFlightData, LightBits2.GEARHANDLE);

        public bool NoseGearLamp => AnyLampOn(_lastFlightData, LightBits3.NoseGearDown);

        public bool LeftGearLamp => AnyLampOn(_lastFlightData, LightBits3.LeftGearDown);

        public bool RightGearLamp => AnyLampOn(_lastFlightData, LightBits3.RightGearDown);

        public bool AuxRwrSearchLamp => AnyLampOn(_lastFlightData, LightBits2.AuxSrch);

        public bool AuxRwrActLamp => AnyLampOn(_lastFlightData, LightBits2.AuxAct);

        public bool AuxRwrLowLamp => AnyLampOn(_lastFlightData, LightBits2.AuxLow);

        public bool RwrLowLamp => AuxRwrLowLamp;

        public bool AuxRwrPwrLamp => AnyLampOn(_lastFlightData, LightBits2.AuxPwr);

        public bool AuxRwrAltLamp => AnyLampOn(_lastFlightData, LightBits.RadarAlt);

        public bool RwrBatteryFail => AnyLampOn(_lastFlightData, LightBits3.BatFail);

        public bool EpuRunLamp => AnyLampOn(_lastFlightData, LightBits2.EPUOn);

        public bool JfsRunLamp => AnyLampOn(_lastFlightData, LightBits2.JFSOn);


        public EcmOperStates EcmState => _lastFlightData.ecmOper;

        public bool EcmPowered => EcmState != EcmOperStates.ECM_OPER_NO_LIT;

        public bool EcmTransmit => EcmState >= EcmOperStates.ECM_OPER_ACTIVE;

        public bool EcmPwrLamp => AnyLampOn(_lastFlightData, LightBits2.EcmPwr);

        public bool EcmFailLamp => AnyLampOn(_lastFlightData, LightBits2.EcmFail);

        public bool EcmAnyLit => _lastFlightData.ecmOper != EcmOperStates.ECM_OPER_NO_LIT;

        public bool EssentialPower => (_lastFlightData.powerBits & (uint)PowerBits.BusPowerEssential) != 0;

        public bool RwrNewDetection => (_lastFlightData.newDetection?.Any(x => x != 0) ?? false);

        public bool RwrActivityFinal => RwrActivityLatched || RwrThreatSelected || RwrThreatHigh || RwrMissileLaunch;

        public bool RwrSearch => RwrPowered && (AuxRwrSearchLamp || _lastFlightData.RwrObjectCount > 0);

        public bool RwrActivity => RwrPowered && (AuxRwrActLamp || RwrActivityFinal);

        public bool RwrPowerLamp => AuxRwrPwrLamp || EcmPwrLamp || EcmPowered || BatteryBusPowered;

        public int RwrCount => _lastFlightData.RwrObjectCount;

        public bool RwrThreatHigh => (_lastFlightData.lethality?.Any(l => l >= 2.0f) ?? false);

        public bool RwrMissileLaunch => (_lastFlightData.missileLaunch?.Any(x => x != 0) ?? false);

        public bool RwrMissileActivity => (_lastFlightData.missileActivity?.Any(x => x != 0) ?? false);

        private DateTime _rwrActivityUntilUtc = DateTime.MinValue;

        public void UpdateRwrActivityLatch()
        {
            bool pulse =
                (_lastFlightData.newDetection?.Any(x => x != 0) ?? false) ||
                (_lastFlightData.missileActivity?.Any(x => x != 0) ?? false) ||
                (_lastFlightData.missileLaunch?.Any(x => x != 0) ?? false);

            if (pulse)
                _rwrActivityUntilUtc = DateTime.UtcNow.AddMilliseconds(750);
        }

        public bool RwrActivityLatched => DateTime.UtcNow <= _rwrActivityUntilUtc;

        public bool RwrThreatSelected => (_lastFlightData.selected?.Any(x => x != 0) ?? false);

        public bool EcmActive => _lastFlightData.ecmOper == EcmOperStates.ECM_OPER_ACTIVE || _lastFlightData.ecmOper == EcmOperStates.ECM_OPER_ALL_LIT;

        public bool EcmStandby => _lastFlightData.ecmOper == EcmOperStates.ECM_OPER_STDBY;

        public bool RwrJammed => (_lastFlightData.RWRjammingStatus?.Any(s => s == JammingStates.JAMMED_YES) ?? false);

        public bool MainGenOnline => (_lastFlightData.powerBits & (uint)PowerBits.MainGenerator) != 0;

        public bool BatteryBusPowered => (_lastFlightData.powerBits & (uint)PowerBits.BusPowerBattery) != 0;

        public bool EmergencyBusPowered => (_lastFlightData.powerBits & (uint)PowerBits.BusPowerEmergency) != 0;

        public bool StandbyGenOnline => (_lastFlightData.powerBits & (uint)PowerBits.StandbyGenerator) != 0;

        public bool EpuActive => BatteryBusPowered && !MainGenOnline && EmergencyBusPowered && !StandbyGenOnline;

        public bool JfsRunning => (_lastFlightData.powerBits & (uint)PowerBits.JetFuelStarter) != 0;

        public bool RwrPowered => BatteryBusPowered;

        public bool MainGenLamp => AnyLampOn(_lastFlightData, LightBits3.MainGen);

        public bool StbyGenLamp => AnyLampOn(_lastFlightData, LightBits3.StbyGen);

        public bool FlcsRlyLamp => AnyLampOn(_lastFlightData, LightBits3.FlcsRly);

        public bool SpeedBrakeLamp => AnyLampOn(_lastFlightData, LightBits3.SpeedBrake);

        public override bool BatteryOn => (_lastFlightData.powerBits & (uint)PowerBits.BusPowerBattery) != 0;
        
        public override uint Transponder => ((uint)((byte)_lastFlightData.iffTransponderActiveCode1) << 24) | ((uint)((byte)_lastFlightData.iffTransponderActiveCode2) << 16) | ((uint)((byte)_lastFlightData.iffTransponderActiveCode3A) << 8) | (byte)_lastFlightData.iffTransponderActiveCodeC;

        public override TransponderMode TransponderMode => TransponderMode.On_Mode_A;

        public override bool Com1Receive => _lastFlightData.RadioClientControlData.Radios[(int)Radios.UHF].IsOn;
        
        public override bool Com2Receive => _lastFlightData.RadioClientControlData.Radios[(int)Radios.VHF].IsOn;
        
        public override bool Com1Transmit => _lastFlightData.RadioClientControlData.Radios[(int)Radios.UHF].PttDepressed;
        
        public override bool Com2Transmit => _lastFlightData.RadioClientControlData.Radios[(int)Radios.VHF].PttDepressed;
        
        public override double Com1Frequency => _lastFlightData.RadioClientControlData.Radios[(int)Radios.UHF].Frequency;
        
        public override double Com2Frequency => _lastFlightData.RadioClientControlData.Radios[(int)Radios.VHF].Frequency;

        public override double Com1StandByFrequency => _lastFlightData.RadioClientControlData.Radios[(int)Radios.UHF].Frequency;

        public override double Com2StandByFrequency => _lastFlightData.RadioClientControlData.Radios[(int)Radios.VHF].Frequency;

        public override Dictionary<string, Aircraft> Traffic => new Dictionary<string, Aircraft>();

        public override bool IdentActive => false;

        public override bool GeneratorOn => (_lastFlightData.powerBits & (uint)PowerBits.MainGenerator) != 0;

        public override bool PitotHeatOn => true;

        public override ComStatus Com1Status => ComStatus.OK;

        public override ComStatus Com2Status => ComStatus.OK;

        public override string Com1ActiveFrequencyIdent => string.Empty;

        public override string Com2ActiveFrequencyIdent => string.Empty;

        public override TunedFacility Com1ActiveFrequencyType => TunedFacility.NONE;

        public override TunedFacility Com2ActiveFrequencyType => TunedFacility.NONE;

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

        private AbortableTaskRunner? _timerMain;
        private bool initialized = false;
        private bool stop = false;
        private FlightData _lastFlightData = new FlightData();
        private Reader _sharedMemReader = new Reader();
        private int _aircraftLoadRequestId = 0;
        private int _aircraftLoadAppliedId = 0;

        static FalconBMSProvider()
        {
            Instance = new FalconBMSProvider();
        }

        FalconBMSProvider()
        {
            Initialize();
        }

        private void Initialize()
        {
            stop = false;
            if (!initialized)
            {
                initialized = true;
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    var list = (IReadOnlyList<FalconBMSCrossref>)FalconBMSCrossref.GetFalconBMSCrossref();
                    Volatile.Write(ref _crossref, list);

                    var name = FalconBMSAircraftName;
                    if (!string.IsNullOrWhiteSpace(name) &&
                        !name.Equals("Unknown Aircraft", StringComparison.OrdinalIgnoreCase))
                    {
                        LoadAircraft(name);
                    }
                });
                if (_timerMain == null)
                {
                    _timerMain = new AbortableTaskRunner();
                    _timerMain.Start(_timerMain_DoWorkAsync);
                }
            }
        }

        private async Task _timerMain_DoWorkAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested && !stop)
            {
                try
                {
                    ReadSharedMem();
                }
                catch (Exception)
                {
                }
                await Task.Delay(30, token);
            }
        }


        private void ReadSharedMem()
        {
            // Snapshot previous state (do NOT call virtual properties repeatedly mid-update)
            var prevReady = (_lastFlightData.currentTime > 0 ? FlightSim.ReadyToFly.Ready : FlightSim.ReadyToFly.Loading);
            var prevConnected = _isConnected;

            var prevAircraftName = GetAircraftName(_lastFlightData.vehicleACD) ?? string.Empty;

            // These are base-class managed, so grab once
            var prevLat = Latitude;
            var prevLng = Longitude;

            // Read once, then commit once
            var data = _sharedMemReader.GetCurrentData();
            var connected = data != null;

            if (data == null)
                data = new FlightData();

            _lastFlightData = data;
            _isConnected = connected;

            // Update position from the same snapshot we just committed
            SetPosition(data.latitude, data.longitude);

            UpdateRwrActivityLatch();

            // Connection edge
            if (prevConnected != _isConnected)
            {
                if (_isConnected) Connected();
                else Quit();
            }

            // ReadyToFly edge (computed from snapshot, not property)
            var newReady = (data.currentTime > 0 ? FlightSim.ReadyToFly.Ready : FlightSim.ReadyToFly.Loading);
            if (newReady != prevReady)
            {
                ReadyToFly(newReady);
            }
            else
            {
                // Distance check must compare old position to the NEW snapshot position
                var distance = Tools.DistanceTo(prevLat, prevLng, data.latitude, data.longitude);

                // Have we moved more than 500m since last tick?
                if (distance >= 500)
                    ReadyToFly(newReady);
            }

            if (connected)
            {
                // Aircraft change detection should also be based on the same snapshot
                var newAircraftName = GetAircraftName(data.vehicleACD) ?? string.Empty;
                if (!prevAircraftName.Equals(newAircraftName, StringComparison.OrdinalIgnoreCase) &&
                    !newAircraftName.Equals("Unknown Aircraft", StringComparison.OrdinalIgnoreCase))
                {
                    LoadAircraft(newAircraftName);
                }
                FlightDataReceived();
            }
        }

        private void LoadAircraft(string? aircraftName)
        {
            var crossrefSnapshot = Volatile.Read(ref _crossref);
            if (crossrefSnapshot.Count == 0)
                return;

            // Latest request wins
            int requestId = Interlocked.Increment(ref _aircraftLoadRequestId);

            // Normalize once
            string requestedName = (aircraftName ?? string.Empty).Trim();

            // Snapshot for fallback (don’t read moving targets in the worker)
            var snap = _lastFlightData;
            int fallbackId = snap.vehicleACD;
            string fallbackName = GetAircraftName(snap.vehicleACD) ?? "Unknown Aircraft";

            ThreadPool.QueueUserWorkItem(_ =>
            {
                // Bail fast if stale
                if (requestId != Volatile.Read(ref _aircraftLoadRequestId))
                    return;

                FalconBMSCrossref? xref = null;
                try
                {
                    xref = crossrefSnapshot.FirstOrDefault(x =>
                        x.AircraftName.Equals(requestedName, StringComparison.OrdinalIgnoreCase));
                }
                catch
                {
                    xref = null;
                }

                // Build locally first
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
                    newId = fallbackId;
                    newName = fallbackName;
                    newType = string.Empty;
                    newModel = string.Empty;
                    newEngine = EngineType.Jet;
                    newHeavy = false;
                    newHelo = false;
                }

                // Don’t apply if stale
                if (requestId != Volatile.Read(ref _aircraftLoadRequestId))
                    return;

                // Commit in one shot
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

        private string GetAircraftName(short vehicleACD)
        {
            var aircraftNames = new Dictionary<short, string>
                {
                    { 61, "F-16C" },
                    { 30, "F-15E" },
                    { 5, "A-10C" },
                    { 46, "F-5E" },
                    { 144, "F/A-18C" },
                    { 23, "MIG-29A" },
                    { 100, "SU-35S" },
                    { 3, "AV-8B" },
                    { 12, "F-4E" },
                    // Add more as necessary
                };
            return aircraftNames.TryGetValue(vehicleACD, out var name) ? name : "Unknown Aircraft";
        }

        private double CalculatePressureAltitude(double aauz, double standardPressure = 29.92f)
        {
            // Pressure altitude formula
            return Math.Abs(aauz) + (standardPressure - 29.92) * 1000.0;
        }

        private double CalculateAtmosphericPressure()
        {
            const double seaLevelPressureInHg = 29.92; // Standard sea level pressure in inHg
            const double scaleHeightFeet = 145442.0;  // Scale height in feet
            const double lapseRateExponent = 5.25588; // Pressure lapse rate exponent

            // Get the pressure altitude in feet (e.g., AltitudePressure)
            double pressureAltitudeFeet = AltitudeTrueFeet;

            // Calculate the atmospheric pressure
            double atmosphericPressure = seaLevelPressureInHg * Math.Pow(1 - (pressureAltitudeFeet / scaleHeightFeet), lapseRateExponent);

            return atmosphericPressure;
        }

        private bool CheckActiveWaypoint(FlightData data)
        {
            return data.courseState != 0 || data.headingState != 0; // Simplified logic
        }

        private bool CheckNavAvailability(FlightData data, string navSource)
        {
            return navSource switch
            {
                "Nav1" => data.courseDeviation != 0 || data.distanceToBeacon != 0,
                "Nav2" => data.desiredHeading != 0 || data.bearingToBeacon != 0,
                _ => false,
            };
        }

        public override void Deinitialize(int timeOut = 1000)
        {
            StopTimerMainAsync(timeOut).GetAwaiter().GetResult();
            DisposeObject(_sharedMemReader);
        }

        public async Task StopTimerMainAsync(int timeOut = 1000)
        {
            stop = true;
            if (_timerMain != null && _timerMain.IsRunning)
            {
                await _timerMain.StopAsync(timeOut).ConfigureAwait(false);
            }
            _timerMain?.Dispose();
            _timerMain = null!;
        }

        public static void DisposeObject(object obj)
        {
            if (obj == null) return;
            try
            {
                var disposable = obj as IDisposable;
                disposable?.Dispose();
            }
            catch (Exception)
            {
            }
        }

        public override void SendCommandToFS(string command)
        {
        }

        public override void SendControlToFS(string control, float value)
        {
        }
    }
}
