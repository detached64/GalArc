using GalArc.Controls;
using System;

namespace ArcFormats.GsPack
{
    public partial class UnpackPAKWidget : WidgetTemplate
    {
        public static UnpackPAKWidget Instance { get; } = new UnpackPAKWidget();

        internal static bool DecryptScripts = true;

        public UnpackPAKWidget()
        {
            InitializeComponent();
        }

        private void chkbxDecScr_CheckedChanged(object sender, EventArgs e)
        {
            DecryptScripts = this.chkbxDecScr.Checked;
        }
    }
}
