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
            this.treeViewOption.ExpandAll();
        }

        private void SettingsWindow_Load(object sender, EventArgs e)
        {
            if (GUISettings.Default.IsTopMost)
            {
                this.TopMost = true;
            }
            this.treeViewOption.SelectedNode = treeViewOption.Nodes[0];
            this.treeViewOption.Nodes[0].Text = GUIStrings.nodeGeneral;
            this.treeViewOption.Nodes[1].Text = GUIStrings.nodePreference;
            this.treeViewOption.Nodes[2].Text = GUIStrings.nodeLog;
            this.treeViewOption.Nodes[3].Text = GUIStrings.nodeDatabase;
        }

        private void treeViewOption_AfterSelect(object sender, TreeViewEventArgs e)
        {
            this.SuspendLayout();
            UserControl userControl = null;
            switch (e.Node.Name)
            {
                case "nodeGeneral":
                    userControl = GeneralSettings.Instance;
                    break;
                case "nodePreference":
                    userControl = PreferenceSettings.Instance;
                    break;
                case "nodeLog":
                    userControl = LogSettings.Instance;
                    break;
                case "nodeDatabase":
                    userControl = DatabaseSettings.Instance;
                    break;
            }
            if (userControl != null)
            {
                this.panel.Controls.Clear();
                userControl.Dock = DockStyle.Fill;
                this.panel.Controls.Add(userControl);
            }
            this.ResumeLayout(false);
        }
    }
}
