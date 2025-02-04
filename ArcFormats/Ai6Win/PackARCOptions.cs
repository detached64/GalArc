using GalArc.Controls;
using System;

namespace ArcFormats.Ai6Win
{
    public partial class PackARCOptions : OptionsTemplate
    {
        internal static bool toCompressContents = false;

        public PackARCOptions(string versions)
        {
            InitializeComponent();
            this.combVersion.Items.AddRange(versions.Split('/'));
            if (this.combVersion.Items.Count > 0)
            {
                this.combVersion.SelectedIndex = 0;
            }
        }

        private void chkbxCompress_CheckedChanged(object sender, EventArgs e)
        {
            toCompressContents = this.chkbxCompress.Checked;
        }

        private void combVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            Version = this.combVersion.Text;
            this.chkbxCompress.Visible = Version != "1";
        }
    }
}
