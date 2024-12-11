using System;
using System.Windows.Forms;

namespace ArcFormats.Softpal
{
    public partial class PackPACOptions : UserControl
    {
        internal static bool toCompute = false;

        internal static bool toEncryptScripts = true;

        private string versions = "1/2";

        public PackPACOptions()
        {
            InitializeComponent();
            this.combVersion.Items.AddRange(versions.Split('/'));
            if (this.combVersion.Items.Count > 0)
            {
                this.combVersion.SelectedIndex = 0;
            }
        }

        private void chkbxCompute_CheckedChanged(object sender, EventArgs e)
        {
            toCompute = this.chkbxCompute.Checked;
        }

        private void combVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            ArcSettings.Version = this.combVersion.Text;
            this.chkbxCompute.Visible = this.combVersion.Text != "1";
        }

        private void chkbxEncScr_CheckedChanged(object sender, EventArgs e)
        {
            toEncryptScripts = this.chkbxEncScr.Checked;
        }
    }
}
