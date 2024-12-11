using ArcFormats.Properties;
using GalArc.Extensions.GARbroDB;
using GalArc.Logs;
using System;
using System.Reflection;
using System.Windows.Forms;

namespace ArcFormats.AnimeGameSystem
{
    public partial class UnpackDATOptions : UserControl
    {
        public UnpackDATOptions()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
            Type type = this.combSchemes.GetType();
            PropertyInfo pi = type.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(this.combSchemes, true, null);

            ImportSchemesFromGARbroDB();
            this.combSchemes.SelectedIndex = 0;
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
