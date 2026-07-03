using System;
using System.IO;

namespace MickeySpeedrunTool.Core
{
    public static class FolderManager
    {
        public static string BasePath { get; private set; } = "";
        public static string SavesPath { get; private set; } = "";
        public static string LogsPath { get; private set; } = "";
        public static string UE4SSPath { get; private set; } = "";

        public static void Initialize()
        {
            BasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MickeySpeedrunTool");

            SavesPath = Path.Combine(BasePath, "Save Files");
            LogsPath = Path.Combine(BasePath, "Logs");
            UE4SSPath = Path.Combine(BasePath, "UE4SS");

            Directory.CreateDirectory(SavesPath);
            Directory.CreateDirectory(LogsPath);
            Directory.CreateDirectory(UE4SSPath);
            Directory.CreateDirectory(BasePath);
        }
    }
}