using System;
using System.Windows.Forms;

namespace ArcFormats.InnocentGrey
{
    public partial class PackIGAOptions : UserControl
    {
        internal static bool toEncryptScripts = true;

        public PackIGAOptions()
        {
            InitializeComponent();
        }

        private void chkbxEncScr_CheckedChanged(object sender, EventArgs e)
        {
            toEncryptScripts = this.chkbxEncScr.Checked;
        }
    }
}
