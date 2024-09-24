using GalArc.Controller;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace GalArc.GUI
{
    public partial class UpdateBox : Form
    {

        private const int delta = 6;

        private const string downloadUrl = "https://github.com/detached64/GalArc/releases/latest";

        private static int maxLocation = 0;

        public UpdateBox()
        {
            InitializeComponent();
            this.lbCurrentVer.Text = UpdateVersion.currentVersion;
            this.lbLatestVer.Text = UpdateVersion.latestVersion;
        }

        private void btDown_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo(downloadUrl) { UseShellExecute = true });
        }

        private void lbCurrentVersion_SizeChanged(object sender, EventArgs e)
        {
            maxLocation = Math.Max(this.lbCurrentVersion.Location.X + this.lbCurrentVersion.Size.Width, this.lbLatestVersion.Location.X + this.lbLatestVersion.Size.Width);
            this.lbCurrentVer.Location = new Point(maxLocation + delta, this.lbCurrentVer.Location.Y);
            this.lbLatestVer.Location = new Point(maxLocation + delta, this.lbLatestVer.Location.Y);
        }

        private void lbLatestVersion_SizeChanged(object sender, EventArgs e)
        {
            maxLocation = Math.Max(this.lbCurrentVersion.Location.X + this.lbCurrentVersion.Size.Width, this.lbLatestVersion.Location.X + this.lbLatestVersion.Size.Width);
            this.lbCurrentVer.Location = new Point(maxLocation + delta, this.lbCurrentVer.Location.Y);
            this.lbLatestVer.Location = new Point(maxLocation + delta, this.lbLatestVer.Location.Y);
        }
    }
}
