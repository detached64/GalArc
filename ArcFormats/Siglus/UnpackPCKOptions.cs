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
            AddSchemes();
        }

        public override void AddSchemes()
        {
            this.combSchemes.Items.Clear();
            this.combSchemes.Items.Add(GUIStrings.ItemTryEveryEnc);
            if (ScenePCK.Scheme?.KnownKeys != null)
            {
                foreach (var scheme in ScenePCK.Scheme.KnownKeys)
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
                    ScenePCK.SelectedKey = null;
                    ScenePCK.TryEachKey = true;
                    this.lbKey.Text = string.Empty;
                    break;
                default:
                    byte[] key = ScenePCK.Scheme.KnownKeys[this.combSchemes.Text];
                    try
                    {
                        ScenePCK.SelectedKey = key;
                        this.lbKey.Text = string.Format(LogStrings.Key, BitConverter.ToString(key));
                    }
                    catch
                    {
                        ScenePCK.SelectedKey = null;
                        this.lbKey.Text = LogStrings.KeyParseError;
                    }
                    ScenePCK.TryEachKey = false;
                    break;
            }
        }
    }
}
