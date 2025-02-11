using ArcFormats;
using GalArc.Common;
using GalArc.Controls;
using GalArc.Database;
using GalArc.GUI.Properties;
using GalArc.Logs;
using GalArc.Strings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GalArc.GUI
{
    public partial class MainWindow : Form
    {
        public static MainWindow Instance;

        private List<TreeNode> TreeNodesUnpack = new List<TreeNode>();
        private List<TreeNode> TreeNodesPack = new List<TreeNode>();

        internal static TreeNode SelectedNodeUnpack;
        internal static TreeNode SelectedNodePack;

        private string Culture;
        private bool IsFirstChangeLang = true;
        private OperationMode Mode => GetOperationMode();

        public MainWindow()
        {
            Instance = this;

            GetLocalCulture();
            SetLocalCulture();

            InitializeComponent();

            Logger.Instance.LogMessageEvent += AppendLog;
            Logger.Instance.StatusMessageEvent += UpdateStatus;
            Logger.Instance.ProgressEvent += ProcessBar;

            this.txtInputPath.DragEnter += txtInputPath_DragEnter;
            this.txtInputPath.DragDrop += txtInputPath_DragDrop;
            this.txtOutputPath.DragEnter += txtOutputPath_DragEnter;
            this.txtOutputPath.DragDrop += txtOutputPath_DragDrop;
            this.Load += ImportSchemesAsync;
        }

        private async void ImportSchemesAsync(object sender, EventArgs e)
        {
            this.pnlOperation.Enabled = false;
            this.lbStatus.Text = LogStrings.Loading;
            try
            {
                await Task.Run(() => LoadSchemes()).ConfigureAwait(true);
            }
            catch
            {
                Logger.Error(LogStrings.ErrorImportScheme, false);
                return;
            }
            this.lbStatus.Text = LogStrings.Ready;
            this.pnlOperation.Enabled = true;
            if (BaseSettings.Default.ToAutoSaveState)
            {
                this.chkbxUnpack.Checked = Settings.Default.IsUnpackMode;
                this.chkbxPack.Checked = Settings.Default.IsPackMode;
            }
        }

        private async void ReloadSchemesAsync(object sender, EventArgs e)
        {
            this.lbStatus.Text = LogStrings.Loading;
            try
            {
                await Task.Run(() => LoadSchemes()).ConfigureAwait(true);
            }
            catch
            {
                Logger.Error(LogStrings.ErrorImportScheme, false);
                return;
            }
            this.lbStatus.Text = LogStrings.Ready;
        }

        private void LoadSchemes()
        {
            int c = ArcSettings.Formats.Count;
            Logger.ResetBar();
            Logger.SetBarMax(c);

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
            Deserializer.ImportedSchemeCount = im_count;
            Logger.Debug(string.Format(LogStrings.SchemeCount, Deserializer.SchemeCount));
            Logger.Debug(string.Format(LogStrings.ImportedSchemeCount, Deserializer.ImportedSchemeCount));
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            this.combLanguages.Items.AddRange(Languages.SupportedLanguages.Keys.ToArray());
            this.TopMost = Settings.Default.IsTopMost;
            this.combLanguages.Text = Languages.SupportedLanguages.FirstOrDefault(x => x.Value == Culture).Key;
            if (BaseSettings.Default.ToAutoSaveState)
            {
                this.matchPathsMenuItem.Checked = Settings.Default.ToMatchPath;
            }
        }

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

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            Logger.Instance.LogMessageEvent -= AppendLog;
            Logger.Instance.StatusMessageEvent -= UpdateStatus;
            Logger.Instance.ProgressEvent -= ProcessBar;
            Logger.FlushLog();
        }

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
                return;
            }
        }

        private void combLanguages_SelectedIndexChanged(object sender, EventArgs e)
        {
            string previousCulture = Culture;
            Culture = Languages.SupportedLanguages[this.combLanguages.Text];
            Settings.Default.LastLanguage = Culture;
            Settings.Default.Save();
            if (IsFirstChangeLang)
            {
                IsFirstChangeLang = false;
            }
            else
            {
                if (previousCulture != Culture)
                {
                    Application.ExitThread();
                    Process.Start(Assembly.GetExecutingAssembly().Location);
                }
            }
        }

        private void chkbxUnpack_CheckedChanged(object sender, EventArgs e)
        {
            this.gbOptions.Controls.Clear();
            if (Mode == OperationMode.Unpack)
            {
                this.btExecute.Text = this.chkbxUnpack.Text;
                UpdateTreeUnpack();
            }
            SyncPath();
            if (BaseSettings.Default.ToAutoSaveState)
            {
                Settings.Default.IsUnpackMode = this.chkbxUnpack.Checked;
                Settings.Default.Save();
            }
        }

        private void chkbxPack_CheckedChanged(object sender, EventArgs e)
        {
            this.gbOptions.Controls.Clear();
            if (Mode == OperationMode.Pack)
            {
                this.btExecute.Text = this.chkbxPack.Text;
                UpdateTreePack();
            }
            SyncPath();
            if (BaseSettings.Default.ToAutoSaveState)
            {
                Settings.Default.IsPackMode = this.chkbxPack.Checked;
                Settings.Default.Save();
            }
        }

        private void treeViewEngines_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Parent == null || Mode == OperationMode.None)
            {
                return;
            }
            if (Mode == OperationMode.Unpack)
            {
                if (BaseSettings.Default.ToAutoSaveState)
                {
                    Settings.Default.UnpackSelectedNode0 = e.Node.Parent.Index;
                    Settings.Default.UnpackSelectedNode1 = e.Node.Index;
                    Settings.Default.Save();
                }
                SelectedNodeUnpack = e.Node;
                Logger.Info(string.Format(LogStrings.SelectUnpackNode, e.Node.Parent.Text, e.Node.Text));
            }
            else
            {
                if (BaseSettings.Default.ToAutoSaveState)
                {
                    Settings.Default.PackSelectedNode0 = e.Node.Parent.Index;
                    Settings.Default.PackSelectedNode1 = e.Node.Index;
                    Settings.Default.Save();
                }
                SelectedNodePack = e.Node;
                Logger.Info(string.Format(LogStrings.SelectPackNode, e.Node.Parent.Text, e.Node.Text));
            }
            SyncPath();
            GetExtraOptions(Mode);
        }

        private void GetExtraOptions(OperationMode mode)
        {
            TreeNode node = null;
            switch (mode)
            {
                case OperationMode.Unpack:
                    node = SelectedNodeUnpack;
                    break;
                case OperationMode.Pack:
                    node = SelectedNodePack;
                    break;
            }
            string fieldName = mode.ToString() + "ExtraOptions";
            string[] infos = node.FullPath.Replace(".", string.Empty).Split('/');
            Assembly assembly = Assembly.Load(nameof(ArcFormats));
            Type type = assembly.GetType($"{nameof(ArcFormats)}.{infos[0]}.{infos[1]}");
            this.SuspendLayout();
            this.gbOptions.Controls.Clear();
            if (type != null)
            {
                PropertyInfo propertyInfo = type.GetProperty(fieldName);
                UserControl userControl = propertyInfo != null ? propertyInfo.GetValue(null) as UserControl : Empty.Instance;
                this.gbOptions.Controls.Add(userControl);
                userControl.Dock = DockStyle.Fill;
            }
            this.ResumeLayout();
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
                    Logger.Error(LogStrings.ErrorNeedSelectOperation, false);
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
                    Logger.Error(LogStrings.ErrorNeedSelectOperation, false);
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
            if (BaseSettings.Default.ToAutoSaveState)
            {
                Settings.Default.ToMatchPath = this.matchPathsMenuItem.Checked;
                Settings.Default.Save();
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
            if (BaseSettings.Default.ToAutoSaveState)
            {
                TreeNode node0 = treeViewEngines.Nodes[Settings.Default.UnpackSelectedNode0];
                TreeNode node1 = node0.Nodes[Settings.Default.UnpackSelectedNode1];
                node1.EnsureVisible();
                treeViewEngines.SelectedNode = node1;
            }
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
            if (BaseSettings.Default.ToAutoSaveState)
            {
                TreeNode node0 = treeViewEngines.Nodes[Settings.Default.PackSelectedNode0];
                TreeNode node1 = node0.Nodes[Settings.Default.PackSelectedNode1];
                node1.EnsureVisible();
                treeViewEngines.SelectedNode = node1;
            }
        }

        private async void btExecute_Click(object sender, EventArgs e)
        {
            if (Mode == OperationMode.None)
            {
                Logger.Error(LogStrings.ErrorNeedSelectOperation, false);
                return;
            }
            if (string.IsNullOrEmpty(this.txtInputPath.Text))
            {
                Logger.Error(LogStrings.ErrorNeedSpecifyInput, false);
                return;
            }
            if (string.IsNullOrEmpty(this.txtOutputPath.Text))
            {
                Logger.Error(LogStrings.ErrorNeedSpecifyOutput, false);
                return;
            }

            if (Mode == OperationMode.Unpack)
            {
                if (!File.Exists(this.txtInputPath.Text))
                {
                    Logger.Error(LogStrings.ErrorFileNotFound, false);
                    return;
                }
                this.lbStatus.Text = LogStrings.Unpacking;
            }
            else
            {
                if (!Directory.Exists(this.txtInputPath.Text))
                {
                    Logger.Error(LogStrings.ErrorDirNotFound, false);
                    return;
                }
                this.lbStatus.Text = LogStrings.Packing;
            }
            Freeze();
            this.btExecute.Enabled = false;

            try
            {
                using (ArchivePackager packager = new ArchivePackager(this.txtInputPath.Text, this.txtOutputPath.Text))
                {
                    await Task.Run(() => packager.Work(Mode));
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    Logger.Error(ex.Message, false);
                    Logger.Debug(ex.ToString());
                }
                else
                {
                    Logger.Error(ex.InnerException.Message, false);
                    Logger.Debug(ex.InnerException.ToString());
                }
                Logger.ResetBar();
            }

            Thaw();
        }

        private void Freeze()
        {
            this.menuStrip.Enabled = false;
            this.chkbxUnpack.Enabled = false;
            this.chkbxPack.Enabled = false;
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
                switch (this.txtInputPath.Text.Count(chr => chr == '.'))
                {
                    case 0:
                        this.txtOutputPath.Text = this.txtInputPath.Text + "_unpacked";
                        break;
                    case 1:
                        this.txtOutputPath.Text = Path.Combine(Path.GetDirectoryName(this.txtInputPath.Text), Path.GetFileNameWithoutExtension(this.txtInputPath.Text));
                        break;
                    default:
                        this.txtOutputPath.Text = this.txtInputPath.Text.Replace('.', '_');
                        break;
                }
            }
            else
            {
                string filePath = this.txtInputPath.Text + "." + SelectedNodePack.Text.ToLower();
                this.txtOutputPath.Text = File.Exists(filePath) ? filePath + ".new" : filePath;
            }
        }

        private void GetLocalCulture()
        {
            Culture = string.IsNullOrEmpty(Settings.Default.LastLanguage) ? CultureInfo.CurrentCulture.Name : Settings.Default.LastLanguage;
            if (!Languages.SupportedLanguages.Values.ToArray().Contains(Culture))
            {
                Culture = "en-US";
            }
        }

        private void SetLocalCulture()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(Culture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Culture);
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

        private void AppendLog(object sender, string message)
        {
            if (this.txtLog.InvokeRequired)
            {
                this.txtLog.Invoke(new Action(() => AppendLog(sender, message)));
                return;
            }

            this.txtLog.AppendText(message + Environment.NewLine);
        }

        private void UpdateStatus(object sender, string message)
        {
            var container = this.lbStatus.Owner;
            if (container.InvokeRequired)
            {
                container.Invoke(new Action(() => UpdateStatus(sender, message)));
                return;
            }

            this.lbStatus.Text = message;
        }

        private void ProcessBar(object sender, ProgressEventArgs e)
        {
            var container = this.pBar.Owner;
            if (container.InvokeRequired)
            {
                container.Invoke(new Action(() => ProcessBar(sender, e)));
                return;
            }

            switch (e.Action)
            {
                case ProgressAction.Progress:
                    this.pBar.Increment(1);
                    break;
                case ProgressAction.Finish:
                    this.pBar.Value = this.pBar.Maximum;
                    break;
                case ProgressAction.SetVal:
                    this.pBar.Value = e.Value;
                    break;
                case ProgressAction.SetMax:
                    this.pBar.Maximum = e.Max;
                    break;
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
    }

    internal enum OperationMode
    {
        Unpack,
        Pack,
        None
    }
}
