using ArcFormats.AdvHD;
using GalArc.Controls;
using System;

namespace ArcFormats.Softpal
{
    public partial class UnpackPACOptions : OptionsTemplate
    {
        public static UnpackPACOptions Instance { get; } = new UnpackPACOptions();

        public AdvHDUnpackOptions Options = new AdvHDUnpackOptions();

        public UnpackPACOptions()
        {
            InitializeComponent();
        }

        private void chkbxDecScr_CheckedChanged(object sender, EventArgs e)
        {
            Options.DecryptScripts = this.chkbxDecScr.Checked;
        }
    }
}
