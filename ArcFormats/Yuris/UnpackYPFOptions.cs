using GalArc.Controls;
using System;

namespace ArcFormats.Yuris
{
    public partial class UnpackYPFOptions : OptionsTemplate
    {
        internal static bool toDecryptScripts = true;
        public UnpackYPFOptions()
        {
            InitializeComponent();
        }

        private void chkbxDecScr_CheckedChanged(object sender, EventArgs e)
        {
            toDecryptScripts = this.chkbxDecScr.Checked;
        }
    }
}
