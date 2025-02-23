using GalArc.Controls;
using System;

namespace ArcFormats.PJADV
{
    public partial class UnpackDATOptions : OptionsTemplate
    {
        internal static bool DecryptScripts = true;

        public UnpackDATOptions()
        {
            InitializeComponent();
        }

        private void chkbxDecScr_CheckedChanged(object sender, EventArgs e)
        {
            DecryptScripts = this.chkbxDecScr.Checked;
        }
    }
}
