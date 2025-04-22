using ArcFormats.AdvHD;
using GalArc.Controls;
using System;

namespace ArcFormats.PJADV
{
    public partial class PackDATOptions : OptionsTemplate
    {
        public static PackDATOptions Instance { get; } = new PackDATOptions();

        public AdvHDPackOptions Options = new AdvHDPackOptions();

        private readonly string Versions = "1/2";

        public PackDATOptions()
        {
            InitializeComponent();
            this.combVersion.Items.AddRange(Versions.Split('/'));
            if (this.combVersion.Items.Count > 0)
            {
                this.combVersion.SelectedIndex = 0;
            }
        }

        private void combVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            Options.Version = this.combVersion.Text;
        }

        private void chkbxEncScr_CheckedChanged(object sender, EventArgs e)
        {
            Options.EncryptScripts = this.chkbxEncScr.Checked;
        }
    }
}
