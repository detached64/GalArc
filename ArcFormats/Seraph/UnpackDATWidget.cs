using GalArc.Controls;
using GalArc.Database;
using System;
using System.Drawing;
using System.Text.RegularExpressions;

namespace ArcFormats.Seraph
{
    public partial class UnpackDATWidget : WidgetTemplate
    {
        public static UnpackDATWidget Instance { get; } = new UnpackDATWidget();

        public SeraphOptions Options = new SeraphOptions();

        public SeraphScheme Scheme;

        public UnpackDATWidget()
        {
            InitializeComponent();
        }

        private void chkbxSpecifyIndex_SizeChanged(object sender, EventArgs e)
        {
            this.txtIndexOffset.Location = new Point(this.chkbxSpecifyIndex.Location.X + this.chkbxSpecifyIndex.Width + 6, this.txtIndexOffset.Location.Y);
        }

        private void chkbxSpecifyIndex_CheckedChanged(object sender, EventArgs e)
        {
            this.txtIndexOffset.Enabled = this.chkbxSpecifyIndex.Checked;
            Options.UseSpecifiedIndexOffset = this.chkbxSpecifyIndex.Checked;
        }

        private void txtIndexOffset_TextChanged(object sender, EventArgs e)
        {
            if (IsValidHex(this.txtIndexOffset.Text))
            {
                Options.SpecifiedIndexOffsetString = this.txtIndexOffset.Text;
            }
        }

        private bool IsValidHex(string hex)
        {
            return hex.Length == 8 && Regex.IsMatch(hex, "^[0-9A-Fa-f]+$");
        }

        private void chkbxBrutalForce_CheckedChanged(object sender, EventArgs e)
        {
            Options.UseBrutalForce = this.chkbxBrutalForce.Checked;
        }
    }

    public class SeraphOptions : ArcOptions
    {
        public bool UseBrutalForce { get; set; } = true;
        public bool UseSpecifiedIndexOffset { get; set; }
        public string SpecifiedIndexOffsetString { get; set; } = "00000000";
    }
}