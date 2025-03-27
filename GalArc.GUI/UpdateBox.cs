using GalArc.Settings;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace GalArc.GUI
{
    public partial class UpdateBox : Form
    {
        private const string DownloadUrl = "https://github.com/detached64/GalArc/releases/latest";

        public UpdateBox()
        {
            InitializeComponent();
            this.lbCurrentVersion.Text = string.Format(this.lbCurrentVersion.Text, Updater.CurrentVersion);
            this.lbLatestVersion.Text = string.Format(this.lbLatestVersion.Text, Updater.LatestVersion);
        }

        private void UpdateBox_Load(object sender, EventArgs e)
        {
            if (GUISettings.Default.IsTopMost)
            {
                this.TopMost = true;
            }
        }

        private void btDown_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo(DownloadUrl) { UseShellExecute = true });
        }
    }
}
