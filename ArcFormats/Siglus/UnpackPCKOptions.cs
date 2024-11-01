using ArcFormats.Properties;
using GalArc.DataBase;
using GalArc.DataBase.Siglus;
using GalArc.Logs;
using System;
using System.Reflection;
using System.Windows.Forms;
using Utility;

namespace ArcFormats.Siglus
{
    public partial class UnpackPCKOptions : UserControl
    {
        public UnpackPCKOptions()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
            Type type = this.combSchemes.GetType();
            PropertyInfo pi = type.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(this.combSchemes, true, null);

            ImportSchemes();
            AddSchemesToComboBox();
        }

        private void ImportSchemes()
        {
            if (ScenePCK.KnownSchemes == null)
            {
                ScenePCK.KnownSchemes = Deserializer.ReadScheme(SiglusScheme.EngineName, "KnownSchemes", SiglusScheme.Instance);
                if (ScenePCK.KnownSchemes != null)
                {
                    Logger.Debug(string.Format(Resources.logImportDataBaseSuccess, ScenePCK.KnownSchemes.Count));
                }
            }
        }

        private void AddSchemesToComboBox()
        {
            if (ScenePCK.KnownSchemes != null)
            {
                this.combSchemes.Items.Add(Siglus.combItemTryEachScheme);

                foreach (var scheme in ScenePCK.KnownSchemes)
                {
                    this.combSchemes.Items.Add(scheme.Key);
                }
                this.combSchemes.SelectedIndex = 0;
            }
        }

        private void combSchemes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.combSchemes.SelectedIndex == 0)
            {
                ScenePCK.SelectedScheme = null;
                ScenePCK.TryEachKey = true;
                this.lbKey.Text = string.Empty;
                return;
            }
            else
            {
                SiglusScheme scheme = (SiglusScheme)ScenePCK.KnownSchemes[this.combSchemes.Text];
                try
                {
                    ScenePCK.SelectedScheme = new Tuple<string, byte[]>(scheme.KnownKey, Utils.HexStringToByteArray(scheme.KnownKey, '-'));
                    this.lbKey.Text = string.Format(Siglus.lbKey, scheme.KnownKey);
                }
                catch
                {
                    ScenePCK.SelectedScheme = null;
                    this.lbKey.Text = Siglus.lbKeyParseError;
                }
                ScenePCK.TryEachKey = false;
            }
        }
    }
}
