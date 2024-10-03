using System;
using System.Windows.Forms;

namespace ArcFormats.Softpal
{
    public partial class UnpackPACOptions : UserControl
    {
        internal static bool toDecryptScripts = true;
        public UnpackPACOptions()
        {
            InitializeComponent();
        }

        private void chkbxDecScr_CheckedChanged(object sender, EventArgs e)
        {
            toDecryptScripts = this.chkbxDecScr.Checked;
        }
    }
}
