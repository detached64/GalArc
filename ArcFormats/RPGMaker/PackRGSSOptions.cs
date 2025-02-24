using GalArc.Controls;
using System;
using System.Drawing;
using System.Text.RegularExpressions;

namespace ArcFormats.RPGMaker
{
    public partial class PackRGSSOptions : OptionsTemplate
    {
        public static PackRGSSOptions Instance { get; } = new PackRGSSOptions();

        internal static string inputSeedString = "00000000";

        private readonly string Versions = "1/3";

        public PackRGSSOptions()
        {
            InitializeComponent();
            this.txtSeed.Text = inputSeedString;
            this.combVersion.Items.AddRange(Versions.Split('/'));
            if (this.combVersion.Items.Count > 0)
            {
                this.combVersion.SelectedIndex = 0;
            }
        }

        private void combVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            Version = this.combVersion.Text;
            this.txtSeed.Visible = this.combVersion.Text != "1";
            this.lbSeed.Visible = this.combVersion.Text != "1";
        }

        private void txtSeed_TextChanged(object sender, EventArgs e)
        {
            if (IsValidHex(this.txtSeed.Text))
            {
                inputSeedString = this.txtSeed.Text;
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
}
