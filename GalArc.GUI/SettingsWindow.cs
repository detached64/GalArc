using GalArc.GUI.Properties;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace GalArc.GUI
{
    public partial class SettingsWindow : Form
    {
        public SettingsWindow()
        {
            InitializeComponent();
            this.treeViewOption.ExpandAll();
        }

        private void SettingsWindow_Load(object sender, EventArgs e)
        {
            if (MainWindow.Instance.TopMost)
            {
                this.TopMost = true;
            }
            this.treeViewOption.BackColor = Color.FromArgb(249, 249, 249);
            this.panel.BackColor = Color.FromArgb(249, 249, 249);
            this.treeViewOption.SelectedNode = treeViewOption.Nodes[0];
            this.treeViewOption.Nodes[0].Text = Resources.nodeGeneral;
            this.treeViewOption.Nodes[1].Text = Resources.nodePreference;
            this.treeViewOption.Nodes[2].Text = Resources.nodeExtensions;
        }

        private void treeViewOption_AfterSelect(object sender, TreeViewEventArgs e)
        {
            UserControl userControl = null;
            switch (e.Node.Name)
            {
                case "nodeGeneral":
                    userControl = GeneralSettings.Instance;
                    break;
                case "nodePreferences":
                    userControl = PreferenceSettings.Instance;
                    break;
                case "nodeExtensions":
                    userControl = ExtensionsSettings.Instance;
                    break;
                case "nodeGARbroDB":
                    userControl = ExtensionGARbroDB.Instance;
                    break;
            }
            if (userControl != null)
            {
                this.panel.Controls.Clear();
                userControl.Dock = DockStyle.Fill;
                this.panel.Controls.Add(userControl);
            }
        }
    }
}
