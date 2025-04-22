using GalArc.Templates;
using System;

namespace ArcFormats.AdvHD
{
    public partial class PackARCWidget : WidgetTemplate
    {
        public static PackARCWidget Instance { get; } = new PackARCWidget();

        public VersionScriptPackOptions Options = new VersionScriptPackOptions();

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
}
