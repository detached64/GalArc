using ArcFormats.AdvHD;
using GalArc.Controls;
using System;

namespace ArcFormats.PJADV
{
    public partial class PackDATWidget : WidgetTemplate
    {
        public static PackDATWidget Instance { get; } = new PackDATWidget();

        public AdvHDPackOptions Options = new AdvHDPackOptions();

        private readonly string Versions = "1/2";

        public PackDATWidget()
        {
            InitializeComponent();
            this.combVersion.Items.AddRange(Versions.Split('/'));
            if (this.combVersion.Items.Count > 0)
            {
                this.combVersion.SelectedIndex = 0;
            }
        }

        private void combVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            Options.Version = this.combVersion.Text;
        }

        private void chkbxEncScr_CheckedChanged(object sender, EventArgs e)
        {
            Options.EncryptScripts = this.chkbxEncScr.Checked;
        }
    }
}
