using GalArc.GUI.Properties;
using GalArc.Logs;
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
            this.chkbxAutoSave.Checked = Settings.Default.AutoSaveState;
            this.chkbxTopMost.Checked = Settings.Default.TopMost;
            this.chkbxFreeze.Checked = Settings.Default.FreezeControls;
            MainWindow.Instance.TopMost = this.chkbxTopMost.Checked;
        }

        private void chkbxAutoSave_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.AutoSaveState = this.chkbxAutoSave.Checked;
            Settings.Default.Save();
            LogConfig.autoSaveState = this.chkbxAutoSave.Checked;
        }

        private void chkbxTopMost_CheckedChanged(object sender, EventArgs e)
        {
            MainWindow.Instance.TopMost = this.chkbxTopMost.Checked;
            Settings.Default.TopMost = this.chkbxTopMost.Checked;
            Settings.Default.Save();
        }

        private void chkbxFreeze_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.FreezeControls = this.chkbxFreeze.Checked;
            Settings.Default.Save();
        }
    }
}
