using System;
using System.Windows.Forms;

namespace ArcFormats.AdvHD
{
    public partial class PackARCOptions : UserControl
    {
        internal static bool toEncryptScripts = true;

        private static string versions = "1/2";

        public PackARCOptions()
        {
            InitializeComponent();
            this.combVersion.Items.AddRange(versions.Split('/'));
            if (this.combVersion.Items.Count > 0)
            {
                this.combVersion.SelectedIndex = 0;
            }
        }

        private void chkbxEncScr_CheckedChanged(object sender, EventArgs e)
        {
            toEncryptScripts = this.chkbxEncScr.Checked;
        }

        private void combVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            Config.Version = this.combVersion.Text;
        }
    }
}
