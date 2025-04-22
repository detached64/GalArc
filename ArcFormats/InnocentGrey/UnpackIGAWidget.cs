using GalArc.Templates;
using System;

namespace ArcFormats.InnocentGrey
{
    public partial class UnpackIGAWidget : WidgetTemplate
    {
        public static UnpackIGAWidget Instance { get; } = new UnpackIGAWidget();

        public ScriptUnpackOptions Options = new ScriptUnpackOptions();

        public UnpackIGAWidget()
        {
            InitializeComponent();
        }

        private void chkbxDecScr_CheckedChanged(object sender, EventArgs e)
        {
            Options.DecryptScripts = this.chkbxDecScr.Checked;
        }
    }
}
