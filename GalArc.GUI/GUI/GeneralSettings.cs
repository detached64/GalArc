using GalArc.Properties;
using Log;
using System;
using System.Windows.Forms;

namespace GalArc.GUI
{
    public partial class GeneralSettings : UserControl
    {
        public static GeneralSettings Instance;

        public GeneralSettings()
        {
            InitializeComponent();
        }

        private void GeneralSettings_Load(object sender, EventArgs e)
        {
            this.chkbxAutoSave.Checked = Settings.Default.AutoSaveState;
            this.chkbxTopMost.Checked = Settings.Default.TopMost;
            chkbxTopMost_CheckedChanged(null, null);
        }

        private void chkbxAutoSave_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.AutoSaveState = this.chkbxAutoSave.Checked;
            Settings.Default.Save();
            LogWindow.ChangeLocalSettings(this.chkbxAutoSave.Checked);
        }

        private void chkbxTopMost_CheckedChanged(object sender, EventArgs e)
        {
            MainWindow.Instance.TopMost = this.chkbxTopMost.Checked;
            Settings.Default.TopMost = this.chkbxTopMost.Checked;
            Settings.Default.Save();
        }
    }
}
