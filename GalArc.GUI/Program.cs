using GalArc.Logs;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace GalArc.GUI
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Logger.NewInstance();
            if (Environment.OSVersion.Version.Major >= 6)
            {
                SetProcessDPIAware();
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
    }
}
