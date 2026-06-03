using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Protoport;

public class PortChangedEventArgs(string oldPort, string newPort) : EventArgs
{
    public string OldPort { get; } = oldPort;
    public string NewPort { get; } = newPort;
    public DateTime ChangedAt { get; } = DateTime.Now;
}

public class ProtonLogWatcher
{
    // Matches: Port pair 12345->12345
    private static readonly Regex PortRegex = new(@"Port pair \d+->(\d+)", RegexOptions.Compiled);

    public event EventHandler<PortChangedEventArgs>? PortChanged;

    public string CurrentPort { get; private set; } = "Unknown";
    public string LogStatus   { get; private set; } = "Initialising…";

    private CancellationTokenSource? _cts;

    public void Start()
    {
        _cts = new CancellationTokenSource();
        Task.Run(() => WatchLoop(_cts.Token));
    }

    public void Stop() => _cts?.Cancel();

    private async Task WatchLoop(CancellationToken ct)
    {
        LogStatus = "Waiting for ProtonVPN log…";

        // Wait for the log file to exist (ProtonVPN may not have started yet)
        while (!File.Exists(AppConfig.ProtonLogPath) && !ct.IsCancellationRequested)
        {
            await Task.Delay(AppConfig.PollIntervalMs, ct).ContinueWith(_ => { });
        }

        if (ct.IsCancellationRequested) return;

        // Seed current port from the most recent Port pair line in the existing log
        CurrentPort = SeedPortFromLog() ?? "Unknown";
        LogStatus = $"Monitoring (current port: {CurrentPort})";

        long lastSize = new FileInfo(AppConfig.ProtonLogPath).Length;

        while (!ct.IsCancellationRequested)
        {
            await Task.Delay(AppConfig.PollIntervalMs, ct).ContinueWith(_ => { });

            if (ct.IsCancellationRequested) break;

            if (!File.Exists(AppConfig.ProtonLogPath))
            {
                LogStatus = "Log file missing — waiting…";
                lastSize = 0;
                continue;
            }

            long currentSize = new FileInfo(AppConfig.ProtonLogPath).Length;

            if (currentSize < lastSize)
            {
                // Log rotated
                LogStatus = "Log rotated — re-reading…";
                lastSize = 0;
            }

            if (currentSize == lastSize) continue;

            // Read only new bytes
            string[] newLines = ReadNewLines(AppConfig.ProtonLogPath, lastSize);
            lastSize = currentSize;

            foreach (string line in newLines)
            {
                var match = PortRegex.Match(line);
                if (!match.Success) continue;

                string port = match.Groups[1].Value;
                if (port == CurrentPort) continue;

                string old = CurrentPort;
                CurrentPort = port;
                LogStatus = $"Monitoring (current port: {CurrentPort})";
                PortChanged?.Invoke(this, new PortChangedEventArgs(old, port));
            }
        }
    }

    private string? SeedPortFromLog()
    {
        try
        {
            // Read log lines in reverse to find the most recent Port pair quickly
            using var fs = new FileStream(AppConfig.ProtonLogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(fs);
            string? lastMatch = null;
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                var m = PortRegex.Match(line);
                if (m.Success) lastMatch = m.Groups[1].Value;
            }
            return lastMatch;
        }
        catch { return null; }
    }

    private static string[] ReadNewLines(string path, long fromOffset)
    {
        try
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            fs.Seek(fromOffset, SeekOrigin.Begin);
            using var reader = new StreamReader(fs);
            var lines = new System.Collections.Generic.List<string>();
            string? line;
            while ((line = reader.ReadLine()) != null)
                lines.Add(line);
            return [.. lines];
        }
        catch { return []; }
    }
}
