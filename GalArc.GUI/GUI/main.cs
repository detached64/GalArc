using GalArc.GUI;
using Log;
using System;
using System.Linq;
using System.Windows.Forms;

namespace GalArc
{
    public partial class main : Form
    {
        public static LogWindow logWindow;
        public static UnpackWindow unpackWindow;
        public static PackWindow packWindow;
        public static OptionWindow optionWindow;
        public static AboutWindow aboutWindow;
        public static main Main;

        internal static string LocalCulture;
        public main()
        {
            Main = this;
            InitializeComponent();
            LogUtility.NewInstance();
            LocalCulture = Controller.Localization.GetLocalCulture();

            logWindow = new LogWindow();
            unpackWindow = new UnpackWindow();
            packWindow = new PackWindow();
            optionWindow = new OptionWindow();
            aboutWindow = new AboutWindow();

            unpackPage.Controls.Add(unpackWindow);
            packPage.Controls.Add(packWindow);
            optionPage.Controls.Add(optionWindow);
            aboutPage.Controls.Add(aboutWindow);

            unpackWindow.Dock = DockStyle.Fill;
            packWindow.Dock = DockStyle.Fill;
            optionWindow.Dock = DockStyle.Fill;
            aboutWindow.Dock = DockStyle.Fill;

            Controller.Localization.SetLocalCulture(LocalCulture);
            Controller.Localization.GetStrings_main();

            Log.Controller.Localization.SetLocalCulture(LocalCulture);
            Log.Controller.Localization.GetStrings_Log();

            Controller.UpdateContent.InitCombobox_Engines();

            LogUtility.Process += ChangeLabel;
            LogUtility.ErrorOccured += ChangeLabel;
        }

        private void main_Load(object sender, EventArgs e)
        {
            LoadState();
        }

        private void main_FormClosing(object sender, FormClosingEventArgs e)
        {
            logWindow.Dispose();
        }
        private void main_LocationChanged(object sender, EventArgs e)
        {
            logWindow.ChangePosition(this.Location.X, this.Location.Y);
        }

        private void LoadState()
        {
            if (Resource.Global.AutoSavePage)
            {
                this.pages.SelectedIndex = Properties.Settings.Default.selectedTab;
            }
            if (Resource.Global.AutoSaveTopMost)
            {
                this.TopMost = Properties.Settings.Default.isTopMost;
            }
        }

        private void pages_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Resource.Global.AutoSavePage)
            {
                Properties.Settings.Default.selectedTab = this.pages.SelectedIndex;
                Properties.Settings.Default.Save();
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
    }
}
