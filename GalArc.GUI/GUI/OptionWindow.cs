using Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace GalArc.GUI
{
    public partial class OptionDialog : Form
    {
        public static OptionDialog Instance;

        public OptionDialog()
        {
            Instance = this;
            InitializeComponent();
            Controller.Localize.SetLocalCulture(MainWindow.LocalCulture);
            Controller.Localize.GetStrings_option();
            LoadState();
        }

        private void OptionWindow_Load(object sender, EventArgs e)
        {
            this.op_cbLang.Text = Resource.Languages.languages.FirstOrDefault(x => x.Value == MainWindow.LocalCulture).Key;
        }

        private void op_cbLang_SelectedIndexChanged(object sender, EventArgs e)
        {
            MainWindow.LocalCulture = Resource.Languages.languages[this.op_cbLang.Text];
            Controller.Localize.SetLocalCulture(MainWindow.LocalCulture);
            Controller.Localize.RefreshStrings();
            if (Resource.Global.AutoSaveLanguage)
            {
                Properties.Settings.Default.lastLang = MainWindow.LocalCulture;
                Properties.Settings.Default.Save();
            }
        }

        private void op_chkbxTopMost_CheckedChanged(object sender, EventArgs e)
        {
            if (this.op_chkbxOnTop.Checked)
            {
                LogWindow.Instance.TopMost = true;
                MainWindow.Instance.TopMost = true;
                if (Resource.Global.AutoSaveTopMost)
                {

                    Properties.Settings.Default.isTopMost = true;
                    Properties.Settings.Default.Save();
                }
            }
            else
            {
                LogWindow.Instance.TopMost = false;
                MainWindow.Instance.TopMost = false;
                if (Resource.Global.AutoSaveTopMost)
                {
                    Properties.Settings.Default.isTopMost = false;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private void LoadState()
        {
            if (Resource.Global.AutoSaveTopMost)
            {
                this.op_chkbxOnTop.Checked = Properties.Settings.Default.isTopMost;
            }
        }
    }
}
