using GalArc.Resource;
using Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace GalArc.GUI
{
    public partial class AboutWindow : UserControl
    {
        private List<EngineInfo> engines = EngineInfos.engineInfos;

        private List<EngineInfo> searchedEngines = new List<EngineInfo>();

        public static AboutWindow Instance;

        private static int maxLocation = 0;

        private const int delta = 6;

        private const string downloadUrl = "https://github.com/detached64/GalArc/releases/latest";

        private const string programUrl = "https://github.com/detached64/GalArc";

        private const string issueUrl = "https://github.com/detached64/GalArc/issues";

        public AboutWindow()
        {
            Instance = this;
            InitializeComponent();
            Controller.Localization.SetLocalCulture(main.LocalCulture);
            Controller.Localization.GetStrings_about();
        }
        private void AboutWindow_Load(object sender, EventArgs e)
        {
            Controller.UpdateContent.InitDataGridView();
            Controller.UpdateContent.UpdateDataGridView(engines);
            Controller.UpdateVersion.InitVersion();
        }

        private void searchText_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.searchText.Text))
            {
                searchedEngines = engines.Where(engine => engine.EngineName.IndexOf(this.searchText.Text, StringComparison.OrdinalIgnoreCase) >= 0 || engine.UnpackFormat.IndexOf(this.searchText.Text, StringComparison.OrdinalIgnoreCase) >= 0 || engine.PackFormat.IndexOf(this.searchText.Text, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                Controller.UpdateContent.UpdateDataGridView(searchedEngines);
            }
            else
            {
                Controller.UpdateContent.UpdateDataGridView(engines);
            }
        }

        private void ab_lbCurrentVersion_SizeChanged(object sender, EventArgs e)
        {
            maxLocation = Math.Max(this.ab_lbCurrentVersion.Location.X + this.ab_lbCurrentVersion.Size.Width, this.ab_lbLatestVersion.Location.X + this.ab_lbLatestVersion.Size.Width);
            this.ab_lbCurrentVer.Location = new System.Drawing.Point(maxLocation + delta, this.ab_lbCurrentVer.Location.Y);
            this.ab_lbLatestVer.Location = new System.Drawing.Point(maxLocation + delta, this.ab_lbLatestVer.Location.Y);
        }
        private void ab_lbLatestVersion_SizeChanged(object sender, EventArgs e)
        {
            maxLocation = Math.Max(this.ab_lbCurrentVersion.Location.X + this.ab_lbCurrentVersion.Size.Width, this.ab_lbLatestVersion.Location.X + this.ab_lbLatestVersion.Size.Width);
            this.ab_lbCurrentVer.Location = new System.Drawing.Point(maxLocation + delta, this.ab_lbCurrentVer.Location.Y);
            this.ab_lbLatestVer.Location = new System.Drawing.Point(maxLocation + delta, this.ab_lbLatestVer.Location.Y);
        }
        private void ab_lbSearch_SizeChanged(object sender, EventArgs e)
        {
            this.searchText.Location = new System.Drawing.Point(this.ab_lbSearch.Location.X + this.ab_lbSearch.Width + delta, this.searchText.Location.Y);
        }

        private async void ab_btnCheckUpdate_Click(object sender, EventArgs e)
        {
            this.ab_btnCheckUpdate.Enabled = false;
            try
            {
                LogUtility.ShowCheckingUpdate();
                await Controller.UpdateVersion.UpdateProgram();
            }
            finally
            {
                this.ab_lbLatestVer.Text = Controller.UpdateVersion.latestVersion;
                LogUtility.ShowCheckSuccess(Controller.UpdateVersion.isNewVerExist);
                if (Controller.UpdateVersion.isNewVerExist)
                {
                    this.ab_btnDownload.Enabled = true;
                }
                else
                {
                    this.ab_btnDownload.Enabled = false;
                }
                this.ab_btnCheckUpdate.Enabled = true;
            }
        }
        private void ab_btnDownload_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo(downloadUrl) { UseShellExecute = true });
        }

        private void ab_linkSite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.ab_linkSite.LinkVisited = true;
            Process.Start(new ProcessStartInfo(programUrl) { UseShellExecute = true });
        }

        private void ab_linkIssue_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.ab_linkIssue.LinkVisited = true;
            Process.Start(new ProcessStartInfo(issueUrl) { UseShellExecute = true });
        }

    }
}
