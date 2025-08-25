using GalArc.Templates;
using System;

namespace ArcFormats.Qlie
{
    public partial class PackPACKWidget : WidgetTemplate
    {
        public static PackPACKWidget Instance { get; } = new PackPACKWidget();

        public VersionOptions Options = new VersionOptions();

        private readonly string Versions = "2.0";

        public PackPACKWidget()
        {
            InitializeComponent();
            this.combVersion.Items.AddRange(Versions.Split('/'));
            this.combVersion.SelectedIndex = 0;
        }

        private void combVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            Options.Version = this.combVersion.Text;
        }
    }
}
