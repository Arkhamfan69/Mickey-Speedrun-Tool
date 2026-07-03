using System;
using System.IO;
using System.Linq;
using MickeySpeedrunTool.Core;

namespace MickeySpeedrunTool.Core
{
    public static class SaveSwapper
    {
        private static string _gameSaveFolder = string.Empty;
        private static readonly string _baseSavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "recolored", "Saved");
        private static readonly string _statesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ILS", "States");
        private static readonly string _backupFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ILS", "Backup");

        public static string GameSaveFolder => _gameSaveFolder;

        public static bool Initialize()
        {
            if (!string.IsNullOrWhiteSpace(SettingsStore.Current.GameSavePath) && TryConfigureManualSavePath(SettingsStore.Current.GameSavePath))
            {
                return true;
            }

            return TryAutoDetectSaveFolder();
        }

        private static bool TryAutoDetectSaveFolder()
        {
            if (!Directory.Exists(_baseSavePath))
            {
                return false;
            }

            var userDir = Directory.GetDirectories(_baseSavePath).FirstOrDefault(d => Path.GetFileName(d).All(char.IsDigit));
            if (userDir == null)
            {
                return false;
            }

            var candidate = Path.Combine(userDir, "SaveGames");
            if (!Directory.Exists(candidate))
            {
                return false;
            }

            _gameSaveFolder = candidate;
            Directory.CreateDirectory(_statesFolder);
            Directory.CreateDirectory(_backupFolder);
            return true;
        }

        public static bool TryConfigureManualSavePath(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
            {
                return false;
            }

            if (Directory.GetFiles(folder, "*.sav").Length == 0)
            {
                return false;
            }

            _gameSaveFolder = folder;
            Directory.CreateDirectory(_statesFolder);
            Directory.CreateDirectory(_backupFolder);
            return true;
        }

        public static bool SaveState(string stateName, out string message)
        {
            if (!ValidateGameFolder())
            {
                message = "Save folder is not configured.";
                return false;
            }

            var path = Path.Combine(_statesFolder, stateName);
            Directory.CreateDirectory(path);
            CopySavFiles(_gameSaveFolder, path);
            message = "Saved successfully.";
            return true;
        }

        public static bool LoadState(string stateName, out string message)
        {
            var path = Path.IsPathRooted(stateName) ? stateName : Path.Combine(_statesFolder, stateName);
            if (!ValidateState(path))
            {
                message = "Save state is not available.";
                return false;
            }

            BackupCurrent();
            ClearSavFiles(_gameSaveFolder);
            CopySavFiles(path, _gameSaveFolder);
            message = "Loaded successfully.";
            return true;
        }

        public static bool RestoreBackup(out string message)
        {
            if (!ValidateState(_backupFolder))
            {
                message = "No backup is available.";
                return false;
            }

            ClearSavFiles(_gameSaveFolder);
            CopySavFiles(_backupFolder, _gameSaveFolder);
            message = "Backup restored successfully.";
            return true;
        }

        public static bool LoadPresetFromFolder(string sourceFolder, out string message)
        {
            if (string.IsNullOrWhiteSpace(sourceFolder) || !Directory.Exists(sourceFolder))
            {
                message = "Preset folder is not available.";
                return false;
            }

            if (!ValidateGameFolder())
            {
                message = "Save folder is not configured.";
                return false;
            }

            if (Directory.GetFiles(sourceFolder, "*.sav", SearchOption.AllDirectories).Length == 0)
            {
                message = "Preset folder does not contain any save files.";
                return false;
            }

            BackupCurrent();
            ClearSavFiles(_gameSaveFolder);
            CopySavFiles(sourceFolder, _gameSaveFolder);
            message = "Preset loaded successfully.";
            return true;
        }

        private static void BackupCurrent()
        {
            try
            {
                ClearDirectory(_backupFolder);
                CopySavFiles(_gameSaveFolder, _backupFolder);
            }
            catch
            {
            }
        }

        private static void CopySavFiles(string source, string dest)
        {
            if (!Directory.Exists(dest))
            {
                Directory.CreateDirectory(dest);
            }

            foreach (var file in Directory.GetFiles(source, "*.sav", SearchOption.AllDirectories))
            {
                var target = Path.Combine(dest, Path.GetFileName(file));
                File.Copy(file, target, true);
            }
        }

        private static void ClearSavFiles(string path)
        {
            foreach (var file in Directory.GetFiles(path, "*.sav"))
            {
                File.Delete(file);
            }
        }

        private static void ClearDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                return;
            }

            foreach (var file in Directory.GetFiles(path, "*.sav", SearchOption.AllDirectories))
            {
                File.Delete(file);
            }
        }

        private static bool ValidateState(string path)
        {
            if (!Directory.Exists(path))
            {
                return false;
            }

            return Directory.GetFiles(path, "*.sav", SearchOption.AllDirectories).Length > 0;
        }

        private static bool ValidateGameFolder()
        {
            if (string.IsNullOrWhiteSpace(_gameSaveFolder) || !Directory.Exists(_gameSaveFolder))
            {
                return false;
            }

            return Directory.GetFiles(_gameSaveFolder, "*.sav", SearchOption.AllDirectories).Length > 0;
        }
    }
}