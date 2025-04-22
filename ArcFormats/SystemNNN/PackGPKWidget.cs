using GalArc.Templates;
using System;

namespace ArcFormats.SystemNNN
{
    public partial class PackGPKWidget : WidgetTemplate
    {
        public static PackGPKWidget Instance { get; } = new PackGPKWidget();

        public VersionOptions Options = new VersionOptions();

        private readonly string Versions = "1/2";

        public PackGPKWidget()
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
    }
}
