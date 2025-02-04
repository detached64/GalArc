using GalArc.Common;
using GalArc.Controls;
using GalArc.GUI.Properties;
using System;
using System.Linq;

namespace GalArc.GUI
{
    public partial class PreferenceSettings : SettingsTemplate
    {
        private static readonly Lazy<PreferenceSettings> lazyInstance =
                new Lazy<PreferenceSettings>(() => new PreferenceSettings());

        public static PreferenceSettings Instance => lazyInstance.Value;

        private PreferenceSettings()
        {
            InitializeComponent();
        }

        private void PreferenceSettings_Load(object sender, EventArgs e)
        {
            if (this.combEncoding.Items.Count == 0)
            {
                this.combEncoding.Items.AddRange(Encodings.CodePages.Keys.ToArray());
            }

            string encoding = Settings.Default.DefaultEncoding;
            if (this.combEncoding.Items.Contains(encoding))
            {
                this.combEncoding.Text = Settings.Default.DefaultEncoding;
            }
            else
            {
                this.combEncoding.Text = "UTF-8";
            }
        }

        private void combEncoding_SelectedIndexChanged(object sender, EventArgs e)
        {
            Settings.Default.DefaultEncoding = this.combEncoding.Text;
            Settings.Default.Save();
        }
    }
}
