using GalArc.Controls;
using System;

namespace ArcFormats.InnocentGrey
{
    public partial class UnpackIGAOptions : OptionsTemplate
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
