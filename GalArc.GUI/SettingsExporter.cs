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
            Extensions.GARbroDB.GARbroDBConfig.IsEnabled = Settings.Default.EnableGARbroDB;
            Extensions.GARbroDB.GARbroDBConfig.Path = Settings.Default.GARbroDBPath;
            Extensions.SiglusKeyFinder.SiglusKeyFinderConfig.IsEnabled = Settings.Default.EnableSiglusKeyFinder;
            Extensions.SiglusKeyFinder.SiglusKeyFinderConfig.Path = Settings.Default.SiglusKeyFinderPath;
            DataBase.DataBaseConfig.DataBasePath = Settings.Default.DataBasePath;
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
