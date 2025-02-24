using GalArc.Controls;
using System;

namespace ArcFormats.InnocentGrey
{
    public partial class PackIGAOptions : OptionsTemplate
    {
        public static PackIGAOptions Instance { get; } = new PackIGAOptions();

        internal static bool EncryptScripts = true;

        public PackIGAOptions()
        {
            InitializeComponent();
        }

        private void chkbxEncScr_CheckedChanged(object sender, EventArgs e)
        {
            EncryptScripts = this.chkbxEncScr.Checked;
        }
    }
}
