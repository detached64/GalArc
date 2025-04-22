using GalArc.Controls;
using System;

namespace ArcFormats.Qlie
{
    public partial class PackPACKOptions : OptionsTemplate
    {
        public static PackPACKOptions Instance { get; } = new PackPACKOptions();

        public VersionOptions Options = new VersionOptions();

        private readonly string Versions = "1.0";

        public PackPACKOptions()
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
