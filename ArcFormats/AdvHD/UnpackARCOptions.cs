using GalArc.Controls;
using System;

namespace ArcFormats.AdvHD
{
    public partial class UnpackARCOptions : OptionsTemplate
    {
        public static UnpackARCOptions Instance { get; } = new UnpackARCOptions();

        internal static bool DecryptScripts = true;

        public UnpackARCOptions()
        {
            InitializeComponent();
        }

        private void chkbxDecScr_CheckedChanged(object sender, EventArgs e)
        {
            DecryptScripts = this.chkbxDecScr.Checked;
        }
    }
}
