using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace Protoport;

public static class QbitManager
{
    private static readonly Regex PortLineRegex =
        new(@"(Session\\Port\s*=\s*)\d+", RegexOptions.Compiled);

    /// <summary>
    /// Closes qBittorrent, writes the new port to the ini, then relaunches.
    /// Returns null on success, or an error message.
    /// </summary>
    public static string? UpdatePortAndRestart(string port)
    {
        try
        {
            // 1. Close qBittorrent first so it can't overwrite the ini on shutdown
            var procs = Process.GetProcessesByName("qbittorrent");
            foreach (var proc in procs)
            {
                proc.CloseMainWindow();
                if (!proc.WaitForExit(15_000))
                    proc.Kill();
                proc.Dispose();
            }

            // 2. Brief pause to let file handles release
            Thread.Sleep(AppConfig.RestartDelayMs);

            // 3. Now write the new port to the ini
            string? iniError = SetPort(port);
            if (iniError != null)
                return iniError;

            // 4. Relaunch
            if (!File.Exists(AppConfig.QbitExePath))
                return $"qBittorrent executable not found at: {AppConfig.QbitExePath}";

            Process.Start(new ProcessStartInfo
            {
                FileName = AppConfig.QbitExePath,
                UseShellExecute = true
            });

            return null;
        }
        catch (Exception ex)
        {
            return $"Failed to restart qBittorrent: {ex.Message}";
        }
    }

    private static string? SetPort(string port)
    {
        if (!File.Exists(AppConfig.QbitIniPath))
            return $"qBittorrent.ini not found at: {AppConfig.QbitIniPath}";

        try
        {
            string content = File.ReadAllText(AppConfig.QbitIniPath);

            if (!PortLineRegex.IsMatch(content))
                return "Session\\Port not found in qBittorrent.ini";

            string updated = PortLineRegex.Replace(content, $"${{1}}{port}");
            File.WriteAllText(AppConfig.QbitIniPath, updated);
            return null;
        }
        catch (Exception ex)
        {
            return $"Failed to update ini: {ex.Message}";
        }
    }
}
