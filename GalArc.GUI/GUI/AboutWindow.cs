using GalArc.Resource;
using Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace GalArc.GUI
{
    public partial class AboutWindow : UserControl
    {
        private List<EngineInfo> engines = EngineInfos.engineInfos;

        private List<EngineInfo> searchedEngines = new List<EngineInfo>();

        public static AboutWindow Instance;

        private static int maxLocation = 0;

        private const int delta = 6;

        private static string currentVersion;

        private const string versionPath = "version.txt";

        private const string versionUrl = "https://raw.githubusercontent.com/detached64/GalArc/master/docs/version.txt";

        private const string downloadUrl = "https://github.com/detached64/GalArc/releases/latest";

        private const string programUrl = "https://github.com/detached64/GalArc";

        private const string issueUrl = "https://github.com/detached64/GalArc/issues";

        private static bool existNewer = false;

        public AboutWindow()
        {
            Instance = this;
            InitializeComponent();
            Controller.Localization.SetLocalCulture(main.LocalCulture);
            Controller.Localization.GetStrings_about();
        }
        private void AboutWindow_Load(object sender, EventArgs e)
        {
            InitializeDataGridView();
            UpdateDataGridView(engines);
            InitVersion();
        }

        private void InitVersion()
        {
            this.ab_lbCurrentVer.Text = Resource.Global.CurrentVersion;
            currentVersion = this.ab_lbCurrentVer.Text;
        }
        internal void SaveEnginesToFile(string filePath, List<EngineInfo> engines)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("EngineName,UnpackFormat,PackFormat");

                foreach (var engine in engines)
                {
                    writer.WriteLine($"{engine.EngineName},{engine.UnpackFormat},{engine.PackFormat}");
                }
            }
        }
        internal List<EngineInfo> LoadEnginesFromFile(string filePath)
        {
            var engines = new List<EngineInfo>();

            using (StreamReader reader = new StreamReader(filePath))
            {
                // 读取表头
                reader.ReadLine();

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var values = line.Split(',');
                    var engine = new EngineInfo(values[0], values[1], values[2]);
                    engines.Add(engine);
                }
            }
            return engines;
        }
        internal void LoadEnginesFromGrid(DataGridView dataGridView, List<EngineInfo> engines)
        {
            engines.Clear();

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                string name = row.Cells["Name"].Value?.ToString();
                string unpackFormat = row.Cells["UnpackFormat"].Value?.ToString();
                string packFormat = row.Cells["PackFormat"].Value?.ToString();

                if (!string.IsNullOrEmpty(name))
                {
                    EngineInfo engine = new EngineInfo(name, unpackFormat, packFormat);
                    engines.Add(engine);
                }
            }
        }
        private void InitializeDataGridView()
        {
            dataGridViewEngines.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }
        private void UpdateDataGridView(List<EngineInfo> engines)
        {
            dataGridViewEngines.Rows.Clear();
            foreach (var engine in engines)
            {
                dataGridViewEngines.Rows.Add(engine.EngineName, engine.UnpackFormat, engine.PackFormat);
            }
        }

        private void DownloadVersion()
        {
            if (File.Exists(versionPath))
            {
                File.Delete(versionPath);
            }

            try
            {
                using (System.Net.WebClient webClient = new System.Net.WebClient())
                {
                    webClient.DownloadFile(versionUrl, versionPath);
                }
            }
            catch (Exception)
            {
                LogUtility.CheckError();
            }
        }
        private string OpenVersion()
        {
            if (!File.Exists(versionPath))
            {
                return "Unknown";
            }
            else
            {
                return File.ReadAllText(versionPath);
            }
        }
        private void CompareVersion(string latestVersion)
        {
            if (latestVersion == "Unknown" || !latestVersion.Contains("."))
            {
                this.ab_btnDownload.Enabled = false;
                return;
            }
            if(File.Exists(versionPath))
            {
                File.Delete(versionPath);
            }
            this.ab_lbLatestVer.Text = latestVersion;
            string[] parts1 = currentVersion.Split('.');
            string[] parts2 = latestVersion.Split('.');

            for (int i = 0; i < Math.Max(parts1.Length, parts2.Length); i++)
            {
                int num1 = (i < parts1.Length) ? int.Parse(parts1[i]) : 0;
                int num2 = (i < parts2.Length) ? int.Parse(parts2[i]) : 0;

                if (num1 < num2)
                {
                    this.ab_btnDownload.Enabled = true;
                    existNewer = true;
                    break;
                }
                else
                {
                    this.ab_btnDownload.Enabled = false;
                }
            }
            LogUtility.CheckSuccess(existNewer);
        }

        private void searchText_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.searchText.Text))
            {
                searchedEngines = engines.Where(engine => engine.EngineName.IndexOf(this.searchText.Text, StringComparison.OrdinalIgnoreCase) >= 0 || engine.UnpackFormat.IndexOf(this.searchText.Text, StringComparison.OrdinalIgnoreCase) >= 0 || engine.PackFormat.IndexOf(this.searchText.Text, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                UpdateDataGridView(searchedEngines);
            }
            else
            {
                UpdateDataGridView(engines);
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
            this.searchText.Location = new System.Drawing.Point(this.ab_lbSearch.Location.X + this.ab_lbSearch.Width +  delta, this.searchText.Location.Y);
        }

        private void ab_btnCheckUpdate_Click(object sender, EventArgs e)
        {
            this.ab_btnCheckUpdate.Enabled = false;
            LogUtility.CheckUpdate();
            DownloadVersion();
            CompareVersion(OpenVersion());
            this.ab_btnCheckUpdate.Enabled = true;
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
