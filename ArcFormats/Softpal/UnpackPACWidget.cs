using ArcFormats.AdvHD;
using GalArc.Controls;
using System;

namespace ArcFormats.Softpal
{
    public partial class UnpackPACWidget : WidgetTemplate
    {
        public static UnpackPACWidget Instance { get; } = new UnpackPACWidget();

        public AdvHDUnpackOptions Options = new AdvHDUnpackOptions();

        public UnpackPACWidget()
        {
            InitializeComponent();
        }

        private void chkbxDecScr_CheckedChanged(object sender, EventArgs e)
        {
            Options.DecryptScripts = this.chkbxDecScr.Checked;
        }
    }
}
