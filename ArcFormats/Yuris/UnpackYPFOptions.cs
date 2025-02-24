using GalArc.Controls;
using System;

namespace ArcFormats.Yuris
{
    public partial class UnpackYPFOptions : OptionsTemplate
    {
        public static UnpackYPFOptions Instance { get; } = new UnpackYPFOptions();

        internal static bool DecryptScripts = true;

        public UnpackYPFOptions()
        {
            InitializeComponent();
        }

        private void chkbxDecScr_CheckedChanged(object sender, EventArgs e)
        {
            DecryptScripts = this.chkbxDecScr.Checked;
        }
    }
}
