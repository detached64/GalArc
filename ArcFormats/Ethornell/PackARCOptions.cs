using GalArc.Controls;
using System;

namespace ArcFormats.Ethornell
{
    public partial class PackARCOptions : OptionsTemplate
    {
        public static PackARCOptions Instance { get; } = new PackARCOptions();

        public VersionOptions Options = new VersionOptions();

        private readonly string Versions = "1/2";

        public PackARCOptions()
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
