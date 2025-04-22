using GalArc.Templates;
using System;

namespace ArcFormats.GsPack
{
    public partial class UnpackPAKWidget : WidgetTemplate
    {
        public static UnpackPAKWidget Instance { get; } = new UnpackPAKWidget();

        public ScriptUnpackOptions Options = new ScriptUnpackOptions();

        public UnpackPAKWidget()
        {
            InitializeComponent();
        }

        private void chkbxDecScr_CheckedChanged(object sender, EventArgs e)
        {
            Options.DecryptScripts = this.chkbxDecScr.Checked;
        }
    }
}
