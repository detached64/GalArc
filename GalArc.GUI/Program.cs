using GalArc.Common;
using GalArc.Logs;
using GalArc.Settings;
using System;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace GalArc.GUI
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            GetLocalCulture();
            SetLocalCulture();
            Logger.NewInstance();
            if (Environment.OSVersion.Version.Major >= 6)
            {
                SetProcessDPIAware();
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        private static void GetLocalCulture()
        {
            MainForm.CurrentCulture = string.IsNullOrEmpty(GUISettings.Default.LastLanguage) ? CultureInfo.CurrentCulture.Name : GUISettings.Default.LastLanguage;
            if (!Languages.SupportedLanguages.Values.ToArray().Contains(MainForm.CurrentCulture))
            {
                MainForm.CurrentCulture = "en-US";
            }
        }

        private static void SetLocalCulture()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(MainForm.CurrentCulture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(MainForm.CurrentCulture);
        }

        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
    }
}
