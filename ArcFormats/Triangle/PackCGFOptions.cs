using GalArc.Controls;
using System;

namespace ArcFormats.Triangle
{
    public partial class PackCGFOptions : OptionsTemplate
    {
        public static PackCGFOptions Instance { get; } = new PackCGFOptions();

        private readonly string Versions = "1";

        public PackCGFOptions()
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
            Version = this.combVersion.Text;
        }
    }
}
