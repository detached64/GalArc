using GalArc.Controls;
using GalArc.Database;
using GalArc.Settings;
using System;
using System.Windows.Forms;

namespace GalArc.GUI
{
    public partial class DatabaseSettings : SettingsTemplate
    {
        public static DatabaseSettings Instance { get; } = new DatabaseSettings();

        private DatabaseSettings()
        {
            InitializeComponent();
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
            this.txtDBInfo.Text = Deserializer.GetInfos().Trim();
        }
    }
}
