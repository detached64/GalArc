using GalArc.Controls;
using System;

namespace ArcFormats.GsPack
{
    public partial class UnpackPAKOptions : OptionsTemplate
    {
        internal static bool toDecryptScripts = true;

        public UnpackPAKOptions()
        {
            InitializeComponent();
        }

        private void chkbxDecScr_CheckedChanged(object sender, EventArgs e)
        {
            toDecryptScripts = this.chkbxDecScr.Checked;
        }
    }
}
