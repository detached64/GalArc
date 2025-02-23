using GalArc.Controls;
using System;

namespace ArcFormats.Majiro
{
    public partial class UnpackARCOptions : OptionsTemplate
    {
        internal static bool DecryptScripts = true;

        public UnpackARCOptions()
        {
            InitializeComponent();
        }

        private void chkbxDecScr_CheckedChanged(object sender, EventArgs e)
        {
            DecryptScripts = chkbxDecScr.Checked;
        }
    }
}
