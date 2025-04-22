using ArcFormats.AdvHD;
using GalArc.Controls;
using System;

namespace ArcFormats.Majiro
{
    public partial class UnpackARCWidget : WidgetTemplate
    {
        public static UnpackARCWidget Instance { get; } = new UnpackARCWidget();

        public AdvHDUnpackOptions Options = new AdvHDUnpackOptions();

        public UnpackARCWidget()
        {
            InitializeComponent();
        }

        private void chkbxDecScr_CheckedChanged(object sender, EventArgs e)
        {
            Options.DecryptScripts = chkbxDecScr.Checked;
        }
    }
}
