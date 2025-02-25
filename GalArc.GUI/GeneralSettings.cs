using GalArc.Controls;
using GalArc.Settings;
using System;

namespace GalArc.GUI
{
    public partial class GeneralSettings : SettingsTemplate
    {
        public static GeneralSettings Instance { get; } = new GeneralSettings();

        private GeneralSettings()
        {
            InitializeComponent();
        }

        private void GeneralSettings_Load(object sender, EventArgs e)
        {
            this.chkbxTopMost.Checked = GUISettings.Default.IsTopMost;
            MainWindow.Instance.TopMost = this.chkbxTopMost.Checked;
        }

        private void chkbxTopMost_CheckedChanged(object sender, EventArgs e)
        {
            SettingsWindow.Instance.TopMost = this.chkbxTopMost.Checked;
            MainWindow.Instance.TopMost = this.chkbxTopMost.Checked;
            GUISettings.Default.IsTopMost = this.chkbxTopMost.Checked;
            GUISettings.Default.Save();
        }
    }
}
