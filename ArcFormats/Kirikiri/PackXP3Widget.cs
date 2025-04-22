using GalArc.Templates;
using System;

namespace ArcFormats.Kirikiri
{
    public partial class PackXP3Widget : WidgetTemplate
    {
        public static PackXP3Widget Instance { get; } = new PackXP3Widget();

        public KirikiriOptions Options = new KirikiriOptions();

        private readonly string Versions = "1/2";

        public PackXP3Widget()
        {
            InitializeComponent();
            this.combVersion.Items.AddRange(Versions.Split('/'));
            if (this.combVersion.Items.Count > 0)
            {
                this.combVersion.SelectedIndex = 0;
            }
        }

        private void combVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            Options.Version = this.combVersion.Text;
        }

        private void chkbxComIndex_CheckedChanged(object sender, EventArgs e)
        {
            Options.CompressIndex = chkbxComIndex.Checked;
        }

        private void chkbxComContents_CheckedChanged(object sender, EventArgs e)
        {
            Options.CompressContents = chkbxComContents.Checked;
        }
    }

    public class KirikiriOptions : VersionOptions
    {
        public bool CompressIndex { get; set; } = true;
        public bool CompressContents { get; set; } = true;
    }
}
