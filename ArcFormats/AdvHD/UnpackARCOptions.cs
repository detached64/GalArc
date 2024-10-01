using System;
using System.Windows.Forms;

namespace ArcFormats.AdvHD
{
    public partial class UnpackARCOptions : UserControl
    {
        internal static bool toDecryptScripts = true;

        public UnpackARCOptions()
        {
            InitializeComponent();
        }

        private void chkbxDecScr_CheckedChanged(object sender, EventArgs e)
        {
            toDecryptScripts = this.chkbxDecScr.Checked;
        }
    }
}
