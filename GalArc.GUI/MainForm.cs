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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GalArc.GUI
{
    public partial class MainForm : Form
    {
        internal static MainForm Instance;

        private List<TreeNode> TreeNodesUnpack = new List<TreeNode>();
        private List<TreeNode> TreeNodesPack = new List<TreeNode>();

        private ArcFormat SelectedFormat;

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
            this.pBar.Maximum = ArcResources.Formats.Count;
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
            Logger.SetBarMax(isFirstLoad ? ArcResources.Formats.Count : ArcResources.Formats.Count * 2);
            int im_count = 0;
            foreach (var format in ArcResources.Formats)
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
            foreach (var format in ArcResources.Formats)
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
            this.CombLanguages.Items.AddRange(Languages.SupportedLanguages.Keys.ToArray());
            this.TopMost = GUISettings.Default.IsTopMost;
            this.CombLanguages.Text = Languages.SupportedLanguages.FirstOrDefault(x => x.Value == CurrentCulture).Key;
            this.MenuMatchPaths.Checked = GUISettings.Default.MatchPath;
            this.MenuBatchExtraction.Checked = BaseSettings.Default.BatchExtraction;
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
        private void MenuAbout_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog();
        }

        private void MenuBatchExtraction_Click(object sender, EventArgs e)
        {
            BaseSettings.Default.BatchExtraction = this.MenuBatchExtraction.Checked;
            BaseSettings.Default.Save();
            if (this.MenuBatchExtraction.Checked)
            {
                Logger.Status(LogStrings.BatchActivated);
            }
        }

        private async void MenuCheckUpdate_Click(object sender, EventArgs e)
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

        private void MenuReimportSchemes_Click(object sender, EventArgs e)
        {
            ReloadSchemesAsync(sender, e);
        }

        private void MenuSettings_Click(object sender, EventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
        }

        private void CombLanguages_SelectedIndexChanged(object sender, EventArgs e)
        {
            string previousCulture = CurrentCulture;
            CurrentCulture = Languages.SupportedLanguages[this.CombLanguages.Text];
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

        private void MenuClearPaths_Click(object sender, EventArgs e)
        {
            this.txtInputPath.Text = string.Empty;
            this.txtOutputPath.Text = string.Empty;
        }

        private void MenuMatchPaths_CheckedChanged(object sender, EventArgs e)
        {
            SyncPath();
            GUISettings.Default.MatchPath = this.MenuMatchPaths.Checked;
            GUISettings.Default.Save();
        }
        #endregion

        #region LoggerEvents
        private void AppendLog(object sender, string msg)
        {
            if (this.txtLog.InvokeRequired)
            {
                BeginInvoke(new Action(() => this.txtLog.AppendText(msg + Environment.NewLine)));
            }
            else
            {
                this.txtLog.AppendText(msg + Environment.NewLine);
            }
        }

        private void UpdateStatus(object sender, string msg)
        {
            if (this.statusStrip.InvokeRequired)
            {
                BeginInvoke(new Action(() => this.lbStatus.Text = msg));
            }
            else
            {
                this.lbStatus.Text = msg;
            }
        }

        private void ProcessBar(object sender, ProgressEventArgs progressArgs)
        {
            if (this.statusStrip.InvokeRequired)
            {
                BeginInvoke(new Action(() => ProcessBar(progressArgs)));
            }
            else
            {
                ProcessBar(progressArgs);
            }
        }

        private void ProcessBar(ProgressEventArgs progressArgs)
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
            SelectedFormat = ArcResources.Formats.FirstOrDefault(x => x.GetType().FullName == fullPath);
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
            CheckMatch();
        }

        private void CheckMatch()
        {
            if (!BaseSettings.Default.BatchExtraction || Mode != OperationMode.Unpack || string.IsNullOrWhiteSpace(this.txtInputPath.Text))
            {
                Logger.Status(LogStrings.Ready);
                return;
            }
            string dir = Path.GetDirectoryName(this.txtInputPath.Text);
            string pattern = Path.GetFileName(this.txtInputPath.Text);
            if (Directory.Exists(dir))
            {
                Logger.Status(string.Format(LogStrings.BatchMatchFiles, Directory.GetFiles(dir, pattern).Length));
            }
            else
            {
                Logger.Status(LogStrings.Ready);
            }
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
            bool is_batch_mode = BaseSettings.Default.BatchExtraction && this.txtInputPath.Text.Contains('*');
            if (Mode == OperationMode.Unpack)
            {
                if (!is_batch_mode && !File.Exists(this.txtInputPath.Text))
                {
                    Logger.Error(LogStrings.ErrorFileNotFound);
                    return;
                }
            }
            else
            {
                if (!Directory.Exists(this.txtInputPath.Text))
                {
                    Logger.Error(LogStrings.ErrorDirNotFound);
                    return;
                }
            }
            Freeze();
            #endregion

            #region Execute
            if (Mode == OperationMode.Unpack)
            {
                if (is_batch_mode)
                {
                    List<int> result = await UnpackAllAsync();
                    Logger.FinishUnpackBatch(result[0], result[1]);
                }
                else
                {
                    await UnpackOneAsync(this.txtInputPath.Text, this.txtOutputPath.Text);
                }
            }
            else
            {
                await PackAsync(this.txtInputPath.Text, this.txtOutputPath.Text);
            }
            #endregion

            #region Thaw controls
            Thaw();
            #endregion
        }

        private async Task<bool> UnpackOneAsync(string input, string output)
        {
            Logger.Status(LogStrings.Unpacking);
            Logger.InitUnpack(input, output);
            try
            {
                await Task.Run(() => SelectedFormat.Unpack(input, output));
                Logger.FinishUnpack(true);
                return true;
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    Logger.Debug(ex.ToString());
                }
                else
                {
                    Logger.Debug(ex.InnerException.ToString());
                }
                Logger.FinishUnpack(false, ex.Message);
                return false;
            }
        }

        private async Task<List<int>> UnpackAllAsync()
        {
            string root = Path.GetDirectoryName(this.txtInputPath.Text);
            string pattern = Path.GetFileName(this.txtInputPath.Text);
            string[] files = Directory.GetFiles(root, pattern);
            int success = 0;
            foreach (string file in files)
            {
                string dst = Path.Combine(this.txtOutputPath.Text, Path.GetFileNameWithoutExtension(file));
                Logger.Info(string.Format(LogStrings.UnpackingFile, Path.GetFileName(file)));
                if (await UnpackOneAsync(file, dst))
                {
                    success++;
                }
            }
            return new List<int> { files.Length, success };
        }

        private async Task PackAsync(string input, string output)
        {
            Logger.Status(LogStrings.Packing);
            Logger.InitPack(input, output);
            try
            {
                await Task.Run(() => SelectedFormat.Pack(input, output));
                Logger.FinishUnpack(true);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    Logger.Debug(ex.ToString());
                }
                else
                {
                    Logger.Debug(ex.InnerException.ToString());
                }
                Logger.FinishPack(false, ex.Message);
            }
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
            if (!this.MenuMatchPaths.Checked || Mode == OperationMode.None || string.IsNullOrWhiteSpace(this.txtInputPath.Text) || string.IsNullOrWhiteSpace(Path.GetDirectoryName(this.txtInputPath.Text)) || this.txtInputPath.Text.Contains('*'))
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
                    case 0 when this.txtInputPath.Text[this.txtInputPath.Text.Length - 1] != '\\':
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
