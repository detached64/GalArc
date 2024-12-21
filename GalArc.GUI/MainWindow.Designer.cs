namespace GalArc.GUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.lbStatus = new System.Windows.Forms.Label();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.optionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.preferenceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.languagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.combLang = new System.Windows.Forms.ToolStripComboBox();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkUpdateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.treeViewEngines = new System.Windows.Forms.TreeView();
            this.chkbxUnpack = new System.Windows.Forms.RadioButton();
            this.chkbxPack = new System.Windows.Forms.RadioButton();
            this.lbInput = new System.Windows.Forms.Label();
            this.txtOutputPath = new System.Windows.Forms.TextBox();
            this.txtInputPath = new System.Windows.Forms.TextBox();
            this.lbOutput = new System.Windows.Forms.Label();
            this.btSelInput = new System.Windows.Forms.Button();
            this.btSelOutput = new System.Windows.Forms.Button();
            this.btClear = new System.Windows.Forms.Button();
            this.chkbxMatch = new System.Windows.Forms.CheckBox();
            this.btExecute = new System.Windows.Forms.Button();
            this.chkbxShowLog = new System.Windows.Forms.CheckBox();
            this.gbOptions = new System.Windows.Forms.GroupBox();
            this.menuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbStatus
            // 
            this.lbStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.lbStatus, "lbStatus");
            this.lbStatus.Name = "lbStatus";
            // 
            // menuStrip
            // 
            resources.ApplyResources(this.menuStrip, "menuStrip");
            this.menuStrip.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.menuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.optionToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip.Name = "menuStrip";
            // 
            // optionToolStripMenuItem
            // 
            this.optionToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.preferenceToolStripMenuItem,
            this.languagesToolStripMenuItem});
            this.optionToolStripMenuItem.Name = "optionToolStripMenuItem";
            resources.ApplyResources(this.optionToolStripMenuItem, "optionToolStripMenuItem");
            // 
            // preferenceToolStripMenuItem
            // 
            this.preferenceToolStripMenuItem.Name = "preferenceToolStripMenuItem";
            resources.ApplyResources(this.preferenceToolStripMenuItem, "preferenceToolStripMenuItem");
            this.preferenceToolStripMenuItem.Click += new System.EventHandler(this.preferenceToolStripMenuItem_Click);
            // 
            // languagesToolStripMenuItem
            // 
            this.languagesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.combLang});
            this.languagesToolStripMenuItem.Name = "languagesToolStripMenuItem";
            resources.ApplyResources(this.languagesToolStripMenuItem, "languagesToolStripMenuItem");
            // 
            // combLang
            // 
            this.combLang.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.combLang, "combLang");
            this.combLang.Name = "combLang";
            this.combLang.SelectedIndexChanged += new System.EventHandler(this.combLang_SelectedIndexChanged);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.checkUpdateToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            resources.ApplyResources(this.helpToolStripMenuItem, "helpToolStripMenuItem");
            // 
            // checkUpdateToolStripMenuItem
            // 
            this.checkUpdateToolStripMenuItem.Name = "checkUpdateToolStripMenuItem";
            resources.ApplyResources(this.checkUpdateToolStripMenuItem, "checkUpdateToolStripMenuItem");
            this.checkUpdateToolStripMenuItem.Click += new System.EventHandler(this.checkUpdateToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            resources.ApplyResources(this.aboutToolStripMenuItem, "aboutToolStripMenuItem");
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // treeViewEngines
            // 
            this.treeViewEngines.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.treeViewEngines.HideSelection = false;
            this.treeViewEngines.HotTracking = true;
            resources.ApplyResources(this.treeViewEngines, "treeViewEngines");
            this.treeViewEngines.Name = "treeViewEngines";
            this.treeViewEngines.PathSeparator = "/";
            this.treeViewEngines.ShowPlusMinus = false;
            this.treeViewEngines.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewEngines_AfterSelect);
            // 
            // chkbxUnpack
            // 
            resources.ApplyResources(this.chkbxUnpack, "chkbxUnpack");
            this.chkbxUnpack.Name = "chkbxUnpack";
            this.chkbxUnpack.UseVisualStyleBackColor = true;
            this.chkbxUnpack.CheckedChanged += new System.EventHandler(this.chkbxUnpack_CheckedChanged);
            this.chkbxUnpack.SizeChanged += new System.EventHandler(this.chkbxUnpack_SizeChanged);
            // 
            // chkbxPack
            // 
            resources.ApplyResources(this.chkbxPack, "chkbxPack");
            this.chkbxPack.Name = "chkbxPack";
            this.chkbxPack.UseVisualStyleBackColor = true;
            this.chkbxPack.CheckedChanged += new System.EventHandler(this.chkbxPack_CheckedChanged);
            // 
            // lbInput
            // 
            resources.ApplyResources(this.lbInput, "lbInput");
            this.lbInput.Name = "lbInput";
            // 
            // txtOutputPath
            // 
            this.txtOutputPath.AllowDrop = true;
            this.txtOutputPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.txtOutputPath, "txtOutputPath");
            this.txtOutputPath.Name = "txtOutputPath";
            // 
            // txtInputPath
            // 
            this.txtInputPath.AllowDrop = true;
            this.txtInputPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.txtInputPath, "txtInputPath");
            this.txtInputPath.Name = "txtInputPath";
            this.txtInputPath.TextChanged += new System.EventHandler(this.txtInputPath_TextChanged);
            // 
            // lbOutput
            // 
            resources.ApplyResources(this.lbOutput, "lbOutput");
            this.lbOutput.Name = "lbOutput";
            // 
            // btSelInput
            // 
            resources.ApplyResources(this.btSelInput, "btSelInput");
            this.btSelInput.Name = "btSelInput";
            this.btSelInput.UseVisualStyleBackColor = true;
            this.btSelInput.Click += new System.EventHandler(this.btSelInput_Click);
            // 
            // btSelOutput
            // 
            resources.ApplyResources(this.btSelOutput, "btSelOutput");
            this.btSelOutput.Name = "btSelOutput";
            this.btSelOutput.UseVisualStyleBackColor = true;
            this.btSelOutput.Click += new System.EventHandler(this.btSelOutput_Click);
            // 
            // btClear
            // 
            resources.ApplyResources(this.btClear, "btClear");
            this.btClear.Name = "btClear";
            this.btClear.UseVisualStyleBackColor = true;
            this.btClear.Click += new System.EventHandler(this.btClear_Click);
            // 
            // chkbxMatch
            // 
            resources.ApplyResources(this.chkbxMatch, "chkbxMatch");
            this.chkbxMatch.Checked = true;
            this.chkbxMatch.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkbxMatch.Name = "chkbxMatch";
            this.chkbxMatch.UseVisualStyleBackColor = true;
            this.chkbxMatch.CheckedChanged += new System.EventHandler(this.chkbxMatch_CheckedChanged);
            // 
            // btExecute
            // 
            resources.ApplyResources(this.btExecute, "btExecute");
            this.btExecute.Name = "btExecute";
            this.btExecute.UseVisualStyleBackColor = true;
            this.btExecute.Click += new System.EventHandler(this.btExecute_Click);
            // 
            // chkbxShowLog
            // 
            resources.ApplyResources(this.chkbxShowLog, "chkbxShowLog");
            this.chkbxShowLog.Name = "chkbxShowLog";
            this.chkbxShowLog.UseVisualStyleBackColor = true;
            this.chkbxShowLog.CheckedChanged += new System.EventHandler(this.chkbxShowLog_CheckedChanged);
            this.chkbxShowLog.SizeChanged += new System.EventHandler(this.chkbxShowLog_SizeChanged);
            // 
            // gbOptions
            // 
            resources.ApplyResources(this.gbOptions, "gbOptions");
            this.gbOptions.Name = "gbOptions";
            this.gbOptions.TabStop = false;
            // 
            // MainWindow
            // 
            this.AllowDrop = true;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbOptions);
            this.Controls.Add(this.chkbxShowLog);
            this.Controls.Add(this.btClear);
            this.Controls.Add(this.chkbxMatch);
            this.Controls.Add(this.btExecute);
            this.Controls.Add(this.btSelInput);
            this.Controls.Add(this.btSelOutput);
            this.Controls.Add(this.lbOutput);
            this.Controls.Add(this.txtOutputPath);
            this.Controls.Add(this.txtInputPath);
            this.Controls.Add(this.lbInput);
            this.Controls.Add(this.chkbxPack);
            this.Controls.Add(this.chkbxUnpack);
            this.Controls.Add(this.treeViewEngines);
            this.Controls.Add(this.lbStatus);
            this.Controls.Add(this.menuStrip);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip;
            this.MaximizeBox = false;
            this.Name = "MainWindow";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainWindow_FormClosing);
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.LocationChanged += new System.EventHandler(this.MainWindow_LocationChanged);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        internal System.Windows.Forms.Label lbStatus;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem optionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkUpdateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem languagesToolStripMenuItem;
        internal System.Windows.Forms.ToolStripComboBox combLang;
        internal System.Windows.Forms.TreeView treeViewEngines;
        private System.Windows.Forms.RadioButton chkbxUnpack;
        private System.Windows.Forms.RadioButton chkbxPack;
        private System.Windows.Forms.ToolStripMenuItem preferenceToolStripMenuItem;
        private System.Windows.Forms.Label lbInput;
        public System.Windows.Forms.TextBox txtOutputPath;
        public System.Windows.Forms.TextBox txtInputPath;
        private System.Windows.Forms.Label lbOutput;
        private System.Windows.Forms.Button btSelInput;
        private System.Windows.Forms.Button btSelOutput;
        internal System.Windows.Forms.Button btClear;
        internal System.Windows.Forms.CheckBox chkbxMatch;
        internal System.Windows.Forms.Button btExecute;
        private System.Windows.Forms.CheckBox chkbxShowLog;
        private System.Windows.Forms.GroupBox gbOptions;
    }
}

