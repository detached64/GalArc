using GalArc.Controls;
using System;

namespace ArcFormats.Ai6Win
{
    public partial class PackARCOptions : OptionsTemplate
    {
        public static PackARCOptions Instance { get; } = new PackARCOptions();

        internal static bool CompressContents = false;

        private readonly string Versions = "1/2";

        public PackARCOptions()
        {
            InitializeComponent();
            this.combVersion.Items.AddRange(Versions.Split('/'));
            if (this.combVersion.Items.Count > 0)
            {
                this.combVersion.SelectedIndex = 0;
            }
        }

        private void chkbxCompress_CheckedChanged(object sender, EventArgs e)
        {
            CompressContents = this.chkbxCompress.Checked;
        }

        private void combVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            Version = this.combVersion.Text;
            this.chkbxCompress.Visible = Version != "1";
        }
    }
}
