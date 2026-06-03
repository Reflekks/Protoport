using System;
using System.Drawing;
using System.Windows.Forms;

namespace Protoport;

public class StatusForm : Form
{
    private readonly Label _lblCurrentPort;
    private readonly Label _lblLastChanged;
    private readonly Label _lblChangedAt;
    private readonly Label _lblLogStatus;
    private readonly RichTextBox _rtbLog;

    public StatusForm()
    {
        Text = "Protoport — Status";
        Size = new Size(420, 340);
        MinimumSize = new Size(420, 340);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        BackColor = Color.FromArgb(30, 30, 30);
        ForeColor = Color.WhiteSmoke;

        // --- Layout ---
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            RowCount = 6,
            ColumnCount = 2,
            BackColor = Color.Transparent,
            AutoSize = false
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        _lblCurrentPort = MakeValueLabel("Unknown");
        _lblLastChanged = MakeValueLabel("Never");
        _lblChangedAt   = MakeValueLabel("—");
        _lblLogStatus   = MakeValueLabel("Initialising…");

        layout.Controls.Add(MakeHeaderLabel("Current Port:"),  0, 0);
        layout.Controls.Add(_lblCurrentPort,                   1, 0);
        layout.Controls.Add(MakeHeaderLabel("Last Changed:"),  0, 1);
        layout.Controls.Add(_lblChangedAt,                     1, 1);
        layout.Controls.Add(MakeHeaderLabel("Previous Port:"), 0, 2);
        layout.Controls.Add(_lblLastChanged,                   1, 2);
        layout.Controls.Add(MakeHeaderLabel("Monitor:"),       0, 3);
        layout.Controls.Add(_lblLogStatus,                     1, 3);

        _rtbLog = new RichTextBox
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(20, 20, 20),
            ForeColor = Color.LightGreen,
            Font = new Font("Consolas", 8.5f),
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
            ScrollBars = RichTextBoxScrollBars.Vertical
        };

        layout.SetColumnSpan(_rtbLog, 2);
        layout.Controls.Add(_rtbLog, 0, 4);
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        Controls.Add(layout);

        // Hide instead of close so tray icon can reopen it
        FormClosing += (_, e) =>
        {
            e.Cancel = true;
            Hide();
        };
    }

    // Called from TrayApplicationContext (UI thread via Invoke)
    public void UpdateStatus(string currentPort, string previousPort, DateTime? changedAt, string logStatus)
    {
        _lblCurrentPort.Text = currentPort;
        _lblLastChanged.Text = previousPort == "Unknown" ? "—" : previousPort;
        _lblChangedAt.Text   = changedAt.HasValue ? changedAt.Value.ToString("dd MMM yyyy HH:mm:ss") : "Never";
        _lblLogStatus.Text   = logStatus;
    }

    public void AppendLog(string message)
    {
        if (_rtbLog.InvokeRequired)
        {
            _rtbLog.Invoke(() => AppendLog(message));
            return;
        }
        _rtbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        _rtbLog.ScrollToCaret();
    }

    private static Label MakeHeaderLabel(string text) => new()
    {
        Text = text,
        AutoSize = true,
        ForeColor = Color.Gray,
        Font = new Font("Segoe UI", 9f),
        Anchor = AnchorStyles.Left | AnchorStyles.Top,
        Margin = new Padding(0, 4, 0, 0)
    };

    private static Label MakeValueLabel(string text) => new()
    {
        Text = text,
        AutoSize = true,
        ForeColor = Color.WhiteSmoke,
        Font = new Font("Segoe UI", 9f, FontStyle.Bold),
        Anchor = AnchorStyles.Left | AnchorStyles.Top,
        Margin = new Padding(0, 4, 0, 0)
    };
}
