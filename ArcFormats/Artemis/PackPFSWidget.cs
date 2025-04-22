using GalArc.Templates;
using System;

namespace ArcFormats.Artemis
{
    public partial class PackPFSWidget : WidgetTemplate
    {
        public static PackPFSWidget Instance { get; } = new PackPFSWidget();

        public VersionOptions Options = new VersionOptions();

        private readonly string Versions = "8/6/2";

        public PackPFSWidget()
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
