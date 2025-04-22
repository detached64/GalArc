using GalArc.Controls;
using GalArc.Database;
using GalArc.Strings;
using System;
using System.Reflection;
using System.Windows.Forms;

namespace ArcFormats.NScripter
{
    public partial class UnpackNS2Widget : WidgetTemplate
    {
        public static UnpackNS2Widget Instance { get; } = new UnpackNS2Widget();

        public Ns2Options Options = new Ns2Options();

        public Ns2Scheme Scheme;

        public UnpackNS2Widget()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
            Type type = this.combSchemes.GetType();
            PropertyInfo pi = type.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(this.combSchemes, true, null);
        }

        private void UnpackNS2Options_Load(object sender, EventArgs e)
        {
            AddSchemes();
        }

        public override void AddSchemes()
        {
            this.combSchemes.Items.Clear();
            this.combSchemes.Items.Add(GUIStrings.ItemNoEnc);
            if (Scheme?.KnownKeys != null)
            {
                foreach (var key in Scheme.KnownKeys)
                {
                    this.combSchemes.Items.Add(key.Key);
                }
            }
            this.combSchemes.SelectedIndex = 0;
        }

        private void combSchemes_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (combSchemes.SelectedIndex)
            {
                case 0:
                    Options.Key = null;
                    break;
                default:
                    Options.Key = Scheme.KnownKeys[combSchemes.Text];
                    break;
            }
        }
    }

    public class Ns2Options
    {
        public string Key { get; set; }
    }
}
