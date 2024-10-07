using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ArcFormats.RPGMaker
{
    public partial class PackRGSSOptions : UserControl
    {
        internal static string inputSeedString = "00000000";

        public PackRGSSOptions(string versions)
        {
            InitializeComponent();
            this.txtSeed.Text = inputSeedString;
            this.combVersion.Items.AddRange(versions.Split('/'));
            if (this.combVersion.Items.Count > 0)
            {
                this.combVersion.SelectedIndex = 0;
            }
        }

        private void combVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            Global.Version = this.combVersion.Text;
            if (this.combVersion.Text == "1")
            {
                this.txtSeed.Enabled = false;
                this.lbSeed.Enabled = false;
            }
            else
            {
                this.txtSeed.Enabled = true;
                this.lbSeed.Enabled = true;
            }
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
            return hex.Length == 8 && Regex.IsMatch(hex, @"^[0-9A-Fa-f]+$");
        }

        private void lbSeed_SizeChanged(object sender, EventArgs e)
        {
            this.txtSeed.Location = new Point(this.lbSeed.Location.X + this.lbSeed.Width + 6, this.txtSeed.Location.Y);
        }
    }
}
