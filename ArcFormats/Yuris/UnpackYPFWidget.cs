using GalArc.Templates;
using System;

namespace ArcFormats.Yuris
{
    public partial class UnpackYPFWidget : WidgetTemplate
    {
        public static UnpackYPFWidget Instance { get; } = new UnpackYPFWidget();

        public ScriptUnpackOptions Options = new ScriptUnpackOptions();

        public UnpackYPFWidget()
        {
            InitializeComponent();
        }

        private void chkbxDecScr_CheckedChanged(object sender, EventArgs e)
        {
            Options.DecryptScripts = this.chkbxDecScr.Checked;
        }
    }
}
