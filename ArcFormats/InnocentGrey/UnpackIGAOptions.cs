using GalArc.Controls;
using System;

namespace ArcFormats.InnocentGrey
{
    public partial class UnpackIGAOptions : OptionsTemplate
    {
        public static UnpackIGAOptions Instance { get; } = new UnpackIGAOptions();

        internal static bool DecryptScripts = true;

        public UnpackIGAOptions()
        {
            InitializeComponent();
        }

        private void chkbxDecScr_CheckedChanged(object sender, EventArgs e)
        {
            DecryptScripts = this.chkbxDecScr.Checked;
        }
    }
}
