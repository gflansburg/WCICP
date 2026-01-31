using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace FlightSim
{
    public static class Tools
    {
        const int BufferSize = 1024 * 1024;

        static Tools()
        {
        }

        public static string Get2024ExeXmlPath()
        {
            return string.Format("{0}\\LocalCache\\exe.xml", Get2024GamePath());
        }

        public static string Get2024CommunityPath()
        {
            return string.Format("{0}\\Community", Get2024InstalledPackagesPath());
        }

        public static string Get2024SimConnectIniPath()
        {
            return string.Format("{0}\\LocalState\\simconnect.ini", Get2024GamePath());
        }

        public static string Get2024UserCfg()
        {
            return string.Format("{0}\\LocalCache\\UserCfg.opt", Get2024GamePath());
        }

        public static string Get2024InstalledPackagesPath()
        {
            if (!string.IsNullOrEmpty(Get2024UserCfg()) && File.Exists(Get2024UserCfg()))
            {
                using (var fileStream = File.OpenRead(Get2024UserCfg()))
                {
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
                    {
                        if (streamReader != null)
                        {
                            string? line;
                            while ((line = streamReader.ReadLine()) != null)
                            {
                                line = line.Trim();
                                if (line.StartsWith("InstalledPackagesPath", StringComparison.OrdinalIgnoreCase))
                                {
                                    return line.Substring(21).Replace("\"", string.Empty);
                                }
                            }
                        }
                    }
                }
            }
            return string.Format("{0}\\LocalCache\\Packages", Get2024GamePath());
        }

        public static string Get2024GamePath()
        {
            return string.Format("{0}\\Local\\Packages\\Microsoft.Limitless_8wekyb3d8bbwe", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("\\Roaming", String.Empty));
        }

        public static string Get2024SteamExeXmlPath()
        {
            return string.Format("{0}\\exe.xml", Get2024GamePath());
        }

        public static string Get2024SteamCommunityPath()
        {
            return string.Format("{0}\\Community", Get2024SteamInstalledPackagesPath());
        }

        public static string Get2024SteamSimConnectIniPath()
        {
            return string.Format("{0}\\simconnect.ini", Get2024SteamGamePath());
        }

        public static string Get2024SteamUserCfg()
        {
            return string.Format("{0}\\LocalCache\\UserCfg.opt", Get2024SteamGamePath());
        }

        public static string Get2024SteamInstalledPackagesPath()
        {
            if (!string.IsNullOrEmpty(Get2024SteamUserCfg()) && File.Exists(Get2024SteamUserCfg()))
            {
                using (var fileStream = File.OpenRead(Get2024SteamUserCfg()))
                {
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
                    {
                        if (streamReader != null)
                        {
                            string? line;
                            while ((line = streamReader.ReadLine()) != null)
                            {
                                line = line.Trim();
                                if (line.StartsWith("InstalledPackagesPath", StringComparison.OrdinalIgnoreCase))
                                {
                                    return line.Substring(21).Replace("\"", string.Empty);
                                }
                            }
                        }
                    }
                }
            }
            return string.Format("{0}\\LocalCache\\Packages", Get2024SteamGamePath());
        }

        public static string Get2024SteamGamePath()
        {
            return string.Format("{0}\\Microsoft Flight Simulator 2024", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
        }


        public static string Get2020ExeXmlPath()
        {
            return string.Format("{0}\\LocalCache\\exe.xml", Get2020GamePath());
        }

        public static string Get2020CommunityPath()
        {
            return string.Format("{0}\\Community", Get2020InstalledPackagesPath());
        }

        public static string Get2020SimConnectIniPath()
        {
            return string.Format("{0}\\LocalState\\simconnect.ini", Get2020GamePath());
        }

        public static string Get2020UserCfg()
        {
            return string.Format("{0}\\LocalCache\\UserCfg.opt", Get2020GamePath());
        }

        public static string Get2020InstalledPackagesPath()
        {
            if (!string.IsNullOrEmpty(Get2020UserCfg()) && File.Exists(Get2020UserCfg()))
            {
                using (var fileStream = File.OpenRead(Get2020UserCfg()))
                {
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
                    {
                        if (streamReader != null)
                        {
                            string? line;
                            while ((line = streamReader.ReadLine()) != null)
                            {
                                line = line.Trim();
                                if (line.StartsWith("InstalledPackagesPath", StringComparison.OrdinalIgnoreCase))
                                {
                                    return line.Substring(21).Replace("\"", string.Empty);
                                }
                            }
                        }
                    }
                }
            }
            return string.Format("{0}\\LocalCache\\Packages", Get2020GamePath());
        }

        public static string Get2020GamePath()
        {
            return string.Format("{0}\\Local\\Packages\\Microsoft.FlightSimulator_8wekyb3d8bbwe", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("\\Roaming", String.Empty));
        }

        public static string Get2020SteamExeXmlPath()
        {
            return string.Format("{0}\\exe.xml", Get2020SteamGamePath());
        }

        public static string Get2020SteamCommunityPath()
        {
            return string.Format("{0}\\Community", Get2020SteamInstalledPackagesPath());
        }

        public static string Get2020SteamSimConnectIniPath()
        {
            return string.Format("{0}\\simconnect.ini", Get2020SteamGamePath());
        }

        public static string Get2020SteamUserCfg()
        {
            return string.Format("{0}\\LocalCache\\UserCfg.opt", Get2020SteamGamePath());
        }

        public static string Get2020SteamInstalledPackagesPath()
        {
            if (!string.IsNullOrEmpty(Get2020SteamUserCfg()) && File.Exists(Get2020SteamUserCfg()))
            {
                using (var fileStream = File.OpenRead(Get2020SteamUserCfg()))
                {
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
                    {
                        if (streamReader != null)
                        {
                            string? line;
                            while ((line = streamReader.ReadLine()) != null)
                            {
                                line = line.Trim();
                                if (line.StartsWith("InstalledPackagesPath", StringComparison.OrdinalIgnoreCase))
                                {
                                    return line.Substring(21).Replace("\"", string.Empty);
                                }
                            }
                        }
                    }
                }
            }
            return string.Format("{0}\\LocalCache\\Packages", Get2020SteamGamePath());
        }

        public static string Get2020SteamGamePath()
        {
            return string.Format("{0}\\Microsoft Flight Simulator", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
        }

        public static PluginInstalled IsPluginInstalled(SimBaseDocument doc, string key)
        {
            return GetLaunchAddon(doc, key) != null ? PluginInstalled.PluginInstalled : PluginInstalled.PluginNotInstalled;
        }

        public static PluginInstalled IsPluginInstalled(MSFSEdition edition, string key)
        {
            if (!string.IsNullOrEmpty(edition == MSFSEdition.Microsoft2020 ? Get2020GamePath() : edition == MSFSEdition.Microsoft2024 ? Get2024GamePath() : edition == MSFSEdition.Steam2024 ? Get2024SteamGamePath() : Get2020SteamGamePath()) && Directory.Exists(edition == MSFSEdition.Microsoft2020 ? Get2020GamePath() : edition == MSFSEdition.Microsoft2024 ? Get2024GamePath() : edition == MSFSEdition.Steam2024 ? Get2024SteamGamePath() : Get2020SteamGamePath()))
            {
                SimBaseDocument doc = GetSimBaseDocument(edition == MSFSEdition.Microsoft2020 ? Get2020ExeXmlPath() : edition == MSFSEdition.Microsoft2024 ? Get2024ExeXmlPath() : edition == MSFSEdition.Steam2024 ? Get2024SteamExeXmlPath() : Get2020SteamExeXmlPath());
                return IsPluginInstalled(doc, key);
            }
            return PluginInstalled.MSFSNotInstalled;
        }

        public static LaunchAddon GetLaunchAddon(SimBaseDocument doc, string key)
        {
            if (doc != null)
            {
                foreach (LaunchAddon launchAddon in doc.LaunchAddons)
                {
                    if ((launchAddon.Name ?? string.Empty).Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        return launchAddon;
                    }
                }
            }
            return null!;
        }

        public static void SaveSimBaseDocument(string filename, SimBaseDocument doc)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return;

            var serializer = new XmlSerializer(typeof(SimBaseDocument));

            // 👇 This prevents xmlns:xsi / xmlns:xsd from being emitted
            var ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);

            var settings = new XmlWriterSettings
            {
                Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), // UTF-8 without BOM
                Indent = true,
                NewLineHandling = NewLineHandling.Entitize
            };

            using var writer = XmlWriter.Create(filename, settings);
            serializer.Serialize(writer, doc, ns);
        }

        public static SimBaseDocument GetSimBaseDocument(string filename)
        {
            if (!string.IsNullOrEmpty(filename) && File.Exists(filename))
            {
                string xml = File.ReadAllText(filename);
                if (!string.IsNullOrEmpty(xml))
                {
                    //xml = xml.Replace("<SimBase.Document Type=\"SimConnect\" version=\"1,0\">", "<SimBase.Document>");
                    try
                    {
                        return (SimBaseDocument)SerializerHelper.FromXml(xml, typeof(SimBaseDocument));
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            return null!;
        }

        static public bool InstallToMSFS(string key)
        {
            bool installed = false;
            string exePath = Environment.ProcessPath != null ? Environment.ProcessPath : string.Empty;
            if (!string.IsNullOrEmpty(Get2020GamePath()) && Directory.Exists(Get2020GamePath()))
            {
                SimBaseDocument doc = GetSimBaseDocument(Get2020ExeXmlPath());
                if (doc == null)
                {
                    doc = new SimBaseDocument()
                    {
                        Descr = "SimConnect",
                        Filename = "SimConnect.xml",
                        Disabled = false,
                        LaunchManualLoad = false
                    };
                }
                LaunchAddon addon = GetLaunchAddon(doc, key);
                if (addon == null)
                {
                    addon = new LaunchAddon()
                    {
                        Name = key,
                        Disabled = false,
                        ManualLoad = false,
                        NewConsole = false,
                        Path = exePath
                    };
                    doc.LaunchAddons.Add(addon);
                }
                else
                {
                    addon.Disabled = false;
                    addon.Path = exePath;
                }
                SaveSimBaseDocument(Get2020ExeXmlPath(), doc);
                installed = true;
            }
            if (!string.IsNullOrEmpty(Get2024GamePath()) && Directory.Exists(Get2024GamePath()))
            {
                SimBaseDocument doc = GetSimBaseDocument(Get2024ExeXmlPath());
                if (doc == null)
                {
                    doc = new SimBaseDocument()
                    {
                        Descr = "SimConnect",
                        Filename = "SimConnect.xml",
                        Disabled = false,
                        LaunchManualLoad = false
                    };
                }
                LaunchAddon addon = GetLaunchAddon(doc, key);
                if (addon == null)
                {
                    addon = new LaunchAddon()
                    {
                        Name = key,
                        Disabled = false,
                        ManualLoad = false,
                        NewConsole = false,
                        Path = exePath
                    };
                    doc.LaunchAddons.Add(addon);
                }
                else
                {
                    addon.Disabled = false;
                    addon.Path = exePath;
                }
                SaveSimBaseDocument(Get2024ExeXmlPath(), doc);
                installed = true;
            }
            if (!string.IsNullOrEmpty(Get2020SteamGamePath()) && Directory.Exists(Get2020SteamGamePath()))
            {
                SimBaseDocument doc = GetSimBaseDocument(Get2020SteamExeXmlPath());
                if (doc == null)
                {
                    doc = new SimBaseDocument()
                    {
                        Descr = "SimConnect",
                        Filename = "SimConnect.xml",
                        Disabled = false,
                        LaunchManualLoad = false
                    };
                }
                LaunchAddon addon = GetLaunchAddon(doc, key);
                if (addon == null)
                {
                    addon = new LaunchAddon()
                    {
                        Name = key,
                        Disabled = false,
                        Path = exePath
                    };
                    doc.LaunchAddons.Add(addon);
                }
                else
                {
                    addon.Disabled = false;
                    addon.Path = exePath;
                }
                SaveSimBaseDocument(Get2020SteamExeXmlPath(), doc);
                installed = true;
            }
            if (!string.IsNullOrEmpty(Get2024SteamGamePath()) && Directory.Exists(Get2024SteamGamePath()))
            {
                SimBaseDocument doc = GetSimBaseDocument(Get2024SteamExeXmlPath());
                if (doc == null)
                {
                    doc = new SimBaseDocument()
                    {
                        Descr = "SimConnect",
                        Filename = "SimConnect.xml",
                        Disabled = false,
                        LaunchManualLoad = false
                    };
                }
                LaunchAddon addon = GetLaunchAddon(doc, key);
                if (addon == null)
                {
                    addon = new LaunchAddon()
                    {
                        Name = key,
                        Disabled = false,
                        Path = exePath
                    };
                    doc.LaunchAddons.Add(addon);
                }
                else
                {
                    addon.Disabled = false;
                    addon.Path = exePath;
                }
                SaveSimBaseDocument(Get2024SteamExeXmlPath(), doc);
                installed = true;
            }
            return installed;
        }

        static public bool UninstallFromMSFS(string key)
        {
            bool uninstalled = false;
            if (!string.IsNullOrEmpty(Get2020GamePath()) && Directory.Exists(Get2020GamePath()))
            {
                if (!string.IsNullOrEmpty(Get2020ExeXmlPath()) && File.Exists(Get2020ExeXmlPath()))
                {
                    SimBaseDocument doc = GetSimBaseDocument(Get2020ExeXmlPath());
                    if (doc != null)
                    {
                        LaunchAddon addon = GetLaunchAddon(doc, key);
                        if (addon != null)
                        {
                            doc.LaunchAddons.Remove(addon);
                            SaveSimBaseDocument(Get2020ExeXmlPath(), doc);
                            uninstalled = true;
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(Get2024GamePath()) && Directory.Exists(Get2024GamePath()))
            {
                if (!string.IsNullOrEmpty(Get2024ExeXmlPath()) && File.Exists(Get2024ExeXmlPath()))
                {
                    SimBaseDocument doc = GetSimBaseDocument(Get2024ExeXmlPath());
                    if (doc != null)
                    {
                        LaunchAddon addon = GetLaunchAddon(doc, key);
                        if (addon != null)
                        {
                            doc.LaunchAddons.Remove(addon);
                            SaveSimBaseDocument(Get2024ExeXmlPath(), doc);
                            uninstalled = true;
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(Get2020SteamGamePath()) && Directory.Exists(Get2020SteamGamePath()))
            {
                if (!string.IsNullOrEmpty(Get2020SteamExeXmlPath()) && File.Exists(Get2020SteamExeXmlPath()))
                {
                    SimBaseDocument doc = GetSimBaseDocument(Get2020SteamExeXmlPath());
                    if (doc != null)
                    {
                        LaunchAddon addon = GetLaunchAddon(doc, key);
                        if (addon != null)
                        {
                            doc.LaunchAddons.Remove(addon);
                            SaveSimBaseDocument(Get2020SteamExeXmlPath(), doc);
                            uninstalled = true;
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(Get2024SteamGamePath()) && Directory.Exists(Get2024SteamGamePath()))
            {
                if (!string.IsNullOrEmpty(Get2024SteamExeXmlPath()) && File.Exists(Get2024SteamExeXmlPath()))
                {
                    SimBaseDocument doc = GetSimBaseDocument(Get2024SteamExeXmlPath());
                    if (doc != null)
                    {
                        LaunchAddon addon = GetLaunchAddon(doc, key);
                        if (addon != null)
                        {
                            doc.LaunchAddons.Remove(addon);
                            SaveSimBaseDocument(Get2024SteamExeXmlPath(), doc);
                            uninstalled = true;
                        }
                    }
                }
            }
            return uninstalled;
        }

        public static double InchesMercuryToHectoPascals(double d) => d * 33.8638866667;
        public static double HectoPascalsToInchesMercury(double d) => d / 33.8638866667;

        public static double FeetToMeters(double ft) => ft * 0.3048;
        public static double MetersToFeet(double m) => m / 0.3048;

        public static double PoundsToKilograms(double lbs) => lbs * 0.45359237;
        public static double KilogramsToPounds(double kg) => kg / 0.45359237;

        public static double KnotsToKph(double kts) => kts * 1.852;
        public static double KnotsToMph(double kts) => kts * 1.150779;

        public static double CelsiusToFahrenheit(double c) => (c * 9.0 / 5.0) + 32.0;
        public static double FahrenheitToCelsius(double f) => (f - 32.0) * 5.0 / 9.0;

        public static double CubicMetersToCubicFeet(double m3) => m3 * 35.3146667215;
        public static double CubicFeetToCubicMeters(double ft3) => ft3 / 35.3146667215;

        public static double MetersToNm(double m) => m / 1852.0;
        public static double MetersToKm(double m) => m / 1000.0;
        public static double KmToMeters(double km) => km * 1000.0;
        public static double NmToMeters(double m) => m * 1852.0;
        public static double NmToMiles(double nm) => nm * 1.150779448;
        public static double MilesToNm(double mi) => mi / 1.150779448;

        public static double PsfToPsi(double psf) => psf / 144.0;
        public static double PsiToPsf(double psi) => psi * 144.0;

        public static double GallonsToLiters(double gallons) => gallons * 3.785411784;

        public static double RadToDeg(double radians) => radians * (180.0 / Math.PI);

        public static double DegToRad(double degrees) => degrees * (Math.PI / 180.0);

        public static double FpmToMps(double fpm) => fpm * 0.00508;
        public static double MpsToFpm(double mps) => mps / 0.00508;

        public static double Clamp(double v, double min, double max) => (v < min) ? min : (v > max) ? max : v;

        // Map +/-10 degrees => +/-100 percent
        public static double DegToPct10(double deg) => Clamp((deg / 10.0) * 100.0, -100.0, 100.0);

        // Map 0..90 degrees => 0..100 percent
        public static double DegToPct90(double deg) => Clamp((deg / 90.0) * 100.0, 0.0, 100.0);

        public static double RadSecToRpm(double radPerSec) => radPerSec * 60.0 / (2.0 * Math.PI);

        public static double Scalar16KToPercent(double value)
        {
            if (value <= 0) return 0;
            return Math.Min((value / 16384.0) * 100.0, 100.0);
        }

        public static double NormalizeDegrees(double degrees)
        {
            degrees %= 360.0;
            if (degrees < 0)
                degrees += 360.0;
            return degrees;
        }

        public static double ClampPercent(double p)
        {
            if (double.IsNaN(p) || double.IsInfinity(p)) return 0;
            if (p < 0) return 0;
            if (p > 100) return 100;
            return p;
        }

        public static double FractionToPercent(double f) => ClampPercent(f * 100.0);

        public static string PercentSignedToString(double pct)
        {
            if (double.IsNaN(pct) || double.IsInfinity(pct)) pct = 0;
            pct = Math.Max(-100, Math.Min(100, pct));
            int p = (int)Math.Round(pct, MidpointRounding.AwayFromZero);
            // Example outputs: "-12", "0", "+25"
            if (p > 0) return $"+{p}";
            return p.ToString(CultureInfo.InvariantCulture);
        }

        public static double BearingToStationDegrees(double aircraftLatDeg, double aircraftLonDeg, double stationLatDeg, double stationLonDeg)
        {
            double lat1 = DegToRad(aircraftLatDeg);
            double lat2 = DegToRad(stationLatDeg);
            double dLon = DegToRad(stationLonDeg - aircraftLonDeg);
            double y = Math.Sin(dLon) * Math.Cos(lat2);
            double x = Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(dLon);
            double brg = Math.Atan2(y, x) * 180.0 / Math.PI;
            return (brg + 360.0) % 360.0;
        }

        public static double DistanceTo(double lat1, double lon1, double lat2, double lon2, char unit = 'E')
        {
            double rlat1 = Math.PI * lat1 / 180;
            double rlat2 = Math.PI * lat2 / 180;
            double theta = lon1 - lon2;
            double rtheta = Math.PI * theta / 180;
            double dist = Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) * Math.Cos(rlat2) * Math.Cos(rtheta);

            dist = Math.Acos(dist);
            dist = dist * 180 / Math.PI;
            dist = dist * 60 * 1.1515;
            switch (unit)
            {
                case 'E': //Meters
                    return dist * 1000.609344;
                case 'K': //Kilometers
                    return dist * 1.609344;
                case 'N': //Nautical Miles 
                    return dist * 0.8684;
                case 'M': //Miles
                    return dist;
            }
            return dist;
        }

        public static uint Bcd2Dec(uint num)
        {
            return HornerScheme(num, 0x10, 10);
        }

        public static uint Dec2Bcd(uint num)
        {
            return HornerScheme(num, 10, 0x10);
        }

        static private uint HornerScheme(uint num, uint divider, uint factor)
        {
            uint remainder = num % divider;
            uint quotient = num / divider;
            uint result = 0;

            if (!(quotient == 0 && remainder == 0))
            {
                result += HornerScheme(quotient, divider, factor) * factor + remainder;
            }
            return result;
        }

        public static string GetExecutingDirectory()
        {
            return AppContext.BaseDirectory;
        }

        internal static bool TryParseUtcOffset(string? offsetText, out TimeSpan offset)
        {
            offset = TimeSpan.Zero;

            if (string.IsNullOrWhiteSpace(offsetText))
                return false;

            // Expected formats: "+03:00", "-05:00"
            // TimeSpan.TryParse handles "hh:mm" but not leading '+', so normalize.
            offsetText = offsetText.Trim();

            if (offsetText.StartsWith("+"))
                offsetText = offsetText.Substring(1);

            if (TimeSpan.TryParse(offsetText, CultureInfo.InvariantCulture, out var ts))
            {
                // If original had '-', keep it negative; if it had '+', it’s already positive.
                if (offsetText.StartsWith("-", StringComparison.Ordinal))
                    offset = ts;   // negative already
                else
                    offset = ts;   // positive
                return true;
            }

            return false;
        }

        internal static IP2Location GetLocation(double lat, double lng)
        {
            try
            {
                using (RestClient client = new RestClient("https://cloud.gafware.com/Home"))
                {
                    RestRequest request = new RestRequest("GetLocationFromLatLng", Method.Post);
                    request.AddParameter("lat", lat);
                    request.AddParameter("lng", lng);
                    request.AddHeader("Accept-Language", "en-US");
                    RestResponse response = client.Execute(request);
                    if (response != null && response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                    {
                        var culture = new CultureInfo("en-US");
                        IP2Location? location = JsonConvert.DeserializeObject<IP2Location>(response.Content, new JsonSerializerSettings() { Culture = culture });
                        if (location != null)
                        {
                            return location;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Offline?
            }
            return null!;
        }

        internal static AircraftData LoadAircraft(string atcType, string atcModel)
        {
            try
            {
                RestClient client = new RestClient("https://cloud.gafware.com/Home");
                RestRequest request = new RestRequest("GetAircraft", Method.Post);
                request.AddParameter("type", atcType);
                request.AddParameter("model", atcModel);
                RestResponse response = client.Execute(request);
                if (response != null && response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    AircraftData? data = JsonConvert.DeserializeObject<AircraftData>(response.Content);
                    if (data != null)
                    {
                        return data;
                    }
                }
            }
            catch (Exception)
            {
                // Offline?
            }
            return null!;
        }

        internal static AircraftData LoadAircraft(string atcModel)
        {
            try
            {
                RestClient client = new RestClient("https://cloud.gafware.com/Home");
                RestRequest request = new RestRequest("GetAircraftByModel", Method.Post);
                request.AddParameter("model", atcModel);
                RestResponse response = client.Execute(request);
                if (response != null && response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    AircraftData? data = JsonConvert.DeserializeObject<AircraftData>(response.Content);
                    if (data != null)
                    {
                        return data;
                    }
                }
            }
            catch (Exception)
            {
                // Offline?
            }
            return DefaultAircraft;
        }


        internal static AircraftData LoadAircraft(int aircraftId)
        {
            try
            {
                RestClient client = new RestClient("https://cloud.gafware.com/Home");
                RestRequest request = new RestRequest("GetAircraftById", Method.Post);
                request.AddParameter("AircraftId", aircraftId);
                RestResponse response = client.Execute(request);
                if (response != null && response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    AircraftData? data = JsonConvert.DeserializeObject<AircraftData>(response.Content);
                    if (data != null)
                    {
                        return data;
                    }
                }
            }
            catch (Exception)
            {
                // Offline?
            }
            return DefaultAircraft;
        }

        public static AircraftData DefaultAircraft => new AircraftData()
        {
            AircraftId = 34,
            Name = "Textron Aviation Cessna 172 Skyhawk",
            FriendlyName = "Cessna 172 Skyhawk",
            FriendlyType = "CESSNA",
            FriendlyModel = "C172",
            EngineType = EngineType.Piston,
            Heavy = false,
            Helo = false
        };

        public static AircraftData GetDefaultAircraft(string atcType, string atcModel)
        {
            string cleanType = NormalizeAircraftToken(atcType, preferLastToken: false);
            string cleanModel = NormalizeAircraftToken(atcModel, preferLastToken: true);

            var aircraft = new AircraftData
            {
                ATCType = cleanType,
                ATCModel = cleanModel
            };

            aircraft.FriendlyType = aircraft.ATCType;
            aircraft.FriendlyModel = aircraft.ATCModel;

            // Avoid double spaces / empty pieces
            aircraft.Name = aircraft.FriendlyName =
                (string.IsNullOrWhiteSpace(aircraft.ATCType) && string.IsNullOrWhiteSpace(aircraft.ATCModel))
                    ? "Unknown Aircraft"
                    : $"{aircraft.ATCType} {aircraft.ATCModel}".Trim();

            return aircraft;
        }

        private static string NormalizeAircraftToken(string? raw, bool preferLastToken)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return string.Empty;

            string s = raw.Trim();

            // If something comes through like "ATCCOM.AC_MODEL C172.0.text"
            // strip leading namespace-ish words and keep the last "payloady" chunk.
            if (s.Contains(' '))
                s = preferLastToken ? s.Split(' ').Last() : s.Split(' ').FirstOrDefault() ?? s;

            // Strip "ATCCOM." prefix if it's actually in the string
            if (s.StartsWith("ATCCOM.", StringComparison.OrdinalIgnoreCase))
                s = s.Substring("ATCCOM.".Length);

            // New MSFS token pattern: BASE.INDEX.text  e.g. "C172.0.text", "CESSNA.0.text"
            s = StripMsfsLocalizationSuffix(s);

            // Legacy patterns: underscores, colons, weird packaging strings
            // Rule: take the most "name-like" token.
            // - For underscore strings, pick last segment that looks like text.
            if (s.Contains('_'))
            {
                var parts = s.Split('_', StringSplitOptions.RemoveEmptyEntries);
                s = parts.Length > 0 ? parts[^1] : s;
                s = StripMsfsLocalizationSuffix(s);
            }

            // If it contains colon, take right side (common in some formats)
            if (s.Contains(':'))
            {
                var parts = s.Split(':', StringSplitOptions.RemoveEmptyEntries);
                s = parts.Length > 0 ? parts[^1] : s;
                s = StripMsfsLocalizationSuffix(s);
            }

            // If it still contains dots but doesn't look like a token, keep the first chunk
            // (e.g., "Cessna.C172" -> "Cessna" or "C172" depending on your preference)
            // For mapping, usually keep the left-most stable chunk:
            if (s.Contains('.') && !s.EndsWith(".text", StringComparison.OrdinalIgnoreCase))
                s = s.Split('.', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? s;

            // Final cleanup
            s = s.Trim();
            return s;
        }

        private static string StripMsfsLocalizationSuffix(string s)
        {
            // BASE.INDEX.text
            // Keep BASE
            var m = System.Text.RegularExpressions.Regex.Match(
                s,
                @"^(?<base>[^.]+)\.(?<idx>\d+)\.text$",
                System.Text.RegularExpressions.RegexOptions.CultureInvariant | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (m.Success)
                return m.Groups["base"].Value;

            // BASE.text -> BASE
            if (s.EndsWith(".text", StringComparison.OrdinalIgnoreCase))
                return s[..^5];

            return s;
        }

        public readonly record struct PackSpec(
            double Scale = 1.0,          // multiply before rounding (e.g., 16.0 for fixed 16ths)
            double Offset = 0.0,         // add after scaling (rare, but handy)
            bool Signed = false,         // if true, encode as two's complement into uint
            uint Min = 0u,
            uint Max = uint.MaxValue
        );

        public static uint Pack(double value, PackSpec spec)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                value = 0.0;

            double scaled = (value * spec.Scale) + spec.Offset;

            // MidpointAwayFromZero matches what most “fixed” encodings expect.
            long n = (long)Math.Round(scaled, MidpointRounding.AwayFromZero);

            if (!spec.Signed)
            {
                if (n < 0) return spec.Min;
                ulong u = (ulong)n;
                if (u < spec.Min) return spec.Min;
                if (u > spec.Max) return spec.Max;
                return (uint)u;
            }
            else
            {
                // two's complement to uint
                // clamp *signed* range using Min/Max as raw uint limits after encoding
                unchecked
                {
                    uint u = (uint)n;
                    if (u < spec.Min) return spec.Min;
                    if (u > spec.Max) return spec.Max;
                    return u;
                }
            }
        }

        public static uint Pack(float value, PackSpec spec) => Pack((double)value, spec);
        
        public static uint Pack(int value, PackSpec spec) => Pack((double)value, spec);
        
        public static uint Pack(bool value, PackSpec spec) => Pack(value ? 1.0 : 0.0, spec);
    }
}
