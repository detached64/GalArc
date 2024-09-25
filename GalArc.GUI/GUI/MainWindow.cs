using GalArc.Controller;
using GalArc.GUI;
using GalArc.Resource;
using Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public MainWindow()
        {
            Instance = this;

            LocalCulture = Localize.GetLocalCulture();

            LogUtility.NewInstance();

            LogWindow logWindow = new LogWindow();

            Localize.SetLocalCulture(LocalCulture);

            Log.Controller.Localize.GetStrings_Log();

            InitializeComponent();

            LogUtility.Process += ChangeLabel;
            LogUtility.ErrorOccured += ChangeLabel;

            this.AllowDrop = true;
            this.txtInputPath.DragEnter += new DragEventHandler(txtInputPath_DragEnter);
            this.txtInputPath.DragDrop += new DragEventHandler(txtInputPath_DragDrop);
            this.txtOutputPath.DragEnter += new DragEventHandler(txtOutputPath_DragEnter);
            this.txtOutputPath.DragDrop += new DragEventHandler(txtOutputPath_DragDrop);
        }

        private void main_Load(object sender, EventArgs e)
        {
            this.combLang.Items.AddRange(Languages.languages.Keys.ToArray());
            this.TopMost = Properties.Settings.Default.TopMost;
            this.combLang.Text = Languages.languages.FirstOrDefault(x => x.Value == LocalCulture).Key;
            this.chkbxUnpack.Checked = Properties.Settings.Default.chkbxUnpack_checked;
            this.chkbxPack.Checked = Properties.Settings.Default.chkbxPack_checked;
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

        private void main_FormClosing(object sender, FormClosingEventArgs e)
        {
            LogWindow.Instance.Dispose();
        }
        private void main_LocationChanged(object sender, EventArgs e)
        {
            LogWindow.Instance.ChangePosition(this.Location.X, this.Location.Y);
        }

        private void ChangeLabel(object sender, string message)
        {
            this.lbStatus.Text = message;
        }

        private void main_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.L)
            {
                if (UnpackWindow.Instance.un_chkbxShowLog.Checked)
                {
                    UnpackWindow.Instance.un_chkbxShowLog.Checked = false;
                }
                else
                {
                    UnpackWindow.Instance.un_chkbxShowLog.Checked = true;
                }
            }
            e.Handled = true;
        }

        internal void BringMainToFront()
        {
            if (this.TopMost)
            {
                this.TopMost = false;
                this.TopMost = true;
            }
            else
            {
                this.TopMost = true;
                this.TopMost = false;
            }
            this.BringToFront();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog();
        }

        private async void checkUpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                LogUtility.ShowCheckingUpdate();
                await UpdateVersion.UpdateProgram();
            }
            catch
            {
                LogUtility.ShowCheckError();
                return;
            }
            LogUtility.ShowCheckSuccess(UpdateVersion.isNewVerExist);
            if (UpdateVersion.isNewVerExist)
            {
                UpdateBox box = new UpdateBox();
                box.ShowDialog();
            }
        }

        private void combLang_SelectedIndexChanged(object sender, EventArgs e)
        {
            string previousCulture = LocalCulture;
            LocalCulture = Languages.languages[this.combLang.Text];
            Properties.Settings.Default.lastLang = LocalCulture;
            Properties.Settings.Default.Save();
            if (isFirstChangeLang)
            {
                isFirstChangeLang = false;
            }
            else
            {
                if (previousCulture != LocalCulture)
                {
                    Application.Exit();
                    Process.Start(Assembly.GetExecutingAssembly().Location);
                }
            }
        }

        private void chkbxUnpack_CheckedChanged(object sender, EventArgs e)
        {
            if (this.chkbxUnpack.Checked)
            {
                UpdateTreeUnpack();
                this.btExecute.Text = this.chkbxUnpack.Text;
            }
            Properties.Settings.Default.chkbxUnpack_checked = this.chkbxUnpack.Checked;
            Properties.Settings.Default.Save();
        }

        private void chkbxPack_CheckedChanged(object sender, EventArgs e)
        {
            if (this.chkbxPack.Checked)
            {
                UpdateTreePack();
                this.btExecute.Text = this.chkbxPack.Text;
            }
            Properties.Settings.Default.chkbxPack_checked = this.chkbxPack.Checked;
            Properties.Settings.Default.Save();
        }

        private void treeViewEngines_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Parent != null)
            {
                LogUtility.InfoRevoke($"{e.Node.Parent.Text} {e.Node.Text} selected.");
                if (this.chkbxUnpack.Checked)
                {
                    selectedNodeUnpack = e.Node;
                    selectedEngineInfo_Unpack = EngineInfos.engineInfos.Where(x => x.EngineName == e.Node.Parent.Text).FirstOrDefault();
                }
                else
                {
                    selectedNodePack = e.Node;
                    selectedEngineInfo_Pack = EngineInfos.engineInfos.Where(x => x.EngineName == e.Node.Parent.Text).FirstOrDefault();
                    if (selectedEngineInfo_Pack != null)
                    {
                        UpdateContent.UpdatePackVersion();
                    }
                }
            }
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
                this.txtOutputPath.Text = ChooseFile() ?? this.txtOutputPath.Text;
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
        }

    }
}
