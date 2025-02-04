using GalArc.Controls;
using System;

namespace ArcFormats.AdvHD
{
    public partial class PackARCOptions : OptionsTemplate
    {
        internal static bool toEncryptScripts = true;

        private readonly string versions = "1/2";

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
            Version = this.combVersion.Text;
        }
    }
}
