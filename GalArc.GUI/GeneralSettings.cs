using GalArc.GUI.Properties;
using System;
using System.Windows.Forms;

namespace GalArc.GUI
{
    public partial class GeneralSettings : UserControl
    {
        private static GeneralSettings instance;

        public static GeneralSettings Instance
        {
            get
            {
                return instance ?? (instance = new GeneralSettings());
            }
        }

        public GeneralSettings()
        {
            InitializeComponent();
        }

        private void GeneralSettings_Load(object sender, EventArgs e)
        {
            this.chkbxAutoSave.Checked = GalArc.Properties.BaseSettings.Default.ToAutoSaveState;
            this.chkbxTopMost.Checked = Settings.Default.IsTopMost;
            this.chkbxFreeze.Checked = Settings.Default.ToFreezeControls;
            MainWindow.Instance.TopMost = this.chkbxTopMost.Checked;
        }

        private void chkbxAutoSave_CheckedChanged(object sender, EventArgs e)
        {
            GalArc.Properties.BaseSettings.Default.ToAutoSaveState = this.chkbxAutoSave.Checked;
            GalArc.Properties.BaseSettings.Default.Save();
        }

        private void chkbxTopMost_CheckedChanged(object sender, EventArgs e)
        {
            MainWindow.Instance.TopMost = this.chkbxTopMost.Checked;
            Settings.Default.IsTopMost = this.chkbxTopMost.Checked;
            Settings.Default.Save();
        }

        private void chkbxFreeze_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.ToFreezeControls = this.chkbxFreeze.Checked;
            Settings.Default.Save();
        }
    }
}
