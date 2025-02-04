using GalArc.Controls;
using GalArc.GUI.Properties;
using System;

namespace GalArc.GUI
{
    public partial class GeneralSettings : SettingsTemplate
    {
        private static readonly Lazy<GeneralSettings> lazyInstance =
                new Lazy<GeneralSettings>(() => new GeneralSettings());

        public static GeneralSettings Instance => lazyInstance.Value;

        private GeneralSettings()
        {
            InitializeComponent();
        }

        private void GeneralSettings_Load(object sender, EventArgs e)
        {
            this.chkbxAutoSave.Checked = BaseSettings.Default.ToAutoSaveState;
            this.chkbxTopMost.Checked = Settings.Default.IsTopMost;
            MainWindow.Instance.TopMost = this.chkbxTopMost.Checked;
        }

        private void chkbxAutoSave_CheckedChanged(object sender, EventArgs e)
        {
            BaseSettings.Default.ToAutoSaveState = this.chkbxAutoSave.Checked;
            BaseSettings.Default.Save();
        }

        private void chkbxTopMost_CheckedChanged(object sender, EventArgs e)
        {
            MainWindow.Instance.TopMost = this.chkbxTopMost.Checked;
            Settings.Default.IsTopMost = this.chkbxTopMost.Checked;
            Settings.Default.Save();
        }
    }
}
