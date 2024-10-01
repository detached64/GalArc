using System;
using System.Windows.Forms;

namespace ArcFormats.Kirikiri
{
    public partial class PackXP3Options : UserControl
    {
        internal static string Version;

        internal static bool CompressIndex = true;

        internal static bool CompressContents = true;

        public PackXP3Options(string versions)
        {
            InitializeComponent();
            this.combVersion.Items.AddRange(versions.Split('/'));
            if (this.combVersion.Items.Count > 0)
            {
                this.combVersion.SelectedIndex = 0;
            }
        }

        private void combVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            Version = this.combVersion.Text;
        }

        private void chkbxComIndex_CheckedChanged(object sender, EventArgs e)
        {
            CompressIndex = chkbxComIndex.Checked;
        }

        private void chkbxComContents_CheckedChanged(object sender, EventArgs e)
        {
            CompressContents = chkbxComContents.Checked;
        }
    }
}
