using GalArc.Controls;
using System;

namespace ArcFormats.InnocentGrey
{
    public partial class PackIGAOptions : OptionsTemplate
    {
        internal static bool toEncryptScripts = true;

        public PackIGAOptions()
        {
            InitializeComponent();
        }

        private void chkbxEncScr_CheckedChanged(object sender, EventArgs e)
        {
            toEncryptScripts = this.chkbxEncScr.Checked;
        }
    }
}
