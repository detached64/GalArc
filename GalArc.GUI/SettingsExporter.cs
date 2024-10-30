using GalArc.Extensions;
using GalArc.GUI.Properties;
using System;

namespace GalArc.GUI
{
    internal class SettingsExporter
    {
        public static void ExportSettings()
        {
            ExtensionsConfig.IsEnabled = Settings.Default.EnableExtensions;
            Extensions.GARbroDB.GARbroDBConfig.IsGARbroDBEnabled = Settings.Default.EnableGARbroDB;
            Extensions.GARbroDB.GARbroDBConfig.GARbroDBPath = Settings.Default.GARbroDBPath;
            Logs.LogConfig.autoSaveState = Settings.Default.AutoSaveState;
        }
    }
}
