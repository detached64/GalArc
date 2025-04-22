using GalArc.Templates;
using System;

namespace ArcFormats.Majiro
{
    public partial class UnpackARCWidget : WidgetTemplate
    {
        public static UnpackARCWidget Instance { get; } = new UnpackARCWidget();

        public ScriptUnpackOptions Options = new ScriptUnpackOptions();

        public UnpackARCWidget()
        {
            InitializeComponent();
        }

        private void chkbxDecScr_CheckedChanged(object sender, EventArgs e)
        {
            Options.DecryptScripts = chkbxDecScr.Checked;
        }
    }
}
