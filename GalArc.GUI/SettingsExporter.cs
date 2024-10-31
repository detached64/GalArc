using ArcFormats;
using GalArc.Common;
using GalArc.Extensions;
using GalArc.GUI.Properties;
using System;
using System.Text;

namespace GalArc.GUI
{
    internal class SettingsExporter
    {
        public static void ExportSettingsToGalArc()
        {
            ExtensionsConfig.IsEnabled = Settings.Default.EnableExtensions;
            Extensions.GARbroDB.GARbroDBConfig.IsGARbroDBEnabled = Settings.Default.EnableGARbroDB;
            Extensions.GARbroDB.GARbroDBConfig.GARbroDBPath = Settings.Default.GARbroDBPath;
            Logs.LogConfig.autoSaveState = Settings.Default.AutoSaveState;
        }

        public static void ExportSettingsToArcFormats()
        {
            if (!string.IsNullOrEmpty(Settings.Default.DefaultEncoding))
            {
                Config.Encoding = Encoding.GetEncoding(Encodings.CodePages[Settings.Default.DefaultEncoding]);
            }
            else
            {
                Config.Encoding = Encoding.UTF8;
            }
        }
    }
}
