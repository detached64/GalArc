using GalArc.Controls;
using System;

namespace ArcFormats.SystemNNN
{
    public partial class PackGPKOptions : OptionsTemplate
    {
        public static PackGPKOptions Instance { get; } = new PackGPKOptions();

        public VersionOptions Options = new VersionOptions();

        private readonly string Versions = "1/2";

        public PackGPKOptions()
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
