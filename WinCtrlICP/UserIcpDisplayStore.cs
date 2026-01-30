using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace WinCtrlICP
{
    public static class UserIcpDisplayStore
    {
        private static string GetFolder()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gafware", "WinCtrlICP"
            );
        }

        private static string GetPath() => Path.Combine(GetFolder(), "displays.json");

        public static void Save(List<UserIcpDisplay> displays)
        {
            Directory.CreateDirectory(GetFolder());

            var json = JsonConvert.SerializeObject(displays, Formatting.Indented);
            File.WriteAllText(GetPath(), json);
        }

        public static List<UserIcpDisplay> Load()
        {
            var path = GetPath();
            if (!File.Exists(path))
                return new List<UserIcpDisplay>();

            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<List<UserIcpDisplay>>(json)
                   ?? new List<UserIcpDisplay>();
        }
    }
}