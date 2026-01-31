using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using FSUIPC;
using System.Diagnostics;

namespace FlightSim
{
    public class FSUIPCProvider : FlightSimProviderBase
    {
        public static readonly FSUIPCProvider Instance;

        public override string Name => "FSUIPC";

        public override double WaypointLongitude => gpsApLon.Value;

        public override double WaypointLatitude => gpsWpLat.Value;

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
                    ident = gpsIdent.Value?.Trim('\0', ' ');
                }
                return ident ?? string.Empty;
            }
        }

        public override bool BalloonAutoFillActive => FlightSimProviders.SimConnect.BalloonAutoFillActive;

        public override double BalloonFillAmountPercent => FlightSimProviders.SimConnect.BalloonFillAmountPercent;

        public override double BalloonGasDensity => FlightSimProviders.SimConnect.BalloonGasDensity;

        public override double BalloonGasTemperatureCelsius => FlightSimProviders.SimConnect.BalloonGasTemperatureCelsius;

        public override double BalloonVentOpenPercent => FlightSimProviders.SimConnect.BalloonVentOpenPercent;

        public override double BalloonBurnerFuelFlowRatePounds => FlightSimProviders.SimConnect.BalloonBurnerFuelFlowRatePounds;

        public override bool BalloonBurnerPilotLightOn => FlightSimProviders.SimConnect.BalloonBurnerPilotLightOn;

        public override double BalloonBurnerValveOpenPercent => FlightSimProviders.SimConnect.BalloonBurnerValveOpenPercent;

        public override AirshipGasType AirshipCompartmentGasType => FlightSimProviders.SimConnect.AirshipCompartmentGasType;

        public override double AirshipCompartmentPressureHectoPascals => FlightSimProviders.SimConnect.AirshipCompartmentPressureHectoPascals;

        public override double AirshipCompartmentOverPressureHectoPascals => FlightSimProviders.SimConnect.AirshipCompartmentOverPressureHectoPascals;

        public override double AirshipCompartmentTemperatureCelsius => FlightSimProviders.SimConnect.AirshipCompartmentTemperatureCelsius;

        public override double AirshipCompartmentVolumeCubicMeters => FlightSimProviders.SimConnect.AirshipCompartmentVolumeCubicMeters;

        public override double AirshipCompartmentWeightPounds => FlightSimProviders.SimConnect.AirshipCompartmentWeightPounds;

        public override double AirshipFanPowerPercent => FlightSimProviders.SimConnect.AirshipFanPowerPercent;

        public override bool AirshipMastTruckDeployment => FlightSimProviders.SimConnect.AirshipMastTruckDeployment;

        public override double AirshipMastTruckExtensionPercent => FlightSimProviders.SimConnect.AirshipMastTruckExtensionPercent;

        public override string Nav1ActiveFrequencyIdent => vor1Ident.Value?.Trim('\0', ' ') ?? string.Empty;

        public override string Nav2ActiveFrequencyIdent => vor2Ident.Value?.Trim('\0', ' ') ?? string.Empty;

        public override string AdfIdent => adfIdentity.Value?.Trim('\0', ' ') ?? string.Empty;

        public override string AdfName => adfName.Value?.Trim('\0', ' ') ?? string.Empty;

        public override double Engine1Rpm => Math.Abs(prop1Rpm.Value);

        public override double Engine2Rpm => Math.Abs(prop2Rpm.Value);

        public override double Engine3Rpm => Math.Abs(prop3Rpm.Value);

        public override double Engine4Rpm => Math.Abs(prop4Rpm.Value);

        public override double Torque1FootPounds => torque1FtLbs.Value;

        public override double Torque2FootPounds => torque2FtLbs.Value;

        public override double Torque3FootPounds => torque3FtLbs.Value;

        public override double Torque4FootPounds => torque4FtLbs.Value;

        public override bool CarbHeatAntiIce1 => carbHeat1.Value != 0;

        public override bool CarbHeatAntiIce2 => carbHeat2.Value != 0;

        public override bool CarbHeatAntiIce3 => carbHeat3.Value != 0;

        public override bool CarbHeatAntiIce4 => carbHeat4.Value != 0;

        public override bool FuelPump1On => fuelPump.Value.IsBitSet(0);

        public override bool FuelPump2On => fuelPump.Value.IsBitSet(1);

        public override bool FuelPump3On => fuelPump.Value.IsBitSet(2);

        public override bool FuelPump4On => fuelPump.Value.IsBitSet(3);

        public override double VerticalSpeedFpm => (verticalSpeed.Value * 60 * 3.28084 / 256);

        public override double PitchDegrees => (pitch.Value * 360.0 / (65536.0 * 65536.0));

        public override double BankDegrees => (bank.Value * 360.0 / (65536.0 * 65536.0));

        public override bool AutopilotAltHold => apAltHold.Value != 0;

        public override bool AutopilotHeadingHold => apHdgHold.Value != 0;

        public override bool AutopilotNavHold => apNavHold.Value != 0;

        public override bool AutopilotSpeedHold => apSpdHold.Value != 0;

        public override bool AutopilotMaster => apMaster.Value != 0;

        public override double AutopilotAltitudeTargetFeet => apAltTargetFt.Value;

        public override double AutopilotSpeedTargetKnots => apSpdTargetKts.Value;

        public override double AutopilotHeadingBugDegrees => Tools.NormalizeDegrees(Math.Abs(headingBug.Value * 360d / 65536d));

        public override double AutopilotNav1CourseDegrees => nav1Obs.Value;

        public override double AutopilotNav2CourseDegrees => nav2Obs.Value;

        public override double ElevatorTrimPercent => Math.Clamp(elevatorTrimPercent.Value, -16384d, 16384d) / 16384d * 100d;

        public override double RudderTrimPercent => Math.Clamp(rudderTrimPercent.Value, -16384d, 16384d) / 16384d * 100d;

        public override double AileronTrimPercent => Math.Clamp(aileronTrimPercent.Value, -16384d, 16384d) / 16384d * 100d;

        public override double FlapsPercent => Math.Clamp(flapsPercent.Value, 0d, 16383d) / 16383d * 100d;

        public override GearState GearState
        {
            get
            {
                float v = gearControl.Value;

                if (v <= 0.01f)
                    return GearState.Up;

                if (v >= 0.99f)
                    return GearState.Down;

                return GearState.Transit;
            }
        }

        public override double SpoilersPercent => Math.Clamp(spoilersPercent.Value, 0d, 16383d) / 16383d * 100d;

        public override bool SpoilersArmed => spoilersArmed.Value != 0;

        public override bool ParkingBrakeOn => parkingBrake.Value != 0;

        public override double FuelRemainingGallons => FlightSimProviders.SimConnect.FuelRemainingGallons;

        public override double FuelCapacityGallons => FlightSimProviders.SimConnect.FuelCapacityGallons;

        public override double Engine1OilPressurePsf => Tools.PsiToPsf(oilPressure1.Value * 55d / 16384d);

        public override double Engine2OilPressurePsf => Tools.PsiToPsf(oilPressure2.Value * 55d / 16384d);

        public override double Engine3OilPressurePsf => Tools.PsiToPsf(oilPressure3.Value * 55d / 16384d);

        public override double Engine4OilPressurePsf => Tools.PsiToPsf(oilPressure4.Value * 55d / 16384d);

        public override bool NavLightsOn => lights.Value.IsBitSet(0);

        public override bool BeaconLightsOn => lights.Value.IsBitSet(1);

        public override bool TaxiLightsOn => lights.Value.IsBitSet(3);

        public override bool LandingLightsOn => lights.Value.IsBitSet(2);

        public override bool StrobeLightsOn => lights.Value.IsBitSet(4);

        public override bool CabinLightsOn => lights.Value.IsBitSet(9);

        public override bool PanelLightsOn => lights.Value.IsBitSet(5);

        public override bool WindowDefrostOn => FlightSimProviders.SimConnect.WindowDefrostOn;

        public override bool Propeller1DeIce => propDeice.Value != 0;

        public override bool Propeller2DeIce => propDeice.Value != 0;

        public override bool Propeller3DeIce => propDeice.Value != 0;

        public override bool Propeller4DeIce => propDeice.Value != 0;

        public override double CowlFlaps1Percent
        {
            get
            {
                double v = cowlFlap1.Value;
                // If it's normalized (0.0–1.0), convert to percent
                if (v >= 0.0 && v <= 1.0)
                    v *= 100.0;
                return Math.Clamp(v, 0.0, 100.0);
            }
        }

        public override double CowlFlaps2Percent
        {
            get
            {
                double v = cowlFlap2.Value;
                // If it's normalized (0.0–1.0), convert to percent
                if (v >= 0.0 && v <= 1.0)
                    v *= 100.0;
                return Math.Clamp(v, 0.0, 100.0);
            }
        }

        public override double CowlFlaps3Percent
        {
            get
            {
                double v = cowlFlap3.Value;
                // If it's normalized (0.0–1.0), convert to percent
                if (v >= 0.0 && v <= 1.0)
                    v *= 100.0;
                return Math.Clamp(v, 0.0, 100.0);
            }
        }

        public override double CowlFlaps4Percent
        {
            get
            {
                double v = cowlFlap4.Value;
                // If it's normalized (0.0–1.0), convert to percent
                if (v >= 0.0 && v <= 1.0)
                    v *= 100.0;
                return Math.Clamp(v, 0.0, 100.0);
            }
        }

        public override double Throttle1Percent => Math.Clamp(throttle1Percent.Value, 0, 16384d) / 16384d * 100d;

        public override double Throttle2Percent => Math.Clamp(throttle2Percent.Value, 0d, 16384d) / 16384d * 100d;

        public override double Throttle3Percent => Math.Clamp(throttle3Percent.Value, 0d, 16384d) / 16384d * 100d;

        public override double Throttle4Percent => Math.Clamp(throttle4Percent.Value, 0d, 16384d) / 16384d * 100d;

        public override double Mixture1Percent => Math.Clamp(mixture1Percent.Value, 0d, 16384d) / 16384d * 100d;

        public override double Mixture2Percent => Math.Clamp(mixture2Percent.Value, 0d, 16384d) / 16384d * 100d;

        public override double Mixture3Percent => Math.Clamp(mixture3Percent.Value, 0d, 16384d) / 16384d * 100d;

        public override double Mixture4Percent => Math.Clamp(mixture4Percent.Value, 0d, 16384d) / 16384d * 100d;

        public double PropPitch1Percent => Math.Clamp(propPitch1Percent.Value, 0d, 16384d) / 16384d * 100d;

        public double PropPitch2Percent => Math.Clamp(propPitch2Percent.Value, 0d, 16384d) / 16384d * 100d;

        public double PropPitch3Percent => Math.Clamp(propPitch3Percent.Value, 0d, 16384d) / 16384d * 100d;

        public double PropPitch4Percent => Math.Clamp(propPitch4Percent.Value, 0d, 16384d) / 16384d * 100d;

        public override double PropPitch1Degrees => Tools.NormalizeDegrees(Tools.RadToDeg(propPitch1Radians.Value));

        public override double PropPitch2Degrees => Tools.NormalizeDegrees(Tools.RadToDeg(propPitch2Radians.Value));

        public override double PropPitch3Degrees => Tools.NormalizeDegrees(Tools.RadToDeg(propPitch3Radians.Value));

        public override double PropPitch4Degrees => Tools.NormalizeDegrees(Tools.RadToDeg(propPitch4Radians.Value));

        public override bool Engine1Failed => FlightSimProviders.SimConnect.Engine1Failed;

        public override bool Engine2Failed => FlightSimProviders.SimConnect.Engine2Failed;

        public override bool Engine3Failed => FlightSimProviders.SimConnect.Engine3Failed;

        public override bool Engine4Failed => FlightSimProviders.SimConnect.Engine4Failed;

        public override int EngineCount => FlightSimProviders.SimConnect.EngineCount;

        public override bool Afterburner1On => afterburner1.Value != 0;

        public override bool Afterburner2On => afterburner2.Value != 0;

        public override bool Afterburner3On => afterburner3.Value != 0;

        public override bool Afterburner4On => afterburner4.Value != 0;

        public override bool Engine1Running => engine1Running.Value != 0;

        public override bool Engine2Running => engine2Running.Value != 0;

        public override bool Engine3Running => engine3Running.Value != 0;

        public override bool Engine4Running => engine4Running.Value != 0;

        public override double Engine1N1Percent => Math.Clamp(engine1N1Percent.Value, 0d, 16384d) / 16384d * 100d;

        public override double Engine2N1Percent => Math.Clamp(engine2N1Percent.Value, 0d, 16384d) / 16384d * 100d;

        public override double Engine3N1Percent => Math.Clamp(engine3N1Percent.Value, 0d, 16384d) / 16384d * 100d;

        public override double Engine4N1Percent => Math.Clamp(engine4N1Percent.Value, 0d, 16384d) / 16384d * 100d;

        public override double Engine1ManifoldPressureInchesMercury => manifold1Inhg.Value / 1024d;

        public override double Engine2ManifoldPressureInchesMercury => manifold2Inhg.Value / 1024d;

        public override double Engine3ManifoldPressureInchesMercury => manifold3Inhg.Value / 1024d;

        public override double Engine4ManifoldPressureInchesMercury => manifold4Inhg.Value / 1024d;

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

        private Dictionary<string, Aircraft> _traffic = new Dictionary<string, Aircraft>();
        public override Dictionary<string, Aircraft> Traffic
        {
            get
            {
                return _traffic;
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

        private bool _isHelo;
        public override bool IsHelo
        {
            get
            {
                return _isHelo;
            }
        }

        public override bool IsGearFloats
        {
            get
            {
                if (aircraftModel.Value.Equals("Airbus-H135") || aircraftModel.Value.Equals("EC135P3H"))
                {
                    return false;
                }
                else
                {
                    return gearTypeExtended.Value.IsBitSet(2);
                }
            }
        }

        public bool IsSkiPlane
        {
            get
            {
                return gearTypeExtended.Value.IsBitSet(1);
            }
        }

        public bool IsSkidPlane
        {
            get
            {
                return gearTypeExtended.Value.IsBitSet(3);
            }
        }

        public override ReadyToFly IsReadyToFly
        {
            get
            {
                return !Location.IsEmpty() ? (ReadyToFly)readyToFly.Value : FlightSim.ReadyToFly.Loading;
            }
        }


        private EngineType _engineType;
        public override EngineType EngineType
        {
            get
            {
                return _engineType;
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

        public double AirSpeedBarberPoleKnots
        {
            get
            {
                return airSpeedBarberPole.Value / 128;
            }
        }

        public override double AirSpeedIndicatedKnots
        {
            get
            {
                return airSpeedIndicated.Value / 128;
            }
        }

        public override double AirSpeedTrueKnots
        {
            get
            {
                return airSpeedTrue.Value / 128;
            }
        }

        public override double GroundSpeedKnots
        {
            get
            {
                return ((groundSpeed.Value / 65536.0) * 1.94384449);
            }
        }

        public override double HeadingMagneticDegrees
        {
            get
            {
                return headingMagnetic.Value;
            }
        }

        public override double HeadingTrueDegrees
        {
            get
            {
                return (headingTrue.Value * 360.0 / (65536.0 * 65536.0));
            }
        }

        public override double HeadingMagneticRadians
        {
            get
            {
                return (headingMagnetic.Value * (Math.PI / 180));
            }
        }

        public override double HeadingTrueRadians
        {
            get
            {
                return ((headingTrue.Value * 360.0 / (65536.0 * 65536.0)) * (Math.PI / 180));
            }
        }

        public override bool OnGround
        {
            get
            {
                return Convert.ToBoolean(onGround.Value);
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

        private string _atcModel = null!;
        public override string ATCModel
        {
            get
            {
                return _atcModel;
            }
        }

        public override double AltitudeMSLFeet
        {
            get
            {
                if (InternationalUnits == InternationalUnits.MetricPlusMeters)
                {
                    return (altitude.Value * 3.28084);
                }
                else
                {
                    return altitude.Value;
                }
            }
        }

        public override double AltitudeMSLMeters
        {
            get
            {
                if (InternationalUnits == InternationalUnits.MetricPlusMeters)
                {
                    return altitude.Value;
                }
                else
                {
                    return (altitude.Value / 3.28084);
                }
            }
        }

        public override double AmbientWindSpeedKnots
        {
            get
            {
                return ambientWindSpeed.Value;
            }
        }

        public override double AmbientWindDirectionDegrees
        {
            get
            {
                return ambientWindDirection.Value;
            }
        }

        public double AmbientWindDirectionRadians
        {
            get
            {
                return (ambientWindDirection.Value * (Math.PI / 180));
            }
        }

        public override double AmbientTemperatureCelsius
        {
            get
            {
                return ambientTemperature.Value;
            }
        }

        public override double AltitudeTrueFeet
        {
            get
            {
                return (pressureAltitude.Value * 3.2808);
            }
        }

        public override double AltitudeAGLFeet
        {
            get
            {
                // Ground elevation MSL in feet (meters * 256 → meters → feet)
                double groundFeet = (groundAltitude.Value / 256.0) * 3.28084;
                // Aircraft altitude MSL already in feet
                return AltitudeMSLFeet - groundFeet;
            }
        }

        public override double GPSRequiredTrueHeadingRadians
        {
            get
            {
                return gpsRequiredTrueHeading.Value;
            }
        }

        public override double GPSRequiredTrueHeadingDegrees
        {
            get
            {
                return (gpsRequiredTrueHeading.Value * (180 / Math.PI));
            }
        }

        public override double GPSRequiredMagneticHeadingRadians
        {
            get
            {
                // F/A-18 glitch: add PI radians
                return (AircraftId == 50 ? gpsRequiredMagneticHeading.Value + Math.PI : gpsRequiredMagneticHeading.Value);
            }
        }

        public override double GPSRequiredMagneticHeadingDegrees
        {
            get
            {
                // F/A-18 glitch: add 180 degrees
                return (AircraftId == 50 ? gpsRequiredMagneticHeading.Value + Math.PI : gpsRequiredMagneticHeading.Value) * (180 / Math.PI);
            }
        }

        public override double GPSCrossTrackErrorMeters
        {
            get
            {
                return gpsCrossTrackError.Value;
            }
        }

        public override bool HasActiveWaypoint
        {
            get
            {
                return (gpsFlags.Value & (((uint)1) << 2)) != 0;
            }
        }

        public InternationalUnits InternationalUnits
        {
            get
            {
                return (InternationalUnits)internationalUnits.Value;
            }
        }

        public double PressureMillibars
        {
            get
            {
                return pressure.Value;
            }
        }

        public override double PressureInchesMercury
        {
            get
            {
                return Tools.HectoPascalsToInchesMercury(PressureMillibars);
            }
        }

        public double KollsmanMillibars
        {
            get
            {
                return (kollsman.Value == 0 ? 1013.25f : (kollsman.Value / 16d));
            }
        }

        public override double KollsmanInchesMercury
        {
            get
            {
                return Tools.HectoPascalsToInchesMercury(KollsmanMillibars);
            }
        }

        public double SecondaryKollsmanMillibars
        {
            get
            {
                return (kollsman2.Value == 0 ? 1013.25f : (kollsman2.Value / 16d));
            }
        }

        public override double SecondaryKollsmanInchesMercury
        {
            get
            {
                return Tools.HectoPascalsToInchesMercury(SecondaryKollsmanMillibars);
            }
        }

        public double PercentFuel
        {
            get
            {
                PayloadServices ps = FSUIPCConnection.PayloadServices;
                ps.RefreshData();
                return ps.FuelPercentage;
            }
        }

        public double FuelLevel
        {
            get
            {
                PayloadServices ps = FSUIPCConnection.PayloadServices;
                ps.RefreshData();
                return (InternationalUnits == InternationalUnits.US ? ps.FuelLevelUSGallons : ps.FuelLevelLitres);
            }
        }

        public double PitchRadians
        {
            get
            {
                return ((pitch.Value * 360.0 / (65536.0 * 65536.0)) * (Math.PI / 180));
            }
        }

        public double BankRadians
        {
            get
            {
                return ((bank.Value * 360.0 / (65536.0 * 65536.0)) * (Math.PI / 180));
            }
        }

        public GearType GearType
        {
            get
            {
                return (GearType)gearType.Value;
            }
        }

        public int Nav1RelativeBearing
        {
            get
            {
                return nav1RelativeBearing.Value;
            }
        }

        public int Nav2RelativeBearing
        {
            get
            {
                return nav2RelativeBearing.Value;
            }
        }

        public string ATCAirline
        {
            get
            {
                return atcAirline.Value;
            }
        }

        public string ATCFlightNumber
        {
            get
            {
                return atcFlightNumber.Value;
            }
        }

        public override string ATCIdentifier
        {
            get
            {
                return atcIdentifier.Value;
            }
        }

        public override double Nav1Radial
        {
            get
            {
                return (nav1Radial.Value * 360 / 65536);
            }
        }

        public override double Nav2Radial
        {
            get
            {
                return (nav2Radial.Value * 360 / 65536);
            }
        }

        public double Nav1MagneticVariance
        {
            get
            {
                return (nav1MagVar.Value * 360 / 65536);
            }
        }

        public double Nav2MagneticVariance
        {
            get
            {
                return (nav2MagVar.Value * 360 / 65536);
            }
        }

        public VorToFromFlag Nav1ToFromFlag
        {
            get
            {
                return (VorToFromFlag)nav1ToFromFlag.Value;
            }
        }

        public VorToFromFlag Nav2ToFromFlag
        {
            get
            {
                return (VorToFromFlag)nav2ToFromFlag.Value;
            }
        }

        public float Nav1CourseDeviation
        {
            get
            {
                return nav1CourseDeviation.Value;
            }
        }

        public float Nav2CourseDeviation
        {
            get
            {
                return nav2CourseDeviation.Value;
            }
        }

        public double Vor1Longitude
        {
            get
            {
                return vor1Longitude.Value.DecimalDegrees;
            }
        }

        public double Vor1Latitude
        {
            get
            {
                return vor1Latitude.Value.DecimalDegrees;
            }
        }

        public double Vor2Longitude
        {
            get
            {
                return vor2Longitude.Value.DecimalDegrees;
            }
        }

        public double Vor2Latitude
        {
            get
            {
                return vor2Latitude.Value.DecimalDegrees;
            }
        }

        public int Vor1Elevation
        {
            get
            {
                return vor1Elevation.Value;
            }
        }

        public int Vor2Elevation
        {
            get
            {
                return vor2Elevation.Value;
            }
        }

        public string Vor1Identity
        {
            get
            {
                return vor1Identity.Value;
            }
        }

        public string Vor2Identity
        {
            get
            {
                return vor2Identity.Value;
            }
        }

        public string Vor1Name
        {
            get
            {
                return vor1Name.Value;
            }
        }

        public string Vor2Name
        {
            get
            {
                return vor2Name.Value;
            }
        }

        public override double AdfRelativeBearing
        {
            get
            {
                return (adfRelativeBearing.Value * 360 / 65536);
            }
        }

        public double AdfLongitude
        {
            get
            {
                return adfLongitude.Value.DecimalDegrees;
            }
        }

        public double AdfLatitude
        {
            get
            {
                return adfLatitude.Value.DecimalDegrees;
            }
        }

        public int AdfElevation
        {
            get
            {
                return adfElevation.Value;
            }
        }

        public override double Nav1DmeDistanceNm
        {
            get
            {
                return (dme1Distance.Value / 10);
            }
        }

        public override double Nav2DmeDistanceNm
        {
            get
            {
                return (dme2Distance.Value / 10);
            }
        }

        public override double Nav1DmeSpeedKts
        {
            get
            {
                return (dme1Speed.Value / 10);
            }
        }

        public override double Nav2DmeSpeedKts
        {
            get
            {
                return (dme2Speed.Value / 10);
            }
        }

        public TimeSpan Dme1TimeToStation
        {
            get
            {
                return new TimeSpan(dme1TimeToStation.Value * 1000000);
            }
        }

        public TimeSpan Dme2TimeToStation
        {
            get
            {
                return new TimeSpan(dme2TimeToStation.Value * 1000000);
            }
        }

        public override bool Nav1Available
        {
            get
            {
                return (nav1Available.Value != 0);
            }
        }

        public override bool Nav2Available
        {
            get
            {
                return (nav2Available.Value != 0);
            }
        }

        public bool Nav1ObsAvailable
        {
            get
            {
                return (nav1ObsAvailable.Value != 0);
            }
        }

        public bool Nav2ObsAvailable
        {
            get
            {
                return (nav2ObsAvailable.Value != 0);
            }
        }

        public override bool Nav1Receive
        {
            get
            {
                return radioSwitches.Value.IsBitSet(4);
            }
        }

        public override bool Nav2Receive
        {
            get
            {
                return radioSwitches.Value.IsBitSet(3);
            }
        }

        public override bool AvionicsOn
        {
            get
            {
                return (avionicsMasterSwitch.Value != 0);
            }
        }

        public override bool BatteryOn
        {
            get
            {
                return (electricalMasterBattery.Value != 0);
            }
        }

        public override bool GeneratorOn
        {
            get
            {
                return (engineGeneratorSwitch.Value != 0);
            }
        }

        public override bool PitotHeatOn
        {
            get
            {
                return Convert.ToBoolean(pitotHeatSwitch.Value);
            }
        }

        public override double Nav1Frequency
        {
            get
            {
                return 100.0d + (Tools.Bcd2Dec(nav1ActiveFrequency.Value) / 100.0d);
            }
        }

        public override double Nav2Frequency
        {
            get
            {
                return 100.0d + (Tools.Bcd2Dec(nav2ActiveFrequency.Value) / 100.0d);
            }
        }

        public override double Nav1StandByFrequency
        {
            get
            {
                return 100.0d + (Tools.Bcd2Dec(nav1StandByFrequency.Value) / 100.0d);
            }
        }

        public override double Nav2StandByFrequency
        {
            get
            {
                return 100.0d + (Tools.Bcd2Dec(nav2StandByFrequency.Value) / 100.0d);
            }
        }

        public override TunedFacility Com1ActiveFrequencyType
        {
            get
            {
                return FlightSimProviders.SimConnect.Com1ActiveFrequencyType;
            }
        }

        public override TunedFacility Com2ActiveFrequencyType
        {
            get
            {
                return FlightSimProviders.SimConnect.Com2ActiveFrequencyType;
            }
        }

        public override double Com1Frequency
        {
            get
            {
                return 100.0d + (Tools.Bcd2Dec(com1ActiveFrequency.Value) / 100.0d);
            }
        }

        public override double Com2Frequency
        {
            get
            {
                return 100.0d + (Tools.Bcd2Dec(com2ActiveFrequency.Value) / 100.0d);
            }
        }

        public override double Com1StandByFrequency
        {
            get
            {
                return 100.0d + (Tools.Bcd2Dec(com1StandByFrequency.Value) / 100.0d);
            }
        }

        public override double Com2StandByFrequency
        {
            get
            {
                return 100.0d + (Tools.Bcd2Dec(com2StandByFrequency.Value) / 100.0d);
            }
        }

        public double AdfActiveFrequency
        {
            get
            {
                return 100.0d + (Tools.Bcd2Dec(adfActiveFrequency.Value) / 100.0d);
            }
        }

        public override uint Transponder
        {
            get
            {
                return Tools.Bcd2Dec(transponder.Value);
            }
        }

        public override TransponderMode TransponderMode
        {
            get
            {
                return FlightSimProviders.SimConnect.TransponderMode;
            }
        }

        public override bool IdentActive
        {
            get
            {
                return FlightSimProviders.SimConnect.IdentActive;
            }
        }

        public override bool Com1Transmit
        {
            get
            {
                return radioSwitches.Value.IsBitSet(7);
            }
        }

        public override bool Com2Transmit
        {
            get
            {
                return radioSwitches.Value.IsBitSet(6);
            }
        }

        public override ComStatus Com1Status
        {
            get
            {
                return FlightSimProviders.SimConnect.Com1Status;
            }
        }

        public override ComStatus Com2Status
        {
            get
            {
                return FlightSimProviders.SimConnect.Com2Status;
            }
        }

        public override bool Com1Receive
        {
            get
            {
                return radioSwitches.Value.IsBitSet(5);
            }
        }

        public override bool Com2Receive
        {
            get
            {
                return radioSwitches.Value.IsBitSet(5);
            }
        }

        public override string Com1ActiveFrequencyIdent
        {
            get
            {
                return FlightSimProviders.SimConnect.Com1ActiveFrequencyIdent;
            }
        }

        public override string Com2ActiveFrequencyIdent
        {
            get
            {
                return FlightSimProviders.SimConnect.Com2ActiveFrequencyIdent;
            }
        }

        public override double CollectivePercent => FlightSimProviders.SimConnect.CollectivePercent;

        public override bool RotorBrakeActive => FlightSimProviders.SimConnect.RotorBrakeActive;

        public override double RotorBrakeHandlePercent => FlightSimProviders.SimConnect.RotorBrakeHandlePercent;

        public override double MainRotorDiskBankPercent => FlightSimProviders.SimConnect.MainRotorDiskBankPercent;

        public override double MainRotorDiskPitchPercent => FlightSimProviders.SimConnect.MainRotorDiskPitchPercent;

        public override double MainRotorDiskConingPercent => FlightSimProviders.SimConnect.MainRotorDiskConingPercent;

        public override double MainRotorRotationAngleDegrees => FlightSimProviders.SimConnect.MainRotorRotationAngleDegrees;

        public override double TailRotorDiskBankPercent => FlightSimProviders.SimConnect.TailRotorDiskBankPercent;

        public override double TailRotorDiskPitchPercent => FlightSimProviders.SimConnect.TailRotorDiskPitchPercent;

        public override double TailRotorDiskConingPercent => FlightSimProviders.SimConnect.TailRotorDiskConingPercent;

        public override double TailRotorRotationAngleDegrees => FlightSimProviders.SimConnect.TailRotorRotationAngleDegrees;

        public override double MainRotorRpm => FlightSimProviders.SimConnect.MainRotorRpm;

        public override double TailRotorRpm => FlightSimProviders.SimConnect.TailRotorRpm;

        public override double MainRotorBladePitchPercent => FlightSimProviders.SimConnect.MainRotorBladePitchPercent;

        public override double TailRotorBladePitchPercent => FlightSimProviders.SimConnect.TailRotorBladePitchPercent;

        public override double RotorLateralTrimPercent => FlightSimProviders.SimConnect.RotorLateralTrimPercent;

        public override double RotorLongitudinalTrimPercent => FlightSimProviders.SimConnect.RotorLongitudinalTrimPercent;

        public override double Engine1RotorRpmCommandPercent => FlightSimProviders.SimConnect.Engine1RotorRpmCommandPercent;

        public override double Engine2RotorRpmCommandPercent => FlightSimProviders.SimConnect.Engine2RotorRpmCommandPercent;

        public override double Engine1TorquePercent => FlightSimProviders.SimConnect.Engine1TorquePercent;

        public override double Engine2TorquePercent => FlightSimProviders.SimConnect.Engine2TorquePercent;

        public override bool RotorGovernor1Active => FlightSimProviders.SimConnect.RotorGovernor1Active;

        public override bool RotorGovernor2Active => FlightSimProviders.SimConnect.RotorGovernor2Active;

        public override bool GpsDrivesNav => gpsDrivesNav.Value != 0;

        public override bool FlightDirectorActive => flightDirector.Value != 0;

        public override bool AutopilotVerticalSpeedHold => vsHold.Value != 0;

        public override double AutopilotVerticalSpeedTargetFpm => vsTarget.Value;

        public override bool FlightPlanIsActiveFlightPlan => FlightSimProviders.SimConnect.FlightPlanIsActiveFlightPlan;

        public override int FlightPlanWaypointsNumber => FlightSimProviders.SimConnect.FlightPlanWaypointsNumber;

        public override int FlightPlanActiveWaypoint => FlightSimProviders.SimConnect.FlightPlanActiveWaypoint;

        public override bool FlightPlanIsDirectTo => FlightSimProviders.SimConnect.FlightPlanIsDirectTo;

        public override string FlightPlanApproachIdent => FlightSimProviders.SimConnect.FlightPlanApproachIdent;

        public override int FlightPlanWaypointIndex => FlightSimProviders.SimConnect.FlightPlanWaypointIndex;

        public override string FlightPlanWaypointIdent => FlightSimProviders.SimConnect.FlightPlanWaypointIdent;

        public override int FlightPlanApproachWaypointsNumber => FlightSimProviders.SimConnect.FlightPlanApproachWaypointsNumber;

        public override int FlightPlanActiveApproachWaypoint => FlightSimProviders.SimConnect.FlightPlanActiveApproachWaypoint;

        public override bool FlightPlanApproachIsWaypointRunway => FlightSimProviders.SimConnect.FlightPlanApproachIsWaypointRunway;

        private AbortableTaskRunner? _timerConnection = null;
        private AbortableTaskRunner? _timerMain = null;
        private bool initialized = false;
        private bool stop = false;

        private Offset<short> kollsman = new Offset<short>(0x0330);
        private Offset<short> kollsman2 = new Offset<short>(0x0332);
        private Offset<double> pressure = new Offset<double>(0x34A0);
        private Offset<int> airSpeedIndicated = new Offset<int>(0x02BC);
        private Offset<int> airSpeedTrue = new Offset<int>(0x02B8);
        private Offset<int> airSpeedBarberPole = new Offset<int>(0x02C4);
        private Offset<int> groundSpeed = new Offset<int>(0x02B4);
        private Offset<int> verticalSpeed = new Offset<int>(0x02C8);
        private Offset<ushort> headingBug = new Offset<ushort>(0x07CC);
        private Offset<double> headingMagnetic = new Offset<double>(0x02CC);
        private Offset<uint> headingTrue = new Offset<uint>(0x0580);
        private Offset<ushort> onGround = new Offset<ushort>(0x0366);
        private Offset<string> aircraftType = new Offset<string>(0x3160, 24);
        private Offset<string> aircraftModel = new Offset<string>(0x3500, 24);
        private Offset<FsLongitude> longitude = new Offset<FsLongitude>(0x0568, 8);
        private Offset<FsLatitude> latitude = new Offset<FsLatitude>(0x0560, 8);
        private Offset<short> internationalUnits = new Offset<short>(0x0C18);
        private Offset<int> altitude = new Offset<int>(0x3324);
        private Offset<int> groundAltitude = new Offset<int>(0x0020);
        private Offset<byte> engineType = new Offset<byte>(0x0609);
        private Offset<short> ambientWindSpeed = new Offset<short>(0x0E90);
        private Offset<double> ambientWindDirection = new Offset<double>(0x3490);
        private Offset<double> ambientTemperature = new Offset<double>(0x34A8);
        private Offset<double> pressureAltitude = new Offset<double>(0x34B0);
        private Offset<byte> readyToFly = new Offset<byte>(0x3364);
        private Offset<double> gpsRequiredMagneticHeading = new Offset<double>(0x6050);
        private Offset<double> gpsRequiredTrueHeading = new Offset<double>(0x6060);
        private Offset<double> gpsCrossTrackError = new Offset<double>(0x6058);
        private Offset<uint> gpsFlags = new Offset<uint>(0x6004);
        private Offset<double> pitch = new Offset<double>(0x0578);
        private Offset<double> bank = new Offset<double>(0x057C);
        private Offset<short> gearType = new Offset<short>(0x060C);
        private Offset<short> nav1RelativeBearing = new Offset<short>(0x0C56);
        private Offset<short> nav2RelativeBearing = new Offset<short>(0x0C5C);
        private Offset<string> title = new Offset<string>(0x3D00, 256);
        private Offset<string> atcAirline = new Offset<string>(0x3148, 24);
        private Offset<string> atcFlightNumber = new Offset<string>(0x3130, 12);
        private Offset<string> atcIdentifier = new Offset<string>(0x313C, 12);
        private Offset<byte> gearTypeExtended = new Offset<byte>(0x05D6);
        private Offset<ushort> nav1Obs = new Offset<ushort>(0x0C4E);
        private Offset<ushort> nav2Obs = new Offset<ushort>(0x0C5E);
        private Offset<ushort> nav1Radial = new Offset<ushort>(0x0C50);
        private Offset<ushort> nav2Radial = new Offset<ushort>(0x0C60);
        private Offset<short> nav1MagVar = new Offset<short>(0x0C40);
        private Offset<short> nav2MagVar = new Offset<short>(0x0C42);
        private Offset<ushort> nav1ActiveFrequency = new Offset<ushort>(0x0350);
        private Offset<ushort> nav2ActiveFrequency = new Offset<ushort>(0x0352);
        private Offset<ushort> nav1StandByFrequency = new Offset<ushort>(0x311E);
        private Offset<ushort> nav2StandByFrequency = new Offset<ushort>(0x3120);
        private Offset<ushort> com1ActiveFrequency = new Offset<ushort>(0x034E);
        private Offset<ushort> com2ActiveFrequency = new Offset<ushort>(0x3118);
        private Offset<ushort> com1StandByFrequency = new Offset<ushort>(0x311A);
        private Offset<ushort> com2StandByFrequency = new Offset<ushort>(0x311C);
        private Offset<ushort> adfActiveFrequency = new Offset<ushort>(0x034C);
        private Offset<ushort> transponder = new Offset<ushort>(0x0354);
        private Offset<byte> nav1ToFromFlag = new Offset<byte>(0x0C4B);
        private Offset<byte> nav2ToFromFlag = new Offset<byte>(0x0C5B);
        private Offset<float> nav1CourseDeviation = new Offset<float>(0x2AAC);
        private Offset<float> nav2CourseDeviation = new Offset<float>(0x2AB4);
        private Offset<FsLongitude> vor1Longitude = new Offset<FsLongitude>(0x0878, 8);
        private Offset<FsLatitude> vor1Latitude = new Offset<FsLatitude>(0x0874, 8);
        private Offset<FsLongitude> vor2Longitude = new Offset<FsLongitude>(0x0860, 8);
        private Offset<FsLatitude> vor2Latitude = new Offset<FsLatitude>(0x0858, 8);
        private Offset<int> vor1Elevation = new Offset<int>(0x087C);
        private Offset<int> vor2Elevation = new Offset<int>(0x0868);
        private Offset<string> vor1Identity = new Offset<string>(0x3000, 6);
        private Offset<string> vor2Identity = new Offset<string>(0x301F, 6);
        private Offset<string> vor1Name = new Offset<string>(0x3006, 25);
        private Offset<string> vor2Name = new Offset<string>(0x3025, 25);
        private Offset<short> adfRelativeBearing = new Offset<short>(0x0C6A);
        private Offset<FsLongitude> adfLongitude = new Offset<FsLongitude>(0x1124, 8);
        private Offset<FsLatitude> adfLatitude = new Offset<FsLatitude>(0x1128, 8);
        private Offset<int> adfElevation = new Offset<int>(0x112C);
        private Offset<string> adfIdentity = new Offset<string>(0x303E, 6);
        private Offset<string> adfName = new Offset<string>(0x3044, 25);
        private Offset<short> dme1Distance = new Offset<short>(0x0300);
        private Offset<short> dme2Distance = new Offset<short>(0x0306);
        private Offset<short> dme1Speed = new Offset<short>(0x0302);
        private Offset<short> dme2Speed = new Offset<short>(0x0308);
        private Offset<short> dme1TimeToStation = new Offset<short>(0x0304);
        private Offset<short> dme2TimeToStation = new Offset<short>(0x030A);
        private Offset<int> nav1Available = new Offset<int>(0x07A0);
        private Offset<int> nav2Available = new Offset<int>(0x07A4);
        private Offset<byte> radioSwitches = new Offset<byte>(0x3122);
        private Offset<int> electricalMasterBattery = new Offset<int>(0x281C);
        private Offset<int> avionicsMasterSwitch = new Offset<int>(0x2E80);
        private Offset<int> engineGeneratorSwitch = new Offset<int>(0x3B78);
        private Offset<byte> pitotHeatSwitch = new Offset<byte>(0x029C);

        private Offset<short> throttle1Percent = new Offset<short>(0x088C);
        private Offset<short> propPitch1Percent = new Offset<short>(0x088E);
        private Offset<short> mixture1Percent = new Offset<short>(0x0890);
        private Offset<short> engine1Running = new Offset<short>(0x0894);
        private Offset<short> engine1N1Percent = new Offset<short>(0x0898);
        private Offset<short> manifold1Inhg = new Offset<short>(0x08C0);
        private Offset<double> prop1Rpm = new Offset<double>(0x2400);
        private Offset<int> torque1FtLbs = new Offset<int>(0x08F4);
        private Offset<short> oilPressure1 = new Offset<short>(0x08BA);
        private Offset<short> carbHeat1 = new Offset<short>(0x08B2);
        private Offset<int> afterburner1 = new Offset<int>(0x2048);
        private Offset<double> cowlFlap1 = new Offset<double>(0x37F0);
        private Offset<double> propPitch1Radians = new Offset<double>(0x2418);

        private Offset<short> throttle2Percent = new Offset<short>(0x0924);
        private Offset<short> propPitch2Percent = new Offset<short>(0x0926);
        private Offset<short> mixture2Percent = new Offset<short>(0x0928);
        private Offset<short> engine2Running = new Offset<short>(0x092C);
        private Offset<short> engine2N1Percent = new Offset<short>(0x0930);
        private Offset<short> manifold2Inhg = new Offset<short>(0x0958);
        private Offset<double> prop2Rpm = new Offset<double>(0x2500);
        private Offset<int> torque2FtLbs = new Offset<int>(0x098C);
        private Offset<short> oilPressure2 = new Offset<short>(0x0952);
        private Offset<short> carbHeat2 = new Offset<short>(0x094A);
        private Offset<int> afterburner2 = new Offset<int>(0x2148);
        private Offset<double> cowlFlap2 = new Offset<double>(0x3730);
        private Offset<double> propPitch2Radians = new Offset<double>(0x2518);

        private Offset<short> throttle3Percent = new Offset<short>(0x09BC);
        private Offset<short> propPitch3Percent = new Offset<short>(0x09BE);
        private Offset<short> mixture3Percent = new Offset<short>(0x09C0);
        private Offset<short> engine3Running = new Offset<short>(0x09C4);
        private Offset<short> engine3N1Percent = new Offset<short>(0x09C8);
        private Offset<short> manifold3Inhg = new Offset<short>(0x09F0);
        private Offset<double> prop3Rpm = new Offset<double>(0x2600);
        private Offset<int> torque3FtLbs = new Offset<int>(0x0A24);
        private Offset<short> oilPressure3 = new Offset<short>(0x09EA);
        private Offset<short> carbHeat3 = new Offset<short>(0x09E2);
        private Offset<int> afterburner3 = new Offset<int>(0x2248);
        private Offset<double> cowlFlap3 = new Offset<double>(0x3670);
        private Offset<double> propPitch3Radians = new Offset<double>(0x2618);

        private Offset<short> throttle4Percent = new Offset<short>(0x0A54);
        private Offset<short> propPitch4Percent = new Offset<short>(0x0A56);
        private Offset<short> mixture4Percent = new Offset<short>(0x0A58);
        private Offset<short> engine4Running = new Offset<short>(0x0A5C);
        private Offset<short> engine4N1Percent = new Offset<short>(0x0A60);
        private Offset<short> manifold4Inhg = new Offset<short>(0x0A88);
        private Offset<double> prop4Rpm = new Offset<double>(0x2700);
        private Offset<int> torque4FtLbs = new Offset<int>(0x0ABC);
        private Offset<short> oilPressure4 = new Offset<short>(0x0A82);
        private Offset<short> carbHeat4 = new Offset<short>(0x0A7A);
        private Offset<int> afterburner4 = new Offset<int>(0x2348);
        private Offset<double> cowlFlap4 = new Offset<double>(0x35B0);
        private Offset<double> propPitch4Radians = new Offset<double>(0x2718);

        private Offset<int> flapsPercent = new Offset<int>(0x0BDC);
        private Offset<int> spoilersArmed = new Offset<int>(0x0BCC);
        private Offset<int> spoilersPercent = new Offset<int>(0x0BD0);
        private Offset<short> parkingBrake = new Offset<short>(0x0BC8);
        private Offset<short> elevatorTrimPercent = new Offset<short>(0x0BC2);
        private Offset<short> aileronTrimPercent = new Offset<short>(0x0BB8);
        private Offset<short> rudderTrimPercent = new Offset<short>(0x0BBC);

        private Offset<short> lights = new Offset<short>(0x0D0C);

        private Offset<int> apMaster = new Offset<int>(0x07BC);
        private Offset<int> apHdgHold = new Offset<int>(0x07C8);
        private Offset<int> apAltHold = new Offset<int>(0x07D0);
        private Offset<int> apNavHold = new Offset<int>(0x07E4);
        private Offset<int> apSpdHold = new Offset<int>(0x07FC);
        private Offset<int> apAltTargetFt = new Offset<int>(0x07D4);
        private Offset<short> apSpdTargetKts = new Offset<short>(0x07E2);
        private Offset<byte> propDeice = new Offset<byte>(0x337C);
        private Offset<float> gearControl = new Offset<float>(0x0BE8);
        private Offset<byte> fuelPump = new Offset<byte>(0x3125);

        private Offset<int> nav1ObsAvailable = new Offset<int>(0x07AC);
        private Offset<int> nav2ObsAvailable = new Offset<int>(0x07B0);

        private Offset<double> gpsWpLat = new Offset<double>(0x60AC);
        private Offset<double> gpsApLon = new Offset<double>(0x60B4);
        private Offset<string> gpsIdent = new Offset<string>(0x60A4, 6);

        private Offset<string> vor1Ident = new Offset<string>(0x3000, 6);
        private Offset<string> vor2Ident = new Offset<string>(0x301F, 6);

        private Offset<int> flightDirector = new Offset<int>(0x2EE0);
        private Offset<int> gpsDrivesNav = new Offset<int>(0x132C);

        private Offset<int> vsHold = new Offset<int>(0x07EC);
        private Offset<short> vsTarget = new Offset<short>(0x07F2);

        public bool RequestTrafficInfo { get; set; } = false;

        static FSUIPCProvider()
        {
            Instance = new FSUIPCProvider();
        }

        FSUIPCProvider()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (!initialized)
            {
                initialized = true;
                if (_traffic == null)
                {
                    _traffic = new Dictionary<string, Aircraft>();
                }
                if (_timerConnection == null)
                {
                    _timerConnection = new AbortableTaskRunner();
                    _timerConnection.Start(timerConnection_DoWorkAsync);
                }
            }
        }

        private async Task timerConnection_DoWorkAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested && !IsConnected && !stop)
            {
                try
                {
                    FSUIPCConnection.Open();
                    _isConnected = true;
                    Connected();
                    _timerMain = new AbortableTaskRunner();
                    _timerMain.Start(ProcessMain);
                    FSUIPCConnection.AITrafficServices.UpdateExtendedPlaneIndentifiers(true, true, true, true);
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

        private string _currentATCType = string.Empty;
        private string _currentATCModel = string.Empty;

        private readonly Stopwatch _positionTimer = Stopwatch.StartNew();
        private long _nextPositionUpdateMs = 0;

        private async Task ProcessMain(CancellationToken token)
        {
            while (!token.IsCancellationRequested && IsConnected && !stop)
            {
                try
                {
                    double lat = Latitude;
                    double lng = Longitude;
                    FSUIPCConnection.Process();
                    long now = _positionTimer.ElapsedMilliseconds;
                    if (now >= _nextPositionUpdateMs)
                    {
                        SetPosition(latitude.Value.DecimalDegrees, longitude.Value.DecimalDegrees);
                        if (RequestTrafficInfo)
                        {
                            GetAITraffic();
                        }
                        _nextPositionUpdateMs = now + 1000;
                    }
                    double distance = Tools.DistanceTo(lat, lng, Latitude, Longitude);
                    //Have we moved more than 500M in 1 millisecond?
                    if (readyToFly.ValueChanged || distance >= 500)
                    {
                        ReadyToFly(IsReadyToFly);
                    }
                    if (string.IsNullOrEmpty(_currentATCModel) || aircraftModel.ValueChanged || !_currentATCModel.Equals(aircraftModel.Value) || string.IsNullOrEmpty(_currentATCType) || aircraftType.ValueChanged || !_currentATCType.Equals(aircraftType.Value))
                    {
                        _atcModel = _currentATCModel = aircraftModel.Value;
                        _atcType = _currentATCType = aircraftType.Value;
                        AircraftData data = Tools.LoadAircraft(aircraftType.Value, aircraftModel.Value);
                        if (data == null)
                        {
                            data = Tools.GetDefaultAircraft(aircraftType.Value, aircraftModel.Value);
                            data.Name = title.Value;
                            data.EngineType = (EngineType)engineType.Value;
                            data.Helo = data.EngineType == EngineType.Helo;
                        }
                        _aircraftId = data.AircraftId;
                        _aircraftName = data.FriendlyName;
                        _atcType = data.FriendlyType;
                        _atcModel = data.FriendlyModel;
                        _engineType = data.EngineType;
                        _isHeavy = data.Heavy;
                        _isHelo = data.Helo;
                        AircraftChange(_aircraftId);
                        ReadyToFly(IsReadyToFly);
                    }
                    FlightDataReceived();
                }
                catch (Exception)
                {
                    _isConnected = false;
                    _atcModel = string.Empty;
                    _atcType = string.Empty;
                    _engineType = EngineType.Piston;
                    _isHeavy = false;
                    _isHelo = false;
                    Quit();
                }
                finally
                {
                    await Task.Delay(30, token);
                }
            }
        }

        private void GetAITraffic()
        {
            FSUIPCConnection.AITrafficServices.RefreshAITrafficInformation();
            List<AIPlaneInfo> allPlanes = FSUIPCConnection.AITrafficServices.AllTraffic;
            foreach (AIPlaneInfo plane in allPlanes)
            {
                try
                {
                    if (!Traffic.ContainsKey(plane.ATCIdentifier))
                    {
                        Aircraft aircraft = new Aircraft(plane);
                        _traffic.Add(plane.ATCIdentifier, aircraft);
                        TrafficReceived(plane.ATCIdentifier, aircraft, TrafficEvent.Add);
                    }
                    else
                    {
                        _traffic[plane.ATCIdentifier].UpdateAircraft(plane);
                        TrafficReceived(plane.ATCIdentifier, Traffic[plane.ATCIdentifier], TrafficEvent.Update);
                    }
                }
                catch (Exception)
                {

                }
            }
            List<Aircraft> aircraftToRemove = Traffic.Values.Where(a => !allPlanes.Any(p => p.ATCIdentifier == a.Callsign)).ToList();
            foreach (Aircraft aircraft in aircraftToRemove)
            {
                _traffic.Remove(aircraft.Callsign);
                TrafficReceived(aircraft.Callsign, aircraft, TrafficEvent.Remove);
            }
        }

        public override void Deinitialize(int timeOut = 1000)
        {
            Task.WhenAll(
                StopTimerConnectionAsync(timeOut),
                StopTimerMainAsync(timeOut)
            ).GetAwaiter().GetResult();
            FSUIPCConnection.Close();
            _isConnected = false;
            Quit();
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

        public async Task StopTimerMainAsync(int timeOut = 1000)
        {
            if (_timerMain != null && _timerMain.IsRunning)
            {
                await _timerMain.StopAsync(timeOut).ConfigureAwait(false);
            }
            _timerMain?.Dispose();
            _timerMain = null;
        }

        public override void SendCommandToFS(string command)
        {
            if (_isConnected)
            {
                FsControl fsControl = (FsControl)Enum.Parse(typeof(FsControl), command, true);
                FSUIPCConnection.SendControlToFS(fsControl, 0);
            }
        }

        public override void SendControlToFS(string control, float value)
        {
            if (_isConnected)
            {
                FsControl fsControl = (FsControl)Enum.Parse(typeof(FsControl), control, true);
                FSUIPCConnection.SendControlToFS(fsControl, Convert.ToInt32(value));
            }
        }

        public override void SendSimControlToFS(string control, float value)
        {
            if (_isConnected)
            {
                FSUIPCControl fsuipcControl = (FSUIPCControl)Enum.Parse(typeof(FSUIPCControl), control, true);
                FSUIPCConnection.SendControlToFS(fsuipcControl, Convert.ToInt32(value));
            }
        }

        public override void SendAutoPilotControlToFS(string control, float value)
        {
            if (_isConnected)
            {
                FSUIPCAutoPilotControl autoPilotControl = (FSUIPCAutoPilotControl)Enum.Parse(typeof(FSUIPCAutoPilotControl), control, true);
                FSUIPCConnection.SendControlToFS(autoPilotControl, Convert.ToInt32(value));
            }
        }

        public override void SendAxisControlToFS(string control, float value)
        {
            if (_isConnected)
            {
                FSUIPCAxisControl axisControl = (FSUIPCAxisControl)Enum.Parse(typeof(FSUIPCAxisControl), control, true);
                FSUIPCConnection.SendControlToFS(axisControl, Convert.ToInt32(value));
            }
        }
    }
}
