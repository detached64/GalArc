using ArcFormats.Properties;
using GalArc;
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
        private string ExtractedKey
        {
            get
            {
                return _ExtractedKey;
            }
            set
            {
                if (string.IsNullOrEmpty(value) || string.Equals(value, "Failed", StringComparison.OrdinalIgnoreCase))
                {
                    _ExtractedKey = null;
                    return;
                }
                _ExtractedKey = value;
            }
        }

        private string _ExtractedKey;

        public UnpackPCKOptions()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
            Type type = this.combSchemes.GetType();
            PropertyInfo pi = type.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(this.combSchemes, true, null);
        }

        private void UnpackPCKOptions_Load(object sender, EventArgs e)
        {
            if (ScenePCK.Scheme != null)
            {
                this.combSchemes.Items.Add(Resources.combTryEveryScheme);
                this.combSchemes.Items.Add(Resources.combCustomScheme);
                foreach (var scheme in ScenePCK.Scheme.KnownSchemes)
                {
                    this.combSchemes.Items.Add(scheme.Key);
                }
            }
            else
            {
                this.combSchemes.Items.Add(Resources.combCustomScheme);
            }
            this.combSchemes.SelectedIndex = 0;
            this.panel.Visible = BaseSettings.Default.IsDatabaseEnabled && BaseSettings.Default.IsSiglusKeyFinderEnabled && KeyFinder.IsValidExe();
        }

        private void combSchemes_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (this.combSchemes.SelectedIndex)
            {
                case 0:
                    ScenePCK.SelectedScheme = null;
                    ScenePCK.TryEachKey = true;
                    this.lbKey.Text = string.Empty;
                    break;
                case 1:
                    try
                    {
                        ScenePCK.SelectedScheme = new Tuple<string, byte[]>(this.combSchemes.Text, Utils.HexStringToByteArray(ExtractedKey));
                    }
                    catch
                    {
                        ScenePCK.SelectedScheme = null;
                    }
                    ScenePCK.TryEachKey = false;
                    this.lbKey.Text = string.Format(Siglus.lbKey, ExtractedKey ?? Siglus.empty);
                    break;
                default:
                    byte[] key = ScenePCK.Scheme.KnownSchemes[this.combSchemes.Text].KnownKey;
                    try
                    {
                        ScenePCK.SelectedScheme = new Tuple<string, byte[]>(this.combSchemes.Text, key);
                        this.lbKey.Text = string.Format(Siglus.lbKey, BitConverter.ToString(key));
                    }
                    catch
                    {
                        ScenePCK.SelectedScheme = null;
                        this.lbKey.Text = Siglus.lbKeyParseError;
                    }
                    ScenePCK.TryEachKey = false;
                    break;
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

                    if (ExtractedKey != null)
                    {
                        this.lbKey.Text = string.Format(Siglus.lbKey, ExtractedKey);
                        Logger.Info(string.Format(Siglus.logFound, ExtractedKey));
                    }
                    else
                    {
                        Logger.Info(Siglus.logFailedFindKey);
                    }
                    this.combSchemes.SelectedIndex = 1;
                }
            }
        }

    }
}
