using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace FlightSim
{
    internal static class DCSLuaBridgeInstaller
    {
        // Folder name in Saved Games\DCS*\Scripts\
        public const string BridgeFolderName = "FlightSimBridge";

        // Marker file inside the installed folder so we know it's "ours"
        private const string MarkerFileName = "fsbridge_manifest.json";

        // Embedded resource zip name (set this to your actual resource name)
        // Example: "FlightSim.Resources.FlightSimBridge.zip"
        private const string EmbeddedZipResourceName = "FlightSim.FlightSimBridge.zip";

        // Avoid double-install if multiple providers touch this simultaneously
        private static int _installAttempted = 0;

        /// <summary>
        /// Ensures the FlightSimBridge Lua scripts are installed under each detected DCS Saved Games folder.
        /// Returns true if at least one DCS root was found (installed or already present), false if DCS not found.
        /// Never throws for "DCS not installed".
        /// </summary>
        public static bool EnsureInstalled()
        {
            // Run once per process
            if (Interlocked.Exchange(ref _installAttempted, 1) == 1)
                return DCSRootsExist();

            var roots = GetDCSSavedGamesRoots();
            if (roots.Length == 0)
                return false; // DCS not installed

            foreach (var dcsRoot in roots)
            {
                try
                {
                    EnsureInstalledForRoot(dcsRoot);
                }
                catch
                {
                    // Intentionally swallow: we don't want the whole app to die.
                    // Optional: Debug.WriteLine(ex);
                }
            }

            return true;
        }

        private static void EnsureInstalledForRoot(string dcsRoot)
        {
            var scriptsDir = Path.Combine(dcsRoot, "Scripts");
            var targetDir = Path.Combine(scriptsDir, BridgeFolderName);
            var markerPath = Path.Combine(targetDir, MarkerFileName);

            // If our marker exists, assume installed
            if (Directory.Exists(targetDir) && File.Exists(markerPath))
                return;

            Directory.CreateDirectory(targetDir);

            // Extract embedded zip into targetDir
            ExtractEmbeddedZipToDirectory(EmbeddedZipResourceName, targetDir);

            // Optional: if zip didn’t include a marker, create one now
            if (!File.Exists(markerPath))
            {
                File.WriteAllText(markerPath,
                    "{ \"name\": \"FlightSimBridge\", \"version\": \"1.0.0\" }");
            }
        }

        private static void ExtractEmbeddedZipToDirectory(string resourceName, string targetDir)
        {
            var asm = Assembly.GetExecutingAssembly();
            using var zipStream = asm.GetManifestResourceStream(resourceName);
            if (zipStream == null)
                return; // Don't throw; keep provider inert if resource missing in some build

            using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);

            foreach (var entry in archive.Entries)
            {
                // Skip directory entries
                if (string.IsNullOrEmpty(entry.Name))
                    continue;

                // Entry paths are normalized, prevent zip-slip
                var entryPath = entry.FullName.Replace('\\', '/');

                // If your zip contains a top-level "FlightSimBridge/" folder, strip it so we extract into targetDir.
                entryPath = StripLeadingFolder(entryPath, BridgeFolderName);

                var destinationPath = Path.GetFullPath(Path.Combine(targetDir, entryPath));
                var targetFullPath = Path.GetFullPath(targetDir);

                if (!destinationPath.StartsWith(targetFullPath, StringComparison.OrdinalIgnoreCase))
                    continue; // blocked zip-slip

                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
                entry.ExtractToFile(destinationPath, overwrite: true);
            }
        }

        private static string StripLeadingFolder(string entryPath, string folderName)
        {
            var prefix = folderName.TrimEnd('/') + "/";
            if (entryPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return entryPath.Substring(prefix.Length);
            return entryPath;
        }

        public static string? GetPreferredDCSSavedGamesRoot()
        {
            return GetDCSSavedGamesRoots().FirstOrDefault();
        }

        private static string[] GetDCSSavedGamesRoots()
        {
            var savedGames = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Saved Games"
            );

            // Probe known variants; only keep those that exist
            var candidates = new[]
            {
                Path.Combine(savedGames, "DCS"),
                Path.Combine(savedGames, "DCS.openbeta"),
            };

            return candidates.Where(Directory.Exists).ToArray();
        }

        private static bool DCSRootsExist() => GetDCSSavedGamesRoots().Length > 0;
    }
}
