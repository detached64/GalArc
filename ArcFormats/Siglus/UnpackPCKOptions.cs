using GalArc.Controls;
using GalArc.Strings;
using System;
using System.Reflection;
using System.Windows.Forms;

namespace ArcFormats.Siglus
{
    public partial class UnpackPCKOptions : OptionsTemplate
    {
        public static UnpackPCKOptions Instance { get; } = new UnpackPCKOptions();

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
            this.combSchemes.Items.Add(GUIStrings.ItemTryEveryEnc);
            if (ScenePCK.Scheme != null)
            {
                foreach (var scheme in ScenePCK.Scheme.KnownSchemes)
                {
                    this.combSchemes.Items.Add(scheme.Key);
                }
            }
            this.combSchemes.SelectedIndex = 0;
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
                default:
                    byte[] key = ScenePCK.Scheme.KnownSchemes[this.combSchemes.Text].KnownKey;
                    try
                    {
                        ScenePCK.SelectedScheme = new Tuple<string, byte[]>(this.combSchemes.Text, key);
                        this.lbKey.Text = string.Format(LogStrings.Key, BitConverter.ToString(key));
                    }
                    catch
                    {
                        ScenePCK.SelectedScheme = null;
                        this.lbKey.Text = LogStrings.KeyParseError;
                    }
                    ScenePCK.TryEachKey = false;
                    break;
            }
        }
    }
}
