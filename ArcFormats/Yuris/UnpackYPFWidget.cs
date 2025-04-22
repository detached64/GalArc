using GalArc.Controls;
using System;

namespace ArcFormats.Yuris
{
    public partial class UnpackYPFWidget : WidgetTemplate
    {
        public static UnpackYPFWidget Instance { get; } = new UnpackYPFWidget();

        internal static bool DecryptScripts = true;

        public UnpackYPFWidget()
        {
            InitializeComponent();
        }

        private void chkbxDecScr_CheckedChanged(object sender, EventArgs e)
        {
            DecryptScripts = this.chkbxDecScr.Checked;
        }
    }
}
