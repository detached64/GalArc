using GalArc.Common;
using GalArc.Controls;
using GalArc.Settings;
using System;
using System.Linq;

namespace GalArc.GUI
{
    public partial class PreferenceSettings : SettingsTemplate
    {
        public static PreferenceSettings Instance { get; } = new PreferenceSettings();

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

            string encoding = GUISettings.Default.DefaultEncoding;
            if (this.combEncoding.Items.Contains(encoding))
            {
                this.combEncoding.Text = GUISettings.Default.DefaultEncoding;
            }
            else
            {
                this.combEncoding.Text = "UTF-8";
            }
        }

        private void combEncoding_SelectedIndexChanged(object sender, EventArgs e)
        {
            GUISettings.Default.DefaultEncoding = this.combEncoding.Text;
            GUISettings.Default.Save();
        }
    }
}
