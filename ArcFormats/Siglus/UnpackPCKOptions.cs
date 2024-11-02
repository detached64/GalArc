using ArcFormats.Properties;
using GalArc.DataBase;
using GalArc.DataBase.Siglus;
using GalArc.Extensions;
using GalArc.Extensions.SiglusKeyFinder;
using GalArc.Logs;
using System;
using System.Reflection;
using System.Windows.Forms;
using Utility;

namespace ArcFormats.Siglus
{
    public partial class UnpackPCKOptions : UserControl
    {
        private static string ExtractedKey
        {
            get
            {
                return _ExtractedKey;
            }
            set
            {
                if (string.IsNullOrEmpty(value) || String.Equals(value, "Failed", StringComparison.OrdinalIgnoreCase))
                {
                    _ExtractedKey = null;
                    return;
                }
                _ExtractedKey = value;
            }
        }

        private static string _ExtractedKey;

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
                this.combSchemes.Items.Add(Siglus.combCustom);

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
            else if (this.combSchemes.SelectedIndex == 1)
            {
                try
                {
                    ScenePCK.SelectedScheme = new Tuple<string, byte[]>(this.combSchemes.Text, Utils.HexStringToByteArray(ExtractedKey, '-'));
                }
                catch
                {
                    ScenePCK.SelectedScheme = null;
                    this.lbKey.Text = Siglus.lbKeyParseError;
                }
                ScenePCK.TryEachKey = false;
                this.lbKey.Text = string.Format(Siglus.lbKey, ExtractedKey ?? Siglus.empty);
                return;
            }
            else
            {
                SiglusScheme scheme = (SiglusScheme)ScenePCK.KnownSchemes[this.combSchemes.Text];
                try
                {
                    ScenePCK.SelectedScheme = new Tuple<string, byte[]>(this.combSchemes.Text, Utils.HexStringToByteArray(scheme.KnownKey, '-'));
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

        private void btCheckExe_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Game Executable file (*.exe)|*.exe";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        ExtractedKey = KeyFinder.FindKey(openFileDialog.FileName);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex.Message, false);
                        return;
                    }
                }
                if (ExtractedKey != null)
                {
                    this.lbKey.Text = string.Format(Siglus.lbKey, ExtractedKey);
                    Logger.InfoRevoke(string.Format(Siglus.logFound, ExtractedKey));
                }
                else
                {
                    Logger.InfoRevoke(Siglus.logFailedFindKey);
                }
                this.combSchemes.SelectedIndex = 1;
            }
        }

        private void UnpackPCKOptions_Load(object sender, EventArgs e)
        {
            if (ExtensionsConfig.IsEnabled && KeyFinderConfig.IsSiglusKeyFinderEnabled && KeyFinder.IsValidExe())
            {
                this.panel.Visible = true;
            }
            else
            {
                this.panel.Visible = false;
            }
        }
    }
}
