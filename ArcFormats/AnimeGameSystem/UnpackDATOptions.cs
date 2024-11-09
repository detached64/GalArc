using ArcFormats.Properties;
using GalArc.Extensions.GARbroDB;
using GalArc.Logs;
using System;
using System.Windows.Forms;

namespace ArcFormats.AnimeGameSystem
{
    public partial class UnpackDATOptions : UserControl
    {
        public UnpackDATOptions()
        {
            InitializeComponent();
            ImportSchemesFromGARbroDB();
        }

        private void ImportSchemesFromGARbroDB()
        {
            if (DAT.ImportedSchemes == null)
            {
                DAT.ImportedSchemes = Deserializer.Deserialize(typeof(AGSScheme), AGSScheme.JsonEngineName) as AGSScheme;
                if (DAT.ImportedSchemes != null)
                {
                    foreach (var scheme in DAT.ImportedSchemes.KnownSchemes)
                    {
                        this.combSchemes.Items.Add(scheme.Key);
                    }
                    this.combSchemes.SelectedIndex = 0;
                    Logger.Debug(string.Format(Resources.logImportGARbroDBSchemeSuccess, DAT.ImportedSchemes.KnownSchemes.Count));
                }
            }
        }

        private void combSchemes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.combSchemes.SelectedIndex > 0)
            {
                try
                {
                    DAT.SelectedScheme = DAT.ImportedSchemes.KnownSchemes[this.combSchemes.SelectedItem.ToString()];
                }
                catch
                {
                    DAT.SelectedScheme = null;
                }
            }
        }
    }
}
