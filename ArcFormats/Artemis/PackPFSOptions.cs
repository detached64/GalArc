using GalArc.Controls;
using System;

namespace ArcFormats.Artemis
{
    public partial class PackPFSOptions : OptionsTemplate
    {
        public static PackPFSOptions Instance { get; } = new PackPFSOptions();

        private readonly string Versions = "8/6/2";

        public PackPFSOptions()
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
