using GalArc.Controls;
using System;

namespace ArcFormats.Softpal
{
    public partial class UnpackPACOptions : OptionsTemplate
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
