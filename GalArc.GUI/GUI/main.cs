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

        private static Timer timer = new Timer { Interval = 5000 };
        public main()
        {
            Main = this;
            InitializeComponent();
            LogUtility.NewInstance();
            Controller.UpdateContent.InitLang();
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

            Controller.UpdateContent.InitCombobox();

            LogUtility.Process += LogUtility_Process;
            LogUtility.ErrorOccured += LogUtility_ErrorOccured;
            timer.Tick += Timer_Tick;
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

        private void LogUtility_Process(object sender, string message)
        {
            this.main_statusLabel.Text = message;
        }
        private void LogUtility_ErrorOccured(object sender, string message)
        {
            this.main_statusLabel.Text = message;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            this.main_statusLabel.Text = string.Empty;
            timer.Stop();
        }

    }
}
