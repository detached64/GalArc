using Log;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            this.chkbxAutoSave.Checked = Properties.Settings.Default.AutoSaveState;
            this.chkbxTopMost.Checked = Properties.Settings.Default.TopMost;
        }

        private void chkbxAutoSave_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.AutoSaveState = this.chkbxAutoSave.Checked;
            Properties.Settings.Default.Save();
        }

        private void chkbxTopMost_CheckedChanged(object sender, EventArgs e)
        {
            if (this.chkbxTopMost.Checked)
            {
                LogWindow.Instance.TopMost = true;
                MainWindow.Instance.TopMost = true;
                Properties.Settings.Default.TopMost = true;
                Properties.Settings.Default.Save();
            }
            else
            {
                LogWindow.Instance.TopMost = false;
                MainWindow.Instance.TopMost = false;
                Properties.Settings.Default.TopMost = false;
                Properties.Settings.Default.Save();
            }
        }
    }
}
