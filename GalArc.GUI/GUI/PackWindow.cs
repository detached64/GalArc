using GalArc.Controller;
using Log;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace GalArc.GUI
{
    public partial class PackWindow : UserControl
    {
        public static PackWindow Instance;
        public PackWindow()
        {
            Instance = this;
            this.AllowDrop = true;
            InitializeComponent();
            Controller.Localize.SetLocalCulture(MainWindow.LocalCulture);

            LogWindow.Instance.LogFormHidden += new EventHandler(LogForm_LogFormHidden);
        }
        private void LogForm_LogFormHidden(object sender, EventArgs e)
        {
            this.pa_chkbxShowLog.Checked = false;
        }

        private void PackWindow_Load(object sender, EventArgs e)
        {
            LoadState();
        }

        private void LoadState()
        {
            if (Resource.Settings.AutoSaveState)
            {
                this.pa_chkbxShowLog.Checked = Properties.Settings.Default.chkbxShowLog_checked;
            }
        }

        private void btnPack_Click(object sender, EventArgs e)
        {
            //if (string.IsNullOrEmpty(this.pa_FolderPath.Text))
            //{
            //    LogUtility.Error("Please specify input folder path.", false);
            //    return;
            //}
            //if (string.IsNullOrEmpty(this.pa_FilePath.Text))
            //{
            //    LogUtility.Error("Please specify output file path.", false);
            //    return;
            //}
            //if (!Directory.Exists(this.pa_FolderPath.Text))
            //{
            //    LogUtility.Error("Folder specified does not exist.", false);
            //    return;
            //}
            try
            {
                //Controller.Execute.InitPack(this.pa_FolderPath.Text, this.pa_FilePath.Text, this.pa_combVersion.Text, this.pa_combEncoding.Text);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    LogUtility.Error(ex.Message, false);
                    LogUtility.Debug(ex.ToString());
                }
                else
                {
                    LogUtility.Error(ex.InnerException.Message, false);
                    LogUtility.Debug(ex.InnerException.ToString());
                }
                LogWindow.Instance.bar.Value = 0;
            }
        }

        private void pa_chkbxShowLog_CheckedChanged(object sender, EventArgs e)
        {
            if (UnpackWindow.Instance.un_chkbxShowLog.Checked != this.pa_chkbxShowLog.Checked)
            {
                UnpackWindow.Instance.un_chkbxShowLog.Checked = this.pa_chkbxShowLog.Checked;
            }
            LogWindow.Instance.ChangePosition(MainWindow.Instance.Location.X, MainWindow.Instance.Location.Y);
            LogWindow.Instance.Visible = this.pa_chkbxShowLog.Checked;
            MainWindow.Instance.BringMainToFront();
            if (Resource.Settings.AutoSaveState)
            {
                Properties.Settings.Default.chkbxShowLog_checked = this.pa_chkbxShowLog.Checked;
                Properties.Settings.Default.Save();
            }
        }

        private void pa_combPackFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateContent.UpdatePackVersion();
        }
    }
}
