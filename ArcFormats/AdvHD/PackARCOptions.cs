using System;
using System.Windows.Forms;

namespace ArcFormats.AdvHD
{
    public partial class PackARCOptions : UserControl
    {
        internal static bool toEncryptScripts = true;

        public PackARCOptions()
        {
            InitializeComponent();
        }
        public PackARCOptions(string versions)
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
            Global.Version = this.combVersion.Text;
        }
    }
}
