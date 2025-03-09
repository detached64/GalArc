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
            this.combSchemes.Items.Add(GUIStrings.ItemCustomEnc);
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
                    txtKey.Text = string.Empty;
                    txtKey.Enabled = false;
                    break;
                case 1:
                    txtKey.Text = string.Empty;
                    txtKey.Enabled = true;
                    break;
                default:
                    txtKey.Text = NS2.Scheme.KnownKeys[combSchemes.SelectedItem.ToString()];
                    txtKey.Enabled = true;
                    break;
            }
        }

        private void txtKey_TextChanged(object sender, EventArgs e)
        {
            NS2.Key = txtKey.Text;
        }
    }
}
