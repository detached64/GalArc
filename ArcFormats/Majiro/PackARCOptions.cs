using GalArc.Controls;
using System;

namespace ArcFormats.Majiro
{
    public partial class PackARCOptions : OptionsTemplate
    {
        public static PackARCOptions Instance { get; } = new PackARCOptions();

        internal static bool EncryptScripts = true;

        private readonly string Versions = "1/2/3";

        public PackARCOptions()
        {
            InitializeComponent();
            this.combVersion.Items.AddRange(Versions.Split('/'));
            this.combVersion.SelectedIndex = 0;
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
