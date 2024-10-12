using GalArc.Properties;
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
        }

        private void SettingsWindow_Load(object sender, EventArgs e)
        {
            if (MainWindow.Instance.TopMost)
            {
                this.TopMost = true;
            }
            this.panel.BackColor = Color.FromArgb(249, 249, 249);
            this.treeViewOption.SelectedNode = treeViewOption.Nodes[0];
            this.treeViewOption.Nodes[0].Text = Resources.nodeGeneral;
            this.treeViewOption.Nodes[1].Text = Resources.nodePreference;
        }

        private void treeViewOption_AfterSelect(object sender, TreeViewEventArgs e)
        {
            UserControl userControl = null;
            switch (e.Node.Index)
            {
                case 0:
                    userControl = new GeneralSettings();
                    break;
                case 1:
                    userControl = new PreferenceSettings();
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
