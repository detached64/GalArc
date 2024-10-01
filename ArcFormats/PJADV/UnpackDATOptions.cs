using System;
using System.Windows.Forms;

namespace ArcFormats.PJADV
{
    public partial class UnpackDATOptions : UserControl
    {
        internal static bool toDecryptScripts = true;

        public UnpackDATOptions()
        {
            InitializeComponent();
        }

        private void chkbxDecScr_CheckedChanged(object sender, EventArgs e)
        {
            toDecryptScripts = this.chkbxDecScr.Checked;
        }
    }
}
