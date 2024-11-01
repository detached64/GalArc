using ArcFormats.Properties;
using GalArc.Extensions;
using GalArc.Extensions.GARbroDB;
using GalArc.Logs;
using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ArcFormats.Seraph
{
    public partial class UnpackDATOptions : UserControl
    {
        internal static string specifiedIndexOffsetString = "00000000";

        internal static bool useSpecifiedIndexOffset = false;

        internal static bool useBrutalForce = true;

        public UnpackDATOptions()
        {
            InitializeComponent();
            this.txtIndexOffset.Enabled = false;
            ImportSchemesFromGARbroDB();
        }

        private void chkbxSpecifyIndex_SizeChanged(object sender, EventArgs e)
        {
            this.txtIndexOffset.Location = new Point(this.chkbxSpecifyIndex.Location.X + this.chkbxSpecifyIndex.Width + 6, this.txtIndexOffset.Location.Y);
        }

        private void chkbxSpecifyIndex_CheckedChanged(object sender, EventArgs e)
        {
            this.txtIndexOffset.Enabled = this.chkbxSpecifyIndex.Checked;
            useSpecifiedIndexOffset = this.chkbxSpecifyIndex.Checked;
        }

        private void txtIndexOffset_TextChanged(object sender, EventArgs e)
        {
            if (IsValidHex(this.txtIndexOffset.Text))
            {
                specifiedIndexOffsetString = this.txtIndexOffset.Text;
            }
        }

        private bool IsValidHex(string hex)
        {
            return hex.Length == 8 && Regex.IsMatch(hex, @"^[0-9A-Fa-f]+$");
        }

        private void chkbxBrutalForce_CheckedChanged(object sender, EventArgs e)
        {
            useBrutalForce = this.chkbxBrutalForce.Checked;
        }

        private void ImportSchemesFromGARbroDB()
        {
            if (ExtensionsConfig.IsEnabled)
            {
                Deserializer.LoadScheme();
            }

            if (DAT.KnownSchemes == null)
            {
                DAT.KnownSchemes = Deserializer.Deserialize(SeraphScheme.Instance);
                if (DAT.KnownSchemes != null)
                {
                    Logger.Debug(string.Format(Resources.logImportGARbroDBSchemeSuccess, DAT.KnownSchemes[SeraphScheme.JsonNodeName].Count));
                }
            }
        }

    }
}
