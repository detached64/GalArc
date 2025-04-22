using GalArc.Controls;
using System;

namespace ArcFormats.InnocentGrey
{
    public partial class PackIGAWidget : WidgetTemplate
    {
        public static PackIGAWidget Instance { get; } = new PackIGAWidget();

        internal static bool EncryptScripts = true;

        public PackIGAWidget()
        {
            InitializeComponent();
        }

        private void chkbxEncScr_CheckedChanged(object sender, EventArgs e)
        {
            EncryptScripts = this.chkbxEncScr.Checked;
        }
    }
}
