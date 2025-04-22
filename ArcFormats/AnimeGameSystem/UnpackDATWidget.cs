using GalArc.Controls;
using GalArc.Database;
using GalArc.Strings;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;

namespace ArcFormats.AnimeGameSystem
{
    public partial class UnpackDATWidget : WidgetTemplate
    {
        public static UnpackDATWidget Instance { get; } = new UnpackDATWidget();

        public AGSOptions Options = new AGSOptions();

        public AGSScheme Scheme;

        public UnpackDATWidget()
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
            AddSchemes();
        }

        public override void AddSchemes()
        {
            this.combSchemes.Items.Clear();
            this.combSchemes.Items.Add(GUIStrings.ItemNoEnc);
            if (Scheme?.KnownSchemes != null)
            {
                foreach (var scheme in Scheme.KnownSchemes)
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
                    Options.FileMap = Scheme.KnownSchemes[this.combSchemes.SelectedItem.ToString()].FileMap;
                }
                catch
                {
                    Options.FileMap = null;
                }
            }
        }
    }

    public class AGSOptions : ArcOptions
    {
        public Dictionary<string, AGSScheme.Key> FileMap;
    }
}
