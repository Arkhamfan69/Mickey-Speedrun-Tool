using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MickeySpeedrunTool.Core
{
    public static class ILManager
    {
        private static readonly Dictionary<string, string> ILMap = new()
        {
            { "Gremlin Village", "01_GremlinVillage" },
            { "Mickeyjunk Mountain", "02_MickeyjunkMountain" },
            { "Tomorrow City", "03_TomorrowCity" },
            { "Pirates of the Wasteland", "04_PiratesWasteland" },
            { "Lonesome Manor", "05_LonesomeManor" },
            { "Bloticles", "06_Bloticles" },
            { "Dark Beauty Castle 2", "07_DarkBeautyCastle2" },
            { "Inside the Blot", "08_InsideTheBlot" }
        };

        public static void ListILs()
        {
            Console.WriteLine("Available ILs:");
            int index = 1;
            foreach (var il in ILMap.Keys)
            {
                Console.WriteLine($"{index}. {il}");
                index++;
            }
        }

        public static IReadOnlyCollection<string> GetAvailableILNames()
        {
            return ILMap.Keys.OrderBy(name => name).ToList().AsReadOnly();
        }

        public static string? GetSaveFolder(string ilName)
        {
            var rootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Save Files");
            var bundledPath = Path.Combine(rootPath, "Any%", ilName);
            if (Directory.Exists(bundledPath))
            {
                return FindDirectoryWithSaveFiles(bundledPath);
            }

            if (ILMap.TryGetValue(ilName, out var folderName))
            {
                var mappedPath = Path.Combine(rootPath, folderName);
                if (Directory.Exists(mappedPath))
                {
                    return FindDirectoryWithSaveFiles(mappedPath);
                }
            }

            return null;
        }

        private static string? FindDirectoryWithSaveFiles(string rootPath)
        {
            if (Directory.GetFiles(rootPath, "*.sav").Length > 0)
            {
                return rootPath;
            }

            foreach (var directory in Directory.GetDirectories(rootPath, "*", SearchOption.AllDirectories))
            {
                if (Directory.GetFiles(directory, "*.sav").Length > 0)
                {
                    return directory;
                }
            }

            return null;
        }

        public static void LoadIL(string ilName)
        {
            string? folder = GetSaveFolder(ilName);
            if (folder == null)
            {
                Console.WriteLine($"IL '{ilName}' not found.");
                return;
            }
            if (SaveSwapper.LoadState(folder, out var message))
            {
                Console.WriteLine(message);
            }
            else
            {
                Console.WriteLine("Failed to load IL state: " + message);
            }
        }

        public static void SaveIL(string ilName)
        {
            string? folder = GetSaveFolder(ilName);
            if (folder == null)
            {
                Console.WriteLine($"IL '{ilName}' not found.");
                return;
            }
            if (SaveSwapper.SaveState(folder, out var message))
            {
                Console.WriteLine(message);
            }
            else
            {
                Console.WriteLine("Failed to save IL state: " + message);
            }
        }
    }
}