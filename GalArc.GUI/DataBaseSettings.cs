using GalArc.DataBase;
using System;
using System.Drawing;
using System.Windows.Forms;
using GalArc.GUI.Properties;

namespace GalArc.GUI
{
    public partial class DataBaseSettings : UserControl
    {
        private static DataBaseSettings instance;
        public static DataBaseSettings Instance
        {
            get
            {
                return instance ?? (instance = new DataBaseSettings());
            }
        }

        public DataBaseSettings()
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
                    DataBaseConfig.Path = folderBrowserDialog.SelectedPath;
                    Settings.Default.DataBasePath = folderBrowserDialog.SelectedPath;
                    Settings.Default.Save();
                }
            }
        }

        private void txtDBPath_TextChanged(object sender, EventArgs e)
        {
            DataBaseConfig.Path = this.txtDBPath.Text;
            Settings.Default.DataBasePath = this.txtDBPath.Text;
            Settings.Default.Save();

            this.txtDBInfo.Text = Deserializer.GetInfos().Trim();
        }

        private void DataBaseSettings_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Settings.Default.DataBasePath))
            {
                this.txtDBPath.Text = Settings.Default.DataBasePath;
            }
            else
            {
                this.txtDBPath.Text = DataBaseConfig.Path;
            }
        }
    }
}
