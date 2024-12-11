using GalArc.Database;
using GalArc.Properties;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace GalArc.GUI
{
    public partial class DatabaseSettings : UserControl
    {
        private static DatabaseSettings instance;
        public static DatabaseSettings Instance
        {
            get
            {
                return instance ?? (instance = new DatabaseSettings());
            }
        }

        public DatabaseSettings()
        {
            InitializeComponent();
            this.txtDBInfo.BackColor = Color.FromArgb(249, 249, 249);
        }

        private void btSelect_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.ShowNewFolderButton = true;
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    this.txtDBPath.Text = folderBrowserDialog.SelectedPath;
                    DatabaseConfig.Path = folderBrowserDialog.SelectedPath;
                    BaseSettings.Default.DatabasePath = folderBrowserDialog.SelectedPath;
                    BaseSettings.Default.Save();
                }
            }
        }

        private void txtDBPath_TextChanged(object sender, EventArgs e)
        {
            DatabaseConfig.Path = this.txtDBPath.Text;
            this.txtDBInfo.Text = Deserializer.GetInfos().Trim();
        }

        private void DatabaseSettings_Load(object sender, EventArgs e)
        {
            this.txtDBPath.Text = DatabaseConfig.Path;
        }
    }
}
