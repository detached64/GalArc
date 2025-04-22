using ArcFormats;
using GalArc.Common;
using GalArc.Templates;
using System;
using System.Text;

namespace GalArc.GUI
{
    public partial class EncodingSettings : SettingsTemplate
    {
        private readonly EncodingSetting Setting;

        private bool is_loading = true;

        public EncodingSettings(EncodingSetting setting)
        {
            InitializeComponent();
            this.Setting = setting;
        }

        private void EncodingSettings_Load(object sender, EventArgs e)
        {
            this.combEncoding.DataSource = Encodings.SupportedEncodings;
            this.combEncoding.DisplayMember = "EncodingName";
            this.combEncoding.SelectedItem = (Encoding)Setting.Value;
            is_loading = false;
        }

        private void combEncoding_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!is_loading && this.combEncoding.SelectedItem != null)
            {
                Setting.Value = this.combEncoding.SelectedItem;
            }
        }
    }
}
