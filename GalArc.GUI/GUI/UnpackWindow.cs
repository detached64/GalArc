using GalArc.Controller;
using Log;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace GalArc.GUI
{
    public partial class UnpackWindow : UserControl
    {
        public static UnpackWindow Instance;
        public UnpackWindow()
        {
            Instance = this;
            this.AllowDrop = true;
            InitializeComponent();
            Controller.Localize.SetLocalCulture(MainWindow.LocalCulture);

            Log.LogWindow.Instance.LogFormHidden += new EventHandler(LogForm_LogFormHidden);
        }
        private void LogForm_LogFormHidden(object sender, EventArgs e)
        {
            this.un_chkbxShowLog.Checked = false;
        }

        private void UnpackWindow_Load(object sender, EventArgs e)
        {
            LoadState();
        }

        private void LoadState()
        {
            if (Properties.Settings.Default.AutoSaveState)
            {
                this.un_chkbxShowLog.Checked = Properties.Settings.Default.chkbxShowLog_checked;
            }
        }

        private void btnUnpack_Click(object sender, EventArgs e)
        {
            //if (string.IsNullOrEmpty(this.un_FilePath.Text))
            //{
            //    LogUtility.Error("Please specify input file path.", false);
            //    return;
            //}
            //if (string.IsNullOrEmpty(this.un_FolderPath.Text))
            //{
            //    LogUtility.Error("Please specify output folder path.", false);
            //    return;
            //}
            //if (!File.Exists(this.un_FilePath.Text))
            //{
            //    LogUtility.Error("File specified does not exist.", false);
            //    return;
            //}
            try
            {
                //Controller.Execute.InitUnpack(this.un_FilePath.Text, this.un_FolderPath.Text, this.un_combEncoding.Text, this.un_chkbxDecScr.Checked);
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
                Log.LogWindow.Instance.bar.Value = 0;
            }
        }

        private void un_chkbxShowLog_CheckedChanged(object sender, EventArgs e)
        {
            if (PackWindow.Instance.pa_chkbxShowLog.Checked == !this.un_chkbxShowLog.Checked)
            {
                PackWindow.Instance.pa_chkbxShowLog.Checked = this.un_chkbxShowLog.Checked;
            }
            Log.LogWindow.Instance.ChangePosition(MainWindow.Instance.Location.X, MainWindow.Instance.Location.Y);
            Log.LogWindow.Instance.Visible = this.un_chkbxShowLog.Checked;
            MainWindow.Instance.BringMainToFront();
            if (Properties.Settings.Default.AutoSaveState)
            {
                Properties.Settings.Default.chkbxShowLog_checked = this.un_chkbxShowLog.Checked;
                Properties.Settings.Default.Save();
            }
        }
    }
}
