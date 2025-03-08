using GalArc.Controls;
using GalArc.Strings;
using System;
using System.Reflection;
using System.Windows.Forms;

namespace ArcFormats.Qlie
{
    public partial class UnpackPACKOptions : OptionsTemplate
    {
        public static UnpackPACKOptions Instance { get; } = new UnpackPACKOptions();

        public UnpackPACKOptions()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
            Type type = this.combSchemes.GetType();
            PropertyInfo pi = type.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(this.combSchemes, true, null);
        }

        private void UnpackPACKOptions_Load(object sender, EventArgs e)
        {
            combSchemes.Items.Add(GUIStrings.ItemDefaultEnc);
            if (PACK.Scheme != null)
            {
                foreach (var key in PACK.Scheme.KnownKeys.Keys)
                {
                    combSchemes.Items.Add(key);
                }
            }
            combSchemes.SelectedIndex = 0;
        }

        private void combSchemes_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (combSchemes.SelectedIndex)
            {
                case 0:
                    PACK.SelectedKey = string.Empty;
                    break;
                default:
                    PACK.SelectedKey = PACK.Scheme.KnownKeys[combSchemes.Text];
                    break;
            }
        }

        private void btSelect_Click(object sender, EventArgs e)
        {
            this.txtPath.Text = ChooseFile("key.fkey|key.fkey") ?? this.txtPath.Text;
        }

        private void txtPath_TextChanged(object sender, EventArgs e)
        {
            PACK.FKeyPath = this.txtPath.Text;
        }

        private void chkbxSaveHash_CheckedChanged(object sender, EventArgs e)
        {
            PACK.SaveHash = this.chkbxSaveHash.Checked;
        }

        private void lbSaveKey_CheckedChanged(object sender, EventArgs e)
        {
            PACK.SaveKey = this.chkbxSaveKey.Checked;
        }
    }
}
