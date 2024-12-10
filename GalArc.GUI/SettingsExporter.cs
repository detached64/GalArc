using ArcFormats;
using GalArc.Common;
using GalArc.DataBase;
using GalArc.Extensions;
using GalArc.Extensions.GARbroDB;
using GalArc.Extensions.SiglusKeyFinder;
using GalArc.GUI.Properties;
using GalArc.Logs;
using System;
using System.Text;

namespace GalArc.GUI
{
    internal class SettingsExporter
    {
        public static void ExportSettingsToGalArc()
        {
            ExtensionsConfig.IsEnabled = Settings.Default.EnableExtensions;
            GARbroDBConfig.IsEnabled = Settings.Default.EnableGARbroDB;
            GARbroDBConfig.Path = Settings.Default.GARbroDBPath;
            SiglusKeyFinderConfig.IsEnabled = Settings.Default.EnableSiglusKeyFinder;
            SiglusKeyFinderConfig.Path = Settings.Default.SiglusKeyFinderPath;
            DataBaseConfig.Path = Settings.Default.DataBasePath;
            LogConfig.autoSaveState = Settings.Default.AutoSaveState;
        }
    }
}
