using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MickeySpeedrunTool.Core
{
    internal static class SteamSupport
    {
        private static readonly string[] SteamRootCandidates =
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Steam"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Steam")
        };

        public static bool TryLaunchViaSteam(string gameExePath, out string message)
        {
            if (!TryFindAppId(gameExePath, out var appId))
            {
                message = "Could not identify the Steam app for the selected game path.";
                return false;
            }

            try
            {
                Process.Start(new ProcessStartInfo($"steam://rungameid/{appId}")
                {
                    UseShellExecute = true
                });
                message = "Launching through Steam.";
                return true;
            }
            catch (Exception ex)
            {
                message = $"Failed to start Steam: {ex.Message}";
                return false;
            }
        }

        public static bool TryFindAppId(string gameExePath, out string appId)
        {
            appId = string.Empty;

            if (!TryGetInstallDirectory(gameExePath, out var installDirectory))
            {
                return false;
            }

            foreach (var manifestPath in GetManifestPaths())
            {
                if (!TryReadManifest(manifestPath, out var manifestName, out var manifestInstallDir))
                {
                    continue;
                }

                if (string.Equals(manifestInstallDir, installDirectory, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(manifestName, installDirectory, StringComparison.OrdinalIgnoreCase))
                {
                    appId = Path.GetFileNameWithoutExtension(manifestPath).Replace("appmanifest_", string.Empty);
                    return !string.IsNullOrWhiteSpace(appId);
                }
            }

            return false;
        }

        public static bool TryGetInstallDirectory(string gameExePath, out string installDirectory)
        {
            installDirectory = string.Empty;

            if (string.IsNullOrWhiteSpace(gameExePath) || !File.Exists(gameExePath))
            {
                return false;
            }

            var currentDirectory = Path.GetDirectoryName(gameExePath);
            while (!string.IsNullOrWhiteSpace(currentDirectory))
            {
                var parentDirectory = Path.GetDirectoryName(currentDirectory);
                if (parentDirectory != null && string.Equals(Path.GetFileName(parentDirectory), "common", StringComparison.OrdinalIgnoreCase))
                {
                    installDirectory = Path.GetFileName(currentDirectory) ?? string.Empty;
                    return !string.IsNullOrWhiteSpace(installDirectory);
                }

                currentDirectory = parentDirectory;
            }

            return false;
        }

        public static bool TryGetGameRoot(string gameExePath, out string gameRoot)
        {
            gameRoot = string.Empty;

            if (!TryGetInstallDirectory(gameExePath, out var installDirectory))
            {
                return false;
            }

            foreach (var steamRoot in GetSteamRoots())
            {
                var candidateRoot = Path.Combine(steamRoot, "steamapps", "common", installDirectory);
                if (Directory.Exists(candidateRoot))
                {
                    gameRoot = candidateRoot;
                    return true;
                }
            }

            return false;
        }

        private static IEnumerable<string> GetSteamRoots()
        {
            var roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var candidate in SteamRootCandidates)
            {
                if (Directory.Exists(candidate))
                {
                    roots.Add(candidate);
                }
            }

            foreach (var steamRoot in roots.ToArray())
            {
                var libraryFoldersFile = Path.Combine(steamRoot, "steamapps", "libraryfolders.vdf");
                if (!File.Exists(libraryFoldersFile))
                {
                    continue;
                }

                foreach (var line in File.ReadLines(libraryFoldersFile))
                {
                    var match = Regex.Match(line, "\"path\"\\s+\"(?<path>[^\"]+)\"");
                    if (!match.Success)
                    {
                        continue;
                    }

                    var libraryRoot = match.Groups["path"].Value;
                    if (Directory.Exists(libraryRoot))
                    {
                        roots.Add(libraryRoot);
                    }
                }
            }

            return roots;
        }

        private static IEnumerable<string> GetManifestPaths()
        {
            foreach (var steamRoot in GetSteamRoots())
            {
                var steamAppsFolder = Path.Combine(steamRoot, "steamapps");
                if (!Directory.Exists(steamAppsFolder))
                {
                    continue;
                }

                foreach (var manifestPath in Directory.GetFiles(steamAppsFolder, "appmanifest_*.acf"))
                {
                    yield return manifestPath;
                }
            }
        }

        private static bool TryReadManifest(string manifestPath, out string name, out string installDir)
        {
            name = string.Empty;
            installDir = string.Empty;

            foreach (var line in File.ReadLines(manifestPath))
            {
                var match = Regex.Match(line, "\"(?<key>[^\"]+)\"\\s+\"(?<value>[^\"]*)\"");
                if (!match.Success)
                {
                    continue;
                }

                var key = match.Groups["key"].Value;
                var value = match.Groups["value"].Value;

                if (string.Equals(key, "name", StringComparison.OrdinalIgnoreCase))
                {
                    name = value;
                }
                else if (string.Equals(key, "installdir", StringComparison.OrdinalIgnoreCase))
                {
                    installDir = value;
                }

                if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(installDir))
                {
                    return true;
                }
            }

            return !string.IsNullOrWhiteSpace(installDir);
        }
    }
}