using GalArc.Controls;
using GalArc.Extensions.SiglusKeyFinder;
using System;
using System.Windows.Forms;

namespace GalArc.GUI
{
    public partial class ExtensionSiglusKeyFinder : SettingsTemplate
    {
        private static readonly Lazy<ExtensionSiglusKeyFinder> lazyInstance =
                new Lazy<ExtensionSiglusKeyFinder>(() => new ExtensionSiglusKeyFinder());

        public static ExtensionSiglusKeyFinder Instance => lazyInstance.Value;

        private ExtensionSiglusKeyFinder()
        {
            InitializeComponent();
        }

        private void ExtensionSiglusKeyFinder_Load(object sender, EventArgs e)
        {
            this.txtExePath.Text = SiglusKeyFinderConfig.Path;
            this.chkbxEnableSiglusFinder.Checked = BaseSettings.Default.IsSiglusKeyFinderEnabled;
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

        private void chkbxEnableFinder_CheckedChanged(object sender, EventArgs e)
        {
            BaseSettings.Default.IsSiglusKeyFinderEnabled = this.chkbxEnableSiglusFinder.Checked;
            BaseSettings.Default.Save();
            this.panel.Enabled = this.chkbxEnableSiglusFinder.Checked;
        }

        private void txtExePath_TextChanged(object sender, EventArgs e)
        {
            SiglusKeyFinderConfig.Path = this.txtExePath.Text;
        }
    }
}
