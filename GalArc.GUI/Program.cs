using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace GalArc.GUI
{
    internal static class Program
    {
        /// <summary>
        /// Entry point.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                SetProcessDPIAware();
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());
        }
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
    }
}
