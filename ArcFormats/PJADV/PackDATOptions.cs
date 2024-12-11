using System;
using System.Windows.Forms;

namespace ArcFormats.PJADV
{
    public partial class PackDATOptions : UserControl
    {
        internal static bool toEncryptScripts = true;

        private string versions = "1/2";

        public PackDATOptions()
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
            ArcSettings.Version = this.combVersion.Text;
        }

        private void chkbxEncScr_CheckedChanged(object sender, EventArgs e)
        {
            toEncryptScripts = this.chkbxEncScr.Checked;
        }
    }
}
