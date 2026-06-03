using System;
using System.IO;

namespace Protoport;

public static class AppConfig
{
    // ProtonVPN log file
    public static string ProtonLogPath { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Proton", "Proton VPN", "Logs", "client-logs.txt");

    // qBittorrent config file
    public static string QbitIniPath { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "qBittorrent", "qBittorrent.ini");

    // qBittorrent executable — user install location first, then system-wide
    public static string QbitExePath { get; } = FindQbit();

    // How often (ms) to check the log for new content
    public static int PollIntervalMs { get; } = 15_000;

    // Grace period (ms) between killing and restarting qBit
    public static int RestartDelayMs { get; } = 4_000;

    private static string FindQbit()
    {
        string userPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Programs", "qBittorrent", "qbittorrent.exe");
        if (File.Exists(userPath)) return userPath;

        string systemPath = @"C:\Program Files\qBittorrent\qbittorrent.exe";
        if (File.Exists(systemPath)) return systemPath;

        return userPath; // Fall back; will surface an error when restart is attempted
    }
}
