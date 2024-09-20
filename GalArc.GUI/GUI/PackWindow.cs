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
            this.pa_FolderPath.DragEnter += new DragEventHandler(pa_FilePath_DragEnter);
            this.pa_FolderPath.DragDrop += new DragEventHandler(pa_FilePath_DragDrop);
            this.pa_FilePath.DragEnter += new DragEventHandler(pa_FolderPath_DragEnter);
            this.pa_FilePath.DragDrop += new DragEventHandler(pa_FolderPath_DragDrop);
            Controller.Localize.SetLocalCulture(main.LocalCulture);
            Controller.Localize.GetStrings_pack();

            LogWindow.Instance.LogFormHidden += new EventHandler(LogForm_LogFormHidden);
        }
        private void LogForm_LogFormHidden(object sender, EventArgs e)
        {
            this.pa_chkbxShowLog.Checked = false;
        }

        private void PackWindow_Load(object sender, EventArgs e)
        {
            if (this.pa_selEngine.Items.Count <= 0)
            {
                LogUtility.Error("Error occurs while initializing engine infos.");
                return;
            }
            LoadState();
        }
        private void pa_FilePath_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }
        private void pa_FilePath_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);
                pa_FolderPath.Lines = fileNames;
            }
        }
        private void pa_FolderPath_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }
        private void pa_FolderPath_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);
                pa_FilePath.Lines = fileNames;
            }
        }

        private void pa_btnSelFile_Click(object sender, EventArgs e)
        {
            if (this.pa_diaSelFile.ShowDialog() == DialogResult.OK)
            {
                this.pa_FilePath.Text = this.pa_diaSelFile.FileName;
            }

        }
        private void pa_btnSelFolder_Click(object sender, EventArgs e)
        {
            if (this.pa_diaSelFolder.ShowDialog() == DialogResult.OK)
            {
                this.pa_FolderPath.Text = this.pa_diaSelFolder.SelectedPath;
            }
        }

        private void pa_btnClear_Click(object sender, EventArgs e)
        {
            this.pa_FolderPath.Clear();
            this.pa_FilePath.Clear();
        }

        private void pa_chkbxMatch_CheckedChanged(object sender, EventArgs e)
        {
            if (Directory.Exists(this.pa_FolderPath.Text) && this.pa_chkbxMatch.Checked && !string.IsNullOrEmpty(this.pa_FolderPath.Text))
            {
                SyncPath.pa_filePathSync();
            }
            if (Resource.Global.AutoSavePackMatchChecked)
            {
                Properties.Settings.Default.pa_chkbxMatch_checked = this.pa_chkbxMatch.Checked;
                Properties.Settings.Default.Save();
            }
        }

        private void pa_FilePath_TextChanged(object sender, EventArgs e)
        {
            if (Directory.Exists(this.pa_FolderPath.Text) && this.pa_chkbxMatch.Checked && !string.IsNullOrEmpty(this.pa_FolderPath.Text))
            {
                SyncPath.pa_filePathSync();
            }
        }

        private void LoadState()
        {
            if (Resource.Global.AutoSavePackMatchChecked)
            {
                this.pa_chkbxMatch.Checked = Properties.Settings.Default.pa_chkbxMatch_checked;
            }
            if (Resource.Global.AutoSaveShowLog)
            {
                this.pa_chkbxShowLog.Checked = Properties.Settings.Default.chkbxShowLog_checked;
            }
            if (Resource.Global.AutoSavePackSelectedIndex)
            {
                this.pa_selEngine.SelectedIndex = Properties.Settings.Default.pa_selEngine_selectedIndex;
            }
        }

        private void btnPack_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.pa_FolderPath.Text))
            {
                LogUtility.Error("Please specify input folder path.", false);
                return;
            }
            if (string.IsNullOrEmpty(this.pa_FilePath.Text))
            {
                LogUtility.Error("Please specify output file path.", false);
                return;
            }
            if (!Directory.Exists(this.pa_FolderPath.Text))
            {
                LogUtility.Error("Folder specified does not exist.", false);
                return;
            }
            try
            {
                Controller.Execute.InitPack(this.pa_FolderPath.Text, this.pa_FilePath.Text, this.pa_combVersion.Text, this.pa_combEncoding.Text);
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

        private void pa_selEngine_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Resource.Global.AutoSavePackSelectedIndex)
            {
                Properties.Settings.Default.pa_selEngine_selectedIndex = this.pa_selEngine.SelectedIndex;
                Properties.Settings.Default.Save();
            }
            UpdateContent.UpdatePackListbox();
            UpdateContent.UpdatePackFormat();
            UpdateContent.UpdatePackVersion();
            UpdateContent.UpdatePackEncoding();
        }

        private void pa_chkbxShowLog_CheckedChanged(object sender, EventArgs e)
        {
            if (UnpackWindow.Instance.un_chkbxShowLog.Checked != this.pa_chkbxShowLog.Checked)
            {
                UnpackWindow.Instance.un_chkbxShowLog.Checked = this.pa_chkbxShowLog.Checked;
            }
            main.logWindow.ChangePosition(main.Main.Location.X, main.Main.Location.Y);
            main.logWindow.Visible = this.pa_chkbxShowLog.Checked;
            main.Main.BringMainToFront();
            if (Resource.Global.AutoSaveShowLog)
            {
                Properties.Settings.Default.chkbxShowLog_checked = this.pa_chkbxShowLog.Checked;
                Properties.Settings.Default.Save();
            }
        }

        private void pa_combPackFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Directory.Exists(this.pa_FolderPath.Text) && !string.IsNullOrEmpty(this.pa_FolderPath.Text))
            {
                SyncPath.pa_filePathSync();
            }
            UpdateContent.UpdatePackVersion();
        }

        private void pa_ShowFormat_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.pa_combPackFormat.Items.Cast<string>().ToArray().Contains(this.pa_ShowFormat.SelectedItem))
            {
                this.pa_combPackFormat.Text = this.pa_ShowFormat.SelectedItem.ToString();
            }
        }
    }
}
