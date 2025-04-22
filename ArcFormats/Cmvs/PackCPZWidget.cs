using GalArc.Controls;
using System;

namespace ArcFormats.Cmvs
{
    public partial class PackCPZWidget : WidgetTemplate
    {
        public static PackCPZWidget Instance { get; } = new PackCPZWidget();

        public VersionOptions Options = new VersionOptions();

        private readonly string Versions = "1";

        public PackCPZWidget()
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
