using GalArc.Templates;
using System;

namespace ArcFormats.InnocentGrey
{
    public partial class PackIGAWidget : WidgetTemplate
    {
        public static PackIGAWidget Instance { get; } = new PackIGAWidget();

        public ScriptPackOptions Options = new ScriptPackOptions();

        public PackIGAWidget()
        {
            InitializeComponent();
        }

        private void chkbxEncScr_CheckedChanged(object sender, EventArgs e)
        {
            Options.EncryptScripts = this.chkbxEncScr.Checked;
        }
    }
}
