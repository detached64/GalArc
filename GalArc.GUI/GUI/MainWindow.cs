using ArcFormats.Templates;
using GalArc.Controller;
using GalArc.GUI;
using GalArc.Properties;
using GalArc.Resource;
using Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace GalArc
{
    public partial class MainWindow : Form
    {
        public static MainWindow Instance;

        internal static string LocalCulture;

        internal static EngineInfo selectedEngineInfo_Unpack;
        internal static EngineInfo selectedEngineInfo_Pack;

        internal static List<TreeNode> treeNodesUnpack = new List<TreeNode>();
        internal static List<TreeNode> treeNodesPack = new List<TreeNode>();

        internal static TreeNode selectedNodeUnpack;
        internal static TreeNode selectedNodePack;

        internal static bool isFirstChangeLang = true;

        private static int deltaStatus = 15;

        private static int delta = 6;

        public MainWindow()
        {
            Instance = this;

            LocalCulture = Localize.GetLocalCulture();
            Localize.SetLocalCulture(LocalCulture);

            LogUtility.NewInstance();

            InitializeComponent();

            LogWindow logWindow = new LogWindow(this.Width, this.Height);
            LogWindow.ChangeLocalSettings(Settings.Default.AutoSaveState);
            LogWindow.Instance.Owner = this;

            LogUtility.Process += ChangeStatus;
            LogUtility.ErrorOccured += ChangeStatus;

            this.txtInputPath.DragEnter += new DragEventHandler(txtInputPath_DragEnter);
            this.txtInputPath.DragDrop += new DragEventHandler(txtInputPath_DragDrop);
            this.txtOutputPath.DragEnter += new DragEventHandler(txtOutputPath_DragEnter);
            this.txtOutputPath.DragDrop += new DragEventHandler(txtOutputPath_DragDrop);
        }

        private void main_Load(object sender, EventArgs e)
        {
            this.combLang.Items.AddRange(Languages.languages.Keys.ToArray());
            this.TopMost = Settings.Default.TopMost;
            LogWindow.Instance.TopMost = this.TopMost;
            this.combLang.Text = Languages.languages.FirstOrDefault(x => x.Value == LocalCulture).Key;
            if (Settings.Default.AutoSaveState)
            {
                this.chkbxUnpack.Checked = Settings.Default.chkbxUnpack_checked;
                this.chkbxPack.Checked = Settings.Default.chkbxPack_checked;
                this.chkbxMatch.Checked = Settings.Default.chkbxMatch_checked;
                this.chkbxShowLog.Checked = Settings.Default.chkbxShowLog_checked;
            }
        }

        private void txtInputPath_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
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
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
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

        private void main_LocationChanged(object sender, EventArgs e)
        {
            LogWindow.Instance.ChangePosition(this.Location.X, this.Location.Y);
        }

        private void ChangeStatus(object sender, string message)
        {
            this.lbStatus.Text = message;
        }

        private void main_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.L)
            {
                this.chkbxShowLog.Checked = !this.chkbxShowLog.Checked;
            }
            e.Handled = true;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog();
        }

        private async void checkUpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var downloader = new UpdateVersion();
            await downloader.DownloadFileAsync();
        }

        private void combLang_SelectedIndexChanged(object sender, EventArgs e)
        {
            string previousCulture = LocalCulture;
            LocalCulture = Languages.languages[this.combLang.Text];
            Settings.Default.lastLang = LocalCulture;
            Settings.Default.Save();
            if (isFirstChangeLang)
            {
                isFirstChangeLang = false;
            }
            else
            {
                if (previousCulture != LocalCulture)
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
            if (Settings.Default.AutoSaveState)
            {
                Settings.Default.chkbxUnpack_checked = this.chkbxUnpack.Checked;
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
            if (Settings.Default.AutoSaveState)
            {
                Settings.Default.chkbxPack_checked = this.chkbxPack.Checked;
                Settings.Default.Save();
            }
        }

        private void treeViewEngines_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Parent != null)
            {
                if (this.chkbxUnpack.Checked)
                {
                    if (Settings.Default.AutoSaveState)
                    {
                        Settings.Default.UnpackSelectedNode0 = e.Node.Parent.Index;
                        Settings.Default.UnpackSelectedNode1 = e.Node.Index;
                        Settings.Default.Save();
                    }
                    selectedNodeUnpack = e.Node;
                    selectedEngineInfo_Unpack = EngineInfos.engineInfos.Where(x => x.EngineName == e.Node.Parent.Text).FirstOrDefault();
                    chkbxMatch_CheckedChanged(null, null);
                    GetExtraOptions(selectedNodeUnpack, "UnpackExtraOptions");
                    LogUtility.InfoRevoke(string.Format(Resources.logSelectUnpackNode, e.Node.Parent.Text, e.Node.Text));
                }
                else if (this.chkbxPack.Checked)
                {
                    if (Settings.Default.AutoSaveState)
                    {
                        Settings.Default.PackSelectedNode0 = e.Node.Parent.Index;
                        Settings.Default.PackSelectedNode1 = e.Node.Index;
                        Settings.Default.Save();
                    }
                    selectedNodePack = e.Node;
                    selectedEngineInfo_Pack = EngineInfos.engineInfos.Where(x => x.EngineName == e.Node.Parent.Text).FirstOrDefault();
                    chkbxMatch_CheckedChanged(null, null);
                    GetExtraOptions(selectedNodePack, "PackExtraOptions");
                    LogUtility.InfoRevoke(string.Format(Resources.logSelectPackNode, e.Node.Parent.Text, e.Node.Text));
                }
            }
        }

        private void GetExtraOptions(TreeNode node, string fieldName)
        {
            string[] infos = node.FullPath.Split('/');
            Assembly assembly = Assembly.Load("ArcFormats");
            Type type = assembly.GetType($"ArcFormats.{infos[0]}.{infos[1]}");
            this.SuspendLayout();
            this.gbOptions.Controls.Clear();
            if (type != null)
            {
                FieldInfo fieldInfo = type.GetField(fieldName);
                UserControl userControl = fieldInfo != null ? fieldInfo.GetValue(null) as UserControl : Empty.Instance;
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
            else
            {
                this.txtInputPath.Text = ChooseFolder() ?? this.txtInputPath.Text;
            }
        }

        private void btSelOutput_Click(object sender, EventArgs e)
        {
            if (this.chkbxUnpack.Checked)
            {
                this.txtOutputPath.Text = ChooseFolder() ?? this.txtOutputPath.Text;
            }
            else
            {
                this.txtOutputPath.Text = SaveFile() ?? this.txtOutputPath.Text;
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
            if (this.chkbxMatch.Checked)
            {
                if (this.chkbxPack.Checked)
                {
                    if (!string.IsNullOrEmpty(this.txtInputPath.Text) && Directory.Exists(this.txtInputPath.Text))
                    {
                        this.txtOutputPath.Text = SyncPath.PackPathSync(this.txtInputPath.Text, selectedNodePack.Text);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(this.txtInputPath.Text) && File.Exists(this.txtInputPath.Text))
                    {
                        this.txtOutputPath.Text = SyncPath.UnpackPathSync(this.txtInputPath.Text);
                    }
                }
            }
            if (Settings.Default.AutoSaveState)
            {
                Settings.Default.chkbxMatch_checked = this.chkbxMatch.Checked;
                Settings.Default.Save();
            }
        }

        private void txtInputPath_TextChanged(object sender, EventArgs e)
        {
            if (this.chkbxMatch.Checked)
            {
                if (this.chkbxPack.Checked)
                {
                    if (!string.IsNullOrEmpty(this.txtInputPath.Text) && Directory.Exists(this.txtInputPath.Text))
                    {
                        this.txtOutputPath.Text = SyncPath.PackPathSync(this.txtInputPath.Text, selectedNodePack.Text);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(this.txtInputPath.Text) && File.Exists(this.txtInputPath.Text))
                    {
                        this.txtOutputPath.Text = SyncPath.UnpackPathSync(this.txtInputPath.Text);
                    }
                }
            }
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
                foreach (var engine in EngineInfos.engineInfos)
                {
                    TreeNode rootNode = new TreeNode(engine.EngineName);
                    string[] extensions = engine.UnpackFormat.Split('/');
                    foreach (var extension in extensions)
                    {
                        TreeNode node = new TreeNode(extension);
                        rootNode.Nodes.Add(node);
                    }
                    this.treeViewEngines.Nodes.Add(rootNode);
                }
            }
            if (Settings.Default.AutoSaveState)
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
                foreach (var engine in EngineInfos.engineInfos)
                {
                    if (!string.IsNullOrEmpty(engine.PackFormat))
                    {
                        TreeNode rootNode = new TreeNode(engine.EngineName);
                        string[] extensions = engine.PackFormat.Split('/');
                        foreach (var extension in extensions)
                        {
                            TreeNode node = new TreeNode(extension);
                            rootNode.Nodes.Add(node);
                        }
                        this.treeViewEngines.Nodes.Add(rootNode);
                    }
                }
            }
            if (Settings.Default.AutoSaveState)
            {
                TreeNode node0 = treeViewEngines.Nodes[Settings.Default.PackSelectedNode0];
                TreeNode node1 = node0.Nodes[Settings.Default.PackSelectedNode1];
                node1.EnsureVisible();
                treeViewEngines.SelectedNode = node1;
            }
        }

        private void chkbxShowLog_SizeChanged(object sender, EventArgs e)
        {
            this.lbStatus.Size = new Size(this.chkbxShowLog.Location.X - this.lbStatus.Location.X - deltaStatus, this.lbStatus.Size.Height);
        }

        private void chkbxUnpack_SizeChanged(object sender, EventArgs e)
        {
            this.chkbxPack.Location = new Point(this.chkbxUnpack.Location.X + this.chkbxUnpack.Size.Width + delta, this.chkbxPack.Location.Y);
        }

        private void btExecute_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.txtInputPath.Text))
            {
                LogUtility.Error(Resources.logErrorNeedSpecifyInput, false);
                return;
            }
            if (string.IsNullOrEmpty(this.txtOutputPath.Text))
            {
                LogUtility.Error(Resources.logErrorNeedSpecifyOutput, false);
                return;
            }

            if (this.chkbxUnpack.Checked)
            {
                if (!File.Exists(this.txtInputPath.Text))
                {
                    LogUtility.Error(Resources.logErrorFileNotFound, false);
                    return;
                }
                try
                {
                    Execute.InitUnpack(this.txtInputPath.Text, this.txtOutputPath.Text);
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
            else if (this.chkbxPack.Checked)
            {
                if (!Directory.Exists(this.txtInputPath.Text))
                {
                    LogUtility.Error(Resources.logErrorDirNotFound, false);
                    return;
                }
                try
                {
                    Execute.InitPack(this.txtInputPath.Text, this.txtOutputPath.Text);
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
            else
            {
                LogUtility.Error(Resources.logErrorNeedSelectOperation, false);
                return;
            }
        }

        private void chkbxShowLog_CheckedChanged(object sender, EventArgs e)
        {
            LogWindow.Instance.ChangePosition(this.Location.X, this.Location.Y);
            LogWindow.Instance.Visible = this.chkbxShowLog.Checked;
            if (Settings.Default.AutoSaveState)
            {
                Settings.Default.chkbxShowLog_checked = this.chkbxShowLog.Checked;
                Settings.Default.Save();
            }
        }
    }
}
