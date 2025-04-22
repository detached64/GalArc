using GalArc.Controls;
using System;

namespace ArcFormats.AdvHD
{
    public partial class PackARCWidget : WidgetTemplate
    {
        public static PackARCWidget Instance { get; } = new PackARCWidget();

        public AdvHDPackOptions Options = new AdvHDPackOptions();

        private readonly string Versions = "1/2";

        public PackARCWidget()
        {
            InitializeComponent();
            this.combVersion.Items.AddRange(Versions.Split('/'));
            this.combVersion.SelectedIndex = 0;
        }

        private void chkbxEncScr_CheckedChanged(object sender, EventArgs e)
        {
            Options.EncryptScripts = this.chkbxEncScr.Checked;
        }

        private void combVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            Options.Version = this.combVersion.Text;
        }
    }

    public class AdvHDPackOptions : VersionOptions
    {
        public bool EncryptScripts { get; set; } = true;
    }
}
