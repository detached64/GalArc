using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ArcFormats.Seraph
{
    public partial class UnpackDATOptions : UserControl
    {
        internal static string SpecifiedIndexOffsetString = "00000000";

        internal static bool UseSpecifiedIndexOffset = false;

        internal static bool UseBrutalForce = true;

        public UnpackDATOptions()
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
            UseSpecifiedIndexOffset = this.chkbxSpecifyIndex.Checked;
        }

        private void txtIndexOffset_TextChanged(object sender, EventArgs e)
        {
            if (IsValidHex(this.txtIndexOffset.Text))
            {
                SpecifiedIndexOffsetString = this.txtIndexOffset.Text;
            }
        }

        private bool IsValidHex(string hex)
        {
            return hex.Length == 8 && Regex.IsMatch(hex, "^[0-9A-Fa-f]+$");
        }

        private void chkbxBrutalForce_CheckedChanged(object sender, EventArgs e)
        {
            UseBrutalForce = this.chkbxBrutalForce.Checked;
        }
    }
}
