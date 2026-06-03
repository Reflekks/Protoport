using System;
using System.Drawing;
using System.Windows.Forms;

namespace Protoport;

public class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _trayIcon;
    private readonly ToolStripMenuItem _menuStatus;
    private readonly ToolStripMenuItem _menuLastChanged;
    private readonly StatusForm _statusForm;
    private readonly ProtonLogWatcher _watcher;

    private string _currentPort  = "Unknown";
    private string _previousPort = "Unknown";
    private DateTime? _lastChanged;

    public TrayApplicationContext()
    {
        _statusForm = new StatusForm();

        // --- Context menu ---
        _menuStatus      = new ToolStripMenuItem("Status: starting…") { Enabled = false };
        _menuLastChanged = new ToolStripMenuItem("Last change: never") { Enabled = false };

        var menu = new ContextMenuStrip();
        menu.Items.Add(_menuStatus);
        menu.Items.Add(_menuLastChanged);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Open Status Window", null, (_, _) => ShowStatus());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => ExitApp());

        // --- Tray icon ---
        _trayIcon = new NotifyIcon
        {
            Text    = "Protoport — monitoring…",
            Icon    = BuildIcon(),
            Visible = true,
            ContextMenuStrip = menu
        };
        _trayIcon.DoubleClick += (_, _) => ShowStatus();

        // --- Start watcher ---
        _watcher = new ProtonLogWatcher();
        _watcher.PortChanged += OnPortChanged;
        _watcher.Start();

        // Seed initial display after short delay (watcher seeds current port)
        var seedTimer = new System.Windows.Forms.Timer { Interval = 3000 };
        seedTimer.Tick += (_, _) =>
        {
            seedTimer.Stop();
            _currentPort = _watcher.CurrentPort;
            RefreshUi(null);
        };
        seedTimer.Start();
    }

    private void OnPortChanged(object? sender, PortChangedEventArgs e)
    {
        // Marshal to UI thread via StatusForm (always exists even when hidden)
        _statusForm.Invoke(() =>
        {
            _previousPort = e.OldPort;
            _currentPort  = e.NewPort;
            _lastChanged  = e.ChangedAt;

            _statusForm.AppendLog($"Port changed: {e.OldPort} → {e.NewPort}");

            // Close qBit, write new port, relaunch
            _statusForm.AppendLog("Closing qBittorrent, updating port, restarting…");
            string? error = QbitManager.UpdatePortAndRestart(e.NewPort);
            if (error != null)
            {
                _statusForm.AppendLog($"ERROR: {error}");
                ShowBalloon("Protoport — Error", error, ToolTipIcon.Error);
            }
            else
            {
                _statusForm.AppendLog("qBittorrent restarted successfully.");
                ShowBalloon("Protoport", $"Port updated: {e.OldPort} → {e.NewPort}", ToolTipIcon.Info);
            }

            RefreshUi(e);
        });
    }

    private void RefreshUi(PortChangedEventArgs? e)
    {
        string tooltip = $"Protoport\nPort: {_currentPort}";
        if (_lastChanged.HasValue)
            tooltip += $"\nLast change: {_lastChanged:HH:mm:ss}";
        _trayIcon.Text = tooltip.Length > 63 ? tooltip[..63] : tooltip;

        _menuStatus.Text      = $"Port: {_currentPort}";
        _menuLastChanged.Text = _lastChanged.HasValue
            ? $"Last change: {_lastChanged:dd MMM HH:mm:ss}"
            : "Last change: never";

        _statusForm.UpdateStatus(_currentPort, _previousPort, _lastChanged, _watcher.LogStatus);
    }

    private void ShowStatus()
    {
        // Refresh before showing
        RefreshUi(null);
        _statusForm.Show();
        _statusForm.BringToFront();
    }

    private void ShowBalloon(string title, string text, ToolTipIcon icon)
    {
        _trayIcon.BalloonTipTitle = title;
        _trayIcon.BalloonTipText  = text;
        _trayIcon.BalloonTipIcon  = icon;
        _trayIcon.ShowBalloonTip(4000);
    }

    private void ExitApp()
    {
        _watcher.Stop();
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        Environment.Exit(0);
    }

    /// <summary>
    /// Generates a simple programmatic icon (green shield) so no .ico file is required.
    /// Replace with a real icon file if desired.
    /// </summary>
    private static Icon BuildIcon()
    {
        using var bmp = new Bitmap(16, 16);
        using var g   = Graphics.FromImage(bmp);
        g.Clear(Color.Transparent);

        // Shield shape
        using var brush = new SolidBrush(Color.FromArgb(0, 200, 100));
        g.FillPolygon(brush, new Point[]
        {
            new(8, 1), new(15, 4), new(15, 10), new(8, 15), new(1, 10), new(1, 4)
        });

        // "Q" hint
        using var font  = new Font("Arial", 6f, FontStyle.Bold);
        using var white = new SolidBrush(Color.White);
        g.DrawString("Q", font, white, 3.5f, 3.5f);

        var hIcon = bmp.GetHicon();
        return Icon.FromHandle(hIcon);
    }
}
