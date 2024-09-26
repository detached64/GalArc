namespace GalArc.GUI
{
    partial class SettingsWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsWindow));
            this.treeViewOption = new System.Windows.Forms.TreeView();
            this.panel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // treeViewOption
            // 
            this.treeViewOption.BackColor = System.Drawing.SystemColors.Control;
            this.treeViewOption.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.treeViewOption, "treeViewOption");
            this.treeViewOption.Name = "treeViewOption";
            this.treeViewOption.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            ((System.Windows.Forms.TreeNode)(resources.GetObject("treeViewOption.Nodes"))),
            ((System.Windows.Forms.TreeNode)(resources.GetObject("treeViewOption.Nodes1")))});
            this.treeViewOption.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewOption_AfterSelect);
            // 
            // panel
            // 
            this.panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.panel, "panel");
            this.panel.Name = "panel";
            // 
            // SettingsWindow
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel);
            this.Controls.Add(this.treeViewOption);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsWindow";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Load += new System.EventHandler(this.SettingsWindow_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView treeViewOption;
        private System.Windows.Forms.Panel panel;
    }
}