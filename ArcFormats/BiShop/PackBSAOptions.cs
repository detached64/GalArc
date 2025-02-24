using GalArc.Controls;
using System;

namespace ArcFormats.BiShop
{
    public partial class PackBSAOptions : OptionsTemplate
    {
        public static PackBSAOptions Instance { get; } = new PackBSAOptions();

        private readonly string Versions = "1/2";

        public PackBSAOptions()
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
