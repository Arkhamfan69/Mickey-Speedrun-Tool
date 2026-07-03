using System;
using System.Diagnostics;
using System.IO;
using Timer = System.Timers.Timer;

namespace MickeySpeedrunTool.Core
{
    public class GameManager
    {
        private const string DefaultProcessName = "recolored-Win64-Shipping";

        public Process? GameProcess { get; private set; }
        public string GamePath { get; private set; } = string.Empty;
        public bool IsGameRunning => GameProcess != null && !GameProcess.HasExited;

        public event Action? OnGameDetected;
        public event Action? OnGameClosed;
        private Timer? refreshTimer;

        public GameManager()
        {
            SetupRefreshTimer();
        }

        #region Detection

        public bool DetectGame(string? processName = null)
        {
            processName ??= DefaultProcessName;
            var processes = Process.GetProcessesByName(processName);

            if (processes.Length > 0)
            {
                GameProcess = processes[0];
                GamePath = GetGamePath(GameProcess);
                OnGameDetected?.Invoke();
                return true;
            }

            GameProcess = null;
            GamePath = string.Empty;
            return false;
        }

        public bool RequireGame()
        {
            if (!IsGameRunning)
            {
                return DetectGame();
            }

            return true;
        }

        private string GetGamePath(Process process)
        {
            try
            {
                return process.MainModule?.FileName ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        #endregion

        #region Launch

        public void LaunchGame(string gameExePath)
        {
            LaunchGame(gameExePath, out _);
        }

        public bool LaunchGame(string gameExePath, out string message)
        {
            if (IsGameRunning)
            {
                message = "Game is already running.";
                return false;
            }

            if (SteamSupport.TryLaunchViaSteam(gameExePath, out message))
            {
                GameProcess = null;
                GamePath = gameExePath;
                return true;
            }

            try
            {
                Process.Start(gameExePath);
                GameProcess = null;
                GamePath = gameExePath;
                message = "Launched directly because Steam launch was unavailable.";
                return true;
            }
            catch (Exception ex)
            {
                message = $"Failed to launch game: {ex.Message}";
                throw new InvalidOperationException(message);
            }
        }

        public bool CloseGame(out string message, int timeoutMs = 5000)
        {
            if (!IsGameRunning)
            {
                message = "Game is not running.";
                return true;
            }

            try
            {
                if (GameProcess.CloseMainWindow())
                {
                    if (!GameProcess.WaitForExit(timeoutMs))
                    {
                        GameProcess.Kill(true);
                        GameProcess.WaitForExit(timeoutMs);
                    }
                }
                else
                {
                    GameProcess.Kill(true);
                    GameProcess.WaitForExit(timeoutMs);
                }

                GameProcess = null;
                GamePath = string.Empty;
                message = "Game closed successfully.";
                return true;
            }
            catch (Exception ex)
            {
                message = $"Failed to close game: {ex.Message}";
                return false;
            }
        }

        #endregion

        #region Refresh Timer

        private void SetupRefreshTimer()
        {
            refreshTimer = new Timer(1000);
            refreshTimer.Elapsed += (s, e) => Refresh();
            refreshTimer.Start();
        }

        public void Refresh()
        {
            if (GameProcess != null && GameProcess.HasExited)
            {
                OnGameClosed?.Invoke();
                GameProcess = null;
                GamePath = string.Empty;
            }
        }

        #endregion

        #region Logging

        public void LogStatus()
        {
            Console.WriteLine("===== Game Manager Status =====");
            Console.WriteLine($"Game Running: {IsGameRunning}");
            Console.WriteLine($"Game Path: {GamePath}");
            Console.WriteLine($"PID: {GameProcess?.Id}");
            Console.WriteLine("================================");
        }

        #endregion

        #region Safety Checks

        public void RequireGameOrThrow()
        {
            if (!RequireGame())
            {
                throw new InvalidOperationException("Game is not running.");
            }
        }

        #endregion

        #region Helpers

        public void OpenGameFolder()
        {
            if (!string.IsNullOrEmpty(GamePath))
            {
                var folder = Path.GetDirectoryName(GamePath);
                if (folder != null && Directory.Exists(folder))
                {
                    Process.Start("explorer.exe", folder);
                }
            }
        }

        #endregion
    }
}