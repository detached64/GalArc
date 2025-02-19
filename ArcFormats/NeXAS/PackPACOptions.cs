using GalArc.Controls;
using System;

namespace ArcFormats.NeXAS
{
    public partial class PackPACOptions : OptionsTemplate
    {
        private const string versions = "1/2";

        private readonly string[] methods =
        {
            "0-None",
            "1-Lzss",
            "2-Huffman",
            "3-Zlib",
            "4-Zlib",
            "5-Zlib",
            "7-Zstd"
        };

        internal static int SelectedMethods;

        public PackPACOptions()
        {
            InitializeComponent();
            this.combVersion.Items.AddRange(versions.Split('/'));
            this.combVersion.SelectedIndex = 0;
            this.combMethods.Items.AddRange(methods);
            this.combMethods.SelectedIndex = 0;
        }

        private void combVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            Version = this.combVersion.Text;
        }

        private void combMethods_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedMethods = this.combMethods.SelectedIndex;
        }
    }
}
