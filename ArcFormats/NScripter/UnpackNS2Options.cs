using GalArc.Controls;
using GalArc.Strings;
using System;
using System.Reflection;
using System.Windows.Forms;

namespace ArcFormats.NScripter
{
    public partial class UnpackNS2Options : OptionsTemplate
    {
        public static UnpackNS2Options Instance { get; } = new UnpackNS2Options();

        public UnpackNS2Options()
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
            if (NS2.Scheme?.KnownKeys != null)
            {
                foreach (var key in NS2.Scheme.KnownKeys)
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
                    NS2.Key = null;
                    break;
                default:
                    NS2.Key = NS2.Scheme.KnownKeys[combSchemes.Text];
                    break;
            }
        }
    }
}
