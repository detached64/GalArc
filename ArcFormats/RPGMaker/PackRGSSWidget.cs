using GalArc.Controls;
using System;
using System.Drawing;
using System.Text.RegularExpressions;

namespace ArcFormats.RPGMaker
{
    public partial class PackRGSSWidget : WidgetTemplate
    {
        public static PackRGSSWidget Instance { get; } = new PackRGSSWidget();

        public RGSSPackOptions Options = new RGSSPackOptions();

        private readonly string Versions = "1/3";

        public PackRGSSWidget()
        {
            InitializeComponent();
            this.txtSeed.Text = Options.Seed;
            this.combVersion.Items.AddRange(Versions.Split('/'));
            if (this.combVersion.Items.Count > 0)
            {
                this.combVersion.SelectedIndex = 0;
            }
        }

        private void combVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            Options.Version = this.combVersion.Text;
            this.txtSeed.Visible = this.combVersion.Text != "1";
            this.lbSeed.Visible = this.combVersion.Text != "1";
        }

        private void txtSeed_TextChanged(object sender, EventArgs e)
        {
            if (IsValidHex(this.txtSeed.Text))
            {
                Options.Seed = this.txtSeed.Text;
            }
        }

        private bool IsValidHex(string hex)
        {
            return hex.Length == 8 && Regex.IsMatch(hex, "^[0-9A-Fa-f]+$");
        }

        private void lbSeed_SizeChanged(object sender, EventArgs e)
        {
            this.txtSeed.Location = new Point(this.lbSeed.Location.X + this.lbSeed.Width + 6, this.txtSeed.Location.Y);
        }
    }

    public class RGSSPackOptions : VersionOptions
    {
        public string Seed { get; set; } = "00000000";
    }
}
