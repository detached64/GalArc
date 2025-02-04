using GalArc.Controls;
using System;

namespace ArcFormats.AdvHD
{
    public partial class UnpackARCOptions : OptionsTemplate
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
