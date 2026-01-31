using System;
using System.Collections.Generic;
using System.Text;

namespace WinCtrlICP
{
    internal class SystemDisplays
    {
        public static readonly List<UserIcpDisplayItem> CNI =
            new()
            {
                // ── COM1 ──────────────────────────────
                new() { X = 0,  Y = 0, Kind = IcpItemKind.Label,      LabelText = "COM1" },
                new() { X = 5,  Y = 0, Kind = IcpItemKind.BoundField, AttributeName = "Com1Frequency" },
                new() { X = 13, Y = 0, Kind = IcpItemKind.Label,      LabelText = "STBY" },
                new() { X = 18, Y = 0, Kind = IcpItemKind.BoundField, AttributeName = "Com1StandByFrequency", Inverted = true },

                // ── NAV1 ──────────────────────────────
                new() { X = 0,  Y = 1, Kind = IcpItemKind.Label,      LabelText = "NAV1" },
                new() { X = 5,  Y = 1, Kind = IcpItemKind.BoundField, AttributeName = "Nav1Frequency" },
                new() { X = 13, Y = 1, Kind = IcpItemKind.Label,      LabelText = "STBY" },
                new() { X = 18, Y = 1, Kind = IcpItemKind.BoundField, AttributeName = "Nav1StandByFrequency", Inverted = true },

                // ── ALT ─────────────────────────
                new() { X = 1, Y = 2, Kind = IcpItemKind.Label,      LabelText = "MSL" },
                new() { X = 5, Y = 2, Kind = IcpItemKind.BoundField, AttributeName = "AltitudeMSL" },
                new() { X = 14, Y = 2, Kind = IcpItemKind.Label,      LabelText = "AGL" },
                new() { X = 18, Y = 2, Kind = IcpItemKind.BoundField, AttributeName = "AltitudeAGL" },

                // ── AIR / GND SPEED ───────────────────
                new() { X = 0,  Y = 3, Kind = IcpItemKind.Label,      LabelText = "ASPD" },
                new() { X = 5,  Y = 3, Kind = IcpItemKind.BoundField, AttributeName = "AirSpeedIndicated" },
                new() { X = 13, Y = 3, Kind = IcpItemKind.Label,      LabelText = "GSPD" },
                new() { X = 18, Y = 3, Kind = IcpItemKind.BoundField, AttributeName = "GroundSpeed" },

                // ── HDG ──────────────────────────────
                new() { X = 1,  Y = 4, Kind = IcpItemKind.Label,      LabelText = "HDG" },
                new() { X = 5,  Y = 4, Kind = IcpItemKind.BoundField, AttributeName = "HeadingMagneticDegrees" },

                // ── XPDR ──────────────────────────────
                new() { X = 14,  Y = 4, Kind = IcpItemKind.Label,      LabelText = "DTK" },
                new() { X = 18,  Y = 4, Kind = IcpItemKind.BoundField, AttributeName = "GPSRequiredMagneticHeadingDegrees" },
            };

        public static readonly List<UserIcpDisplayItem> NAV1 =
            new()
            {
                // ── NAV 1 ─────────────────────────────
                new() { X = 0,  Y = 0, Kind = IcpItemKind.Label,      LabelText = "NAV1" },
                new() { X = 5,  Y = 0, Kind = IcpItemKind.BoundField, AttributeName = "Nav1Frequency" },
                new() { X = 13, Y = 0, Kind = IcpItemKind.Label,      LabelText = "STBY" },
                new() { X = 18, Y = 0, Kind = IcpItemKind.BoundField, AttributeName = "Nav1StandByFrequency", Inverted = true },

                // ── VOR 1 ────────────────────────
                new() { X = 0,  Y = 1, Kind = IcpItemKind.Label,      LabelText = "VOR1" },
                new() { X = 5,  Y = 1, Kind = IcpItemKind.BoundField, AttributeName = "Nav1Radial" },
                new() { X = 13, Y = 1, Kind = IcpItemKind.Label,      LabelText = "BRG1" },
                new() { X = 18, Y = 1, Kind = IcpItemKind.BoundField, AttributeName = "Nav1BearingDegrees" },

                // ── DME ──────────────────────
                new() { X = 0, Y = 2, Kind = IcpItemKind.Label,      LabelText = "DME1" },
                new() { X = 5, Y = 2, Kind = IcpItemKind.BoundField, AttributeName = "Nav1DmeDistance" },
                new() { X = 13, Y = 2, Kind = IcpItemKind.Label,      LabelText = "SPD1" },
                new() { X = 18, Y = 2, Kind = IcpItemKind.BoundField, AttributeName = "Nav1DmeSpeed" },

               // ── CRS ──────────
                new() { X = 0, Y = 3, Kind = IcpItemKind.Label,      LabelText = "CRS1" },
                new() { X = 5, Y = 3, Kind = IcpItemKind.BoundField, AttributeName = "AutopilotNav1CourseDegrees" },
                new() { X = 14, Y = 3, Kind = IcpItemKind.Label,      LabelText = "BUG" },
                new() { X = 18, Y = 3, Kind = IcpItemKind.BoundField, AttributeName = "AutopilotHeadingBugDegrees" },

                // ── IDENT / STATUS ──────────────────────
                new() { X = 1,  Y = 4, Kind = IcpItemKind.Label,      LabelText = "ID1" },
                new() { X = 5,  Y = 4, Kind = IcpItemKind.BoundField, AttributeName = "Nav1ActiveFrequencyIdent" },
                new() { X = 14,  Y = 4, Kind = IcpItemKind.BoundField, AttributeName = "Nav1Available" },
                new() { X = 18,  Y = 4, Kind = IcpItemKind.BoundField, AttributeName = "Nav1Receive" },
            };

        public static readonly List<UserIcpDisplayItem> NAV2 =
            new()
            {
                // ── NAV 2 ─────────────────────────────
                new() { X = 0,  Y = 0, Kind = IcpItemKind.Label,      LabelText = "NAV2" },
                new() { X = 5,  Y = 0, Kind = IcpItemKind.BoundField, AttributeName = "Nav2Frequency" },
                new() { X = 13, Y = 0, Kind = IcpItemKind.Label,      LabelText = "STBY" },
                new() { X = 18, Y = 0, Kind = IcpItemKind.BoundField, AttributeName = "Nav2StandByFrequency", Inverted = true },

                // ── VOR 1 ────────────────────────
                new() { X = 0,  Y = 1, Kind = IcpItemKind.Label,      LabelText = "VOR2" },
                new() { X = 5,  Y = 1, Kind = IcpItemKind.BoundField, AttributeName = "Nav2Radial" },
                new() { X = 13, Y = 1, Kind = IcpItemKind.Label,      LabelText = "BRG2" },
                new() { X = 18, Y = 1, Kind = IcpItemKind.BoundField, AttributeName = "Nav2BearingDegrees" },

                // ── DME ──────────────────────
                new() { X = 0, Y = 2, Kind = IcpItemKind.Label,      LabelText = "DME2" },
                new() { X = 5, Y = 2, Kind = IcpItemKind.BoundField, AttributeName = "Nav2DmeDistance" },
                new() { X = 13, Y = 2, Kind = IcpItemKind.Label,      LabelText = "SPD2" },
                new() { X = 18, Y = 2, Kind = IcpItemKind.BoundField, AttributeName = "Nav2DmeSpeed" },

               // ── CRS ──────────
                new() { X = 0, Y = 3, Kind = IcpItemKind.Label,      LabelText = "CRS2" },
                new() { X = 5, Y = 3, Kind = IcpItemKind.BoundField, AttributeName = "AutopilotNav2CourseDegrees" },
                new() { X = 14, Y = 3, Kind = IcpItemKind.Label,      LabelText = "BUG" },
                new() { X = 18, Y = 3, Kind = IcpItemKind.BoundField, AttributeName = "AutopilotHeadingBugDegrees" },

                // ── IDENT / STATUS ──────────────────────
                new() { X = 1,  Y = 4, Kind = IcpItemKind.Label,      LabelText = "ID2" },
                new() { X = 5,  Y = 4, Kind = IcpItemKind.BoundField, AttributeName = "Nav2ActiveFrequencyIdent" },
                new() { X = 14,  Y = 4, Kind = IcpItemKind.BoundField, AttributeName = "Nav2Available" },
                new() { X = 18,  Y = 4, Kind = IcpItemKind.BoundField, AttributeName = "Nav2Receive" },
            };

        public static readonly List<UserIcpDisplayItem> IFF =
            new()
            {
                // ── XPDR ──────────────────────────────
                new() { X = 0,  Y = 0, Kind = IcpItemKind.Label,      LabelText = "XPDR" },
                new() { X = 5,  Y = 0, Kind = IcpItemKind.BoundField, AttributeName = "Transponder" },
                new() { X = 11, Y = 0, Kind = IcpItemKind.BoundField, AttributeName = "TransponderMode" },
                new() { X = 16, Y = 0, Kind = IcpItemKind.BoundField, AttributeName = "IdentActive" },

                // ── TRUE ALT ──────────────────────
                new() { X = 0,  Y = 1, Kind = IcpItemKind.Label,      LabelText = "TALT" },
                new() { X = 5,  Y = 1, Kind = IcpItemKind.BoundField, AttributeName = "AltitudeTrue" },

                // ── AVIONICS ──────────────────────────
                new() { X = 0,  Y = 2, Kind = IcpItemKind.Label,      LabelText = "AVIO" },
                new() { X = 5,  Y = 2, Kind = IcpItemKind.BoundField, AttributeName = "AvionicsOn" },

                // ── PITOT HEAT ────────────────────────
                new() { X = 11, Y = 2, Kind = IcpItemKind.Label,      LabelText = "HEAT" },
                new() { X = 16, Y = 2, Kind = IcpItemKind.BoundField, AttributeName = "PitotHeatOn" },

                // ── BATTERY ───────────────────────────
                new() { X = 0,  Y = 3, Kind = IcpItemKind.Label,      LabelText = "BATT" },
                new() { X = 5,  Y = 3, Kind = IcpItemKind.BoundField, AttributeName = "BatteryOn" },

                // ── GENERATOR ─────────────────────────
                new() { X = 1,  Y = 4, Kind = IcpItemKind.Label,      LabelText = "GEN" },
                new() { X = 5,  Y = 4, Kind = IcpItemKind.BoundField, AttributeName = "GeneratorOn" },

                // ── ENGINE ───────────────────────────
                new() { X = 12,  Y = 3, Kind = IcpItemKind.Label,      LabelText = "ENG" },
                new() { X = 16,  Y = 3, Kind = IcpItemKind.BoundField, AttributeName = "EngineRunning" },
            };

        public static readonly List<UserIcpDisplayItem> COM1 =
            new()
            {
                // ── COM 1 FREQUENCIES ─────────────────
                new() { X = 0,  Y = 0, Kind = IcpItemKind.Label,      LabelText = "COM1" },
                new() { X = 5,  Y = 0, Kind = IcpItemKind.BoundField, AttributeName = "Com1Frequency" },
                new() { X = 13, Y = 0, Kind = IcpItemKind.Label,      LabelText = "SDBY" },
                new() { X = 18, Y = 0, Kind = IcpItemKind.BoundField, AttributeName = "Com1StandByFrequency", Inverted = true },

                // ── COM 1 STATUS / TYPE ───────────────
                new() { X = 0,  Y = 1, Kind = IcpItemKind.BoundField, AttributeName = "Com1ActiveFrequencyType" },
                new() { X = 5,  Y = 1, Kind = IcpItemKind.BoundField, AttributeName = "Com1ActiveFrequencyIdent" },
                new() { X = 13, Y = 1, Kind = IcpItemKind.Label,      LabelText = "STAT" },
                new() { X = 18, Y = 1, Kind = IcpItemKind.BoundField, AttributeName = "Com1Status" },

                // ── COM 1 AUDIO ───────────────────────
                new() { X = 0,  Y = 2, Kind = IcpItemKind.Label,      LabelText = "MON1" },
                new() { X = 5,  Y = 2, Kind = IcpItemKind.BoundField, AttributeName = "Com1Receive" },
                new() { X = 13, Y = 2, Kind = IcpItemKind.Label,      LabelText = "MIC1" },
                new() { X = 18, Y = 2, Kind = IcpItemKind.BoundField, AttributeName = "Com1Transmit" },

                // ── POWER ────────────────────────────
                new() { X = 0,  Y = 3, Kind = IcpItemKind.Label,      LabelText = "BATT" },
                new() { X = 5,  Y = 3, Kind = IcpItemKind.BoundField, AttributeName = "BatteryOn" },
                new() { X = 14, Y = 3, Kind = IcpItemKind.Label,      LabelText = "GEN" },
                new() { X = 18, Y = 3, Kind = IcpItemKind.BoundField, AttributeName = "GeneratorOn" },

                new() { X = 0,  Y = 4, Kind = IcpItemKind.Label,      LabelText = "AVIO" },
                new() { X = 5,  Y = 4, Kind = IcpItemKind.BoundField, AttributeName = "AvionicsOn" },
            };

        public static readonly List<UserIcpDisplayItem> COM2 =
            new()
            {
                // ── COM 2 FREQUENCIES ─────────────────
                new() { X = 0,  Y = 0, Kind = IcpItemKind.Label,      LabelText = "COM2" },
                new() { X = 5,  Y = 0, Kind = IcpItemKind.BoundField, AttributeName = "Com2Frequency" },
                new() { X = 13, Y = 0, Kind = IcpItemKind.Label,      LabelText = "STBY" },
                new() { X = 18, Y = 0, Kind = IcpItemKind.BoundField, AttributeName = "Com2StandByFrequency", Inverted = true },

                // ── COM 2 STATUS / TYPE ───────────────
                new() { X = 0,  Y = 1, Kind = IcpItemKind.BoundField, AttributeName = "Com2ActiveFrequencyType" },
                new() { X = 5,  Y = 1, Kind = IcpItemKind.BoundField, AttributeName = "Com2ActiveFrequencyIdent" },
                new() { X = 13, Y = 1, Kind = IcpItemKind.Label,      LabelText = "STAT" },
                new() { X = 18, Y = 1, Kind = IcpItemKind.BoundField, AttributeName = "Com2Status" },

                // ── COM 2 AUDIO ───────────────────────
                new() { X = 0,  Y = 2, Kind = IcpItemKind.Label,      LabelText = "MON2" },
                new() { X = 5,  Y = 2, Kind = IcpItemKind.BoundField, AttributeName = "Com2Receive" },
                new() { X = 13, Y = 2, Kind = IcpItemKind.Label,      LabelText = "MIC2" },
                new() { X = 18, Y = 2, Kind = IcpItemKind.BoundField, AttributeName = "Com2Transmit" },

                // ── POWER ────────────────────────────
                new() { X = 0,  Y = 3, Kind = IcpItemKind.Label,      LabelText = "BATT" },
                new() { X = 5,  Y = 3, Kind = IcpItemKind.BoundField, AttributeName = "BatteryOn" },
                new() { X = 14, Y = 3, Kind = IcpItemKind.Label,      LabelText = "GEN" },
                new() { X = 18, Y = 3, Kind = IcpItemKind.BoundField, AttributeName = "GeneratorOn" },

                new() { X = 0,  Y = 4, Kind = IcpItemKind.Label,      LabelText = "AVIO" },
                new() { X = 5,  Y = 4, Kind = IcpItemKind.BoundField, AttributeName = "AvionicsOn" },
            };

        public static readonly List<UserIcpDisplayItem> INFO =
            new()
            {
                // ── IDENTIFICATION ────────────────────
                new() { X = 0,  Y = 0, Kind = IcpItemKind.BoundField, AttributeName = "ATCIdentifier" },
                new() { X = 0,  Y = 1, Kind = IcpItemKind.BoundField, AttributeName = "AircraftName" },

                // ── ICAO / ENGINE ─────────────────────
                new() { X = 0,  Y = 2, Kind = IcpItemKind.Label,      LabelText = "ICAO" },
                new() { X = 5,  Y = 2, Kind = IcpItemKind.BoundField, AttributeName = "ATCModel" },
                new() { X = 12, Y = 0, Kind = IcpItemKind.BoundField, AttributeName = "EngineType" },

                // ── POSITION ──────────────────────────
                new() { X = 1,  Y = 3, Kind = IcpItemKind.Label,      LabelText = "LAT" },
                new() { X = 5,  Y = 3, Kind = IcpItemKind.BoundField, AttributeName = "Latitude" },

                new() { X = 1,  Y = 4, Kind = IcpItemKind.Label,      LabelText = "LON" },
                new() { X = 5,  Y = 4, Kind = IcpItemKind.BoundField, AttributeName = "Longitude" },
            };

        public static readonly List<UserIcpDisplayItem> WX =
            new()
            {
                // ── TEMP + WIND (single top row) ──────────────────────────
                new () { X = 0,  Y = 0, Kind = IcpItemKind.Label,      LabelText = "TEMP" },
                new () { X = 5,  Y = 0, Kind = IcpItemKind.BoundField, AttributeName = "AmbientTemperature" },

                new () { X = 11, Y = 0, Kind = IcpItemKind.Label,      LabelText = "WIND" },
                new () { X = 16, Y = 0, Kind = IcpItemKind.BoundField, AttributeName = "AmbientWindDirectionDegrees" },
                new () { X = 20, Y = 0, Kind = IcpItemKind.Label,      LabelText = "/" },
                new () { X = 21, Y = 0, Kind = IcpItemKind.BoundField, AttributeName = "AmbientWindSpeed" },

                // ── STATIC PRESSURE ──────────────────────────────────────
                new () { X = 0,  Y = 1, Kind = IcpItemKind.Label,      LabelText = "STAT" },
                new () { X = 5,  Y = 1, Kind = IcpItemKind.BoundField, AttributeName = "Pressure" },

                // ── QNH (Kollsman) ────────────────────────────────────────
                new () { X = 1,  Y = 2, Kind = IcpItemKind.Label,      LabelText = "QNH" },
                new () { X = 5,  Y = 2, Kind = IcpItemKind.BoundField, AttributeName = "Kollsman" },

                // ── LOCATION (CITY / COUNTRY) ─────────────────────────────
                new () { X = 0,  Y = 3, Kind = IcpItemKind.BoundField, AttributeName = "PositionCityRegion" },
                new () { X = 0,  Y = 4, Kind = IcpItemKind.BoundField, AttributeName = "PositionCountry" },
            };

        public static readonly List<UserIcpDisplayItem> TIME =
            new()
            {
                // Labels
                new() { X = 1, Y = 0, Kind = IcpItemKind.Label, LabelText = "LOC" },
                new() { X = 1, Y = 1, Kind = IcpItemKind.Label, LabelText = "POS" },
                new() { X = 0, Y = 2, Kind = IcpItemKind.Label, LabelText = "ZULU" },

                // Times
                new() { X = 5, Y = 0, Kind = IcpItemKind.BoundField, AttributeName = "LocalTime24Hour" },
                new() { X = 5, Y = 1, Kind = IcpItemKind.BoundField, AttributeName = "PositionTime24Hour" },
                new() { X = 5, Y = 2, Kind = IcpItemKind.BoundField, AttributeName = "ZuluTime24Hour" },

                // Location
                new() { X = 0, Y = 3, Kind = IcpItemKind.BoundField, AttributeName = "PositionCityRegion" },
                new() { X = 0, Y = 4, Kind = IcpItemKind.BoundField, AttributeName = "PositionCountry" },
            };

        public static readonly List<UserIcpDisplayItem> GPS =
            new()
            {
                // ── ROW 0 ─────────────────────────────────────────────
                new() { X = 0,  Y = 0, Kind = IcpItemKind.Label,      LabelText = "DTRK" },
                new() { X = 5,  Y = 0, Kind = IcpItemKind.BoundField, AttributeName = "GPSRequiredMagneticHeadingDegrees" },
                new() { X = 13, Y = 0, Kind = IcpItemKind.Label,      LabelText = "XTRK" },
                new() { X = 18, Y = 0, Kind = IcpItemKind.BoundField, AttributeName = "GPSCrossTrackError" },

                // ── ROW 1 ─────────────────────────────────────────────
                new() { X = 0,  Y = 1, Kind = IcpItemKind.Label,      LabelText = "DIST" },
                new() { X = 5,  Y = 1, Kind = IcpItemKind.BoundField, AttributeName = "DistanceToWaypoint" },
                new() { X = 13, Y = 1, Kind = IcpItemKind.Label,      LabelText = "TIME" },
                new() { X = 18, Y = 1, Kind = IcpItemKind.BoundField, AttributeName = "TimeToWaypointDisplay" },

                // ── ROW 2 ─────────────────────────────────────────────
                new() { X = 0,  Y = 2, Kind = IcpItemKind.Label,      LabelText = "IDNT" },
                new() { X = 5,  Y = 2, Kind = IcpItemKind.BoundField, AttributeName = "WaypointIdent" },
                new() { X = 14, Y = 2, Kind = IcpItemKind.BoundField, AttributeName = "HasActiveWaypoint" },

                // ── ROW 3 ─────────────────────────────────────────────
                new() { X = 1,  Y = 3, Kind = IcpItemKind.Label,      LabelText = "LOC" },
                new() { X = 5,  Y = 3, Kind = IcpItemKind.BoundField, AttributeName = "WaypointLatitudeShort" },
                new() { X = 15, Y = 3, Kind = IcpItemKind.BoundField, AttributeName = "WaypointLongitudeShort" },

                // ── ROW 4 ─────────────────────────────────────────────
                new() { X = 1,  Y = 4, Kind = IcpItemKind.Label,      LabelText = "POS" },
                new() { X = 5,  Y = 4, Kind = IcpItemKind.BoundField, AttributeName = "LatitudeShort" },
                new() { X = 15, Y = 4, Kind = IcpItemKind.BoundField, AttributeName = "LongitudeShort" },
            };

        public static readonly List<UserIcpDisplayItem> LGHT =
            new()
            {
                // Title
                new() { X = 10, Y = 0, Kind = IcpItemKind.Label, LabelText = "LGHT" },

                // Cabin / Beacon
                new() { X = 0,  Y = 1, Kind = IcpItemKind.Label,      LabelText = "CABN" },
                new() { X = 5,  Y = 1, Kind = IcpItemKind.BoundField, AttributeName = "CabinLightsOn" },
                new() { X = 14, Y = 1, Kind = IcpItemKind.Label,      LabelText = "BEAC" },
                new() { X = 19, Y = 1, Kind = IcpItemKind.BoundField, AttributeName = "BeaconLightsOn" },

                // Taxi / Landing
                new() { X = 0,  Y = 2, Kind = IcpItemKind.Label,      LabelText = "TAXI" },
                new() { X = 5,  Y = 2, Kind = IcpItemKind.BoundField, AttributeName = "TaxiLightsOn" },
                new() { X = 14, Y = 2, Kind = IcpItemKind.Label,      LabelText = "LNDG" },
                new() { X = 19, Y = 2, Kind = IcpItemKind.BoundField, AttributeName = "LandingLightsOn" },

                // Strobe / Nav
                new() { X = 0,  Y = 3, Kind = IcpItemKind.Label,      LabelText = "STRB" },
                new() { X = 5,  Y = 3, Kind = IcpItemKind.BoundField, AttributeName = "StrobeLightsOn" },
                new() { X = 15, Y = 3, Kind = IcpItemKind.Label,      LabelText = "NAV" },
                new() { X = 19, Y = 3, Kind = IcpItemKind.BoundField, AttributeName = "NavLightsOn" },

                // Panel
                new() { X = 0,  Y = 4, Kind = IcpItemKind.Label,      LabelText = "PANL" },
                new() { X = 5,  Y = 4, Kind = IcpItemKind.BoundField, AttributeName = "PanelLightsOn" },
            };

        public static readonly List<UserIcpDisplayItem> SWCH =
            new()
            {
                // Battery / Generator
                new() { X = 0,  Y = 0, Kind = IcpItemKind.Label,      LabelText = "BATT" },
                new() { X = 5,  Y = 0, Kind = IcpItemKind.BoundField, AttributeName = "BatteryOn" },
                new() { X = 15, Y = 0, Kind = IcpItemKind.Label,      LabelText = "GEN" },
                new() { X = 19, Y = 0, Kind = IcpItemKind.BoundField, AttributeName = "GeneratorOn" },

                // Pitot / Avionixa
                new() { X = 0,  Y = 1, Kind = IcpItemKind.Label,      LabelText = "HEAT" },
                new() { X = 5,  Y = 1, Kind = IcpItemKind.BoundField, AttributeName = "PitotHeatOn" },
                new() { X = 14,  Y = 1, Kind = IcpItemKind.Label,      LabelText = "AVIO" },
                new() { X = 19,  Y = 1, Kind = IcpItemKind.BoundField, AttributeName = "AvionicsOn" },

                // Defrost / Prop De-Ice
                new() { X = 0,  Y = 2, Kind = IcpItemKind.Label,      LabelText = "DEFR" },
                new() { X = 5,  Y = 2, Kind = IcpItemKind.BoundField, AttributeName = "WindowDefrostOn" },
                new() { X = 14, Y = 2, Kind = IcpItemKind.Label,      LabelText = "PRPD" },
                new() { X = 19, Y = 2, Kind = IcpItemKind.BoundField, AttributeName = "PropellerDeIce" },

                // Parking Brake / Carb Heat
                new() { X = 0,  Y = 3, Kind = IcpItemKind.Label,      LabelText = "PKBK" },
                new() { X = 5,  Y = 3, Kind = IcpItemKind.BoundField, AttributeName = "ParkingBrakeOn" },
                new() { X = 14, Y = 3, Kind = IcpItemKind.Label,      LabelText = "CRBD" },
                new() { X = 19, Y = 3, Kind = IcpItemKind.BoundField, AttributeName = "CarbHeatAntiIce" },

                // Fuel Pump / Cowl Flaps
                new() { X = 0,  Y = 4, Kind = IcpItemKind.Label,      LabelText = "FUPM" },
                new() { X = 5,  Y = 4, Kind = IcpItemKind.BoundField, AttributeName = "FuelPumpOn" },
                new() { X = 14, Y = 4, Kind = IcpItemKind.Label,      LabelText = "COWL" },
                new() { X = 19, Y = 4, Kind = IcpItemKind.BoundField, AttributeName = "CowlFlapsOpen" },
            };

        public static readonly List<UserIcpDisplayItem> AP =
            new()
            {
                // Master
                new() { X = 0,  Y = 0, Kind = IcpItemKind.Label, LabelText = "MST" },
                new() { X = 4, Y = 0, Kind = IcpItemKind.BoundField, AttributeName = "AutopilotMaster" },
                new() { X = 7, Y = 0, Kind = IcpItemKind.BoundField, AttributeName = "FlightDirectorActive" },
                new() { X = 10, Y = 0, Kind = IcpItemKind.BoundField, AttributeName = "GpsDrivesNav" },

                // Hold modes
                new() { X = 0, Y = 1, Kind = IcpItemKind.BoundField, AttributeName = "AutopilotAltHold" },
                new() { X = 0, Y = 2, Kind = IcpItemKind.BoundField, AttributeName = "AutopilotHeadingHold" },
                new() { X = 0, Y = 3, Kind = IcpItemKind.BoundField, AttributeName = "AutopilotNavHold" },
                new() { X = 0, Y = 4, Kind = IcpItemKind.BoundField, AttributeName = "AutopilotSpeedHold" },
                new() { X = 13, Y = 1, Kind = IcpItemKind.BoundField, AttributeName = "AutopilotVerticalSpeedHold" },

                // Targets
                new() { X = 4, Y = 1, Kind = IcpItemKind.BoundField, AttributeName = "AutopilotAltitudeTarget" },
                new() { X = 4, Y = 2, Kind = IcpItemKind.BoundField, AttributeName = "AutopilotHeadingBugDegrees" },
                new() { X = 4, Y = 3, Kind = IcpItemKind.BoundField, AttributeName = "AutopilotNavCourseDegrees" },
                new() { X = 4, Y = 4, Kind = IcpItemKind.BoundField, AttributeName = "AutopilotSpeedTarget" },
                new() { X = 16, Y = 1, Kind = IcpItemKind.BoundField, AttributeName = "AutopilotVerticalSpeedTarget" },
            };

        public static readonly List<UserIcpDisplayItem> HROT =
            new()
            {
                // ── ROTOR RPM ─────────────────────────────
                new() { X = 0,  Y = 0, Kind = IcpItemKind.Label,      LabelText = "MRPM" },
                new() { X = 5,  Y = 0, Kind = IcpItemKind.BoundField, AttributeName = "MainRotorRpm" },
                new() { X = 13, Y = 0, Kind = IcpItemKind.Label,      LabelText = "TRPM" },
                new() { X = 18, Y = 0, Kind = IcpItemKind.BoundField, AttributeName = "TailRotorRpm" },

                // ── TORQUE ────────────────────────────────
                new() { X = 0,  Y = 1, Kind = IcpItemKind.Label,      LabelText = "TRQ1" },
                new() { X = 5,  Y = 1, Kind = IcpItemKind.BoundField, AttributeName = "Engine1TorquePercent" },
                new() { X = 13, Y = 1, Kind = IcpItemKind.Label,      LabelText = "TRQ2" },
                new() { X = 18, Y = 1, Kind = IcpItemKind.BoundField, AttributeName = "Engine2TorquePercent" },

                // ── GOVERNORS ─────────────────────────────
                new() { X = 0,  Y = 2, Kind = IcpItemKind.Label,      LabelText = "GOV1" },
                new() { X = 5,  Y = 2, Kind = IcpItemKind.BoundField, AttributeName = "RotorGovernor1Active" },
                new() { X = 13, Y = 2, Kind = IcpItemKind.Label,      LabelText = "GOV2" },
                new() { X = 18, Y = 2, Kind = IcpItemKind.BoundField, AttributeName = "RotorGovernor2Active" },

                // ── COLLECTIVE / BRAKE ────────────────────
                new() { X = 0,  Y = 3, Kind = IcpItemKind.Label,      LabelText = "COLL" },
                new() { X = 5,  Y = 3, Kind = IcpItemKind.BoundField, AttributeName = "CollectivePercent" },
                new() { X = 13, Y = 3, Kind = IcpItemKind.Label,      LabelText = "BRKE" },
                new() { X = 18, Y = 3, Kind = IcpItemKind.BoundField, AttributeName = "RotorBrakeActive" },

                // ── ROTOR PHASE ANGLES ────────────────────
                new() { X = 0,  Y = 4, Kind = IcpItemKind.Label,      LabelText = "PHAS" },
                new() { X = 5,  Y = 4, Kind = IcpItemKind.BoundField, AttributeName = "MainRotorRotationAngleDegrees" },
                new() { X = 13, Y = 4, Kind = IcpItemKind.BoundField, AttributeName = "TailRotorRotationAngleDegrees" },
            };

        public static readonly List<UserIcpDisplayItem> HDIS =
            new()
            {
                // ── DISK BANK ─────────────────────────────
                new() { X = 0,  Y = 0, Kind = IcpItemKind.Label,      LabelText = "MDBK" },
                new() { X = 5,  Y = 0, Kind = IcpItemKind.BoundField, AttributeName = "MainRotorDiskBankPercent" },
                new() { X = 14, Y = 0, Kind = IcpItemKind.Label,      LabelText = "TDBK" },
                new() { X = 19, Y = 0, Kind = IcpItemKind.BoundField, AttributeName = "TailRotorDiskBankPercent" },

                // ── DISK PITCH ────────────────────────────
                new() { X = 0,  Y = 1, Kind = IcpItemKind.Label,      LabelText = "MDPT" },
                new() { X = 5,  Y = 1, Kind = IcpItemKind.BoundField, AttributeName = "MainRotorDiskPitchPercent" },
                new() { X = 14, Y = 1, Kind = IcpItemKind.Label,      LabelText = "TDPT" },
                new() { X = 19, Y = 1, Kind = IcpItemKind.BoundField, AttributeName = "TailRotorDiskPitchPercent" },

                // ── DISK CONING ───────────────────────────
                new() { X = 0,  Y = 2, Kind = IcpItemKind.Label,      LabelText = "MDCN" },
                new() { X = 5,  Y = 2, Kind = IcpItemKind.BoundField, AttributeName = "MainRotorDiskConingPercent" },
                new() { X = 14, Y = 2, Kind = IcpItemKind.Label,      LabelText = "TDCN" },
                new() { X = 19, Y = 2, Kind = IcpItemKind.BoundField, AttributeName = "TailRotorDiskConingPercent" },

                // ── TRIMS ─────────────────────────────────
                new() { X = 1,  Y = 3, Kind = IcpItemKind.Label,      LabelText = "LAT" },
                new() { X = 5,  Y = 3, Kind = IcpItemKind.BoundField, AttributeName = "RotorLateralTrimPercent" },
                new() { X = 14, Y = 3, Kind = IcpItemKind.Label,      LabelText = "LONG" },
                new() { X = 19, Y = 3, Kind = IcpItemKind.BoundField, AttributeName = "RotorLongitudinalTrimPercent" },

                // ── ROTOR PHASE ───────────────────────────
                new() { X = 0,  Y = 4, Kind = IcpItemKind.Label,      LabelText = "PHAS" },
                new() { X = 5,  Y = 4, Kind = IcpItemKind.BoundField, AttributeName = "MainRotorRotationAngleDegrees" },
                new() { X = 14, Y = 4, Kind = IcpItemKind.BoundField, AttributeName = "TailRotorRotationAngleDegrees" },
            };

        public static readonly List<UserIcpDisplayItem> HCTL =
            new()
            {
                // ── COLLECTIVE / CYCLIC ────────────────────
                new() { X = 0,  Y = 0, Kind = IcpItemKind.Label,      LabelText = "COLL" },
                new() { X = 5,  Y = 0, Kind = IcpItemKind.BoundField, AttributeName = "CollectivePercent" },
                new() { X = 14, Y = 0, Kind = IcpItemKind.Label,      LabelText = "CYCL" },
                new() { X = 19, Y = 0, Kind = IcpItemKind.BoundField, AttributeName = "CyclicPercent" },

                // ── TRIMS ─────────────────────────────────
                new() { X = 1,  Y = 1, Kind = IcpItemKind.Label,      LabelText = "LAT" },
                new() { X = 5,  Y = 1, Kind = IcpItemKind.BoundField, AttributeName = "RotorLateralTrimPercent" },
                new() { X = 14, Y = 1, Kind = IcpItemKind.Label,      LabelText = "LONG" },
                new() { X = 19, Y = 1, Kind = IcpItemKind.BoundField, AttributeName = "RotorLongitudinalTrimPercent" },

                // ── ROTOR RPM COMMAND ─────────────────────
                new() { X = 0,  Y = 2, Kind = IcpItemKind.Label,      LabelText = "CMD1" },
                new() { X = 5,  Y = 2, Kind = IcpItemKind.BoundField, AttributeName = "Engine1RotorRpmCommandPercent" },
                new() { X = 14, Y = 2, Kind = IcpItemKind.Label,      LabelText = "CMD2" },
                new() { X = 19, Y = 2, Kind = IcpItemKind.BoundField, AttributeName = "Engine2RotorRpmCommandPercent" },

                // ── TORQUE ────────────────────────────────
                new() { X = 0,  Y = 3, Kind = IcpItemKind.Label,      LabelText = "TRQ1" },
                new() { X = 5,  Y = 3, Kind = IcpItemKind.BoundField, AttributeName = "Engine1TorquePercent" },
                new() { X = 14, Y = 3, Kind = IcpItemKind.Label,      LabelText = "TRQ2" },
                new() { X = 19, Y = 3, Kind = IcpItemKind.BoundField, AttributeName = "Engine2TorquePercent" },

                // ── BRAKE / GOVERNOR ──────────────────────
                new() { X = 0,  Y = 4, Kind = IcpItemKind.Label,      LabelText = "BRKE" },
                new() { X = 5,  Y = 4, Kind = IcpItemKind.BoundField, AttributeName = "RotorBrakeActive" },
                new() { X = 15, Y = 4, Kind = IcpItemKind.Label,      LabelText = "GOV" },
                new() { X = 19, Y = 4, Kind = IcpItemKind.BoundField, AttributeName = "RotorGovernorActive" },
            };

        public static readonly List<UserIcpDisplayItem> FLPN =
            new()
            {
                // ── ROW 0: Status ────────────────────────────────────
                new() { X = 0,  Y = 0, Kind = IcpItemKind.Label,      LabelText = "ACT"  },
                new() { X = 4,  Y = 0, Kind = IcpItemKind.BoundField, AttributeName = "FlightPlanIsActiveFlightPlan" },
                new() { X = 12, Y = 0, Kind = IcpItemKind.Label,      LabelText = "DTO"  },
                new() { X = 16, Y = 0, Kind = IcpItemKind.BoundField, AttributeName = "FlightPlanIsDirectTo" },

                // ── ROW 1: Counts / Indexes ──────────────────────────
                new() { X = 0,  Y = 1, Kind = IcpItemKind.Label,      LabelText = "WPT" },
                new() { X = 4,  Y = 1, Kind = IcpItemKind.BoundField, AttributeName = "FlightPlanWaypointsNumber" },
                new() { X = 8,  Y = 1, Kind = IcpItemKind.Label,      LabelText = "ACT" },
                new() { X = 12, Y = 1, Kind = IcpItemKind.BoundField, AttributeName = "FlightPlanActiveWaypoint" },
                new() { X = 16, Y = 1, Kind = IcpItemKind.Label,      LabelText = "IDX" },
                new() { X = 20, Y = 1, Kind = IcpItemKind.BoundField, AttributeName = "FlightPlanWaypointIndex" },

                // ── ROW 2: DEP / DEST ────────────────────────────────
                new() { X = 0,  Y = 2, Kind = IcpItemKind.Label,      LabelText = "IDF" },
                new() { X = 4,  Y = 2, Kind = IcpItemKind.BoundField, AttributeName = "FlightPlanWaypointIdent" },
                new() { X = 12, Y = 2, Kind = IcpItemKind.Label,      LabelText = "APR" },
                new() { X = 16, Y = 2, Kind = IcpItemKind.BoundField, AttributeName = "FlightPlanApproachIdent" },

                // ── ROW 3: Active Waypoint ───────────────────────────
                new() { X = 0,  Y = 3, Kind = IcpItemKind.Label,      LabelText = "LOC" },
                new() { X = 4,  Y = 3, Kind = IcpItemKind.BoundField, AttributeName = "WaypointLatitudeShort" },
                new() { X = 14, Y = 3, Kind = IcpItemKind.BoundField, AttributeName = "WaypointLongitudeShort" },

                // ── ROW 4: Unified Position ──────────────────────────
                new() { X = 0,  Y = 4, Kind = IcpItemKind.Label,      LabelText = "POS" },
                new() { X = 4,  Y = 4, Kind = IcpItemKind.BoundField, AttributeName = "LatitudeShort" },
                new() { X = 14, Y = 4, Kind = IcpItemKind.BoundField, AttributeName = "LongitudeShort" }

            };


        // ─────────────────────────────────────────────────────────────
        // BALN - Balloons
        // ─────────────────────────────────────────────────────────────
        public static readonly List<UserIcpDisplayItem> BALN =
            new()
            {
                // ── ROW 0: Auto / Fill ────────────────────────────────
                new() { X = 0,  Y = 0, Kind = IcpItemKind.Label,      LabelText = "AUTO" },
                new() { X = 5,  Y = 0, Kind = IcpItemKind.BoundField, AttributeName = "BalloonAutoFillActive" },
                new() { X = 13, Y = 0, Kind = IcpItemKind.Label,      LabelText = "FILL" },
                new() { X = 18, Y = 0, Kind = IcpItemKind.BoundField, AttributeName = "BalloonFillAmountPercent" },

                // ── ROW 1: Density / Temp ─────────────────────────────
                new() { X = 0,  Y = 1, Kind = IcpItemKind.Label,      LabelText = "DENS" },
                new() { X = 5,  Y = 1, Kind = IcpItemKind.BoundField, AttributeName = "BalloonGasDensity" },
                new() { X = 13, Y = 1, Kind = IcpItemKind.Label,      LabelText = "TEMP" },
                new() { X = 18, Y = 1, Kind = IcpItemKind.BoundField, AttributeName = "BalloonGasTemperature" },

                // ── ROW 2: Vent / Valve ───────────────────────────────
                new() { X = 0,  Y = 2, Kind = IcpItemKind.Label,      LabelText = "VENT" },
                new() { X = 5,  Y = 2, Kind = IcpItemKind.BoundField, AttributeName = "BalloonVentOpenPercent" },
                new() { X = 13, Y = 2, Kind = IcpItemKind.Label,      LabelText = "VALV" },
                new() { X = 18, Y = 2, Kind = IcpItemKind.BoundField, AttributeName = "BalloonBurnerValveOpenPercent" },

                // ── ROW 3: Flow / Pilot ───────────────────────────────
                new() { X = 0,  Y = 3, Kind = IcpItemKind.Label,      LabelText = "FLOW" },
                new() { X = 5,  Y = 3, Kind = IcpItemKind.BoundField, AttributeName = "BalloonBurnerFuelFlowRate" },
                new() { X = 13, Y = 3, Kind = IcpItemKind.Label,      LabelText = "PILT" },
                new() { X = 18, Y = 3, Kind = IcpItemKind.BoundField, AttributeName = "BalloonBurnerPilotLightOn" },
            };

        // ─────────────────────────────────────────────────────────────
        // AIRS - Airships
        // ─────────────────────────────────────────────────────────────
        public static readonly List<UserIcpDisplayItem> AIRS =
            new()
            {
                // ── ROW 0: Gas / Fan ──────────────────────────────────
                new() { X = 0,  Y = 0, Kind = IcpItemKind.Label,      LabelText = "GAS" },
                new() { X = 5,  Y = 0, Kind = IcpItemKind.BoundField, AttributeName = "AirshipCompartmentGasType" },
                new() { X = 13, Y = 0, Kind = IcpItemKind.Label,      LabelText = "FAN" },
                new() { X = 18, Y = 0, Kind = IcpItemKind.BoundField, AttributeName = "AirshipFanPowerPercent" },

                // ── ROW 1: Pressure / Over-Pressure ───────────────────
                new() { X = 0,  Y = 1, Kind = IcpItemKind.Label,      LabelText = "PRES" },
                new() { X = 5,  Y = 1, Kind = IcpItemKind.BoundField, AttributeName = "AirshipCompartmentPressure" },
                new() { X = 13, Y = 1, Kind = IcpItemKind.Label,      LabelText = "OPRS" },
                new() { X = 18, Y = 1, Kind = IcpItemKind.BoundField, AttributeName = "AirshipCompartmentOverPressure" },

                // ── ROW 2: Temp / Volume ──────────────────────────────
                new() { X = 0,  Y = 2, Kind = IcpItemKind.Label,      LabelText = "TEMP" },
                new() { X = 5,  Y = 2, Kind = IcpItemKind.BoundField, AttributeName = "AirshipCompartmentTemperature" },
                new() { X = 13, Y = 2, Kind = IcpItemKind.Label,      LabelText = "VOL" },
                new() { X = 18, Y = 2, Kind = IcpItemKind.BoundField, AttributeName = "AirshipCompartmentVolume" },

                // ── ROW 3: Weight / Mast Deploy ───────────────────────
                new() { X = 0,  Y = 3, Kind = IcpItemKind.Label,      LabelText = "WGHT" },
                new() { X = 5,  Y = 3, Kind = IcpItemKind.BoundField, AttributeName = "AirshipCompartmentWeight" },
                new() { X = 13, Y = 3, Kind = IcpItemKind.Label,      LabelText = "MAST" },
                new() { X = 18, Y = 3, Kind = IcpItemKind.BoundField, AttributeName = "AirshipMastTruckDeployment" },

                // ── ROW 4: Mast Extension ─────────────────────────────
                new() { X = 0,  Y = 4, Kind = IcpItemKind.Label,      LabelText = "EXT" },
                new() { X = 5,  Y = 4, Kind = IcpItemKind.BoundField, AttributeName = "AirshipMastTruckExtensionPercent" },
            };
    }
}
