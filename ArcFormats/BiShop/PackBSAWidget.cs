using GalArc.Templates;
using System;

namespace ArcFormats.BiShop
{
    public partial class PackBSAWidget : WidgetTemplate
    {
        public static PackBSAWidget Instance { get; } = new PackBSAWidget();

        public VersionOptions Options = new VersionOptions();

        private readonly string Versions = "1/2";

        public PackBSAWidget()
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
