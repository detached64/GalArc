using GalArc.Controls;
using System;

namespace ArcFormats.NeXAS
{
    public partial class PackPACOptions : OptionsTemplate
    {
        public static PackPACOptions Instance { get; } = new PackPACOptions();

        public NeXASOptions Options = new NeXASOptions();

        private readonly string Versions = "1/2";

        public PackPACOptions()
        {
            InitializeComponent();
        }

        private void PackPACOptions_Load(object sender, EventArgs e)
        {
            this.combVersion.Items.AddRange(Versions.Split('/'));
            this.combVersion.SelectedIndex = 0;
            this.combMethods.Items.AddRange(Options.Methods);
            this.combMethods.SelectedIndex = 0;
        }

        private void combVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            Options.Version = this.combVersion.Text;
        }

        private void combMethods_SelectedIndexChanged(object sender, EventArgs e)
        {
            Options.Method = this.combMethods.SelectedIndex;
        }
    }

    public class NeXASOptions : VersionOptions
    {
        public int Method { get; set; }

        public readonly string[] Methods =
        {
            "0-None",
            "1-Lzss",
            "2-Huffman",
            "3-Zlib",
            "4-Zlib",
            "5-Zlib",
            "7-Zstd"
        };
    }
}
