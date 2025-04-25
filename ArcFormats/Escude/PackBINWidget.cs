using GalArc.Templates;

namespace ArcFormats.Escude
{
    public partial class PackBINWidget : WidgetTemplate
    {
        public static PackBINWidget Instance { get; } = new PackBINWidget();

        public VersionOptions Options = new VersionOptions();

        private readonly string[] Versions = {
            "ACPXPK01",
            "ESC-ARC1",
            "ESC-ARC2"
        };

        public PackBINWidget()
        {
            InitializeComponent();
            this.combVersion.Items.AddRange(Versions);
            this.combVersion.SelectedIndex = 0;
        }

        private void combVersion_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            Options.Version = this.combVersion.Text;
        }
    }
}
