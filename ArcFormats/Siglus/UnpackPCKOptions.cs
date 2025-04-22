using GalArc.Controls;
using GalArc.Database;
using GalArc.Strings;
using System;
using System.Reflection;
using System.Windows.Forms;

namespace ArcFormats.Siglus
{
    public partial class UnpackPCKOptions : OptionsTemplate
    {
        public static UnpackPCKOptions Instance { get; } = new UnpackPCKOptions();

        public SiglusOptions Options = new SiglusOptions();

        public SiglusScheme Scheme;

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
            if (Scheme?.KnownKeys != null)
            {
                foreach (var s in Scheme.KnownKeys)
                {
                    this.combSchemes.Items.Add(s.Key);
                }
            }
            this.combSchemes.SelectedIndex = 0;
        }

        private void combSchemes_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (this.combSchemes.SelectedIndex)
            {
                case 0:
                    Options.TryEachKey = true;
                    Options.Key = null;
                    this.lbKey.Text = string.Empty;
                    break;
                default:
                    byte[] key = Scheme.KnownKeys[this.combSchemes.Text];
                    try
                    {
                        this.lbKey.Text = string.Format(LogStrings.Key, BitConverter.ToString(key));
                        Options.Key = key;
                    }
                    catch
                    {
                        this.lbKey.Text = LogStrings.KeyParseError;
                        Options.Key = null;
                    }
                    Options.TryEachKey = false;
                    break;
            }
        }
    }

    public class SiglusOptions : ArcOptions
    {
        public byte[] Key { get; set; }
        public bool TryEachKey { get; set; }
    }
}
