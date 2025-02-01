using ArcFormats.Templates;
using GalArc.Common;
using GalArc.GUI.Properties;
using GalArc.Logs;
using GalArc.Strings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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

        private List<TreeNode> treeNodesUnpack = new List<TreeNode>();
        private List<TreeNode> treeNodesPack = new List<TreeNode>();

        internal static TreeNode selectedNodeUnpack;
        internal static TreeNode selectedNodePack;

        private string localCulture;
        private bool isFirstChangeLang = true;

        private const int delta = 6;

        enum Mode
        {
            Unpack,
            Pack
        }

        public MainWindow()
        {
            Instance = this;

            localCulture = GetLocalCulture();
            SetLocalCulture(localCulture);

            InitializeComponent();

            Logger.Instance.LogMessageEvent += AppendLog;
            Logger.Instance.StatusMessageEvent += UpdateStatus;
            Logger.Instance.ProgressEvent += ProcessBar;

            this.txtInputPath.DragEnter += txtInputPath_DragEnter;
            this.txtInputPath.DragDrop += txtInputPath_DragDrop;
            this.txtOutputPath.DragEnter += txtOutputPath_DragEnter;
            this.txtOutputPath.DragDrop += txtOutputPath_DragDrop;
            this.Load += async (sender, e) =>
            {
                this.lbStatus.Text = LogStrings.Loading;
                await Task.Run(() => LoadSchemes());
                this.lbStatus.Text = LogStrings.Ready;
                if (BaseSettings.Default.ToAutoSaveState)
                {
                    this.chkbxUnpack.Checked = Settings.Default.IsUnpackMode;
                    this.chkbxPack.Checked = Settings.Default.IsPackMode;
                }
            };
        }

        private void LoadSchemes()
        {
            foreach (var format in ArcFormats.ArcSettings.Formats)
            {
                format.DeserializeScheme(out string name, out int count);
                if (!string.IsNullOrEmpty(name) && count > 0)
                {
                    Logger.ImportDatabaseScheme(name, count);
                }
            }
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            this.combLang.Items.AddRange(Languages.SupportedLanguages.Keys.ToArray());
            this.TopMost = Settings.Default.IsTopMost;
            this.combLang.Text = Languages.SupportedLanguages.FirstOrDefault(x => x.Value == localCulture).Key;
            if (BaseSettings.Default.ToAutoSaveState)
            {
                this.chkbxMatch.Checked = Settings.Default.ToMatchPath;
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
            Logger.Instance.Flush(true);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog();
        }

        private async void checkUpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Updater updater = new Updater();
            await updater.DownloadFileAsync();
        }

        private void combLang_SelectedIndexChanged(object sender, EventArgs e)
        {
            string previousCulture = localCulture;
            localCulture = Languages.SupportedLanguages[this.combLang.Text];
            Settings.Default.LastLanguage = localCulture;
            Settings.Default.Save();
            if (isFirstChangeLang)
            {
                isFirstChangeLang = false;
            }
            else
            {
                if (previousCulture != localCulture)
                {
                    Application.ExitThread();
                    Process.Start(Assembly.GetExecutingAssembly().Location);
                }
            }
        }

        private void chkbxUnpack_CheckedChanged(object sender, EventArgs e)
        {
            this.gbOptions.Controls.Clear();
            if (this.chkbxUnpack.Checked)
            {
                UpdateTreeUnpack();
                this.btExecute.Text = this.chkbxUnpack.Text;
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
            if (this.chkbxPack.Checked)
            {
                UpdateTreePack();
                this.btExecute.Text = this.chkbxPack.Text;
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
            if (e.Node.Parent != null)
            {
                if (this.chkbxUnpack.Checked)
                {
                    if (BaseSettings.Default.ToAutoSaveState)
                    {
                        Settings.Default.UnpackSelectedNode0 = e.Node.Parent.Index;
                        Settings.Default.UnpackSelectedNode1 = e.Node.Index;
                        Settings.Default.Save();
                    }
                    selectedNodeUnpack = e.Node;
                    SyncPath();
                    Logger.Info(string.Format(Resources.logSelectUnpackNode, e.Node.Parent.Text, e.Node.Text));
                    GetExtraOptions(Mode.Unpack);
                }
                else if (this.chkbxPack.Checked)
                {
                    if (BaseSettings.Default.ToAutoSaveState)
                    {
                        Settings.Default.PackSelectedNode0 = e.Node.Parent.Index;
                        Settings.Default.PackSelectedNode1 = e.Node.Index;
                        Settings.Default.Save();
                    }
                    selectedNodePack = e.Node;
                    SyncPath();
                    Logger.Info(string.Format(Resources.logSelectPackNode, e.Node.Parent.Text, e.Node.Text));
                    GetExtraOptions(Mode.Pack);
                }
            }
        }

        private void GetExtraOptions(Mode mode)
        {
            TreeNode node = null;
            switch (mode)
            {
                case Mode.Unpack:
                    node = selectedNodeUnpack;
                    break;
                case Mode.Pack:
                    node = selectedNodePack;
                    break;
            }
            string fieldName = mode.ToString() + "ExtraOptions";
            string[] infos = node.FullPath.Replace(".", string.Empty).Split('/');
            Assembly assembly = Assembly.Load("ArcFormats");
            Type type = assembly.GetType($"ArcFormats.{infos[0]}.{infos[1]}");
            this.SuspendLayout();
            this.gbOptions.Controls.Clear();
            if (type != null)
            {
                //FieldInfo fieldInfo = type.GetField(fieldName);
                PropertyInfo propertyInfo = type.GetProperty(fieldName);
                UserControl userControl = propertyInfo != null ? propertyInfo.GetValue(null) as UserControl : Empty.Instance;
                this.gbOptions.Controls.Add(userControl);
                userControl.Dock = DockStyle.Fill;
            }
            this.ResumeLayout();
        }

        private void btSelInput_Click(object sender, EventArgs e)
        {
            if (this.chkbxUnpack.Checked)
            {
                this.txtInputPath.Text = ChooseFile() ?? this.txtInputPath.Text;
            }
            else if (this.chkbxPack.Checked)
            {
                this.txtInputPath.Text = ChooseFolder() ?? this.txtInputPath.Text;
            }
            else
            {
                Logger.Error(Resources.logErrorNeedSelectOperation, false);
            }
        }

        private void btSelOutput_Click(object sender, EventArgs e)
        {
            if (this.chkbxUnpack.Checked)
            {
                this.txtOutputPath.Text = ChooseFolder() ?? this.txtOutputPath.Text;
            }
            else if (this.chkbxPack.Checked)
            {
                this.txtOutputPath.Text = SaveFile() ?? this.txtOutputPath.Text;
            }
            else
            {
                Logger.Error(Resources.logErrorNeedSelectOperation, false);
            }
        }

        private static string ChooseFile()
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

        private static string SaveFile()
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

        private static string ChooseFolder()
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

        private void btClear_Click(object sender, EventArgs e)
        {
            this.txtInputPath.Text = string.Empty;
            this.txtOutputPath.Text = string.Empty;
        }

        private void chkbxMatch_CheckedChanged(object sender, EventArgs e)
        {
            SyncPath();
            if (BaseSettings.Default.ToAutoSaveState)
            {
                Settings.Default.ToMatchPath = this.chkbxMatch.Checked;
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
            if (treeNodesUnpack.Count > 0)
            {
                foreach (var node in treeNodesUnpack)
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
            if (treeNodesPack.Count > 0)
            {
                foreach (var node in treeNodesPack)
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

        private void chkbxUnpack_SizeChanged(object sender, EventArgs e)
        {
            this.chkbxPack.Location = new Point(this.chkbxUnpack.Location.X + this.chkbxUnpack.Size.Width + delta, this.chkbxPack.Location.Y);
        }

        private async void btExecute_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.txtInputPath.Text))
            {
                Logger.Error(Resources.logErrorNeedSpecifyInput, false);
                return;
            }
            if (string.IsNullOrEmpty(this.txtOutputPath.Text))
            {
                Logger.Error(Resources.logErrorNeedSpecifyOutput, false);
                return;
            }

            if (this.chkbxUnpack.Checked)
            {
                if (!File.Exists(this.txtInputPath.Text))
                {
                    Logger.Error(Resources.logErrorFileNotFound, false);
                    return;
                }
                if (Settings.Default.ToFreezeControls)
                {
                    Freeze();
                }
                else
                {
                    this.btExecute.Enabled = false;
                }
                this.lbStatus.Text = Resources.logUnpacking;

                try
                {
                    using (ArchivePackager packager = new ArchivePackager(this.txtInputPath.Text, this.txtOutputPath.Text))
                    {
                        await Task.Run(() => packager.UnpackOne());
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
                this.btExecute.Enabled = true;
            }
            else if (this.chkbxPack.Checked)
            {
                if (!Directory.Exists(this.txtInputPath.Text))
                {
                    Logger.Error(Resources.logErrorDirNotFound, false);
                    return;
                }
                if (Settings.Default.ToFreezeControls)
                {
                    Freeze();
                }
                else
                {
                    this.btExecute.Enabled = false;
                }
                this.lbStatus.Text = Resources.logPacking;

                try
                {
                    using (ArchivePackager packager = new ArchivePackager(this.txtInputPath.Text, this.txtOutputPath.Text))
                    {
                        await Task.Run(() => packager.Pack());
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
                this.btExecute.Enabled = true;
            }
            else
            {
                Logger.Error(Resources.logErrorNeedSelectOperation, false);
            }

            if (Settings.Default.ToFreezeControls)
            {
                Thaw();
            }
            else
            {
                this.btExecute.Enabled = true;
            }
        }

        private void Freeze()
        {
            this.chkbxUnpack.Enabled = false;
            this.chkbxPack.Enabled = false;
            this.chkbxMatch.Enabled = false;
            this.btExecute.Enabled = false;
            this.btClear.Enabled = false;
            this.btSelInput.Enabled = false;
            this.btSelOutput.Enabled = false;
            this.treeViewEngines.Enabled = false;
            this.txtInputPath.Enabled = false;
            this.txtOutputPath.Enabled = false;
            this.gbOptions.Enabled = false;
            this.menuStrip.Enabled = false;
        }

        private void Thaw()
        {
            this.chkbxUnpack.Enabled = true;
            this.chkbxPack.Enabled = true;
            this.chkbxMatch.Enabled = true;
            this.btExecute.Enabled = true;
            this.btClear.Enabled = true;
            this.btSelInput.Enabled = true;
            this.btSelOutput.Enabled = true;
            this.treeViewEngines.Enabled = true;
            this.txtInputPath.Enabled = true;
            this.txtOutputPath.Enabled = true;
            this.gbOptions.Enabled = true;
            this.menuStrip.Enabled = true;
        }

        private void SyncPath()
        {
            if (this.chkbxMatch.Checked)
            {
                if (this.chkbxUnpack.Checked)
                {
                    if (!string.IsNullOrEmpty(this.txtInputPath.Text) && File.Exists(this.txtInputPath.Text))
                    {
                        string folderPath = Path.Combine(Path.GetDirectoryName(this.txtInputPath.Text), Path.GetFileNameWithoutExtension(this.txtInputPath.Text));
                        this.txtOutputPath.Text = File.Exists(folderPath) ? folderPath.Replace('.', '_') + "_unpacked" : folderPath;
                    }
                }
                else if (this.chkbxPack.Checked)
                {
                    if (!string.IsNullOrEmpty(this.txtInputPath.Text) && Directory.Exists(this.txtInputPath.Text))
                    {
                        string filePath = this.txtInputPath.Text + "." + selectedNodePack.Text.ToLower();
                        this.txtOutputPath.Text = File.Exists(filePath) ? filePath + ".new" : filePath;
                    }
                }
            }
        }

        private string GetLocalCulture()
        {
            string LocalCulture = string.IsNullOrEmpty(Settings.Default.LastLanguage) ? CultureInfo.CurrentCulture.Name : Settings.Default.LastLanguage;
            if (!Languages.SupportedLanguages.Values.ToArray().Contains(LocalCulture))
            {
                LocalCulture = "en-US";
            }
            return LocalCulture;
        }

        private void SetLocalCulture(string LocalCulture)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(LocalCulture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(LocalCulture);
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
    }
}
