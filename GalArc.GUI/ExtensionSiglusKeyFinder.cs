using GalArc.Extensions.SiglusKeyFinder;
using GalArc.GUI.Properties;
using System;
using System.Windows.Forms;

namespace GalArc.GUI
{
    public partial class ExtensionSiglusKeyFinder : UserControl
    {
        private static ExtensionSiglusKeyFinder instance;

        public static ExtensionSiglusKeyFinder Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ExtensionSiglusKeyFinder();
                }
                return instance;
            }
        }

        public ExtensionSiglusKeyFinder()
        {
            InitializeComponent();
        }

        private void ExtensionSiglusKeyFinder_Load(object sender, EventArgs e)
        {
            this.txtExePath.Text = SiglusKeyFinderConfig.Path;
            this.chkbxEnableGARbroDB.Checked = Settings.Default.EnableSiglusKeyFinder;
            SiglusKeyFinderConfig.IsEnabled = this.chkbxEnableGARbroDB.Checked;
        }

        private void btSelect_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "SiglusKeyFinder.exe|*.exe";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    this.txtExePath.Text = openFileDialog.FileName;
                    SiglusKeyFinderConfig.Path = openFileDialog.FileName;
                    Settings.Default.SiglusKeyFinderPath = openFileDialog.FileName;
                    Settings.Default.Save();
                }
            }
        }

        private void chkbxEnableGARbroDB_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableSiglusKeyFinder = this.chkbxEnableGARbroDB.Checked;
            Settings.Default.Save();
            SiglusKeyFinderConfig.IsEnabled = this.chkbxEnableGARbroDB.Checked;
            this.panel.Enabled = this.chkbxEnableGARbroDB.Checked;
        }

        private void txtExePath_TextChanged(object sender, EventArgs e)
        {
            SiglusKeyFinderConfig.Path = this.txtExePath.Text;
            Settings.Default.SiglusKeyFinderPath = this.txtExePath.Text;
            Settings.Default.Save();
        }
    }
}
