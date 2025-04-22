using ArcFormats.AdvHD;
using GalArc.Controls;
using System;

namespace ArcFormats.Majiro
{
    public partial class UnpackARCOptions : OptionsTemplate
    {
        public static UnpackARCOptions Instance { get; } = new UnpackARCOptions();

        public AdvHDUnpackOptions Options = new AdvHDUnpackOptions();

        public UnpackARCOptions()
        {
            InitializeComponent();
        }

        private void chkbxDecScr_CheckedChanged(object sender, EventArgs e)
        {
            Options.DecryptScripts = chkbxDecScr.Checked;
        }
    }
}
