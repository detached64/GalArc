using ArcFormats;
using GalArc.Settings;
using GalArc.Strings;
using System;
using System.Windows.Forms;

namespace GalArc.GUI
{
    public partial class SettingsWindow : Form
    {
        public static SettingsWindow Instance { get; } = new SettingsWindow();

        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void SettingsWindow_Load(object sender, EventArgs e)
        {
            if (GUISettings.Default.IsTopMost)
            {
                this.TopMost = true;
            }
            this.treeOptions.Nodes.Clear();
            TreeNode[] nodes = new TreeNode[]
            {
                new TreeNode(GUIStrings.nodeGeneral) { Tag = GeneralSettings.Instance },
                new TreeNode(GUIStrings.nodePreference) { Tag = null },
                new TreeNode(GUIStrings.nodeLog) { Tag = LogSettings.Instance },
                new TreeNode(GUIStrings.nodeDatabase) { Tag = DatabaseSettings.Instance }
            };
            this.treeOptions.Nodes.AddRange(nodes);
            this.treeOptions.SelectedNode = this.treeOptions.Nodes[0];

            foreach (var format in ArcResources.Formats)
            {
                if (format.Settings != null)
                {
                    foreach (var setting in format.Settings)
                    {
                        if (setting is EncodingSetting encodingSetting)
                        {
                            this.treeOptions.Nodes[1].Nodes.Add(new TreeNode()
                            {
                                Text = setting.Name.Replace("Encoding", string.Empty),
                                Tag = new EncodingSettings(encodingSetting)
                            });
                        }
                        else
                        {
                            throw new NotImplementedException("Not supported setting type.");
                        }
                    }
                }
            }
            this.treeOptions.ExpandAll();
        }

        private void treeViewOption_AfterSelect(object sender, TreeViewEventArgs e)
        {
            this.SuspendLayout();
            UserControl userControl = e.Node.Tag as UserControl;
            this.panel.Controls.Clear();
            if (userControl != null)
            {
                userControl.Dock = DockStyle.Fill;
                this.panel.Controls.Add(userControl);
            }
            this.ResumeLayout(true);
        }
    }
}
