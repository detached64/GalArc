using ArcFormats.AdvHD;
using GalArc.Controls;
using System;

namespace ArcFormats.PJADV
{
    public partial class UnpackDATOptions : OptionsTemplate
    {
        public static UnpackDATOptions Instance { get; } = new UnpackDATOptions();

        public AdvHDUnpackOptions Options = new AdvHDUnpackOptions();

        public UnpackDATOptions()
        {
            InitializeComponent();
        }

        private void chkbxDecScr_CheckedChanged(object sender, EventArgs e)
        {
            Options.DecryptScripts = this.chkbxDecScr.Checked;
        }
    }
}
