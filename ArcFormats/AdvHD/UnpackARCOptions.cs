using GalArc.Controls;
using System;

namespace ArcFormats.AdvHD
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
            Options.DecryptScripts = this.chkbxDecScr.Checked;
        }
    }

    public class AdvHDUnpackOptions : VersionOptions
    {
        public bool DecryptScripts { get; set; } = true;
    }
}
