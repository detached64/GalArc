using GalArc.Controls;
using System;

namespace ArcFormats.Ai6Win
{
    public partial class PackARCWidget : WidgetTemplate
    {
        public static PackARCWidget Instance { get; } = new PackARCWidget();

        public Ai6WinOptions Options = new Ai6WinOptions();

        private readonly string Versions = "1/2";

        public PackARCWidget()
        {
            InitializeComponent();
            this.combVersion.Items.AddRange(Versions.Split('/'));
            if (this.combVersion.Items.Count > 0)
            {
                this.combVersion.SelectedIndex = 0;
            }
        }

        private void chkbxCompress_CheckedChanged(object sender, EventArgs e)
        {
            Options.CompressContents = this.chkbxCompress.Checked;
        }

        private void combVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            Options.Version = this.combVersion.Text;
            this.chkbxCompress.Visible = Options.Version != "1";
        }
    }

    public class Ai6WinOptions : VersionOptions
    {
        public bool CompressContents { get; set; } = false;
    }
}