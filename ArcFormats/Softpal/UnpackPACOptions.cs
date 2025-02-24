using GalArc.Controls;
using System;

namespace ArcFormats.Softpal
{
    public partial class UnpackPACOptions : OptionsTemplate
    {
        public static UnpackPACOptions Instance { get; } = new UnpackPACOptions();

        internal static bool DecryptScripts = true;

        public UnpackPACOptions()
        {
            InitializeComponent();
        }

        private void chkbxDecScr_CheckedChanged(object sender, EventArgs e)
        {
            DecryptScripts = this.chkbxDecScr.Checked;
        }
    }
}
