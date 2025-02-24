using GalArc.Controls;
using System;

namespace ArcFormats.GsPack
{
    public partial class UnpackPAKOptions : OptionsTemplate
    {
        public static UnpackPAKOptions Instance { get; } = new UnpackPAKOptions();

        internal static bool DecryptScripts = true;

        public UnpackPAKOptions()
        {
            InitializeComponent();
        }

        private void chkbxDecScr_CheckedChanged(object sender, EventArgs e)
        {
            DecryptScripts = this.chkbxDecScr.Checked;
        }
    }
}
