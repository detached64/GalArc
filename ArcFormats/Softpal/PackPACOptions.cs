using GalArc.Controls;
using System;

namespace ArcFormats.Softpal
{
    public partial class PackPACOptions : OptionsTemplate
    {
        public static PackPACOptions Instance { get; } = new PackPACOptions();

        internal static bool ComputeChecksum = false;

        internal static bool EncryptScripts = true;

        private readonly string Versions = "1/2";

        public PackPACOptions()
        {
            InitializeComponent();
            this.combVersion.Items.AddRange(Versions.Split('/'));
            if (this.combVersion.Items.Count > 0)
            {
                this.combVersion.SelectedIndex = 0;
            }
        }

        private void chkbxCompute_CheckedChanged(object sender, EventArgs e)
        {
            ComputeChecksum = this.chkbxCompute.Checked;
        }

        private void combVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            Version = this.combVersion.Text;
            this.chkbxCompute.Visible = this.combVersion.Text != "1";
        }

        private void chkbxEncScr_CheckedChanged(object sender, EventArgs e)
        {
            EncryptScripts = this.chkbxEncScr.Checked;
        }
    }
}
