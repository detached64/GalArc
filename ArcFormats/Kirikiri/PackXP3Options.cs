using GalArc.Controls;
using System;

namespace ArcFormats.Kirikiri
{
    public partial class PackXP3Options : OptionsTemplate
    {
        public static PackXP3Options Instance { get; } = new PackXP3Options();

        internal static bool CompressIndex = true;

        internal static bool CompressContents = true;

        private readonly string Versions = "1/2";

        public PackXP3Options()
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
