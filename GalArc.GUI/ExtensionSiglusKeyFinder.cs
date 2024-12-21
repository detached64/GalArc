using GalArc.Extensions.SiglusKeyFinder;
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
                return instance ?? (instance = new ExtensionSiglusKeyFinder());
            }
        }

        public ExtensionSiglusKeyFinder()
        {
            InitializeComponent();
        }

        private void ExtensionSiglusKeyFinder_Load(object sender, EventArgs e)
        {
            this.txtExePath.Text = SiglusKeyFinderConfig.Path;
            this.chkbxEnableGARbroDB.Checked = BaseSettings.Default.IsSiglusKeyFinderEnabled;
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
                }
            }
        }

        private void chkbxEnableGARbroDB_CheckedChanged(object sender, EventArgs e)
        {
            BaseSettings.Default.IsSiglusKeyFinderEnabled = this.chkbxEnableGARbroDB.Checked;
            BaseSettings.Default.Save();
            this.panel.Enabled = this.chkbxEnableGARbroDB.Checked;
        }

        private void txtExePath_TextChanged(object sender, EventArgs e)
        {
            SiglusKeyFinderConfig.Path = this.txtExePath.Text;
        }
    }
}
