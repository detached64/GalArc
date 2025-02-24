using GalArc.Controls;
using System;

namespace ArcFormats.NeXAS
{
    public partial class PackPACOptions : OptionsTemplate
    {
        public static PackPACOptions Instance { get; } = new PackPACOptions();

        internal static readonly string[] methods =
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

        private readonly string Versions = "1/2";

        public PackPACOptions()
        {
            InitializeComponent();
            this.combVersion.Items.AddRange(Versions.Split('/'));
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
