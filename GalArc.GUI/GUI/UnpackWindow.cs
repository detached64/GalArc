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
            this.un_FilePath.DragEnter += new DragEventHandler(un_FilePath_DragEnter);
            this.un_FilePath.DragDrop += new DragEventHandler(un_FilePath_DragDrop);
            this.un_FolderPath.DragEnter += new DragEventHandler(un_FolderPath_DragEnter);
            this.un_FolderPath.DragDrop += new DragEventHandler(un_FolderPath_DragDrop);
            Controller.Localize.SetLocalCulture(main.LocalCulture);
            Controller.Localize.GetStrings_unpack();

            LogWindow.Instance.LogFormHidden += new EventHandler(LogForm_LogFormHidden);
        }
        private void LogForm_LogFormHidden(object sender, EventArgs e)
        {
            this.un_chkbxShowLog.Checked = false;
        }

        private void UnpackWindow_Load(object sender, EventArgs e)
        {
            if (this.un_selEngine.Items.Count <= 0)
            {
                LogUtility.Error("Error occurs while initializing engine infos.");
                return;
            }
            LoadState();
        }

        private void un_FilePath_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }
        private void un_FilePath_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);
                un_FilePath.Lines = fileNames;
            }
        }
        private void un_FolderPath_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }
        private void un_FolderPath_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);
                un_FolderPath.Lines = fileNames;
            }
        }

        private void un_btnSelFile_Click(object sender, EventArgs e)
        {
            if (this.un_diaSelFile.ShowDialog() == DialogResult.OK)
            {
                this.un_FilePath.Text = this.un_diaSelFile.FileName;
            }
        }
        private void un_btnSelFolder_Click(object sender, EventArgs e)
        {
            if (this.un_diaSelFolder.ShowDialog() == DialogResult.OK)
            {
                this.un_FolderPath.Text = this.un_diaSelFolder.SelectedPath;
            }
        }

        private void un_btnClear_Click(object sender, EventArgs e)
        {
            this.un_FilePath.Clear();
            this.un_FolderPath.Clear();
        }

        private void un_chkbxMatch_CheckedChanged(object sender, EventArgs e)
        {
            if (File.Exists(this.un_FilePath.Text) && this.un_chkbxMatch.Checked && !string.IsNullOrEmpty(this.un_FilePath.Text))
            {
                SyncPath.un_folderPathSync();
            }
            if (Resource.Global.AutoSaveUnpackMatchChecked)
            {
                Properties.Settings.Default.un_chkbxMatch_checked = this.un_chkbxMatch.Checked;
                Properties.Settings.Default.Save();
            }
        }

        private void un_FilePath_TextChanged(object sender, EventArgs e)
        {
            if (File.Exists(this.un_FilePath.Text) && this.un_chkbxMatch.Checked && !string.IsNullOrEmpty(this.un_FilePath.Text))
            {
                SyncPath.un_folderPathSync();
            }
        }

        private void LoadState()
        {
            if (Resource.Global.AutoSaveUnpackMatchChecked)
            {
                this.un_chkbxMatch.Checked = Properties.Settings.Default.un_chkbxMatch_checked;
            }
            if (Resource.Global.AutoSaveShowLog)
            {
                this.un_chkbxShowLog.Checked = Properties.Settings.Default.chkbxShowLog_checked;
            }
            if (Resource.Global.AutoSaveUnpackSelectedIndex)
            {
                this.un_selEngine.SelectedIndex = Properties.Settings.Default.un_selEngine_selectedIndex;
            }
            if (Resource.Global.AutoSaveDecryptScripts)
            {
                this.un_chkbxDecScr.Checked = Properties.Settings.Default.un_chkbxDecScr_checked;
            }
        }

        private void btnUnpack_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.un_FilePath.Text))
            {
                LogUtility.Error("Please specify input file path.", false);
                return;
            }
            if (string.IsNullOrEmpty(this.un_FolderPath.Text))
            {
                LogUtility.Error("Please specify output folder path.", false);
                return;
            }
            if (!File.Exists(this.un_FilePath.Text))
            {
                LogUtility.Error("File specified does not exist.", false);
                return;
            }
            try
            {
                Controller.Execute.InitUnpack(this.un_FilePath.Text, this.un_FolderPath.Text, this.un_combEncoding.Text, this.un_chkbxDecScr.Checked);
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

        private void un_selEngine_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Resource.Global.AutoSaveUnpackMatchChecked)
            {
                Properties.Settings.Default.un_selEngine_selectedIndex = this.un_selEngine.SelectedIndex;
                Properties.Settings.Default.Save();
            }
            UpdateContent.UpdateUnpackListbox();
            UpdateContent.UpdateUnpackEncoding();
            UpdateContent.UpdateDecScr();
        }

        private void un_chkbxShowLog_CheckedChanged(object sender, EventArgs e)
        {
            if (PackWindow.Instance.pa_chkbxShowLog.Checked == !this.un_chkbxShowLog.Checked)
            {
                PackWindow.Instance.pa_chkbxShowLog.Checked = this.un_chkbxShowLog.Checked;
            }
            main.logWindow.ChangePosition(main.Main.Location.X, main.Main.Location.Y);
            main.logWindow.Visible = this.un_chkbxShowLog.Checked;
            main.Main.BringMainToFront();
            if (Resource.Global.AutoSaveShowLog)
            {
                Properties.Settings.Default.chkbxShowLog_checked = this.un_chkbxShowLog.Checked;
                Properties.Settings.Default.Save();
            }
        }

        private void un_chkbxDecScr_CheckedChanged(object sender, EventArgs e)
        {
            if (Resource.Global.AutoSaveDecryptScripts)
            {
                Properties.Settings.Default.un_chkbxDecScr_checked = this.un_chkbxDecScr.Checked;
                Properties.Settings.Default.Save();
            }
        }
    }
}
