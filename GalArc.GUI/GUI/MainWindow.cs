using GalArc.Controller;
using GalArc.GUI;
using GalArc.Properties;
using GalArc.Resource;
using Log;
using System;
using System.Windows.Forms;
using System.Linq;

namespace GalArc
{
    public partial class MainWindow : Form
    {
        public static MainWindow Instance;

        internal static string LocalCulture;

        public MainWindow()
        {
            Instance = this;
            InitializeComponent();

            LocalCulture = Localize.GetLocalCulture();

            LogUtility.NewInstance();

            LogWindow logWindow = new LogWindow();
            UnpackWindow unpackWindow = new UnpackWindow();
            PackWindow packWindow = new PackWindow();

            unpackPage.Controls.Add(unpackWindow);
            packPage.Controls.Add(packWindow);

            unpackWindow.Dock = DockStyle.Fill;
            packWindow.Dock = DockStyle.Fill;

            Localize.SetLocalCulture(LocalCulture);
            Localize.GetStrings_main();

            //Log.Controller.Localize.SetLocalCulture(LocalCulture);
            Log.Controller.Localize.GetStrings_Log();

            UpdateContent.InitCombobox_Engines();
            UpdateContent.InitCombobox_Languages();
            UpdateContent.InitEncoding();

            LogUtility.Process += ChangeLabel;
            LogUtility.ErrorOccured += ChangeLabel;
        }

        private void main_Load(object sender, EventArgs e)
        {
            LoadState();
        }

        private void main_FormClosing(object sender, FormClosingEventArgs e)
        {
            LogWindow.Instance.Dispose();
        }
        private void main_LocationChanged(object sender, EventArgs e)
        {
            LogWindow.Instance.ChangePosition(this.Location.X, this.Location.Y);
        }

        private void LoadState()
        {
            if (Global.AutoSavePage)
            {
                this.pages.SelectedIndex = Settings.Default.selectedTab;
            }
            if (Global.AutoSaveTopMost)
            {
                this.TopMost = Settings.Default.isTopMost;
            }
            if (Global.AutoSaveLanguage)
            {
                this.combLang.Text = Resource.Languages.languages.FirstOrDefault(x => x.Value == LocalCulture).Key;
            }
        }

        private void pages_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Global.AutoSavePage)
            {
                Settings.Default.selectedTab = this.pages.SelectedIndex;
                Settings.Default.Save();
            }
        }

        private void ChangeLabel(object sender, string message)
        {
            this.main_statusLabel.Text = message;
        }

        private void main_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.L)
            {
                if (UnpackWindow.Instance.un_chkbxShowLog.Checked)
                {
                    UnpackWindow.Instance.un_chkbxShowLog.Checked = false;
                }
                else
                {
                    UnpackWindow.Instance.un_chkbxShowLog.Checked = true;
                }
            }
            e.Handled = true;
        }

        internal void BringMainToFront()
        {
            if (this.TopMost)
            {
                this.TopMost = false;
                this.TopMost = true;
            }
            else
            {
                this.TopMost = true;
                this.TopMost = false;
            }
            this.BringToFront();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog();
        }

        private async void checkUpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                LogUtility.ShowCheckingUpdate();
                await UpdateVersion.UpdateProgram();
            }
            catch
            {
                LogUtility.ShowCheckError();
                return;
            }
            LogUtility.ShowCheckSuccess(UpdateVersion.isNewVerExist);
            if (UpdateVersion.isNewVerExist)
            {
                UpdateBox box = new UpdateBox();
                box.ShowDialog();
            }
        }

        private void combLang_SelectedIndexChanged(object sender, EventArgs e)
        {
            LocalCulture = Resource.Languages.languages[this.combLang.Text];
            Controller.Localize.SetLocalCulture(LocalCulture);
            Controller.Localize.RefreshStrings();
            if (Resource.Global.AutoSaveLanguage)
            {
                Properties.Settings.Default.lastLang = LocalCulture;
                Properties.Settings.Default.Save();
            }

        }
    }
}
