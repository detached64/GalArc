using GalArc.Controls;
using System;

namespace ArcFormats.PJADV
{
    public partial class UnpackDATOptions : OptionsTemplate
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
