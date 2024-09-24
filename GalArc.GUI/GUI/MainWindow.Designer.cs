namespace GalArc
{
    partial class MainWindow
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.main_statusLabel = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.optionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.languagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.combLang = new System.Windows.Forms.ToolStripComboBox();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkUpdateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.packPage = new System.Windows.Forms.TabPage();
            this.unpackPage = new System.Windows.Forms.TabPage();
            this.pages = new System.Windows.Forms.TabControl();
            this.menuStrip1.SuspendLayout();
            this.pages.SuspendLayout();
            this.SuspendLayout();
            // 
            // main_statusLabel
            // 
            this.main_statusLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.main_statusLabel.Location = new System.Drawing.Point(16, 691);
            this.main_statusLabel.Name = "main_statusLabel";
            this.main_statusLabel.Size = new System.Drawing.Size(1097, 27);
            this.main_statusLabel.TabIndex = 2;
            this.main_statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // menuStrip1
            // 
            this.menuStrip1.AutoSize = false;
            this.menuStrip1.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.optionToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1129, 32);
            this.menuStrip1.TabIndex = 3;
            this.menuStrip1.Text = "menuStrip";
            // 
            // optionToolStripMenuItem
            // 
            this.optionToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.languagesToolStripMenuItem});
            this.optionToolStripMenuItem.Name = "optionToolStripMenuItem";
            this.optionToolStripMenuItem.Size = new System.Drawing.Size(87, 28);
            this.optionToolStripMenuItem.Text = "Option";
            // 
            // languagesToolStripMenuItem
            // 
            this.languagesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.combLang});
            this.languagesToolStripMenuItem.Name = "languagesToolStripMenuItem";
            this.languagesToolStripMenuItem.Size = new System.Drawing.Size(270, 34);
            this.languagesToolStripMenuItem.Text = "Languages";
            // 
            // combLang
            // 
            this.combLang.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.combLang.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.combLang.Name = "combLang";
            this.combLang.Size = new System.Drawing.Size(121, 32);
            this.combLang.SelectedIndexChanged += new System.EventHandler(this.combLang_SelectedIndexChanged);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.checkUpdateToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(67, 28);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // checkUpdateToolStripMenuItem
            // 
            this.checkUpdateToolStripMenuItem.Name = "checkUpdateToolStripMenuItem";
            this.checkUpdateToolStripMenuItem.Size = new System.Drawing.Size(266, 34);
            this.checkUpdateToolStripMenuItem.Text = "Check for updates";
            this.checkUpdateToolStripMenuItem.Click += new System.EventHandler(this.checkUpdateToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(266, 34);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // packPage
            // 
            this.packPage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.packPage.Location = new System.Drawing.Point(4, 36);
            this.packPage.Name = "packPage";
            this.packPage.Padding = new System.Windows.Forms.Padding(3);
            this.packPage.Size = new System.Drawing.Size(1097, 593);
            this.packPage.TabIndex = 1;
            this.packPage.Text = "Pack";
            this.packPage.UseVisualStyleBackColor = true;
            // 
            // unpackPage
            // 
            this.unpackPage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.unpackPage.Location = new System.Drawing.Point(4, 36);
            this.unpackPage.Name = "unpackPage";
            this.unpackPage.Padding = new System.Windows.Forms.Padding(3);
            this.unpackPage.Size = new System.Drawing.Size(1097, 593);
            this.unpackPage.TabIndex = 0;
            this.unpackPage.Text = "Unpack";
            this.unpackPage.UseVisualStyleBackColor = true;
            // 
            // pages
            // 
            this.pages.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.pages.Controls.Add(this.unpackPage);
            this.pages.Controls.Add(this.packPage);
            this.pages.Location = new System.Drawing.Point(12, 55);
            this.pages.Name = "pages";
            this.pages.SelectedIndex = 0;
            this.pages.Size = new System.Drawing.Size(1105, 633);
            this.pages.TabIndex = 0;
            this.pages.SelectedIndexChanged += new System.EventHandler(this.pages_SelectedIndexChanged);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1129, 732);
            this.Controls.Add(this.main_statusLabel);
            this.Controls.Add(this.pages);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "MainWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "GalArc";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.main_FormClosing);
            this.Load += new System.EventHandler(this.main_Load);
            this.LocationChanged += new System.EventHandler(this.main_LocationChanged);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.main_KeyDown);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.pages.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        internal System.Windows.Forms.Label main_statusLabel;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem optionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        public System.Windows.Forms.TabPage packPage;
        public System.Windows.Forms.TabPage unpackPage;
        public System.Windows.Forms.TabControl pages;
        private System.Windows.Forms.ToolStripMenuItem checkUpdateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem languagesToolStripMenuItem;
        internal System.Windows.Forms.ToolStripComboBox combLang;
    }
}

