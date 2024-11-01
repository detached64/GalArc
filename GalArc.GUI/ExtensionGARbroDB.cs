using GalArc.Extensions;
using GalArc.Extensions.GARbroDB;
using GalArc.GUI.Properties;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace GalArc.GUI
{
    public partial class ExtensionGARbroDB : UserControl
    {
        private static ExtensionGARbroDB instance;

        public static ExtensionGARbroDB Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ExtensionGARbroDB();
                }
                return instance;
            }
        }

        public ExtensionGARbroDB()
        {
            InitializeComponent();
            this.txtDBInfo.BackColor = Color.FromArgb(249, 249, 249);
        }

        private void ExtensionGARbroDB_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Settings.Default.GARbroDBPath))
            {
                this.txtJsonPath.Text = Settings.Default.GARbroDBPath;
            }
            else
            {
                this.txtJsonPath.Text = GARbroDBConfig.DefaultGARbroDBPath;
            }
            this.chkbxEnableGARbroDB.Checked = Settings.Default.EnableGARbroDB;
        }

        private void btSelect_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "JSON files (*.json)|*.json";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    this.txtJsonPath.Text = openFileDialog.FileName;
                    GARbroDBConfig.GARbroDBPath = openFileDialog.FileName;
                    Settings.Default.GARbroDBPath = openFileDialog.FileName;
                    Settings.Default.Save();
                }
            }
        }

        private void txtJsonPath_TextChanged(object sender, EventArgs e)
        {
            this.txtDBInfo.Text = Deserializer.GetJsonInfo(this.txtJsonPath.Text);
            Settings.Default.GARbroDBPath = this.txtJsonPath.Text;
            Settings.Default.Save();
        }

        private void chkbxEnableGARbroDB_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableGARbroDB = chkbxEnableGARbroDB.Checked;
            Settings.Default.Save();
            GARbroDBConfig.IsGARbroDBEnabled = chkbxEnableGARbroDB.Checked;
            this.panel.Enabled = chkbxEnableGARbroDB.Checked;
        }
    }
}
