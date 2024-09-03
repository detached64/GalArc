using Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace GalArc.GUI
{
    public partial class OptionWindow : UserControl
    {
        public static OptionWindow Instance;

        internal static Dictionary<string, string> languages = new Dictionary<string, string>();

        public OptionWindow()
        {
            Instance = this;
            InitializeComponent();
            Controller.Localization.SetLocalCulture(main.LocalCulture);
            Controller.Localization.GetStrings_option();
            LoadState();
            Controller.UpdateContent.InitCombobox_Lang();
        }

        private void OptionWindow_Load(object sender, EventArgs e)
        {
            this.op_cbLang.Text = languages.FirstOrDefault(x => x.Value == main.LocalCulture).Key;
        }

        private void op_cbLang_SelectedIndexChanged(object sender, EventArgs e)
        {
            main.LocalCulture = languages[this.op_cbLang.Text];
            Controller.Localization.SetLocalCulture(main.LocalCulture);
            Controller.Localization.RefreshStrings();
            if (Resource.Global.AutoSaveLanguage)
            {
                Properties.Settings.Default.lastLang = main.LocalCulture;
                Properties.Settings.Default.Save();
            }
        }

        private void op_chkbxTopMost_CheckedChanged(object sender, EventArgs e)
        {
            if (Resource.Global.AutoSaveTopMost)
            {
                if (this.op_chkbxOnTop.Checked)
                {
                    LogWindow.Instance.TopMost = true;
                    main.Main.TopMost = true;
                    Properties.Settings.Default.isTopMost = true;
                    Properties.Settings.Default.Save();
                }
                else
                {
                    LogWindow.Instance.TopMost = false;
                    main.Main.TopMost = false;
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
