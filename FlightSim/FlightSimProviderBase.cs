using System.Globalization;
using System.Reflection;
#if DEBUG
using NLog;
#endif

namespace FlightSim
{
    public abstract class FlightSimProviderBase
    {
#if DEBUG
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
#endif
        public delegate void FlightSimEventHandler(FlightSimProviderBase sender);
        public delegate void FlightSimTrafficEventHandler(FlightSimProviderBase sender, string callsign, Aircraft? aircraft, TrafficEvent eventType);
        public delegate void FlightSimReadyEventHandler(FlightSimProviderBase sender, ReadyToFly readyToFly);
        public delegate void FlightSimAircraftChangeEventHandler(FlightSimProviderBase sender, int aircraftId);
        public delegate void FlightSimErrorEventHandler(FlightSimProviderBase sender, FlightSimProviderException ex);
        public event FlightSimEventHandler? OnConnected;
        public event FlightSimEventHandler? OnQuit;
        public event FlightSimTrafficEventHandler? OnTrafficReceived;
        public event FlightSimEventHandler? OnFlightDataReceived;
        public event FlightSimReadyEventHandler? OnReadyToFly;
        public event FlightSimAircraftChangeEventHandler? OnAircraftChange;
        public event FlightSimEventHandler? OnSetLeds;
        public event FlightSimEventHandler? OnStopTimer;
        public event FlightSimEventHandler? OnUdatePage;
        public event FlightSimErrorEventHandler? OnError;

        private TimeSpan _posUtcOffset = TimeSpan.Zero;
        private double _posOffsetLat;
        private double _posOffsetLon;
        private DateTime _posOffsetLastUtc = DateTime.MinValue;
        private int _posOffsetRefreshGate = 0;
        private volatile IP2Location? _posLocation;
        private string _posTzText = "+00:00"; // optional convenience
        private string _posLocationSig = ""; // last known signature (normalized)

        private static string Norm(string? s) => (s ?? "").Trim();

        private static string LocationSignature(IP2Location? loc)
        {
            if (loc == null) return "";

            // Only fields you care about
            return string.Join("|", new[]
            {
                Norm(loc.timezone),
                Norm(loc.city),
                Norm(loc.region),
                Norm(loc.country)
            }).ToUpperInvariant();
        }

        private int _posRefreshScheduled = 0;
        private CancellationTokenSource? _posRefreshCts;

        private void SchedulePositionRefresh()
        {
            // allow only one scheduled refresh at a time
            if (Interlocked.Exchange(ref _posRefreshScheduled, 1) == 1)
                return;

            var cts = new CancellationTokenSource();
            var prev = Interlocked.Exchange(ref _posRefreshCts, cts);
            prev?.Cancel();
            prev?.Dispose();

            _ = Task.Run(async () =>
            {
                try
                {
                    // Small debounce window so Lat+Lon setters collapse into one call
                    await Task.Delay(75, cts.Token);
                    RefreshPositionOffsetIfNeeded();   // your existing method
                }
                catch (OperationCanceledException) { }
                finally
                {
                    Interlocked.Exchange(ref _posRefreshScheduled, 0);
                }
            });
        }

        private void RefreshPositionOffsetIfNeeded()
        {
            if (Interlocked.Exchange(ref _posOffsetRefreshGate, 1) == 1)
                return;

            _ = Task.Run(() =>
            {
                try
                {
                    const double thresholdDegrees = 0.25;
                    var nowUtc = DateTime.UtcNow;

                    bool needs =
                        (nowUtc - _posOffsetLastUtc) > TimeSpan.FromMinutes(5) ||
                        Math.Abs(Latitude - _posOffsetLat) > thresholdDegrees ||
                        Math.Abs(Longitude - _posOffsetLon) > thresholdDegrees;

                    if (!needs) return;

                    _posOffsetLat = Latitude;
                    _posOffsetLon = Longitude;

                    var loc = Tools.GetLocation(_posOffsetLat, _posOffsetLon);

                    if (loc != null)
                    {
                        _posLocation = loc;
                        _posTzText = loc.timezone ?? "+00:00";

                        // 1) Base offset from timezone text
                        if (Tools.TryParseUtcOffset(loc.timezone, out var off))
                        {
                            _posUtcOffset = off;
                        }
                        else
                        {
                            _posUtcOffset = TimeSpan.Zero;
                        }
                        // 2) Apply DST "good enough" rule
                        // If loc.dst is true AND local time is currently in DST, add 1 hour.
                        if (loc.dst)
                        {
                            var localNow = nowUtc + _posUtcOffset;

                            if (IsDst(localNow, _posOffsetLat))
                            {
                                _posUtcOffset = _posUtcOffset + TimeSpan.FromHours(1);
                                _posTzText = FormatUtcOffset(_posUtcOffset);
                            }
                        }

                        _posOffsetLastUtc = nowUtc;

                        var sig = LocationSignature(loc);
                        if (!string.Equals(sig, _posLocationSig, StringComparison.Ordinal))
                        {
                            _posLocationSig = sig;
                            FlightDataReceived();
                        }
                    }
                    else
                    {
                        _posUtcOffset = TimeSpan.Zero;
                        _posTzText = "+00:00";
                        _posOffsetLastUtc = nowUtc;
                    }
                }
                catch
                {
                    // keep last known offset/location
                }
                finally
                {
                    Interlocked.Exchange(ref _posOffsetRefreshGate, 0);
                }
            });
        }

        private static string FormatUtcOffset(TimeSpan offset)
        {
            // Produces +HH:MM or -HH:MM
            return (offset < TimeSpan.Zero ? "-" : "+") + offset.Duration().ToString(@"hh\:mm");
        }

        private static bool IsDst(DateTime localTime, double latitude)
        {
            return latitude >= 0 ? IsNorthernDst(localTime) : IsSouthernDst(localTime);
        }

        private static bool IsNorthernDst(DateTime localTime)
        {
            int year = localTime.Year;

            // US-style: 2nd Sunday in March @ 02:00
            DateTime start = NthSundayOfMonth(year, 3, 2).AddHours(2);

            // 1st Sunday in November @ 02:00
            DateTime end = NthSundayOfMonth(year, 11, 1).AddHours(2);

            return localTime >= start && localTime < end;
        }

        private static bool IsSouthernDst(DateTime localTime)
        {
            int year = localTime.Year;

            // Typical southern pattern:
            // Starts: 1st Sunday in October @ 02:00
            // Ends:   1st Sunday in April @ 03:00
            DateTime start = NthSundayOfMonth(year, 10, 1).AddHours(2);
            DateTime end = NthSundayOfMonth(year, 4, 1).AddHours(3);

            // Because it crosses New Year:
            return localTime >= start || localTime < end;
        }

        private static DateTime NthSundayOfMonth(int year, int month, int nth)
        {
            var firstDay = new DateTime(year, month, 1);
            int delta = ((int)DayOfWeek.Sunday - (int)firstDay.DayOfWeek + 7) % 7;
            var firstSunday = firstDay.AddDays(delta);
            return firstSunday.AddDays(7 * (nth - 1));
        }

        public abstract void Deinitialize(int timeOut = 1000);

        public abstract void SendControlToFS(string control, float value);

        public abstract void SendCommandToFS(string command);

        public virtual void SendSimControlToFS(string control, float value)
        {
        }

        public virtual void SendAutoPilotControlToFS(string control, float value)
        {
        }

        public virtual void SendAxisControlToFS(string control, float value)
        {
        }

        [FlightSimField("Provider Name", MaxLength = 25, PadAlign = FieldPadAlign.Left)]
        public abstract string Name { get; }

        public abstract Dictionary<string, Aircraft> Traffic { get; }

        public LatLong Location => new LatLong(Latitude, Longitude);

        public static UnitSystem Units { get; set; } = UnitSystem.Aviation;

        [FlightSimField("Position City", MaxLength = 25, PadAlign = FieldPadAlign.Left)]
        public string PositionCity
        {
            get { RefreshPositionOffsetIfNeeded(); return _posLocation?.city ?? ""; }
        }

        [FlightSimField("Position City, Region Code", MaxLength = 25, PadAlign = FieldPadAlign.Left)]
        public string PositionCityRegion
        {
            get 
            { 
                RefreshPositionOffsetIfNeeded();
                if (_posLocation != null)
                {
                    if (!string.IsNullOrEmpty(_posLocation.regioncode))
                    {
                        string region = $", {_posLocation.regioncode}";
                        if (!string.IsNullOrEmpty(_posLocation.city))
                        {
                            string city = _posLocation.city.Substring(0, Math.Min(_posLocation.city.Length, 25 - region.Length));
                            return city + region;
                        }
                        return _posLocation.regioncode;
                    }
                    else
                    {
                        return _posLocation.city ?? string.Empty;
                    }
                }
                return string.Empty; 
            }
        }

        [FlightSimField("Position Region", MaxLength = 25, PadAlign = FieldPadAlign.Left)]
        public string PositionRegion
        {
            get { RefreshPositionOffsetIfNeeded(); return _posLocation?.region ?? string.Empty; }
        }

        [FlightSimField("Position Region Code", MaxLength = 3, PadAlign = FieldPadAlign.Left)]
        public string PositionRegionCode
        {
            get { RefreshPositionOffsetIfNeeded(); return _posLocation?.regioncode ?? string.Empty; }
        }

        [FlightSimField("Position Country", MaxLength = 25, PadAlign = FieldPadAlign.Left)]
        public string PositionCountry
        {
            get { RefreshPositionOffsetIfNeeded(); return _posLocation?.country ?? string.Empty; }
        }

        [FlightSimField("Position Country Code", MaxLength = 2, PadAlign = FieldPadAlign.Left)]
        public string PositionCountryCountry
        {
            get { RefreshPositionOffsetIfNeeded(); return _posLocation?.countrycode ?? string.Empty; }
        }

        [FlightSimField("Position Timezone", MaxLength = 6, PadAlign = FieldPadAlign.Left)]
        public string PositionTimeZoneOffset
        {
            get { RefreshPositionOffsetIfNeeded(); return _posTzText; }
        }

        [FlightSimField("Unit System", MaxLength = 4, PadAlign = FieldPadAlign.Left)]
        public string UnitSystemDisplay => Units == UnitSystem.Aviation ? "AVTN" : "MET";

        [FlightSimField("Vertical Speed Scale", MaxLength = 3, PadAlign = FieldPadAlign.Left)]
        public string VerticalSpeedScaleDisplay => Units == UnitSystem.Aviation ? "FPM" : "MPS";

        [FlightSimField("Speed Scale", MaxLength = 3, PadAlign = FieldPadAlign.Left)]
        public string SpeedScaleDisplay => Units == UnitSystem.Aviation ? "KTS" : "KPH";

        [FlightSimField("Distance Scale", MaxLength = 2, PadAlign = FieldPadAlign.Left)]
        public string DistanceScaleDisplay => Units == UnitSystem.Aviation ? "NM" : "KM";

        [FlightSimField("Temperature Scale", MaxLength = 1, PadAlign = FieldPadAlign.Left)]
        public string TemperatureScaleDisplay => Units == UnitSystem.Aviation ? "F" : "C";

        [FlightSimField("Volume Scale", MaxLength = 1, PadAlign = FieldPadAlign.Left)]
        public string VolumeScaleDisplay => Units == UnitSystem.Aviation ? "GAL" : "L";

        [FlightSimField("Pressure Scale", MaxLength = 4, PadAlign = FieldPadAlign.Left)]
        public string PressureScaleDisplay => Units == UnitSystem.Aviation ? "INHG" : "HPA";

        [FlightSimField("Altitude Scale", MaxLength = 2, PadAlign = FieldPadAlign.Left)]
        public string AltitudeScaleDisplay => Units == UnitSystem.Aviation ? "FT" : "M";

        [FlightSimField("Local Time (12 hour)", MaxLength = 11, PadAlign = FieldPadAlign.Left)]
        public string LocalTime12Hour => DateTime.Now.ToString("hh:mm:ss tt");

        [FlightSimField("Local Time (24 hour)", MaxLength = 8, PadAlign = FieldPadAlign.Left)]
        public string LocalTime24Hour => DateTime.Now.ToString("HH:mm:ss");

        [FlightSimField("Zulu Time (12 hour)", MaxLength = 11, PadAlign = FieldPadAlign.Left)]
        public string ZuluTime12Hour => DateTime.UtcNow.ToString("hh:mm:ss tt");

        [FlightSimField("Zulu Time (24 hour)", MaxLength = 8, PadAlign = FieldPadAlign.Left)]
        public string ZuluTime24Hour => DateTime.UtcNow.ToString("HH:mm:ss");

        [FlightSimField("Position Time (12 hour)", MaxLength = 11, PadAlign = FieldPadAlign.Left)]
        public string PositionTime12Hour
        {
            get
            {
                RefreshPositionOffsetIfNeeded();
                var local = DateTimeOffset.UtcNow.ToOffset(_posUtcOffset);
                return local.ToString("hh:mm:ss tt", CultureInfo.InvariantCulture);
            }
        }

        [FlightSimField("Position Time (24 hour)", MaxLength = 8, PadAlign = FieldPadAlign.Left)]
        public string PositionTime24Hour
        {
            get
            {
                RefreshPositionOffsetIfNeeded();
                var local = DateTimeOffset.UtcNow.ToOffset(_posUtcOffset);
                return local.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
            }
        }

        [FlightSimField("Position UTC Offset", MaxLength = 6, PadAlign = FieldPadAlign.Left)]
        public string PositionUtcOffset
        {
            get
            {
                RefreshPositionOffsetIfNeeded();
                var off = _posUtcOffset;
                char sign = off < TimeSpan.Zero ? '-' : '+';
                off = off.Duration();
                return $"{sign}{off:hh\\:mm}";
            }
        }

        [FlightSimField("Aircraft ID", Format = "0", MaxLength = 6)]
        public abstract int AircraftId { get; }

        [FlightSimField("Aircraft Name", MaxLength = 25, PadAlign = FieldPadAlign.Left)]
        public abstract string AircraftName { get; }

        [FlightSimField("Altitude MSL (disp)", Format = "0", MaxLength = 8)]
        public virtual string AltitudeMSLDisplay => AltitudeMSL.ToString("0", CultureInfo.InvariantCulture) + " " + AltitudeScaleDisplay;

        [FlightSimField("Altitude AGL (disp)", Format = "0", MaxLength = 8)]
        public virtual string AltitudeAGLDisplay => AltitudeAGL.ToString("0", CultureInfo.InvariantCulture) + " " + AltitudeScaleDisplay;

        [FlightSimField("Altitude True (disp)", Format = "0", MaxLength = 8)]
        public virtual string AltitudeTrueDisplay => AltitudeTrue.ToString("0", CultureInfo.InvariantCulture) + " " + AltitudeScaleDisplay;

        [FlightSimField("Altitude MSL (sys)", MaxLength = 5, PadAlign = FieldPadAlign.Left)] 
        public virtual double AltitudeMSL => Units == UnitSystem.Metric ? AltitudeMSLMeters : AltitudeMSLFeet;
        
        [FlightSimField("Altitude AGL (sys)", MaxLength = 5, PadAlign = FieldPadAlign.Left)] 
        public virtual double AltitudeAGL => Units == UnitSystem.Metric ? AltitudeAGLMeters : AltitudeAGLFeet;

        [FlightSimField("Altitude True (sys)", Format = "00000", MaxLength = 5)]
        public virtual double AltitudeTrue => Units == UnitSystem.Metric ? AltitudeTrueMeters : AltitudeTrueFeet;

        [FlightSimField("Altitude MSL (ft)", Format = "00000", MaxLength = 5)]
        public abstract double AltitudeMSLFeet { get; }

        [FlightSimField("Altitude AGL (ft)", Format = "00000", MaxLength = 5)]
        public abstract double AltitudeAGLFeet { get; }

        [FlightSimField("Pressure Altitude (ft)", Format = "00000", MaxLength = 5)]
        public abstract double AltitudeTrueFeet { get; }

        [FlightSimField("Altitude MSL (meters)", Format = "0000", MaxLength = 4)]
        public virtual double AltitudeMSLMeters => Tools.FeetToMeters(AltitudeMSLFeet);

        [FlightSimField("Altitude AGL (meters)", Format = "0000", MaxLength = 4)]
        public virtual double AltitudeAGLMeters => Tools.FeetToMeters(AltitudeAGLFeet);

        [FlightSimField("Pressure Altitude (meters)", Format = "0000", MaxLength = 4)]
        public virtual double AltitudeTrueMeters => Tools.FeetToMeters(AltitudeTrueFeet);

        [FlightSimField("Heading Magnetic (deg)", Format = "0°", MaxLength = 4, PadAlign = FieldPadAlign.Left)]
        public abstract double HeadingMagneticDegrees { get; }

        [FlightSimField("Heading True (deg)", Format = "0°", MaxLength = 4, PadAlign = FieldPadAlign.Left)]
        public abstract double HeadingTrueDegrees { get; }

        public abstract double HeadingMagneticRadians { get; }

        public abstract double HeadingTrueRadians { get; }

        public abstract bool IsConnected { get; }

        [FlightSimField("ATC Identifier", MaxLength = 8, PadAlign = FieldPadAlign.Left)]
        public abstract string ATCIdentifier { get; }

        [FlightSimField("ATC Model", MaxLength = 16, PadAlign = FieldPadAlign.Left)]
        public abstract string ATCModel { get; }

        [FlightSimField("ATC Type", MaxLength = 16, PadAlign = FieldPadAlign.Left)]
        public abstract string ATCType { get; }

        public abstract bool IsHeavy { get; }

        public abstract bool IsGearFloats { get; }

        [FlightSimField("Ident", MaxLength = 4, TrueText = "IDNT", FalseText = "", InvertWhenTrue = true)]
        public abstract bool IdentActive { get; }

        public abstract bool IsHelo { get; }

        [FlightSimField("Engine Type", MaxLength = 9, PadAlign = FieldPadAlign.Left)]
        public abstract EngineType EngineType { get; }

        public abstract bool OnGround { get; }

        [FlightSimField("Ground Speed (disp)", MaxLength = 6, PadAlign = FieldPadAlign.Left)]
        public virtual string GroundSpeedDisplay => GroundSpeed.ToString("0", CultureInfo.InvariantCulture) + " " + SpeedScaleDisplay;

        [FlightSimField("IAS (disp)", MaxLength = 6, PadAlign = FieldPadAlign.Left)]
        public virtual string AirSpeedIndicatedDisplay => AirSpeedIndicated.ToString("0", CultureInfo.InvariantCulture) + " " + SpeedScaleDisplay;

        [FlightSimField("TAS (disp)", MaxLength = 6, PadAlign = FieldPadAlign.Left)]
        public virtual string AirSpeedTrueDisplay => AirSpeedTrue.ToString("0", CultureInfo.InvariantCulture) + " " + SpeedScaleDisplay;

        [FlightSimField("Ground Speed (sys)", Format = "000", MaxLength = 3)]
        public virtual double GroundSpeed => Units == UnitSystem.Metric ? GroundSpeedKph : GroundSpeedKnots;

        [FlightSimField("IAS (sys)", Format = "000", MaxLength = 3)]
        public virtual double AirSpeedIndicated => Units == UnitSystem.Metric ? AirSpeedIndicatedKph : AirSpeedIndicatedKnots;

        [FlightSimField("TAS (sys)", Format = "000", MaxLength = 3)]
        public virtual double AirSpeedTrue => Units == UnitSystem.Metric ? AirSpeedTrueKph : AirSpeedTrueKnots;

        [FlightSimField("Ground Speed (kts)", Format = "000", MaxLength = 3)]
        public abstract double GroundSpeedKnots { get; }

        [FlightSimField("IAS (kts)", Format = "000", MaxLength = 3)]
        public abstract double AirSpeedIndicatedKnots { get; }

        [FlightSimField("TAS (kts)", Format = "000", MaxLength = 3)]
        public abstract double AirSpeedTrueKnots { get; }

        [FlightSimField("Ground Speed (mph)", Format = "000", MaxLength = 3)]
        public virtual double GroundSpeedMph => Tools.KnotsToMph(GroundSpeedKnots);

        [FlightSimField("IAS (mph)", Format = "000", MaxLength = 3)]
        public virtual double AirSpeedIndicatedMph => Tools.KnotsToMph(AirSpeedIndicatedKnots);

        [FlightSimField("TAS (mph)", Format = "000", MaxLength = 3)]
        public virtual double AirSpeedTrueMph => Tools.KnotsToMph(AirSpeedTrueKnots);

        [FlightSimField("Ground Speed (kph)", Format = "000", MaxLength = 3)]
        public virtual double GroundSpeedKph => Tools.KnotsToKph(GroundSpeedKnots);

        [FlightSimField("IAS (kph)", Format = "000", MaxLength = 3)]
        public virtual double AirSpeedIndicatedKph => Tools.KnotsToKph(AirSpeedIndicatedKnots);

        [FlightSimField("TAS (kph)", Format = "000", MaxLength = 3)]
        public virtual double AirSpeedTrueKph => Tools.KnotsToKph(AirSpeedTrueKnots);

        [FlightSimField("OAT (display)", MaxLength = 6, PadAlign = FieldPadAlign.Left)]
        public virtual string AmbientTemperatureDisplay => AmbientTemperature.ToString("0°", CultureInfo.InvariantCulture) + " " + TemperatureScaleDisplay;

        [FlightSimField("OAT (sys)", Format = "0°", MaxLength = 4, PadAlign = FieldPadAlign.Left)]
        public virtual double AmbientTemperature => Units == UnitSystem.Metric ? AmbientTemperatureCelsius : AmbientTemperatureFahrenheit;

        [FlightSimField("OAT (°C)", Format = "0°", MaxLength = 4)]
        public abstract double AmbientTemperatureCelsius { get; }

        [FlightSimField("OAT (°F)", Format = "0°", MaxLength = 4)]
        public virtual double AmbientTemperatureFahrenheit => Tools.CelsiusToFahrenheit(AmbientTemperatureCelsius);

        [FlightSimField("Wind Dir (deg)", Format = "000°", MaxLength = 4)]
        public abstract double AmbientWindDirectionDegrees { get; }

        [FlightSimField("Wind Speed (disp)", MaxLength = 6, PadAlign = FieldPadAlign.Left)]
        public virtual string AmbientWindSpeedDisplay => AmbientWindSpeed.ToString("0", CultureInfo.InvariantCulture) + " " + TemperatureScaleDisplay;

        [FlightSimField("Wind Speed (sys)", Format = "000", MaxLength = 3)]
        public virtual double AmbientWindSpeed => Units == UnitSystem.Metric ? AmbientWindSpeedKph : AmbientWindSpeedKnots;

        [FlightSimField("Wind Speed (kts)", Format = "000", MaxLength = 3)]
        public abstract double AmbientWindSpeedKnots { get; }

        [FlightSimField("Wind Speed (mph)", Format = "000", MaxLength = 3)]
        public virtual double AmbientWindSpeedMph => Tools.KnotsToMph(AmbientWindSpeedKnots);

        [FlightSimField("Wind Speed (kph)", Format = "000", MaxLength = 3)]
        public virtual double AmbientWindSpeedKph => Tools.KnotsToKph(AmbientWindSpeedKnots);

        [FlightSimField("Kollsman (disp)", MaxLength = 10, PadAlign = FieldPadAlign.Left)]
        public virtual string KollsmanDisplay => Kollsman.ToString(Units == UnitSystem.Metric ? "0" : "00.00", CultureInfo.InvariantCulture) + " " + PressureScaleDisplay;

        [FlightSimField("Secondary Kollsman (disp)", MaxLength = 10, PadAlign = FieldPadAlign.Left)]
        public virtual string SecondaryKollsmanDisplay => SecondaryKollsman.ToString(Units == UnitSystem.Metric ? "0" : "00.00", CultureInfo.InvariantCulture) + " " + PressureScaleDisplay;

        [FlightSimField("Pressure (disp)", MaxLength = 10, PadAlign = FieldPadAlign.Left)]
        public virtual string PressureDisplay => Pressure.ToString(Units == UnitSystem.Metric ? "0" : "00.00", CultureInfo.InvariantCulture) + " " + PressureScaleDisplay;

        [FlightSimField("Kollsman (sys)", MaxLength = 5, PadAlign = FieldPadAlign.Left)]
        public virtual double Kollsman=> Units == UnitSystem.Metric ? KollsmanHectoPascals : KollsmanInchesMercury;

        [FlightSimField("SecondaryKollsman (sys)", MaxLength = 5, PadAlign = FieldPadAlign.Left)]
        public virtual double SecondaryKollsman => Units == UnitSystem.Metric ? SecondaryKollsmanHectoPascals : SecondaryKollsmanInchesMercury;

        [FlightSimField("Pressure (sys)", MaxLength = 5, PadAlign = FieldPadAlign.Left)]
        public virtual double Pressure => Units == UnitSystem.Metric ? PressureHectoPascals : PressureInchesMercury;

        [FlightSimField("Kollsman (inHg)", Format = "00.00", MaxLength = 5)]
        public abstract double KollsmanInchesMercury { get; }

        [FlightSimField("SecondaryKollsman (inHg)", Format = "00.00", MaxLength = 5)]
        public abstract double SecondaryKollsmanInchesMercury { get; }

        [FlightSimField("Pressure (inHg)", Format = "00.00", MaxLength = 5)]
        public abstract double PressureInchesMercury { get; }

        [FlightSimField("Kollsman (hPa)", Format = "0000", MaxLength = 4)]
        public virtual double KollsmanHectoPascals => Tools.InchesMercuryToHectoPascals(KollsmanInchesMercury);

        [FlightSimField("SecondaryKollsman (hPa)", Format = "0000", MaxLength = 4)]
        public virtual double SecondaryKollsmanHectoPascals => Tools.InchesMercuryToHectoPascals(SecondaryKollsmanInchesMercury);

        [FlightSimField("Pressure (hPa)", Format = "0000", MaxLength = 4)]
        public virtual double PressureHectoPascals => Tools.InchesMercuryToHectoPascals(PressureInchesMercury);

        public abstract ReadyToFly IsReadyToFly { get; }

        public abstract double GPSRequiredMagneticHeadingRadians { get; }

        public abstract double GPSRequiredTrueHeadingRadians { get; }

        [FlightSimField("GPS Required Magnetic Heading (deg)", Format = "0°", MaxLength = 4, PadAlign = FieldPadAlign.Left)]
        public virtual double GPSRequiredMagneticHeadingDegrees => Tools.NormalizeDegrees(Tools.RadToDeg(GPSRequiredMagneticHeadingRadians));

        [FlightSimField("GPS Required True Heading (deg)", Format = "0°", MaxLength = 4, PadAlign = FieldPadAlign.Left)]
        public virtual double GPSRequiredTrueHeadingDegrees => Tools.NormalizeDegrees(Tools.RadToDeg(GPSRequiredTrueHeadingRadians));

        [FlightSimField("Time To Waypoint (hh:mm)", MaxLength = 6, PadAlign = FieldPadAlign.Left)]
        public virtual string TimeToWaypointDisplay
        {
            get
            {
                var t = TimeToWaypoint;
                if (t <= TimeSpan.Zero)
                    return string.Empty;

                int totalHours = (int)t.TotalHours;
                int minutes = t.Minutes;

                // hhh:mm when hours >= 100
                if (totalHours >= 100)
                    return $"{totalHours:000}:{minutes:00}";

                // hh:mm otherwise
                return $"{totalHours:00}:{minutes:00}";
            }
        }

        public virtual TimeSpan TimeToWaypoint
        {
            get
            {
                if (!HasActiveWaypoint)
                    return TimeSpan.Zero;

                double wlat = WaypointLatitude;
                double wlon = WaypointLongitude;

                if (wlat == 0 && wlon == 0)
                    return TimeSpan.Zero;

                // Distance in nautical miles
                double distanceNm = Tools.DistanceTo(
                    Latitude,
                    Longitude,
                    wlat,
                    wlon,
                    'N'
                );

                if (distanceNm <= 0)
                    return TimeSpan.Zero;

                // Use TRUE airspeed in knots
                double speedKts = AirSpeedTrueKnots;

                // Avoid divide-by-zero / taxi situations
                if (speedKts < 5.0)
                    return TimeSpan.Zero;

                double hours = distanceNm / speedKts;

                return TimeSpan.FromHours(hours);
            }
        }

        [FlightSimField("Waypoint Distance (disp)", MaxLength = 7, PadAlign = FieldPadAlign.Left)]
        public virtual string DistanceToWaypointDisplay => DistanceToWaypoint.ToString("0", CultureInfo.InvariantCulture) + " " + DistanceScaleDisplay;

        [FlightSimField("Waypoint Distance (sys)", MaxLength = 5, Format = "00000")]
        public virtual double DistanceToWaypoint => Units == UnitSystem.Metric ? DistanceToWaypointKm : DistanceToWaypointNm;

        [FlightSimField("Waypoint Distance (nm)", MaxLength = 5, Format = "00000")]
        public virtual double DistanceToWaypointNm
        {
            get
            {
                if (!HasActiveWaypoint) return 0;

                double wlat = WaypointLatitude;
                double wlon = WaypointLongitude;
                if (wlat == 0 && wlon == 0) return 0; // “not available” convention

                return Tools.DistanceTo(Latitude, Longitude, wlat, wlon, 'N');
            }
        }

        [FlightSimField("Waypoint Distance (km)", MaxLength = 5, Format = "00000")]
        public virtual double DistanceToWaypointKm
        {
            get
            {
                if (!HasActiveWaypoint) return 0;

                double wlat = WaypointLatitude;
                double wlon = WaypointLongitude;
                if (wlat == 0 && wlon == 0) return 0;

                return Tools.DistanceTo(Latitude, Longitude, wlat, wlon, 'K');
            }
        }

        [FlightSimField("Waypoint Distance (mi)", MaxLength = 5, Format = "00000")]
        public virtual double DistanceToWaypointMi
        {
            get
            {
                if (!HasActiveWaypoint) return 0;

                double wlat = WaypointLatitude;
                double wlon = WaypointLongitude;
                if (wlat == 0 && wlon == 0) return 0;

                return Tools.DistanceTo(Latitude, Longitude, wlat, wlon, 'M');
            }
        }

        [FlightSimField("Waypoint ID", MaxLength = 5, PadAlign = FieldPadAlign.Left)]
        public abstract string WaypointIdent { get; }

        [FlightSimField("Waypoint Latitude", MaxLength = 14)]
        public abstract double WaypointLatitude { get; }

        [FlightSimField("Waypoint Longitude", MaxLength = 14)]
        public abstract double WaypointLongitude { get; }

        [FlightSimField("Waypoint Latitude (short)", MaxLength = 9)]
        public virtual string WaypointLatitudeShort => new Latitude(WaypointLatitude).ToShortString();

        [FlightSimField("Waypoint Longitude (short)", MaxLength = 9)]
        public virtual string WaypointLongitudeShort => new Longitude(WaypointLongitude).ToShortString();

        [FlightSimField("Active Waypoint", MaxLength = 3, TrueText = "ACT", FalseText = "")]
        public abstract bool HasActiveWaypoint { get; }

        [FlightSimField("GPS Cross-Track Error", MaxLength = 7, PadAlign = FieldPadAlign.Left)]
        public double GPSCrossTrackError => Units == UnitSystem.Metric ? GPSCrossTrackErrorKm : GPSCrossTrackErrorNm;

        [FlightSimField("GPS Cross-Track Error (meters)", MaxLength = 7, PadAlign = FieldPadAlign.Left)]
        public abstract double GPSCrossTrackErrorMeters { get; }

        [FlightSimField("GPS Cross-Track Error (ft)", MaxLength = 7, PadAlign = FieldPadAlign.Left)]
        public double GPSCrossTrackErrorFeet => Tools.MetersToFeet(GPSCrossTrackErrorMeters);

        [FlightSimField("GPS Cross-Track Error (km)", MaxLength = 7, PadAlign = FieldPadAlign.Left)]
        public double GPSCrossTrackErrorKm => Tools.MetersToKm(GPSCrossTrackErrorMeters);

        [FlightSimField("GPS Cross-Track Error (Nm)", MaxLength = 7, PadAlign = FieldPadAlign.Left)]
        public double GPSCrossTrackErrorNm => Tools.MetersToNm(GPSCrossTrackErrorMeters);

        [FlightSimField("NAV 1 Bearing (deg)", MaxLength = 4, Format = "0°", PadAlign = FieldPadAlign.Left)]
        public virtual double Nav1BearingDegrees => Tools.NormalizeDegrees(Nav1Radial + 180.0);

        [FlightSimField("NAV 2 Bearing (deg)", MaxLength = 4, Format = "0°", PadAlign = FieldPadAlign.Left)]
        public virtual double Nav2BearingDegrees => Tools.NormalizeDegrees(Nav2Radial + 180.0);

        [FlightSimField("NAV 1 Radial (deg)", Format = "0°", MaxLength = 4, PadAlign = FieldPadAlign.Left)]
        public abstract double Nav1Radial { get; }

        [FlightSimField("NAV 2 Radial (deg)", Format = "0°", MaxLength = 4, PadAlign = FieldPadAlign.Left)]
        public abstract double Nav2Radial { get; }

        [FlightSimField("NAV 1 DME (disp)", MaxLength = 7, PadAlign = FieldPadAlign.Left)]
        public virtual string Nav1DmeDistanceDisplay => Nav1DmeDistance.ToString("0", CultureInfo.InvariantCulture) + " " + DistanceScaleDisplay;

        [FlightSimField("NAV 2 DME (disp)", MaxLength = 7, PadAlign = FieldPadAlign.Left)]
        public virtual string Nav2DmeDistanceDisplay => Nav2DmeDistance.ToString("0", CultureInfo.InvariantCulture) + " " + DistanceScaleDisplay;

        [FlightSimField("NAV 1 DME (sys)", MaxLength = 3, Format = "000")]
        public virtual double Nav1DmeDistance => Units == UnitSystem.Metric ? Nav1DmeDistanceKm : Nav1DmeDistanceNm;

        [FlightSimField("NAV 2 DME (sys)", MaxLength = 3, Format = "000")]
        public virtual double Nav2DmeDistance => Units == UnitSystem.Metric ? Nav2DmeDistanceKm : Nav2DmeDistanceNm;

        [FlightSimField("NAV 1 DME (nm)", MaxLength = 5, Format = "0", PadAlign = FieldPadAlign.Left)]
        public abstract double Nav1DmeDistanceNm { get; }

        [FlightSimField("NAV 2 DME (nm)", MaxLength = 5, Format = "0", PadAlign = FieldPadAlign.Left)]
        public abstract double Nav2DmeDistanceNm { get; }

        [FlightSimField("NAV 1 DME (km)", MaxLength = 5, Format = "0", PadAlign = FieldPadAlign.Left)]
        public virtual double Nav1DmeDistanceKm => Tools.MetersToKm(Tools.NmToMeters(Nav1DmeDistanceNm));

        [FlightSimField("NAV 2 DME (km)", MaxLength = 5, Format = "0", PadAlign = FieldPadAlign.Left)]
        public virtual double Nav2DmeDistanceKm => Tools.MetersToKm(Tools.NmToMeters(Nav2DmeDistanceNm));

        [FlightSimField("NAV 1 DME (mi)", MaxLength = 5, Format = "0", PadAlign = FieldPadAlign.Left)]
        public virtual double Nav1DmeDistanceMi => Tools.NmToMiles(Nav1DmeDistanceNm);

        [FlightSimField("NAV 2 DME (mi)", MaxLength = 5, Format = "0", PadAlign = FieldPadAlign.Left)]
        public virtual double Nav2DmeDistanceMi => Tools.NmToMiles(Nav2DmeDistanceNm);

        [FlightSimField("NAV 1 DME Speed (disp)", MaxLength = 6, PadAlign = FieldPadAlign.Left)]
        public virtual string Nav1DmeSpeedDisplay => Nav1DmeSpeed.ToString("0", CultureInfo.InvariantCulture) + " " + SpeedScaleDisplay;

        [FlightSimField("NAV 2 DME Speed (disp)", MaxLength = 6, PadAlign = FieldPadAlign.Left)]
        public virtual string Nav2DmeSpeedDisplay => Nav2DmeSpeed.ToString("0", CultureInfo.InvariantCulture) + " " + SpeedScaleDisplay;

        [FlightSimField("NAV 1 DME Speed (sys)", MaxLength = 3, Format = "000")]
        public virtual double Nav1DmeSpeed => Units == UnitSystem.Metric ? Nav1DmeSpeedKph : Nav1DmeSpeedKts;

        [FlightSimField("NAV 2 DME Speed (sys)", MaxLength = 3, Format = "000")]
        public virtual double Nav2DmeSpeed => Units == UnitSystem.Metric ? Nav2DmeSpeedKph : Nav2DmeSpeedKts;

        [FlightSimField("NAV 1 DME Speed (kts)", MaxLength = 3, Format = "000")]
        public abstract double Nav1DmeSpeedKts { get; }

        [FlightSimField("NAV 2 DME Speed (kts)", MaxLength = 3, Format = "000")]
        public abstract double Nav2DmeSpeedKts { get; }

        [FlightSimField("NAV 1 DME Speed (kph)", MaxLength = 3, Format = "000")]
        public virtual double Nav1DmeSpeedKph => Tools.KnotsToKph(Nav1DmeSpeedKts);

        [FlightSimField("NAV 2 DME Speed (kph)", MaxLength = 3, Format = "000")]
        public virtual double Nav2DmeSpeedKph => Tools.KnotsToKph(Nav2DmeSpeedKts);

        [FlightSimField("NAV 1 DME Speed (mph)", MaxLength = 3, Format = "000")]
        public virtual double Nav1DmeSpeedMph => Tools.KnotsToMph(Nav1DmeSpeedKts);

        [FlightSimField("NAV 2 DME Speed (mph)", MaxLength = 3, Format = "000")]
        public virtual double Nav2DmeSpeedMph => Tools.KnotsToMph(Nav2DmeSpeedKts);

        [FlightSimField("NAV 1 Available", MaxLength = 3, TrueText = "AVL", FalseText = "")]
        public abstract bool Nav1Available { get; }

        [FlightSimField("NAV 2 Available", MaxLength = 3, TrueText = "AVL", FalseText = "")]
        public abstract bool Nav2Available { get; }

        [FlightSimField("NAV 1 Receive", MaxLength = 2, TrueText = "RX", FalseText = "RX", InvertWhenTrue = true)]
        public abstract bool Nav1Receive { get; }

        [FlightSimField("NAV 2 Receive", MaxLength = 2, TrueText = "RX", FalseText = "RX", InvertWhenTrue = true)]
        public abstract bool Nav2Receive { get; }

        [FlightSimField("NAV 1 Active Frequency", Format = "000.000", MaxLength = 7)]
        public abstract double Nav1Frequency { get; }

        [FlightSimField("NAV 2 Active Frequency", Format = "000.000", MaxLength = 7)]
        public abstract double Nav2Frequency { get; }

        [FlightSimField("NAV 1 Standby Frequency", Format = "000.000", MaxLength = 7)]
        public abstract double Nav1StandByFrequency { get; }

        [FlightSimField("NAV 2 Standby Frequency", Format = "000.000", MaxLength = 7)]
        public abstract double Nav2StandByFrequency { get; }

        [FlightSimField("NAV 1 Active Frequency Ident", MaxLength = 4, PadAlign = FieldPadAlign.Left)]
        public abstract string Nav1ActiveFrequencyIdent { get; }

        [FlightSimField("NAV 2 Active Frequency Ident", MaxLength = 4, PadAlign = FieldPadAlign.Left)]
        public abstract string Nav2ActiveFrequencyIdent { get; }

        [FlightSimField("ADF (deg)", Format = "0°", MaxLength = 4, PadAlign = FieldPadAlign.Left)]
        public abstract double AdfRelativeBearing { get; }

        [FlightSimField("ADF Ident", MaxLength = 4, PadAlign = FieldPadAlign.Left)]
        public abstract string AdfIdent { get; }

        [FlightSimField("ADF Name", MaxLength = 4, PadAlign = FieldPadAlign.Left)]
        public abstract string AdfName { get; }

        protected void SetPosition(double lat, double lon)
        {
            _latitude = lat;
            _longitude = lon;
            SchedulePositionRefresh();
        }

        [FlightSimField("Latitude (short)", MaxLength = 9)]
        public virtual string LatitudeShort => new Latitude(Latitude).ToShortString();

        [FlightSimField("Longitude (short)", MaxLength = 9)]
        public virtual string LongitudeShort => new Longitude(Longitude).ToShortString();

        private double _latitude;

        [FlightSimField("Latitude", MaxLength = 14)]
        public double Latitude 
        {
            get => _latitude;
            protected set
            {
                _latitude = value;
                SchedulePositionRefresh();
            }
        }

        private double _longitude;
        
        [FlightSimField("Longitude", MaxLength = 14)]
        public double Longitude 
        { 
            get => _longitude; 
            protected set
            {
                _longitude = value;
                SchedulePositionRefresh();
            }
        }

        [FlightSimField("Vertical Speed (disp)", MaxLength = 9, PadAlign = FieldPadAlign.Left)]
        public virtual string VerticalSpeedDisplay => VerticalSpeed.ToString(Units == UnitSystem.Metric ? "+000;-000" : "+0000;-0000", CultureInfo.InvariantCulture) + " " + VerticalSpeedScaleDisplay;

        [FlightSimField("Vertical Speed (sys)", MaxLength = 5, PadAlign = FieldPadAlign.Left)]
        public virtual double VerticalSpeed => Units == UnitSystem.Metric ? VerticalSpeedMps : VerticalSpeedFpm;

        [FlightSimField("Vertical Speed (f/m)", MaxLength = 5, Format = "+0000;-0000")]
        public abstract double VerticalSpeedFpm { get; }

        [FlightSimField("Vertical Speed (m/s)", MaxLength = 4, Format = "+000;-000")]
        public virtual double VerticalSpeedMps => Tools.FpmToMps(VerticalSpeedFpm);

        [FlightSimField("Avionics On", MaxLength = 3, TrueText = "ON", FalseText = "OFF")]
        public abstract bool AvionicsOn { get; }

        [FlightSimField("Pitch (deg)", MaxLength = 4, Format = "0°", PadAlign = FieldPadAlign.Left)]
        public abstract double PitchDegrees { get; }

        [FlightSimField("Bank (deg)", MaxLength = 4, Format = "0°", PadAlign = FieldPadAlign.Left)]
        public abstract double BankDegrees { get; }

        [FlightSimField("GPS Drives NAV", MaxLength = 3, TrueText = "GPS", FalseText = "GPS", InvertWhenTrue = true)]
        public abstract bool GpsDrivesNav { get; }

        [FlightSimField("Flight Director", MaxLength = 2, TrueText = "FD", FalseText = "FD", InvertWhenTrue = true)]
        public abstract bool FlightDirectorActive { get; }

        [FlightSimField("Autopilot", MaxLength = 2, TrueText = "AP", FalseText = "AP", InvertWhenTrue = true)]
        public abstract bool AutopilotMaster { get; }

        [FlightSimField("Autopilot Heading Hold", MaxLength = 3, TrueText = "HDG", FalseText = "HDG", InvertWhenTrue = true)]
        public abstract bool AutopilotHeadingHold { get; }

        [FlightSimField("Autopilot Altitude Hold", MaxLength = 3, TrueText = "ALT", FalseText = "ALT", InvertWhenTrue = true)]
        public abstract bool AutopilotAltHold { get; }

        [FlightSimField("Autopilot Nav Hold", MaxLength = 3, TrueText = "NAV", FalseText = "NAV", InvertWhenTrue = true)]
        public abstract bool AutopilotNavHold { get; }

        [FlightSimField("Autopilot Speed Hold", MaxLength = 3, TrueText = "SPD", FalseText = "SPD", InvertWhenTrue = true)]
        public abstract bool AutopilotSpeedHold { get; }

        [FlightSimField("Autopilot Vertical Speed Hold", MaxLength = 3, TrueText = "VS", FalseText = "Vs", InvertWhenTrue = true)]
        public abstract bool AutopilotVerticalSpeedHold { get; }

        [FlightSimField("Autopilot Heading Bug (deg)", MaxLength = 4, Format = "0°", PadAlign = FieldPadAlign.Left)]
        public abstract double AutopilotHeadingBugDegrees { get; }

        [FlightSimField("Autopilot Altitude (disp)", MaxLength = 7, PadAlign = FieldPadAlign.Left)]
        public virtual string AutopilotAltitudeTargetDisplay => AutopilotAltitudeTarget.ToString("0", CultureInfo.InvariantCulture) + " " + AltitudeScaleDisplay;

        [FlightSimField("Autopilot Altitude (sys)", MaxLength = 5, PadAlign = FieldPadAlign.Left)]
        public virtual double AutopilotAltitudeTarget => Units == UnitSystem.Metric ? AutopilotAltitudeTargetMeters : AutopilotAltitudeTargetFeet;

        [FlightSimField("Autopilot Altitude (ft)", MaxLength = 5, Format = "00000")]
        public abstract double AutopilotAltitudeTargetFeet { get; }

        [FlightSimField("Autopilot Altitude (m)", MaxLength = 5, Format = "00000")]
        public virtual double AutopilotAltitudeTargetMeters => Tools.FeetToMeters(AutopilotAltitudeTargetFeet);

        [FlightSimField("Autopilot Speed (disp)", MaxLength = 6, PadAlign = FieldPadAlign.Left)]
        public virtual string AutopilotSpeedTargetDisplay => AutopilotSpeedTarget.ToString("0", CultureInfo.InvariantCulture) + " " + SpeedScaleDisplay;

        [FlightSimField("Autopilot Speed (sys)", MaxLength = 3, Format ="000")]
        public virtual double AutopilotSpeedTarget => Units == UnitSystem.Metric ? AutopilotSpeedTargetKph : AutopilotSpeedTargetKnots;

        [FlightSimField("Autopilot Speed (kts)", MaxLength = 3, Format = "000")]
        public abstract double AutopilotSpeedTargetKnots { get; }

        [FlightSimField("Autopilot Speed (kph)", MaxLength = 3, Format = "000")]
        public virtual double AutopilotSpeedTargetKph => Tools.KnotsToKph(AutopilotSpeedTargetKnots);

        [FlightSimField("Autopilot Speed (mph)", MaxLength = 3, Format = "000")]
        public virtual double AutopilotSpeedTargetMph => Tools.KnotsToMph(AutopilotSpeedTargetKnots);

        [FlightSimField("Autopilot Vertical Speed (disp)", MaxLength = 9, PadAlign = FieldPadAlign.Left)]
        public virtual string AutopilotVerticalSpeedTargetDisplay => AutopilotVerticalSpeedTarget.ToString(Units == UnitSystem.Metric ? "+000;-000" : "+0000;-0000", CultureInfo.InvariantCulture) + " " + VerticalSpeedScaleDisplay;

        [FlightSimField("Autopilot Vertical Speed (sys)", MaxLength = 5, PadAlign = FieldPadAlign.Left)]
        public virtual double AutopilotVerticalSpeedTarget => Units == UnitSystem.Metric ? AutopilotVerticalSpeedTargetMps : AutopilotVerticalSpeedTargetFpm;

        [FlightSimField("Autopilot Vertical Speed (f/m)", MaxLength = 5, Format = "+0000;-0000")]
        public abstract double AutopilotVerticalSpeedTargetFpm { get; }

        [FlightSimField("Autopilot Vertical Speed (m/s)", MaxLength = 4, Format = "+000;-000")]
        public virtual double AutopilotVerticalSpeedTargetMps => Tools.FpmToMps(AutopilotVerticalSpeedTargetFpm);

        [FlightSimField("Autopilot Active Nav (OBS) Course (deg)", MaxLength = 4, Format = "0°", PadAlign = FieldPadAlign.Left)]
        public virtual double AutopilotNavCourseDegrees => Nav2Receive ? AutopilotNav2CourseDegrees : AutopilotNav1CourseDegrees;

        [FlightSimField("Autopilot Nav 1 (OBS) Course (deg)", MaxLength = 4, Format = "0°", PadAlign = FieldPadAlign.Left)]
        public abstract double AutopilotNav1CourseDegrees { get; }

        [FlightSimField("Autopilot Nav 2 (OBS) Course (deg)", MaxLength = 4, Format = "0°", PadAlign = FieldPadAlign.Left)]
        public abstract double AutopilotNav2CourseDegrees { get; }

        [FlightSimField("Elevator Trim", MaxLength = 4, PadAlign = FieldPadAlign.Left)]
        public virtual string ElevatorTrimDisplay => Tools.PercentSignedToString(ElevatorTrimPercent);

        [FlightSimField("Aileron Trim", MaxLength = 4, PadAlign = FieldPadAlign.Left)]
        public virtual string AileronTrimDisplay => Tools.PercentSignedToString(AileronTrimPercent);

        [FlightSimField("Rudder Trim", MaxLength = 4, PadAlign = FieldPadAlign.Left)]
        public virtual string RudderTrimDisplay => Tools.PercentSignedToString(RudderTrimPercent);

        [FlightSimField("Elevator Trim (pct)", MaxLength = 4, Format = "0%", PadAlign = FieldPadAlign.Left)]
        public abstract double ElevatorTrimPercent { get; }

        [FlightSimField("Aileron Trim (pct)", MaxLength = 4, Format = "0%", PadAlign = FieldPadAlign.Left)]
        public abstract double AileronTrimPercent { get; }

        [FlightSimField("Rudder Trim (pct)", MaxLength = 4, Format = "0%", PadAlign = FieldPadAlign.Left)]
        public abstract double RudderTrimPercent { get; }

        [FlightSimField("Battery On", MaxLength = 3, TrueText = "ON", FalseText = "OFF")]
        public abstract bool BatteryOn { get; }

        [FlightSimField("Generator On", MaxLength = 3, TrueText = "ON", FalseText = "OFF")]
        public abstract bool GeneratorOn { get; }

        [FlightSimField("Pitot Heat", MaxLength = 3, TrueText = "ON", FalseText = "OFF")]
        public abstract bool PitotHeatOn { get; }

        [FlightSimField("Transponder Code", Format = "0000", MaxLength = 4)]
        public abstract uint Transponder { get; }

        [FlightSimField("Transponder Mode", MaxLength = 4)]
        public abstract TransponderMode TransponderMode { get; }

        [FlightSimField("COM 1 Status", MaxLength = 6, PadAlign = FieldPadAlign.Left)]
        public abstract ComStatus Com1Status { get; }

        [FlightSimField("COM 2 Status", MaxLength = 6, PadAlign = FieldPadAlign.Left)]
        public abstract ComStatus Com2Status { get; }

        [FlightSimField("COM 1 Receive", MaxLength = 2, TrueText = "RX", FalseText = "RX", InvertWhenTrue = true)]
        public abstract bool Com1Receive { get; }

        [FlightSimField("COM 2 Receive", MaxLength = 2, TrueText = "RX", FalseText = "RX", InvertWhenTrue = true)]
        public abstract bool Com2Receive { get; }

        [FlightSimField("COM 1 Transmit", MaxLength = 2, TrueText = "TX", FalseText = "TX", InvertWhenTrue = true)]
        public abstract bool Com1Transmit { get; }

        [FlightSimField("COM 2 Transmit", MaxLength = 2, TrueText = "TX", FalseText = "TX", InvertWhenTrue = true)]
        public abstract bool Com2Transmit { get; }

        [FlightSimField("COM 1 Active Frequency Ident", MaxLength = 4, PadAlign = FieldPadAlign.Left)]
        public abstract string Com1ActiveFrequencyIdent { get; }

        [FlightSimField("COM 2 Active Frequency Ident", MaxLength = 4, PadAlign = FieldPadAlign.Left)]
        public abstract string Com2ActiveFrequencyIdent { get; }

        [FlightSimField("COM 1 Active Frequency Type", MaxLength = 4, PadAlign = FieldPadAlign.Left)]
        public abstract TunedFacility Com1ActiveFrequencyType { get; }

        [FlightSimField("COM 2 Active Frequency Type", MaxLength = 4, PadAlign = FieldPadAlign.Left)]
        public abstract TunedFacility Com2ActiveFrequencyType { get; }

        [FlightSimField("COM 1 Active Frequency", Format = "000.000", MaxLength = 7)]
        public abstract double Com1Frequency { get; }

        [FlightSimField("COM 2 Active Frequency", Format = "000.000", MaxLength = 7)]
        public abstract double Com2Frequency { get; }

        [FlightSimField("COM 1 Standby Frequency", Format = "000.000", MaxLength = 7)]
        public abstract double Com1StandByFrequency { get; }

        [FlightSimField("COM 2 Standby Frequency", Format = "000.000", MaxLength = 7)]
        public abstract double Com2StandByFrequency { get; }

        // Engine / Propulsion
        [FlightSimField("Throttle (pct)", MaxLength = 4, Format = "0%", PadAlign = FieldPadAlign.Left)]
        public virtual double ThrottlePercent => AverageByEngine(Throttle1Percent, Throttle2Percent, Throttle3Percent, Throttle4Percent);

        [FlightSimField("Mixture (pct)", MaxLength = 4, Format = "0%", PadAlign = FieldPadAlign.Left)]
        public virtual double MixturePercent => AverageByEngine(Mixture1Percent, Mixture2Percent, Mixture3Percent, Mixture4Percent);

        [FlightSimField("Propeller Pitch (deg)", MaxLength = 4, Format = "0°", PadAlign = FieldPadAlign.Left)]
        public virtual double PropPitchDegrees => AverageByEngine(PropPitch1Degrees, PropPitch2Degrees, PropPitch3Degrees, PropPitch4Degrees);

        [FlightSimField("Mixture", MaxLength = 4, PadAlign = FieldPadAlign.Left)]
        public virtual string MixtureDisplay => PercentToEndpointsOrPct(MixturePercent, low: "LEAN", high: "RICH");

        [FlightSimField("Throttle", MaxLength = 4, PadAlign = FieldPadAlign.Left)]
        public virtual string ThrottleDisplay => PercentToUpFullOrPct(ThrottlePercent, "IDLE", "MAX");

        [FlightSimField("Engine Running", MaxLength = 3, TrueText = "RUN", FalseText = "OFF")]
        public virtual bool EngineRunning => AnyByEngine(Engine1Running, Engine2Running, Engine3Running, Engine4Running);

        [FlightSimField("Propeller De-Ice", MaxLength = 3, TrueText = "ON", FalseText = "OFF")]
        public virtual bool PropellerDeIce => AnyByEngine(Propeller1DeIce, Propeller2DeIce, Propeller3DeIce, Propeller4DeIce);

        [FlightSimField("Carb Heat/Anti-Ice", MaxLength = 3, TrueText = "ON", FalseText = "OFF")]
        public virtual bool CarbHeatAntiIce => AnyByEngine(CarbHeatAntiIce1, CarbHeatAntiIce2, CarbHeatAntiIce3, CarbHeatAntiIce4);

        [FlightSimField("N1 (jet) (pct)", MaxLength = 4, Format = "0%", PadAlign = FieldPadAlign.Left)]
        public virtual double N1Percent => Tools.ClampPercent(AverageByEngine(Engine1N1Percent, Engine2N1Percent, Engine3N1Percent, Engine4N1Percent));

        [FlightSimField("Torque (turboprop) (ft-lbs)", MaxLength = 6, Format = "000000")]
        public virtual double TorqueFootPounds => AverageByEngine(Torque1FootPounds, Torque2FootPounds, Torque3FootPounds, Torque4FootPounds);

        [FlightSimField("Manifold Pressure (disp) (piston)", MaxLength = 9, PadAlign = FieldPadAlign.Left)]
        public virtual string ManifoldPressureDisplay => ManifoldPressure.ToString(Units == UnitSystem.Metric ? "0000" : "00.0", CultureInfo.InvariantCulture) + " " + PressureScaleDisplay;

        [FlightSimField("Manifold Pressure (sys) (piston)", MaxLength = 4, PadAlign = FieldPadAlign.Left)]
        public virtual double ManifoldPressure => Units == UnitSystem.Metric ? ManifoldPressureHectoPascals : ManifoldPressureInchesMercury;

        [FlightSimField("Manifold Pressure (inHg) (piston)", MaxLength = 4, Format = "00.0")]
        public virtual double ManifoldPressureInchesMercury => AverageByEngine(Engine1ManifoldPressureInchesMercury, Engine2ManifoldPressureInchesMercury, Engine3ManifoldPressureInchesMercury, Engine4ManifoldPressureInchesMercury);

        [FlightSimField("Manifold Pressure (hPa) (piston)", MaxLength = 4, Format = "0000")]
        public virtual double ManifoldPressureHectoPascals => AverageByEngine(Engine1ManifoldPressureHectoPascals, Engine2ManifoldPressureHectoPascals, Engine3ManifoldPressureHectoPascals, Engine4ManifoldPressureHectoPascals);

        [FlightSimField("Afterburner", MaxLength = 2, TrueText = "AB", FalseText = "")]
        public virtual bool AfterburnerOn => AnyByEngine(Afterburner1On, Afterburner2On, Afterburner3On, Afterburner4On);

        // Flight Controls / Configuration
        [FlightSimField("Flaps (pct)", MaxLength = 4, Format = "0%", PadAlign = FieldPadAlign.Left)]
        public abstract double FlapsPercent { get; }

        public abstract GearState GearState { get; }

        [FlightSimField("Spoilers (pct)", MaxLength = 4, Format = "0%", PadAlign = FieldPadAlign.Left)]
        public abstract double SpoilersPercent { get; }

        public abstract bool SpoilersArmed { get; }

        [FlightSimField("Parking Brake", MaxLength = 3, TrueText = "ON", FalseText = "OFF")]
        public abstract bool ParkingBrakeOn { get; }

        [FlightSimField("Flaps", MaxLength = 4, PadAlign = FieldPadAlign.Left)]
        public virtual string FlapsDisplay => PercentToUpFullOrPct(FlapsPercent, "UP", "FULL");

        [FlightSimField("Spoilers", MaxLength = 4, PadAlign = FieldPadAlign.Left)]
        public virtual string SpoilersDisplay
        {
            get
            {
                double p = Tools.ClampPercent(SpoilersPercent);
                if (p <= 0.5)
                    return SpoilersArmed ? "ARM" : "RETR";
                if (p >= 99.5)
                    return "FULL";
                int pct = (int)Math.Round(p, MidpointRounding.AwayFromZero);
                pct = Math.Max(1, Math.Min(99, pct));
                return $"{pct}%";
            }
        }

        [FlightSimField("Gear", MaxLength = 3, PadAlign = FieldPadAlign.Left)]
        public virtual string GearDisplay => GearState switch
        {
            GearState.Up => "UP",
            GearState.Down => "DN",
            _ => "MOV"
        };

        // Fuel
        [FlightSimField("Fuel Pump", MaxLength = 3, TrueText = "ON", FalseText = "OFF")]
        public virtual bool FuelPumpOn => AnyByEngine(FuelPump1On, FuelPump2On, FuelPump3On, FuelPump4On);

        [FlightSimField("Fuel Remaining (disp)", MaxLength = 7, PadAlign = FieldPadAlign.Left)]
        public virtual string FuelRemainingDisplay => FuelRemaining.ToString("0", CultureInfo.InvariantCulture) + " " + VolumeScaleDisplay;

        [FlightSimField("Fuel Capacity (disp)", MaxLength = 7, PadAlign = FieldPadAlign.Left)]
        public virtual string FuelCapacityDisplay => FuelCapacity.ToString("0", CultureInfo.InvariantCulture) + " " + VolumeScaleDisplay;

        [FlightSimField("Fuel Remaining (sys)", MaxLength = 3, Format = "000")]
        public virtual double FuelRemaining => Units == UnitSystem.Metric ? FuelRemainingLiters : FuelRemainingGallons;

        [FlightSimField("Fuel Capacity (sys)", MaxLength = 3, Format = "000")]
        public virtual double FuelCapacity => Units == UnitSystem.Metric ? FuelCapacityLiters : FuelCapacityGallons;

        [FlightSimField("Fuel Remaining (gal)", MaxLength = 3, Format = "000")]
        public abstract double FuelRemainingGallons { get; }

        [FlightSimField("Fuel Capacity (gal)", MaxLength = 3, Format = "000")]
        public abstract double FuelCapacityGallons { get; }

        [FlightSimField("Fuel Remaining (lters)", MaxLength = 3, Format = "000")]
        public virtual double FuelRemainingLiters => Tools.GallonsToLiters(FuelRemainingGallons);

        [FlightSimField("Fuel Capacity (liters)", MaxLength = 3, Format = "000")]
        public virtual double FuelCapacityLiters => Tools.GallonsToLiters(FuelCapacityGallons);

        // Lights
        [FlightSimField("Navigation Lights", MaxLength = 3, TrueText = "ON", FalseText = "OFF")]
        public abstract bool NavLightsOn { get; }

        [FlightSimField("Beacon Lights", MaxLength = 3, TrueText = "ON", FalseText = "OFF")]
        public abstract bool BeaconLightsOn { get; }

        [FlightSimField("Strobe Light", MaxLength = 3, TrueText = "ON", FalseText = "OFF")]
        public abstract bool StrobeLightsOn { get; }

        [FlightSimField("Landing Lights", MaxLength = 3, TrueText = "ON", FalseText = "OFF")]
        public abstract bool LandingLightsOn { get; }

        [FlightSimField("Taxi Lights", MaxLength = 3, TrueText = "ON", FalseText = "OFF")]
        public abstract bool TaxiLightsOn { get; }

        [FlightSimField("Cabin Lights", MaxLength = 3, TrueText = "ON", FalseText = "OFF")]
        public abstract bool CabinLightsOn { get; }

        [FlightSimField("Panel Lights", MaxLength = 3, TrueText = "ON", FalseText = "OFF")]
        public abstract bool PanelLightsOn { get; }

        [FlightSimField("Engine Failed", MaxLength = 3, TrueText = "YES", FalseText = "NO")]
        public virtual bool EngineFailed => AnyByEngine(Engine1Failed, Engine2Failed, Engine3Failed, Engine4Failed);

        [FlightSimField("Engine 1 Failed", MaxLength = 3, TrueText = "YES", FalseText = "NO")]
        public abstract bool Engine1Failed { get; }

        [FlightSimField("Engine 2 Failed", MaxLength = 3, TrueText = "YES", FalseText = "NO")]
        public abstract bool Engine2Failed { get; }

        [FlightSimField("Engine 3 Failed", MaxLength = 3, TrueText = "YES", FalseText = "NO")]
        public abstract bool Engine3Failed { get; }

        [FlightSimField("Engine 4 Failed", MaxLength = 3, TrueText = "YES", FalseText = "NO")]
        public abstract bool Engine4Failed { get; }

        [FlightSimField("Cowl Flaps", MaxLength = 3, TrueText = "OPN", FalseText = "CLS")]
        public virtual bool CowlFlapsOpen => AnyByEngine(CowlFlaps1Open, CowlFlaps2Open, CowlFlaps3Open, CowlFlaps4Open);

        [FlightSimField("Cowl Flaps 1", MaxLength = 3, TrueText = "OPN", FalseText = "CLS")]
        public virtual bool CowlFlaps1Open => CowlFlaps1Percent > 5.0;

        [FlightSimField("Cowl Flaps 2", MaxLength = 3, TrueText = "OPN", FalseText = "CLS")]
        public virtual bool CowlFlaps2Open => CowlFlaps2Percent > 5.0;

        [FlightSimField("Cowl Flaps 3", MaxLength = 3, TrueText = "OPN", FalseText = "CLS")]
        public virtual bool CowlFlaps3Open => CowlFlaps3Percent > 5.0;

        [FlightSimField("Cowl Flaps 4", MaxLength = 3, TrueText = "OPN", FalseText = "CLS")]
        public virtual bool CowlFlaps4Open => CowlFlaps4Percent > 5.0;

        [FlightSimField("Cowl Flaps (pct)", MaxLength = 4, Format = "0%", PadAlign = FieldPadAlign.Left)]
        public virtual double CowlFlapsPercent => AverageByEngine(CowlFlaps1Percent, CowlFlaps2Percent, CowlFlaps3Percent, CowlFlaps4Percent);

        [FlightSimField("Cowl Flaps 1 (pct)", MaxLength = 4, Format = "0%", PadAlign = FieldPadAlign.Left)]
        public abstract double CowlFlaps1Percent { get; }

        [FlightSimField("Cowl Flaps 2 (pct)", MaxLength = 4, Format = "0%", PadAlign = FieldPadAlign.Left)]
        public abstract double CowlFlaps2Percent { get; }

        [FlightSimField("Cowl Flaps 3 (pct)", MaxLength = 4, Format = "0%", PadAlign = FieldPadAlign.Left)]
        public abstract double CowlFlaps3Percent { get; }

        [FlightSimField("Cowl Flaps 4 (pct)", MaxLength = 4, Format = "0%", PadAlign = FieldPadAlign.Left)]
        public abstract double CowlFlaps4Percent { get; }

        [FlightSimField("Engine Oil Pressure (psi)", MaxLength = 3, Format = "000")]
        public virtual double EngineOilPressurePsi => AverageByEngine(Engine1OilPressurePsi, Engine2OilPressurePsi, Engine3OilPressurePsi, Engine4OilPressurePsi);

        [FlightSimField("Engine 1 Oil Pressure (psi)", MaxLength = 3, Format = "000")]
        public virtual double Engine1OilPressurePsi => Tools.PsfToPsi(Engine1OilPressurePsf);

        [FlightSimField("Engine 2 Oil Pressure (psi)", MaxLength = 3, Format = "000")]
        public virtual double Engine2OilPressurePsi => Tools.PsfToPsi(Engine1OilPressurePsf);

        [FlightSimField("Engine 3 Oil Pressure (psi)", MaxLength = 3, Format = "000")]
        public virtual double Engine3OilPressurePsi => Tools.PsfToPsi(Engine1OilPressurePsf);

        [FlightSimField("Engine 4 Oil Pressure (psi)", MaxLength = 3, Format = "000")]
        public virtual double Engine4OilPressurePsi => Tools.PsfToPsi(Engine1OilPressurePsf);

        [FlightSimField("Engine Oil Pressure (psf)", MaxLength = 3, Format = "000")]
        public virtual double EngineOilPressurePsf => AverageByEngine(Engine1OilPressurePsf, Engine2OilPressurePsf, Engine3OilPressurePsf, Engine4OilPressurePsf);

        [FlightSimField("Engine 1 Oil Pressure (psf)", MaxLength = 3, Format = "000")]
        public abstract double Engine1OilPressurePsf { get; }

        [FlightSimField("Engine 2 Oil Pressure (psf)", MaxLength = 3, Format = "000")]
        public abstract double Engine2OilPressurePsf { get; }

        [FlightSimField("Engine 3 Oil Pressure (psf)", MaxLength = 3, Format = "000")]
        public abstract double Engine3OilPressurePsf { get; }

        [FlightSimField("Engine 4 Oil Pressure (psf)", MaxLength = 3, Format = "000")]
        public abstract double Engine4OilPressurePsf { get; }

        [FlightSimField("Window Defrost", MaxLength = 3, TrueText = "ON", FalseText = "OFF")]
        public abstract bool WindowDefrostOn { get; }

        // Multi-Engine
        [FlightSimField("Throttle 1 (pct)", MaxLength = 4, Format = "0%", PadAlign = FieldPadAlign.Left)]
        public abstract double Throttle1Percent { get; }

        [FlightSimField("Throttle 2 (pct)", MaxLength = 4, Format = "0%", PadAlign = FieldPadAlign.Left)]
        public abstract double Throttle2Percent { get; }

        [FlightSimField("Throttle 3 (pct)", MaxLength = 4, Format = "0%", PadAlign = FieldPadAlign.Left)]
        public abstract double Throttle3Percent { get; }
        
        [FlightSimField("Throttle 4 (pct)", MaxLength = 4, Format = "0%", PadAlign = FieldPadAlign.Left)]
        public abstract double Throttle4Percent { get; }
        
        [FlightSimField("Mixture 1 (pct)", MaxLength = 4, Format = "0%", PadAlign = FieldPadAlign.Left)]
        public abstract double Mixture1Percent { get; }

        [FlightSimField("Mixture 2 (pct)", MaxLength = 4, Format = "0%", PadAlign = FieldPadAlign.Left)]
        public abstract double Mixture2Percent { get; }

        [FlightSimField("Mixture 3 (pct)", MaxLength = 4, Format = "0%", PadAlign = FieldPadAlign.Left)]
        public abstract double Mixture3Percent { get; }

        [FlightSimField("Mixture 4 (pct)", MaxLength = 4, Format = "0%", PadAlign = FieldPadAlign.Left)]
        public abstract double Mixture4Percent { get; }

        [FlightSimField("Propeller Picth 1 (deg)", MaxLength = 4, Format = "0°", PadAlign = FieldPadAlign.Left)]
        public abstract double PropPitch1Degrees { get; }

        [FlightSimField("Propeller Picth 2 (deg)", MaxLength = 4, Format = "0°", PadAlign = FieldPadAlign.Left)]
        public abstract double PropPitch2Degrees { get; }

        [FlightSimField("Propeller Picth 3 (deg)", MaxLength = 4, Format = "0°", PadAlign = FieldPadAlign.Left)]
        public abstract double PropPitch3Degrees { get; }

        [FlightSimField("Propeller Picth 4 (deg)", MaxLength = 4, Format = "0°", PadAlign = FieldPadAlign.Left)]
        public abstract double PropPitch4Degrees { get; }

        [FlightSimField("Engine Count", MaxLength = 1, Format = "0")]
        public abstract int EngineCount { get; }
        
        [FlightSimField("Throttle Max (pct)", MaxLength = 4, Format = "0%", PadAlign = FieldPadAlign.Left)]
        public virtual double ThrottleMaxPercent => MaxByEngine(Throttle1Percent, Throttle2Percent, Throttle3Percent, Throttle4Percent);

        [FlightSimField("Mixture 1", MaxLength = 4, PadAlign = FieldPadAlign.Left)]
        public virtual string Mixture1Display => PercentToEndpointsOrPct(Mixture1Percent, low: "LEAN", high: "RICH");

        [FlightSimField("Mixture 2", MaxLength = 4, PadAlign = FieldPadAlign.Left)]
        public virtual string Mixture2Display => PercentToEndpointsOrPct(Mixture2Percent, low: "LEAN", high: "RICH");

        [FlightSimField("Mixture 3", MaxLength = 4, PadAlign = FieldPadAlign.Left)]
        public virtual string Mixture3Display => PercentToEndpointsOrPct(Mixture3Percent, low: "LEAN", high: "RICH");

        [FlightSimField("Mixture 4", MaxLength = 4, PadAlign = FieldPadAlign.Left)]
        public virtual string Mixture4Display => PercentToEndpointsOrPct(Mixture4Percent, low: "LEAN", high: "RICH");

        [FlightSimField("Throttle 1", MaxLength = 4, PadAlign = FieldPadAlign.Left)]
        public virtual string Throttle1Display => PercentToUpFullOrPct(Throttle1Percent, "IDLE", "MAX");

        [FlightSimField("Throttle 2", MaxLength = 4, PadAlign = FieldPadAlign.Left)]
        public virtual string Throttle2Display => PercentToUpFullOrPct(Throttle2Percent, "IDLE", "MAX");

        [FlightSimField("Throttle 3", MaxLength = 4, PadAlign = FieldPadAlign.Left)]
        public virtual string Throttle3Display => PercentToUpFullOrPct(Throttle3Percent, "IDLE", "MAX");

        [FlightSimField("Throttle 4", MaxLength = 4, PadAlign = FieldPadAlign.Left)]
        public virtual string Throttle4Display => PercentToUpFullOrPct(Throttle4Percent, "IDLE", "MAX");

        // Engine State (per-engine)
        [FlightSimField("Fuel Pump 1", MaxLength = 3, TrueText = "ON", FalseText = "OFF")]
        public abstract bool FuelPump1On { get; }

        [FlightSimField("Fuel Pump 2", MaxLength = 3, TrueText = "ON", FalseText = "OFF")]
        public abstract bool FuelPump2On { get; }

        [FlightSimField("Fuel Pump 3", MaxLength = 3, TrueText = "ON", FalseText = "OFF")]
        public abstract bool FuelPump3On { get; }

        [FlightSimField("Fuel Pump 4", MaxLength = 3, TrueText = "ON", FalseText = "OFF")]
        public abstract bool FuelPump4On { get; }

        [FlightSimField("Engine 1 Running", MaxLength = 3, TrueText = "RUN", FalseText = "OFF")]
        public abstract bool Engine1Running { get; }

        [FlightSimField("Engine 2 Running", MaxLength = 3, TrueText = "RUN", FalseText = "OFF")]
        public abstract bool Engine2Running { get; }

        [FlightSimField("Engine 3 Running", MaxLength = 3, TrueText = "RUN", FalseText = "OFF")]
        public abstract bool Engine3Running { get; }

        [FlightSimField("Engine 4 Running", MaxLength = 3, TrueText = "RUN", FalseText = "OFF")]
        public abstract bool Engine4Running { get; }

        [FlightSimField("All Engines Running", MaxLength = 3, TrueText = "RUN", FalseText = "OFF")]
        public virtual bool AllEnginesRunning => AllByEngine(Engine1Running, Engine2Running, Engine3Running, Engine4Running);

        [FlightSimField("Propeller 1 De-Ice", MaxLength = 3, TrueText = "ON", FalseText = "OFF")]
        public abstract bool Propeller1DeIce { get; }

        [FlightSimField("Propeller 2 De-Ice", MaxLength = 3, TrueText = "ON", FalseText = "OFF")]
        public abstract bool Propeller2DeIce { get; }

        [FlightSimField("Propeller 3 De-Ice", MaxLength = 3, TrueText = "ON", FalseText = "OFF")]
        public abstract bool Propeller3DeIce { get; }

        [FlightSimField("Propeller 4 De-Ice", MaxLength = 3, TrueText = "ON", FalseText = "OFF")]
        public abstract bool Propeller4DeIce { get; }

        [FlightSimField("Carb Heat/Anti-Ice 1", MaxLength = 3, TrueText = "ON", FalseText = "OFF")]
        public abstract bool CarbHeatAntiIce1 { get; }

        [FlightSimField("Carb Heat/Anti-Ice 2", MaxLength = 3, TrueText = "ON", FalseText = "OFF")]
        public abstract bool CarbHeatAntiIce2 { get; }

        [FlightSimField("Carb Heat/Anti-Ice 3", MaxLength = 3, TrueText = "ON", FalseText = "OFF")]
        public abstract bool CarbHeatAntiIce3 { get; }

        [FlightSimField("Carb Heat/Anti-Ice 4", MaxLength = 3, TrueText = "ON", FalseText = "OFF")]
        public abstract bool CarbHeatAntiIce4 { get; }

        [FlightSimField("Engine RPM", MaxLength = 5, Format = "00000")]
        public virtual double EngineRpm => AverageByEngine(Engine1Rpm, Engine2Rpm, Engine3Rpm, Engine4Rpm);

        [FlightSimField("Engine 1 RPM", MaxLength = 5, Format = "00000")]
        public abstract double Engine1Rpm { get; }

        [FlightSimField("Engine 2 RPM", MaxLength = 5, Format = "00000")]
        public abstract double Engine2Rpm { get; }

        [FlightSimField("Engine 3 RPM", MaxLength = 5, Format = "00000")]
        public abstract double Engine3Rpm { get; }

        [FlightSimField("Engine 4 RPM", MaxLength = 5, Format = "00000")]
        public abstract double Engine4Rpm { get; }

        [FlightSimField("N1 1 (jet) (pct)", MaxLength = 4, Format = "0%", PadAlign = FieldPadAlign.Left)]
        public abstract double Engine1N1Percent { get; }

        [FlightSimField("N1 2 (jet) (pct)", MaxLength = 4, Format = "0%", PadAlign = FieldPadAlign.Left)]
        public abstract double Engine2N1Percent { get; }

        [FlightSimField("N1 3 (jet) (pct)", MaxLength = 4, Format = "0%", PadAlign = FieldPadAlign.Left)]
        public abstract double Engine3N1Percent { get; }

        [FlightSimField("N1 4 (jet) (pct)", MaxLength = 4, Format = "0%", PadAlign = FieldPadAlign.Left)]
        public abstract double Engine4N1Percent { get; }

        [FlightSimField("Torque 1 (turboprop) (ft-lbs)", MaxLength = 6, Format = "000000")]
        public abstract double Torque1FootPounds { get; }

        [FlightSimField("Torque 2 (turboprop) (ft-lbs)", MaxLength = 6, Format = "000000")]
        public abstract double Torque2FootPounds { get; }

        [FlightSimField("Torque 3 (turboprop) (ft-lbs)", MaxLength = 6, Format = "000000")]
        public abstract double Torque3FootPounds { get; }

        [FlightSimField("Torque 4 (turboprop) (ft-lbs)", MaxLength = 6, Format = "000000")]
        public abstract double Torque4FootPounds { get; }

        [FlightSimField("Engine 1 Manifold Pressure (disp) (piston)", MaxLength = 9, PadAlign = FieldPadAlign.Left)]
        public virtual string Engine1ManifoldPressureDisplay => Engine1ManifoldPressure.ToString(Units == UnitSystem.Metric ? "0000" : "00.0", CultureInfo.InvariantCulture) + " " + PressureScaleDisplay;

        [FlightSimField("Engine 2 Manifold Pressure (disp) (piston)", MaxLength = 9, PadAlign = FieldPadAlign.Left)]
        public virtual string Engine2ManifoldPressureDisplay => Engine2ManifoldPressure.ToString(Units == UnitSystem.Metric ? "0000" : "00.0", CultureInfo.InvariantCulture) + " " + PressureScaleDisplay;

        [FlightSimField("Engine 3 Manifold Pressure (disp) (piston)", MaxLength = 9, PadAlign = FieldPadAlign.Left)]
        public virtual string Engine3ManifoldPressureDisplay => Engine3ManifoldPressure.ToString(Units == UnitSystem.Metric ? "0000" : "00.0", CultureInfo.InvariantCulture) + " " + PressureScaleDisplay;

        [FlightSimField("Engine 4 Manifold Pressure (disp) (piston)", MaxLength = 9, PadAlign = FieldPadAlign.Left)]
        public virtual string Engine4ManifoldPressureDisplay => Engine4ManifoldPressure.ToString(Units == UnitSystem.Metric ? "0000" : "00.0", CultureInfo.InvariantCulture) + " " + PressureScaleDisplay;

        [FlightSimField("Engine 1 Manifold Pressure (sys) (piston)", MaxLength = 4)]
        public virtual double Engine1ManifoldPressure => Units == UnitSystem.Metric ? Engine1ManifoldPressureHectoPascals : Engine1ManifoldPressureInchesMercury;

        [FlightSimField("Engine 2 Manifold Pressure (sys) (piston)", MaxLength = 4)]
        public virtual double Engine2ManifoldPressure => Units == UnitSystem.Metric ? Engine2ManifoldPressureHectoPascals : Engine2ManifoldPressureInchesMercury;

        [FlightSimField("Engine 3 Manifold Pressure (sys) (piston)", MaxLength = 4)]
        public virtual double Engine3ManifoldPressure => Units == UnitSystem.Metric ? Engine3ManifoldPressureHectoPascals : Engine3ManifoldPressureInchesMercury;

        [FlightSimField("Engine 4 Manifold Pressure (sys) (piston)", MaxLength = 4)]
        public virtual double Engine4ManifoldPressure => Units == UnitSystem.Metric ? Engine4ManifoldPressureHectoPascals : Engine4ManifoldPressureInchesMercury;

        [FlightSimField("Engine 1 Manifold Pressure (inHg) (piston)", MaxLength = 4, Format = "00.0")]
        public abstract double Engine1ManifoldPressureInchesMercury { get; }

        [FlightSimField("Engine 2 Manifold Pressure (inHg) (piston)", MaxLength = 4, Format = "00.0")]
        public abstract double Engine2ManifoldPressureInchesMercury { get; }

        [FlightSimField("Engine 3 Manifold Pressure (inHg) (piston)", MaxLength = 4, Format = "00.0")]
        public abstract double Engine3ManifoldPressureInchesMercury { get; }

        [FlightSimField("Engine 4 Manifold Pressure (inHg) (piston)", MaxLength = 4, Format = "00.0")]
        public abstract double Engine4ManifoldPressureInchesMercury { get; }

        [FlightSimField("Engine 1 Manifold Pressure (hPa) (piston)", MaxLength = 4, Format = "0000")]
        public virtual double Engine1ManifoldPressureHectoPascals => Tools.InchesMercuryToHectoPascals(Engine1ManifoldPressureInchesMercury);

        [FlightSimField("Engine 2 Manifold Pressure (hPa) (piston)", MaxLength = 4, Format = "0000")]
        public virtual double Engine2ManifoldPressureHectoPascals => Tools.InchesMercuryToHectoPascals(Engine2ManifoldPressureInchesMercury);

        [FlightSimField("Engine 3 Manifold Pressure (hPa) (piston)", MaxLength = 4, Format = "0000")]
        public virtual double Engine3ManifoldPressureHectoPascals => Tools.InchesMercuryToHectoPascals(Engine3ManifoldPressureInchesMercury);

        [FlightSimField("Engine 4 Manifold Pressure (hPa) (piston)", MaxLength = 4, Format = "0000")]
        public virtual double Engine4ManifoldPressureHectoPascals => Tools.InchesMercuryToHectoPascals(Engine4ManifoldPressureInchesMercury);

        [FlightSimField("Afterburner 1", MaxLength = 2, TrueText = "AB", FalseText = "")]
        public abstract bool Afterburner1On { get; }

        [FlightSimField("Afterburner 2", MaxLength = 2, TrueText = "AB", FalseText = "")]
        public abstract bool Afterburner2On { get; }

        [FlightSimField("Afterburner 3", MaxLength = 2, TrueText = "AB", FalseText = "")]
        public abstract bool Afterburner3On { get; }

        [FlightSimField("Afterburner 4", MaxLength = 2, TrueText = "AB", FalseText = "")]
        public abstract bool Afterburner4On { get; }

        [FlightSimField("Engine Power (rpm/torque/n1)", MaxLength = 5, Format = "0")]
        public virtual double EnginePowerValue => EngineType switch
        {
            EngineType.Jet or EngineType.Rocket => N1Percent,
            EngineType.Turboprop or EngineType.Helo => TorqueFootPounds,
            EngineType.Balloon or EngineType.Sailplane => 0,
            _ => EngineRpm,
        };

        // ==========================
        // Helicopter – Controls
        // ==========================

        [FlightSimField("Helo: Collective (pct)", MaxLength = 4, Format = "0%", PadAlign = FieldPadAlign.Left)]
        public abstract double CollectivePercent { get; }

        [FlightSimField("Helo: Cyclic (pct)", MaxLength = 5, Format = "+0%;-0%", PadAlign = FieldPadAlign.Left)]
        public virtual double CyclicPercent => MainRotorDiskPitchPercent;

        [FlightSimField("Helo: Rotor Brake", MaxLength = 3, TrueText = "ON", FalseText = "OFF")]
        public abstract bool RotorBrakeActive { get; }

        [FlightSimField("Helo: Rotor Brake Handle (pct)", MaxLength = 4, Format = "0%", PadAlign = FieldPadAlign.Left)]
        public abstract double RotorBrakeHandlePercent { get; }

        // ==========================
        // Rotor Disk Attitude (Main)
        // ==========================

        [FlightSimField("Helo: Main Rotor Disk Bank (pct)", MaxLength = 5, Format = "+0%;-0%", PadAlign = FieldPadAlign.Left)]
        public abstract double MainRotorDiskBankPercent { get; }

        [FlightSimField("Helo: Main Rotor Disk Pitch (pct)", MaxLength = 5, Format = "+0%;-0%", PadAlign = FieldPadAlign.Left)]
        public abstract double MainRotorDiskPitchPercent { get; }

        [FlightSimField("Helo: Main Rotor Disk Coning (pct)", MaxLength = 5, Format = "+0%;-0%", PadAlign = FieldPadAlign.Left)]
        public abstract double MainRotorDiskConingPercent { get; }

        [FlightSimField("Helo: Main Rotor Angle (deg)", MaxLength = 4, Format = "0°", PadAlign = FieldPadAlign.Left)]
        public abstract double MainRotorRotationAngleDegrees { get; }

        // ==========================
        // Rotor Disk Attitude (Tail)
        // ==========================

        [FlightSimField("Helo: Tail Rotor Disk Bank (pct)", MaxLength = 5, Format = "+0%;-0%", PadAlign = FieldPadAlign.Left)]
        public abstract double TailRotorDiskBankPercent { get; }

        [FlightSimField("Helo: Tail Rotor Disk Pitch (pct)", MaxLength = 5, Format = "+0%;-0%", PadAlign = FieldPadAlign.Left)]
        public abstract double TailRotorDiskPitchPercent { get; }

        [FlightSimField("Helo: Tail Rotor Disk Coning (pct)", MaxLength = 5, Format = "+0%;-0%", PadAlign = FieldPadAlign.Left)]
        public abstract double TailRotorDiskConingPercent { get; }

        [FlightSimField("Helo: Tail Rotor Angle (deg)", MaxLength = 4, Format = "0°", PadAlign = FieldPadAlign.Left)]
        public abstract double TailRotorRotationAngleDegrees { get; }

        // ==========================
        // Rotor RPM
        // ==========================

        [FlightSimField("Helo: Main Rotor RPM", MaxLength = 5, Format = "00000")]
        public abstract double MainRotorRpm { get; }

        [FlightSimField("Helo: Tail Rotor RPM", MaxLength = 5, Format = "00000")]
        public abstract double TailRotorRpm { get; }

        // ==========================
        // Rotor Blade Pitch & Trim
        // ==========================

        [FlightSimField("Helo: Main Rotor Blade Pitch (pct)", MaxLength = 5, Format = "+0%;-0%", PadAlign = FieldPadAlign.Left)]
        public abstract double MainRotorBladePitchPercent { get; }

        [FlightSimField("Helo: Tail Rotor Blade Pitch (pct)", MaxLength = 5, Format = "+0%;-0%", PadAlign = FieldPadAlign.Left)]
        public abstract double TailRotorBladePitchPercent { get; }

        [FlightSimField("Helo: Rotor Lateral Trim (pct)", MaxLength = 5, Format = "+0%;-0%", PadAlign = FieldPadAlign.Left)]
        public abstract double RotorLateralTrimPercent { get; }

        [FlightSimField("Helo: Rotor Longitudinal Trim (pct)", MaxLength = 5, Format = "+0%;-0%", PadAlign = FieldPadAlign.Left)]
        public abstract double RotorLongitudinalTrimPercent { get; }

        // ==========================
        // Rotor / Engine Coupling
        // ==========================

        [FlightSimField("Helo: Engine 1 Rotor RPM Cmd (pct)", MaxLength = 4, Format = "0%", PadAlign = FieldPadAlign.Left)]
        public abstract double Engine1RotorRpmCommandPercent { get; }

        [FlightSimField("Helo: Engine 2 Rotor RPM Cmd (pct)", MaxLength = 4, Format = "0%", PadAlign = FieldPadAlign.Left)]
        public abstract double Engine2RotorRpmCommandPercent { get; }

        [FlightSimField("Helo: Engine 1 Torque (pct)", MaxLength = 4, Format = "0%", PadAlign = FieldPadAlign.Left)]
        public abstract double Engine1TorquePercent { get; }

        [FlightSimField("Helo: Engine 2 Torque (pct)", MaxLength = 4, Format = "0%", PadAlign = FieldPadAlign.Left)]
        public abstract double Engine2TorquePercent { get; }

        // ==========================
        // Governor
        // ==========================

        [FlightSimField("Helo: Governor", MaxLength = 3, TrueText = "ON", FalseText = "OFF")]
        public virtual bool RotorGovernorActive => AnyByEngine(RotorGovernor1Active, RotorGovernor2Active);

        [FlightSimField("Helo: Governor 1", MaxLength = 3, TrueText = "ON", FalseText = "OFF")]
        public abstract bool RotorGovernor1Active { get; }

        [FlightSimField("Helo: Governor 2", MaxLength = 3, TrueText = "ON", FalseText = "OFF")]
        public abstract bool RotorGovernor2Active { get; }


        // Flight Plan
        [FlightSimField("Flight Plan: Is Active", MaxLength = 3, TrueText = "YES", FalseText = "NO")]
        public abstract bool FlightPlanIsActiveFlightPlan { get; }

        [FlightSimField("Flight Plan: Waypoint Count", MaxLength = 3, Format = "000")]
        public abstract int FlightPlanWaypointsNumber { get; }

        [FlightSimField("Flight Plan: Active Waypoint", MaxLength = 3, Format = "000")]
        public abstract int FlightPlanActiveWaypoint { get; }

        [FlightSimField("Flight Plan: Is Direct To", MaxLength = 3, TrueText = "YES", FalseText = "NO")]
        public abstract bool FlightPlanIsDirectTo { get; }

        [FlightSimField("Flight Plan: Approach Ident", MaxLength = 5, PadAlign = FieldPadAlign.Left)]
        public abstract string FlightPlanApproachIdent { get; }

        [FlightSimField("Flight Plan: Waypoint Index", MaxLength = 3, Format = "000")]
        public abstract int FlightPlanWaypointIndex { get; }

        [FlightSimField("Flight Plan: Waypoint Ident", MaxLength = 5, PadAlign = FieldPadAlign.Left)]
        public abstract string FlightPlanWaypointIdent { get; }

        [FlightSimField("Flight Plan: Approach Waypoints Count", MaxLength = 3, Format = "000")]
        public abstract int FlightPlanApproachWaypointsNumber { get; }

        [FlightSimField("Flight Plan: Actie Approach Waypoint", MaxLength = 3, Format = "000")]
        public abstract int FlightPlanActiveApproachWaypoint { get; }

        [FlightSimField("Flight Plan: Approach Waypoint Is Runway", MaxLength = 3, TrueText = "YES", FalseText = "NO")]
        public abstract bool FlightPlanApproachIsWaypointRunway { get; }

        protected bool AllByEngine(bool e1, bool e2, bool e3, bool e4)
        {
            int n = EngineCount;
            if (n < 1) n = 1;
            if (n > 4) n = 4;

            if (n >= 1 && !e1) return false;
            if (n >= 2 && !e2) return false;
            if (n >= 3 && !e3) return false;
            if (n >= 4 && !e4) return false;

            return true;
        }

        protected static string PercentToUpFullOrPct(double percent, string upText, string fullText)
        {
            percent = Tools.ClampPercent(percent);

            if (percent <= 0.5) return upText;
            if (percent >= 99.5) return fullText;

            // Round to nearest int for display
            int p = (int)Math.Round(percent, MidpointRounding.AwayFromZero);
            p = Math.Max(1, Math.Min(99, p)); // keep it sane between endpoints

            return $"{p}%";
        }

        protected static string PercentToEndpointsOrPct(double percent, string low, string high)
        {
            percent = Tools.ClampPercent(percent);

            if (percent <= 0.5) return low;
            if (percent >= 99.5) return high;

            int p = (int)Math.Round(percent, MidpointRounding.AwayFromZero);
            p = Math.Max(1, Math.Min(99, p));

            return $"{p}%";
        }

        protected double AverageByEngine(double e1, double e2, double e3, double e4)
        {
            int n = Math.Max(1, Math.Min(4, EngineCount));
            double sum = 0;
            if (n >= 1) sum += e1;
            if (n >= 2) sum += e2;
            if (n >= 3) sum += e3;
            if (n >= 4) sum += e4;
            return sum / n;
        }

        protected double MaxByEngine(double e1, double e2, double e3, double e4)
        {
            int n = Math.Max(1, Math.Min(4, EngineCount));
            double max = e1;
            if (n >= 2) max = Math.Max(max, e2);
            if (n >= 3) max = Math.Max(max, e3);
            if (n >= 4) max = Math.Max(max, e4);
            return max;
        }

        protected bool AnyByEngine(bool e1, bool e2, bool e3 = false, bool e4 = false)
        {
            int n = Math.Max(1, Math.Min(4, EngineCount));
            if (n >= 1 && e1) return true;
            if (n >= 2 && e2) return true;
            if (n >= 3 && e3) return true;
            if (n >= 4 && e4) return true;
            return false;
        }

        protected FlightSimProviderException WrapProviderError(Exception ex)
        {
            var dataBag = ex.Data.Count > 0 ? ex.Data.Cast<System.Collections.DictionaryEntry>().ToDictionary(e => e.Key.ToString()!, e => e.Value) : null;
            return new FlightSimProviderException(provider: Name, message: ex.Message, code: ex.GetType().Name, innerException: ex, dataBag: dataBag);
        }

        public virtual void Connected()
        {
            OnConnected?.Invoke(this);
        }

        public virtual void AircraftChange(int aircraftId)
        {
            OnAircraftChange?.Invoke(this, aircraftId);
        }

        public virtual void ReadyToFly(ReadyToFly readyToFly)
        {
            OnReadyToFly?.Invoke(this, readyToFly);
        }

        private int _isSendingFlightData = 0;

        public virtual void FlightDataReceived()
        {
            if (Interlocked.Exchange(ref _isSendingFlightData, 1) == 1)
                return;

            ThreadPool.QueueUserWorkItem(_ =>
            {
                try { OnFlightDataReceived?.Invoke(this); }
                finally { Interlocked.Exchange(ref _isSendingFlightData, 0); }
            });
        }

        public virtual void TrafficReceived(string callsign, Aircraft? aircraft, TrafficEvent eventType)
        {
            OnTrafficReceived?.Invoke(this, callsign, aircraft, eventType);
        }

        public virtual void Quit()
        {
            OnQuit?.Invoke(this);
        }

        public virtual void SetLeds()
        {
            OnSetLeds?.Invoke(this);
        }
        public virtual void StopTimer()
        {
            OnStopTimer?.Invoke(this);
        }

        public virtual void UdatePage()
        {
            OnUdatePage?.Invoke(this);
        }

        public virtual void Error(FlightSimProviderException ex)
        {
            OnError?.Invoke(this, ex);
        }

        public bool HasConnected
        {
            get
            {
                return OnConnected != null;
            }
        }

        public bool HasQuit
        {
            get
            {
                return OnQuit != null;
            }
        }

        public string FormatTransponderMode(TransponderMode mode)
        {
            return mode switch
            {
                TransponderMode.Off => "OFF",

                TransponderMode.Standby or
                TransponderMode.Test or
                TransponderMode.Ground_Mode_S => "STBY",

                TransponderMode.On_Mode_A => "ON",

                TransponderMode.Alt_Mode_C => "ALT",

                _ => "STBY"
            };
        }

        private static string ApplyPadding(string s, FlightSimFieldAttribute meta)
        {
            if (meta.MaxLength <= 0 || s.Length >= meta.MaxLength)
                return s;

            int pad = meta.MaxLength - s.Length;

            return meta.PadAlign switch
            {
                FieldPadAlign.Left =>
                    s.PadRight(meta.MaxLength, meta.PadChar),

                FieldPadAlign.Right =>
                    s.PadLeft(meta.MaxLength, meta.PadChar),

                FieldPadAlign.Center =>
                    new string(meta.PadChar, pad / 2) + s + new string(meta.PadChar, pad - (pad / 2)),

                _ =>
                    s.PadLeft(meta.MaxLength, meta.PadChar)
            };
        }

        private static string ClipToMax(string s, FlightSimFieldAttribute meta)
        {
            if (meta.MaxLength <= 0 || s.Length <= meta.MaxLength)
                return s;

            // Preserve previous semantics: for "right-aligned" style fields, keep rightmost chars.
            // For left-aligned fields, keep leftmost chars.
            return meta.PadAlign == FieldPadAlign.Left
                ? s.Substring(0, meta.MaxLength)
                : s.Substring(s.Length - meta.MaxLength);
        }

        private const char INV_ON = '⟦';
        private const char INV_OFF = '⟧';
        
        public string GetFormattedValue(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                return string.Empty;

            var prop = GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (prop == null)
                return string.Empty;

            var meta = prop.GetCustomAttribute<FlightSimFieldAttribute>(inherit: true);
            if (meta == null)
                return string.Empty;

            object? rawValue = prop.GetValue(this);
            if (rawValue == null)
                return string.Empty;

            // --- Special-case: GPS Cross-Track Error (append L/R, NO unit conversion) ---
            // Handles GPSCrossTrackError, GPSCrossTrackErrorMeters/Feet/Km/Nm, etc.
            if (prop.Name.StartsWith("GPSCrossTrackError", StringComparison.Ordinal) && rawValue is double xtk)
            {
                char side = xtk >= 0 ? 'R' : 'L';
                double mag = Math.Abs(xtk);

                // Use attribute format if provided; otherwise default to 0.00 for XTK.
                string fmt = string.IsNullOrWhiteSpace(meta.Format) ? "0.00" : meta.Format;

                // Core token only; padding/clip applied below.
                string xtkText = mag.ToString(fmt, CultureInfo.InvariantCulture) + side;

                xtkText = xtkText.ToUpperInvariant();
                xtkText = ApplyPadding(xtkText, meta);
                xtkText = ClipToMax(xtkText, meta);
                return xtkText;
            }

            // --- Special-case: Lat/Long should use Latitude/Longitude classes' ToString() ---
            if (rawValue is double d)
            {
                if (prop.Name == nameof(Latitude) || prop.Name == nameof(WaypointLatitude))
                {
                    string s = new Latitude(d).ToString(false).ToUpperInvariant();
                    s = ApplyPadding(s, meta);
                    s = ClipToMax(s, meta);
                    return s;
                }

                if (prop.Name == nameof(Longitude) || prop.Name == nameof(WaypointLongitude))
                {
                    string s = new Longitude(d).ToString(false).ToUpperInvariant();
                    s = ApplyPadding(s, meta);
                    s = ClipToMax(s, meta);
                    return s;
                }
            }

            string result;
            bool isBool = rawValue is bool;

            // Track invert intent for this field
            bool invert = false;

            if ((prop.Name == nameof(AltitudeMSL)
                || prop.Name == nameof(AltitudeAGL)
                || prop.Name == nameof(AltitudeTrue)
                || prop.Name == nameof(AutopilotAltitudeTarget))
                && rawValue is double alt)
            {
                string fmt = meta.Format;
                string s = string.IsNullOrWhiteSpace(fmt) ? alt.ToString(Units == UnitSystem.Metric ? "0000" : "00000", CultureInfo.InvariantCulture) : alt.ToString(fmt, CultureInfo.InvariantCulture);
                s = s.ToUpperInvariant();
                s = ApplyPadding(s, meta);
                s = ClipToMax(s, meta);
                return s;
            }
            if ((prop.Name == nameof(VerticalSpeed)
                 || prop.Name == nameof(AutopilotVerticalSpeedTarget))
                 && rawValue is double vs)
            {
                string fmt = meta.Format;
                string s = string.IsNullOrWhiteSpace(fmt) ? vs.ToString(Units == UnitSystem.Metric ? "+000;-000" : "+0000;-0000", CultureInfo.InvariantCulture) : vs.ToString(fmt, CultureInfo.InvariantCulture);
                s = s.ToUpperInvariant();
                s = ApplyPadding(s, meta);
                s = ClipToMax(s, meta);
                return s;
            }
            if ((prop.Name == nameof(Kollsman)
                || prop.Name == nameof(SecondaryKollsman)
                || prop.Name == nameof(Pressure))
                && rawValue is double psr)
            {
                string fmt = meta.Format;
                string s = string.IsNullOrWhiteSpace(fmt) ? psr.ToString(Units == UnitSystem.Metric ? "0000" : "00.00", CultureInfo.InvariantCulture) : psr.ToString(fmt, CultureInfo.InvariantCulture);
                s = s.ToUpperInvariant();
                s = ApplyPadding(s, meta);
                s = ClipToMax(s, meta);
                return s;
            }
            if ((prop.Name == nameof(ManifoldPressure)
                || prop.Name == nameof(Engine1ManifoldPressure)
                || prop.Name == nameof(Engine2ManifoldPressure)
                || prop.Name == nameof(Engine3ManifoldPressure)
                || prop.Name == nameof(Engine4ManifoldPressure))
                && rawValue is double psr2)
            {
                string fmt = meta.Format;
                string s = string.IsNullOrWhiteSpace(fmt) ? psr2.ToString(Units == UnitSystem.Metric ? "0000" : "00.0", CultureInfo.InvariantCulture) : psr2.ToString(fmt, CultureInfo.InvariantCulture);
                s = s.ToUpperInvariant();
                s = ApplyPadding(s, meta);
                s = ClipToMax(s, meta);
                return s;
            }

            switch (rawValue)
            {
                case bool b:
                    // Attribute-driven true/false text
                    result = b ? meta.TrueText : meta.FalseText;

                    // NEW: invert when true if enabled
                    invert = b && meta.InvertWhenTrue;
                    break;

                case TransponderMode tm:
                    result = FormatTransponderMode(tm); // return core token (no padding)
                    break;

                case IFormattable formattable when !string.IsNullOrEmpty(meta.Format):
                    result = formattable.ToString(meta.Format, CultureInfo.InvariantCulture);
                    break;

                default:
                    result = rawValue.ToString() ?? string.Empty;
                    break;
            }

            result = result.ToUpperInvariant();

            if (rawValue is string)
            {
                // Sim strings often arrive with leading spaces due to fixed-width buffers.
                result = result.Trim();
            }
            // Apply padding rules (you already have these helpers)
            if (!isBool && meta.MaxLength > 0)
                result = ApplyPadding(result, meta);

            result = ClipToMax(result, meta);

            // Apply invert markers LAST so you don't pad/clip the control codes
            if (invert && result.Length > 0)
                result = $"{INV_ON}{result}{INV_OFF}";

            return result;
        }

        protected void Log(string message)
        {
            Log(Name, message);
        }

        protected void Log(Exception ex)
        {
            Log(Name, ex);
        }

        public static void Log(string name, string message)
        {
#if DEBUG
            Logger.Info($"{name}: {message}");
#endif
        }

        public static void Log(string name, Exception ex)
        {
            Log(name, ex, ex.Message);
        }

        public static void Log(string name, Exception ex, string message)
        {
#if DEBUG
            Logger.Error(ex, $"{name}: {message}");
#endif
        }
    }
}