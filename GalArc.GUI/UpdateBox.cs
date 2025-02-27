using GalArc.Settings;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace GalArc.GUI
{
    public partial class UpdateBox : Form
    {
        private const int Delta = 6;

        private const string DownloadUrl = "https://github.com/detached64/GalArc/releases/latest";

        private int maxLocation;

        public UpdateBox()
        {
            InitializeComponent();
            this.lbCurrentVer.Text = Updater.CurrentVersion.ToString();
            this.lbLatestVer.Text = Updater.LatestVersion;
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

        private void lbCurrentVersion_SizeChanged(object sender, EventArgs e)
        {
            maxLocation = Math.Max(this.lbCurrentVersion.Location.X + this.lbCurrentVersion.Size.Width, this.lbLatestVersion.Location.X + this.lbLatestVersion.Size.Width);
            this.lbCurrentVer.Location = new Point(maxLocation + Delta, this.lbCurrentVer.Location.Y);
            this.lbLatestVer.Location = new Point(maxLocation + Delta, this.lbLatestVer.Location.Y);
        }

        private void lbLatestVersion_SizeChanged(object sender, EventArgs e)
        {
            maxLocation = Math.Max(this.lbCurrentVersion.Location.X + this.lbCurrentVersion.Size.Width, this.lbLatestVersion.Location.X + this.lbLatestVersion.Size.Width);
            this.lbCurrentVer.Location = new Point(maxLocation + Delta, this.lbCurrentVer.Location.Y);
            this.lbLatestVer.Location = new Point(maxLocation + Delta, this.lbLatestVer.Location.Y);
        }
    }
}
