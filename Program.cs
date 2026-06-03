using System;
using System.Windows.Forms;

namespace Protoport;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Run without a visible main window — tray icon manages lifetime
        Application.Run(new TrayApplicationContext());
    }
}
