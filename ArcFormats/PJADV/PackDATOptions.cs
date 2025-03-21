using GalArc.Controls;
using System;

namespace ArcFormats.PJADV
{
    public partial class PackDATOptions : OptionsTemplate
    {
        public static PackDATOptions Instance { get; } = new PackDATOptions();

        internal static bool EncryptScripts = true;

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
            Version = this.combVersion.Text;
        }

        private void chkbxEncScr_CheckedChanged(object sender, EventArgs e)
        {
            EncryptScripts = this.chkbxEncScr.Checked;
        }
    }
}
