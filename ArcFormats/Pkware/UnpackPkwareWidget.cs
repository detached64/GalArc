using GalArc.Controls;
using GalArc.Database;
using System;

namespace ArcFormats.Pkware
{
    public partial class UnpackPkwareWidget : WidgetTemplate
    {
        public static UnpackPkwareWidget Instance { get; } = new UnpackPkwareWidget();

        public PkwareOptions Options = new PkwareOptions();

        public PkwareScheme Scheme;

        public UnpackPkwareWidget()
        {
            InitializeComponent();
        }

        private void UnpackPkwareOptions_Load(object sender, EventArgs e)
        {
            AddSchemes();
        }

        public override void AddSchemes()
        {
            combSchemes.Items.Clear();
            foreach (var scheme in Scheme.KnownKeys)
            {
                combSchemes.Items.Add(scheme.Key);
            }
            combSchemes.SelectedIndex = 0;
        }

        private void combSchemes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Scheme.KnownKeys.TryGetValue(combSchemes.Text, out var key))
            {
                Options.ContentKey = key.ContentKey;
                Options.PlayerKey = key.PlayerKey;
            }
        }
    }

    public class PkwareOptions : ArcOptions
    {
        public string ContentKey { get; set; }
        public string PlayerKey { get; set; }
    }
}
