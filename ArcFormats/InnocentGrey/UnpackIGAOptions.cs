using System;
using System.Windows.Forms;

namespace ArcFormats.InnocentGrey
{
    public partial class UnpackIGAOptions : UserControl
    {
        internal static bool toDecryptScripts = true;

        public UnpackIGAOptions()
        {
            InitializeComponent();
        }

        private void chkbxDecScr_CheckedChanged(object sender, EventArgs e)
        {
            toDecryptScripts = this.chkbxDecScr.Checked;
        }
    }
}
