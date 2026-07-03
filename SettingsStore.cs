using System;
using System.IO;
using System.Text.Json;

namespace MickeySpeedrunTool.Core
{
    public sealed class TrickStats
    {
        public int SuccessCount { get; set; }
        public int FailCount { get; set; }
    }

    public sealed class AppSettings
    {
        public string? GameSavePath { get; set; }
        public string? GameExePath { get; set; }
        public string? Ue4ssTargetPath { get; set; }
        public Dictionary<string, TrickStats> TrickHistory { get; set; } = new();
    }

    public static class SettingsStore
    {
        private static readonly string ConfigFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MickeySpeedrunTool");

        private static readonly string SettingsFile = Path.Combine(ConfigFolder, "settings.json");

        public static AppSettings Current { get; private set; } = new AppSettings();

        public static void Initialize()
        {
            Directory.CreateDirectory(ConfigFolder);
            Load();
        }

        public static void Save()
        {
            Directory.CreateDirectory(ConfigFolder);
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(SettingsFile, JsonSerializer.Serialize(Current, options));
        }

        private static void Load()
        {
            if (!File.Exists(SettingsFile))
            {
                Current = new AppSettings();
                return;
            }

            try
            {
                var json = File.ReadAllText(SettingsFile);
                Current = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            catch
            {
                Current = new AppSettings();
            }
        }
    }
}
