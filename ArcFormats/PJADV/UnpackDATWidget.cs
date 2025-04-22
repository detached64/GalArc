using ArcFormats.AdvHD;
using GalArc.Controls;
using System;

namespace ArcFormats.PJADV
{
    public partial class UnpackDATWidget : WidgetTemplate
    {
        public static UnpackDATWidget Instance { get; } = new UnpackDATWidget();

        public AdvHDUnpackOptions Options = new AdvHDUnpackOptions();

        public UnpackDATWidget()
        {
            InitializeComponent();
        }

        private void chkbxDecScr_CheckedChanged(object sender, EventArgs e)
        {
            Options.DecryptScripts = this.chkbxDecScr.Checked;
        }
    }
}
