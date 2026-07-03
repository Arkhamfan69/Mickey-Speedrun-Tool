using System;
using System.IO;
using System.Linq;
using MickeySpeedrunTool.Core;

namespace MickeySpeedrunTool.Core
{
    public static class UE4SSInstaller
    {
        private static readonly string DefaultTargetPath = @"C:\Program Files (x86)\Steam\steamapps\common\Disney Epic Mickey Rebrushed\recolored\Binaries\Win64";
        public static string SourcePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UE4SS");
        public static bool SourceExists => Directory.Exists(SourcePath);

        public static string GetRecommendedTargetPath(string? gameExePath)
        {
            if (!string.IsNullOrWhiteSpace(gameExePath) && SteamSupport.TryGetGameRoot(gameExePath, out var gameRoot))
            {
                return Path.Combine(gameRoot, "recolored", "Binaries", "Win64");
            }

            return DefaultTargetPath;
        }

        public static bool IsInstalled(string targetPath)
        {
            if (string.IsNullOrWhiteSpace(targetPath) || !Directory.Exists(targetPath))
            {
                return false;
            }

            var expectedDll = Path.Combine(targetPath, "UE4SS.dll");
            return File.Exists(expectedDll);
        }

        public static bool Install()
        {
            return Install(DefaultTargetPath, SourcePath);
        }

        public static bool Install(string targetPath, string sourcePath)
        {
            return Install(targetPath, sourcePath, out _);
        }

        public static bool Install(string targetPath, string sourcePath, out string message)
        {
            if (!Directory.Exists(sourcePath))
            {
                message = "UE4SS source folder is missing.";
                return false;
            }

            try
            {
                Directory.CreateDirectory(targetPath);
                CopyAll(sourcePath, targetPath);
                message = "UE4SS installed successfully.";
                return true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
        }

        public static bool Uninstall()
        {
            return Uninstall(DefaultTargetPath, out _);
        }

        public static bool Uninstall(string targetPath, out string message)
        {
            if (string.IsNullOrWhiteSpace(targetPath) || !Directory.Exists(targetPath))
            {
                message = "Target installation path does not exist.";
                return false;
            }

            try
            {
                if (SourceExists)
                {
                    DeleteMatching(SourcePath, targetPath);
                }
                else
                {
                    DeleteKnownUe4ssFiles(targetPath);
                }

                message = "UE4SS uninstalled successfully.";
                return true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
        }

        private static void CopyAll(string source, string destination)
        {
            Directory.CreateDirectory(destination);
            foreach (var file in Directory.GetFiles(source))
            {
                var destFile = Path.Combine(destination, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }
            foreach (var dir in Directory.GetDirectories(source))
            {
                var destDir = Path.Combine(destination, Path.GetFileName(dir));
                CopyAll(dir, destDir);
            }
        }

        private static void DeleteAllFiles(string path)
        {
            foreach (var file in Directory.GetFiles(path))
            {
                File.Delete(file);
            }
            foreach (var dir in Directory.GetDirectories(path))
            {
                DeleteAllFiles(dir);
                Directory.Delete(dir);
            }
        }

        private static void DeleteMatching(string sourcePath, string targetPath)
        {
            foreach (var sourceFile in Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(sourcePath, sourceFile);
                var targetFile = Path.Combine(targetPath, relative);
                if (File.Exists(targetFile))
                {
                    File.Delete(targetFile);
                }
            }

            foreach (var sourceDir in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                var relativeDir = Path.GetRelativePath(sourcePath, sourceDir);
                var targetDir = Path.Combine(targetPath, relativeDir);
                if (Directory.Exists(targetDir) && !Directory.EnumerateFileSystemEntries(targetDir).Any())
                {
                    Directory.Delete(targetDir, false);
                }
            }
        }

        private static void DeleteKnownUe4ssFiles(string targetPath)
        {
            var knownFiles = new[]
            {
                "UE4SS.dll",
                "dxgi.dll",
                "UE4SS.ini",
                "UE4SS.ini.bak",
                "UE4SS.ini.backup"
            };

            foreach (var fileName in knownFiles)
            {
                var filePath = Path.Combine(targetPath, fileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }
    }
}