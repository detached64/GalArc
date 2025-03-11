using ArcFormats;
using GalArc.Common;
using GalArc.Database;
using GalArc.Logs;
using GalArc.Settings;
using GalArc.Strings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GalArc.GUI
{
    public partial class MainForm : Form
    {
        internal static MainForm Instance;

        private List<TreeNode> TreeNodesUnpack = new List<TreeNode>();
        private List<TreeNode> TreeNodesPack = new List<TreeNode>();

        private ArchiveFormat SelectedFormat;

        internal static string CurrentCulture;
        private bool IsFirstChangeLang = true;
        private OperationMode Mode => GetOperationMode();

        public MainForm()
        {
            Instance = this;
            InitializeComponent();
            this.BringToFront();

            Logger.Instance.LogMessageEvent += AppendLog;
            Logger.Instance.StatusMessageEvent += UpdateStatus;
            Logger.Instance.ProgressEvent += ProcessBar;

            this.txtInputPath.DragEnter += txtInputPath_DragEnter;
            this.txtInputPath.DragDrop += txtInputPath_DragDrop;
            this.txtOutputPath.DragEnter += txtOutputPath_DragEnter;
            this.txtOutputPath.DragDrop += txtOutputPath_DragDrop;
            this.Load += ImportSchemesAsync;
        }

        #region SchemeImport
        private async void ImportSchemesAsync(object sender, EventArgs e)
        {
            this.pnlOperation.Enabled = false;
            this.pBar.Value = 0;
            this.pBar.Maximum = ArcSettings.Formats.Count;
            try
            {
                await Task.Run(() => LoadSchemes(true)).ConfigureAwait(true);
            }
            catch
            {
                Logger.Error(LogStrings.ErrorImportScheme);
                return;
            }
            this.lbStatus.Text = LogStrings.Ready;
            this.pnlOperation.Enabled = true;
            this.chkbxUnpack.Checked = GUISettings.Default.IsUnpackMode;
            this.chkbxPack.Checked = GUISettings.Default.IsPackMode;
        }

        private async void ReloadSchemesAsync(object sender, EventArgs e)
        {
            try
            {
                await Task.Run(() => LoadSchemes(false)).ConfigureAwait(true);
                RefreshSchemes();
            }
            catch
            {
                Logger.Error(LogStrings.ErrorImportScheme);
                return;
            }
            this.lbStatus.Text = LogStrings.Ready;
        }

        private void LoadSchemes(bool isFirstLoad)
        {
            Logger.InfoInvoke(LogStrings.SchemeLoading);
            Logger.Debug(string.Format(LogStrings.SchemeCount, Deserializer.SchemeCount));
            Logger.ResetBar();
            Logger.SetBarMax(isFirstLoad ? ArcSettings.Formats.Count : ArcSettings.Formats.Count * 2);
            int im_count = 0;
            foreach (var format in ArcSettings.Formats)
            {
                format.DeserializeScheme(out string name, out int count);
                if (!string.IsNullOrEmpty(name) && count > 0)
                {
                    Logger.ImportDatabaseScheme(name, count);
                    im_count++;
                }
                Logger.UpdateBar();
            }
            Deserializer.LoadedSchemeCount = im_count;
            Logger.Info(string.Format(LogStrings.SchemeLoadedWithCount, Deserializer.LoadedSchemeCount));
        }

        private void RefreshSchemes()
        {
            Logger.InfoInvoke(LogStrings.SchemeRefreshing);
            foreach (var format in ArcSettings.Formats)
            {
                format.UnpackExtraOptions.AddSchemes();
                format.PackExtraOptions.AddSchemes();
                Logger.UpdateBar();
            }
            Logger.Info(LogStrings.SchemeRefreshed);
        }
        #endregion

        #region MainFormEvents
        private void MainForm_Load(object sender, EventArgs e)
        {
            this.combLanguages.Items.AddRange(Languages.SupportedLanguages.Keys.ToArray());
            this.TopMost = GUISettings.Default.IsTopMost;
            this.combLanguages.Text = Languages.SupportedLanguages.FirstOrDefault(x => x.Value == CurrentCulture).Key;
            this.matchPathsMenuItem.Checked = GUISettings.Default.MatchPath;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Logger.Instance.LogMessageEvent -= AppendLog;
            Logger.Instance.StatusMessageEvent -= UpdateStatus;
            Logger.Instance.ProgressEvent -= ProcessBar;
            Logger.Instance.Dispose();
        }
        #endregion

        #region DragDropEvents
        private void txtInputPath_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void txtInputPath_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                this.txtInputPath.Text = files[0];
            }
        }

        private void txtOutputPath_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void txtOutputPath_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                this.txtOutputPath.Text = files[0];
            }
        }
        #endregion

        #region ToolStripMenuEvents
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog();
        }

        private async void checkUpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Updater updater = new Updater();
            try
            {
                await updater.DownloadVersionAsync();
            }
            catch
            {
                Logger.Info(LogStrings.UpdateError);
            }
        }

        private void reimportSchemesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ReloadSchemesAsync(sender, e);
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
        }

        private void combLanguages_SelectedIndexChanged(object sender, EventArgs e)
        {
            string previousCulture = CurrentCulture;
            CurrentCulture = Languages.SupportedLanguages[this.combLanguages.Text];
            GUISettings.Default.LastLanguage = CurrentCulture;
            GUISettings.Default.Save();
            if (IsFirstChangeLang)
            {
                IsFirstChangeLang = false;
            }
            else
            {
                if (previousCulture != CurrentCulture)
                {
                    Application.Restart();
                    Environment.Exit(0);
                }
            }
        }

        private void preferenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
        }

        private void clearPathsMenuStrip_Click(object sender, EventArgs e)
        {
            this.txtInputPath.Text = string.Empty;
            this.txtOutputPath.Text = string.Empty;
        }

        private void matchPathsMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            SyncPath();
            GUISettings.Default.MatchPath = this.matchPathsMenuItem.Checked;
            GUISettings.Default.Save();
        }
        #endregion

        #region LoggerEvents
        private void AppendLog(object sender, string msg)
        {
            if (this.txtLog.InvokeRequired)
            {
                this.txtLog.BeginInvoke(new Action(() => AppendLog(sender, msg)));
                return;
            }

            try
            {
                this.txtLog.AppendText(msg + Environment.NewLine);
            }
            catch
            {
                MessageBox.Show("AppendLog Error");
            }
        }

        private void UpdateStatus(object sender, string msg)
        {
            if (this.statusStrip.InvokeRequired)
            {
                this.statusStrip.BeginInvoke(new Action(() => UpdateStatus(sender, msg)));
                return;
            }

            try
            {
                this.lbStatus.Text = msg;
            }
            catch
            {
                MessageBox.Show("UpdateStatus Error");
            }
        }

        private void ProcessBar(object sender, ProgressEventArgs progressArgs)
        {
            if (this.statusStrip.InvokeRequired)
            {
                this.statusStrip.BeginInvoke(new Action(() => ProcessBar(sender, progressArgs)));
                return;
            }

            try
            {
                switch (progressArgs.Action)
                {
                    case ProgressAction.Progress:
                        this.pBar.Value = Math.Min(this.pBar.Value + 1, this.pBar.Maximum);
                        break;
                    case ProgressAction.Finish:
                        this.pBar.Value = this.pBar.Maximum;
                        break;
                    case ProgressAction.SetVal:
                        this.pBar.Value = Math.Min(progressArgs.Value, this.pBar.Maximum);
                        break;
                    case ProgressAction.SetMax:
                        this.pBar.Maximum = progressArgs.Max;
                        break;
                }
            }
            catch
            {
                MessageBox.Show("ProcessBar Error");
            }
        }
        #endregion

        private void chkbxUnpack_CheckedChanged(object sender, EventArgs e)
        {
            this.gbOptions.Controls.Clear();
            if (Mode == OperationMode.Unpack)
            {
                this.btExecute.Text = this.chkbxUnpack.Text;
                UpdateTreeUnpack();
            }
            GUISettings.Default.IsUnpackMode = this.chkbxUnpack.Checked;
            GUISettings.Default.Save();
        }

        private void chkbxPack_CheckedChanged(object sender, EventArgs e)
        {
            this.gbOptions.Controls.Clear();
            if (Mode == OperationMode.Pack)
            {
                this.btExecute.Text = this.chkbxPack.Text;
                UpdateTreePack();
            }
            GUISettings.Default.IsPackMode = this.chkbxPack.Checked;
            GUISettings.Default.Save();
        }

        private void treeViewEngines_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Parent == null || Mode == OperationMode.None)
            {
                return;
            }
            if (Mode == OperationMode.Unpack)
            {
                GUISettings.Default.UnpackSelectedNode0 = e.Node.Parent.Index;
                GUISettings.Default.UnpackSelectedNode1 = e.Node.Index;
                GUISettings.Default.Save();
                Logger.Info(string.Format(LogStrings.SelectUnpackNode, e.Node.Parent.Text, e.Node.Text));
            }
            else
            {
                GUISettings.Default.PackSelectedNode0 = e.Node.Parent.Index;
                GUISettings.Default.PackSelectedNode1 = e.Node.Index;
                GUISettings.Default.Save();
                Logger.Info(string.Format(LogStrings.SelectPackNode, e.Node.Parent.Text, e.Node.Text));
            }
            SyncPath();
            GetExtraOptions();
        }

        private void GetExtraOptions()
        {
            if (Mode == OperationMode.None)
            {
                Logger.Error(LogStrings.ErrorNeedSelectOperation);
                return;
            }
            string[] infos = this.treeViewEngines.SelectedNode.FullPath.Replace(".", string.Empty).Split('/');
            string fullPath = $"{nameof(ArcFormats)}.{infos[0]}.{infos[1]}";
            SelectedFormat = ArcSettings.Formats.FirstOrDefault(x => x.GetType().FullName == fullPath);
            if (SelectedFormat != null)
            {
                this.SuspendLayout();
                this.gbOptions.Controls.Clear();
                UserControl options = Mode == OperationMode.Unpack ? SelectedFormat.UnpackExtraOptions : SelectedFormat.PackExtraOptions;
                this.gbOptions.Controls.Add(options);
                options.Dock = DockStyle.Fill;
                this.ResumeLayout();
            }
        }

        private void btSelInput_Click(object sender, EventArgs e)
        {
            switch (Mode)
            {
                case OperationMode.Unpack:
                    this.txtInputPath.Text = ChooseFile() ?? this.txtInputPath.Text;
                    break;
                case OperationMode.Pack:
                    this.txtInputPath.Text = ChooseFolder() ?? this.txtInputPath.Text;
                    break;
                default:
                    Logger.Error(LogStrings.ErrorNeedSelectOperation);
                    break;
            }
        }

        private void btSelOutput_Click(object sender, EventArgs e)
        {
            switch (Mode)
            {
                case OperationMode.Unpack:
                    this.txtOutputPath.Text = ChooseFolder() ?? this.txtOutputPath.Text;
                    break;
                case OperationMode.Pack:
                    this.txtOutputPath.Text = SaveFile() ?? this.txtOutputPath.Text;
                    break;
                default:
                    Logger.Error(LogStrings.ErrorNeedSelectOperation);
                    break;
            }
        }

        private string ChooseFile()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(openFileDialog.FileName))
                {
                    return openFileDialog.FileName;
                }
                return null;
            }
        }

        private string SaveFile()
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                if (saveFileDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(saveFileDialog.FileName))
                {
                    return saveFileDialog.FileName;
                }
                return null;
            }
        }

        private string ChooseFolder()
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(folderBrowserDialog.SelectedPath))
                {
                    return folderBrowserDialog.SelectedPath;
                }
                return null;
            }
        }

        private void txtInputPath_TextChanged(object sender, EventArgs e)
        {
            SyncPath();
        }

        private void UpdateTreeUnpack()
        {
            this.treeViewEngines.Nodes.Clear();
            if (TreeNodesUnpack.Count > 0)
            {
                foreach (var node in TreeNodesUnpack)
                {
                    this.treeViewEngines.Nodes.Add(node);
                }
            }
            else
            {
                foreach (var engine in EngineInfo.Infos)
                {
                    TreeNode rootNode = new TreeNode(engine.EngineName);
                    foreach (var extension in engine.UnpackFormat.Split('/'))
                    {
                        TreeNode node = new TreeNode(extension);
                        rootNode.Nodes.Add(node);
                    }
                    this.treeViewEngines.Nodes.Add(rootNode);
                }
            }
            TreeNode node0 = treeViewEngines.Nodes[GUISettings.Default.UnpackSelectedNode0];
            TreeNode node1 = node0.Nodes[GUISettings.Default.UnpackSelectedNode1];
            node1.EnsureVisible();
            treeViewEngines.SelectedNode = node1;
        }

        private void UpdateTreePack()
        {
            this.treeViewEngines.Nodes.Clear();
            if (TreeNodesPack.Count > 0)
            {
                foreach (var node in TreeNodesPack)
                {
                    this.treeViewEngines.Nodes.Add(node);
                }
            }
            else
            {
                foreach (var engine in EngineInfo.Infos)
                {
                    if (!string.IsNullOrEmpty(engine.PackFormat))
                    {
                        TreeNode rootNode = new TreeNode(engine.EngineName);
                        foreach (var extension in engine.PackFormat.Split('/'))
                        {
                            TreeNode node = new TreeNode(extension);
                            rootNode.Nodes.Add(node);
                        }
                        this.treeViewEngines.Nodes.Add(rootNode);
                    }
                }
            }
            TreeNode node0 = treeViewEngines.Nodes[GUISettings.Default.PackSelectedNode0];
            TreeNode node1 = node0.Nodes[GUISettings.Default.PackSelectedNode1];
            node1.EnsureVisible();
            treeViewEngines.SelectedNode = node1;
        }

        private async void btExecute_Click(object sender, EventArgs e)
        {
            #region Check valid & Freeze controls
            if (Mode == OperationMode.None)
            {
                Logger.Error(LogStrings.ErrorNeedSelectOperation);
                return;
            }
            if (string.IsNullOrEmpty(this.txtInputPath.Text))
            {
                Logger.Error(LogStrings.ErrorNeedSpecifyInput);
                return;
            }
            if (string.IsNullOrEmpty(this.txtOutputPath.Text))
            {
                Logger.Error(LogStrings.ErrorNeedSpecifyOutput);
                return;
            }
            if (Mode == OperationMode.Unpack)
            {
                if (!File.Exists(this.txtInputPath.Text))
                {
                    Logger.Error(LogStrings.ErrorFileNotFound);
                    return;
                }
                this.lbStatus.Text = LogStrings.Unpacking;
            }
            else
            {
                if (!Directory.Exists(this.txtInputPath.Text))
                {
                    Logger.Error(LogStrings.ErrorDirNotFound);
                    return;
                }
                this.lbStatus.Text = LogStrings.Packing;
            }
            Freeze();
            #endregion

            #region Set encoding
            if (!string.IsNullOrEmpty(GUISettings.Default.DefaultEncoding))
            {
                ArcSettings.Encoding = Encoding.GetEncoding(Encodings.CodePages[GUISettings.Default.DefaultEncoding]);
            }
            else
            {
                ArcSettings.Encoding = Encoding.UTF8;
                GUISettings.Default.DefaultEncoding = "UTF-8";
                GUISettings.Default.Save();
            }
            #endregion

            #region Execute
            try
            {
                if (Mode == OperationMode.Unpack)
                {
                    Logger.InitUnpack(this.txtInputPath.Text, this.txtOutputPath.Text);
                    await Task.Run(() => SelectedFormat.Unpack(this.txtInputPath.Text, this.txtOutputPath.Text));
                    Logger.FinishUnpack();
                }
                else
                {
                    Logger.InitPack(this.txtInputPath.Text, this.txtOutputPath.Text);
                    await Task.Run(() => SelectedFormat.Pack(this.txtInputPath.Text, this.txtOutputPath.Text));
                    Logger.FinishPack();
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    Logger.Error(ex.Message);
                    Logger.Debug(ex.ToString());
                }
                else
                {
                    Logger.Error(ex.InnerException.Message);
                    Logger.Debug(ex.InnerException.ToString());
                }
                Logger.ResetBar();
            }
            #endregion

            #region Thaw controls
            Thaw();
            #endregion
        }

        private void Freeze()
        {
            this.menuStrip.Enabled = false;
            this.chkbxUnpack.Enabled = false;
            this.chkbxPack.Enabled = false;
            this.btExecute.Enabled = false;
            this.btSelInput.Enabled = false;
            this.btSelOutput.Enabled = false;
            this.treeViewEngines.Enabled = false;
            this.txtInputPath.Enabled = false;
            this.txtOutputPath.Enabled = false;
            this.gbOptions.Enabled = false;
        }

        private void Thaw()
        {
            this.menuStrip.Enabled = true;
            this.chkbxUnpack.Enabled = true;
            this.chkbxPack.Enabled = true;
            this.btExecute.Enabled = true;
            this.btSelInput.Enabled = true;
            this.btSelOutput.Enabled = true;
            this.treeViewEngines.Enabled = true;
            this.txtInputPath.Enabled = true;
            this.txtOutputPath.Enabled = true;
            this.gbOptions.Enabled = true;
        }

        private void SyncPath()
        {
            if (!this.matchPathsMenuItem.Checked || Mode == OperationMode.None || string.IsNullOrEmpty(this.txtInputPath.Text))
            {
                return;
            }
            if (Mode == OperationMode.Unpack)
            {
                // handle two cases:
                // 1. xxx.pfs.001
                // 2. xxx
                switch (Path.GetFileName(this.txtInputPath.Text).Count(chr => chr == '.'))
                {
                    case 0:
                        this.txtOutputPath.Text = this.txtInputPath.Text + "_unpacked";
                        break;
                    case 1:
                        this.txtOutputPath.Text = Path.Combine(Path.GetDirectoryName(this.txtInputPath.Text), Path.GetFileNameWithoutExtension(this.txtInputPath.Text));
                        break;
                    default:
                        this.txtOutputPath.Text = Path.Combine(Path.GetDirectoryName(this.txtInputPath.Text), Path.GetFileName(this.txtInputPath.Text).Replace('.', '_'));
                        break;
                }
            }
            else
            {
                string filePath = this.txtInputPath.Text + "." + this.treeViewEngines.SelectedNode.Text.ToLower();
                this.txtOutputPath.Text = File.Exists(filePath) ? filePath + ".new" : filePath;
            }
        }

        private OperationMode GetOperationMode()
        {
            if (this.chkbxUnpack.Checked)
            {
                return OperationMode.Unpack;
            }
            else if (this.chkbxPack.Checked)
            {
                return OperationMode.Pack;
            }
            return OperationMode.None;
        }
    }

    internal enum OperationMode
    {
        Unpack,
        Pack,
        None
    }
}
