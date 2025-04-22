using ArcFormats.AdvHD;
using GalArc.Controls;
using System;

namespace ArcFormats.Softpal
{
    public partial class PackPACWidget : WidgetTemplate
    {
        public static PackPACWidget Instance { get; } = new PackPACWidget();

        public SoftpalOptions Options = new SoftpalOptions();

        private readonly string Versions = "1/2";

        public PackPACWidget()
        {
            InitializeComponent();
            this.combVersion.Items.AddRange(Versions.Split('/'));
            if (this.combVersion.Items.Count > 0)
            {
                this.combVersion.SelectedIndex = 0;
            }
        }

        private void chkbxCompute_CheckedChanged(object sender, EventArgs e)
        {
            Options.ComputeChecksum = this.chkbxCompute.Checked;
        }

        private void combVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            Options.Version = this.combVersion.Text;
            this.chkbxCompute.Visible = this.combVersion.Text != "1";
        }

        private void chkbxEncScr_CheckedChanged(object sender, EventArgs e)
        {
            Options.EncryptScripts = this.chkbxEncScr.Checked;
        }
    }

    public class SoftpalOptions : AdvHDPackOptions
    {
        public bool ComputeChecksum { get; set; }
    }
}
