using GalArc.Controls;
using System;

namespace ArcFormats.InnocentGrey
{
    public partial class UnpackIGAWidget : WidgetTemplate
    {
        public static UnpackIGAWidget Instance { get; } = new UnpackIGAWidget();

        internal static bool DecryptScripts = true;

        public UnpackIGAWidget()
        {
            InitializeComponent();
        }

        private void chkbxDecScr_CheckedChanged(object sender, EventArgs e)
        {
            DecryptScripts = this.chkbxDecScr.Checked;
        }
    }
}
