using GalArc.Controls;
using System;
using System.Reflection;
using System.Windows.Forms;

namespace ArcFormats.AnimeGameSystem
{
    public partial class UnpackDATOptions : OptionsTemplate
    {
        public static UnpackDATOptions Instance { get; } = new UnpackDATOptions();

        public UnpackDATOptions()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
            Type type = this.combSchemes.GetType();
            PropertyInfo pi = type.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(this.combSchemes, true, null);
        }

        private void UnpackDATOptions_Load(object sender, EventArgs e)
        {
            if (DAT.Scheme != null)
            {
                foreach (var scheme in DAT.Scheme.KnownSchemes)
                {
                    this.combSchemes.Items.Add(scheme.Key);
                }
            }
            this.combSchemes.SelectedIndex = 0;
        }

        private void combSchemes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.combSchemes.SelectedIndex > 0)
            {
                try
                {
                    DAT.SelectedScheme = DAT.Scheme.KnownSchemes[this.combSchemes.SelectedItem.ToString()];
                }
                catch
                {
                    DAT.SelectedScheme = null;
                }
            }
        }
    }
}
