using GalArc.Controls;
using System;

namespace ArcFormats.Ethornell
{
    public partial class PackARCWidget : WidgetTemplate
    {
        public static PackARCWidget Instance { get; } = new PackARCWidget();

        public VersionOptions Options = new VersionOptions();

        private readonly string Versions = "1/2";

        public PackARCWidget()
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
