namespace GalArc.GUI
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.optionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuMatchPaths = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuClearPaths = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuReimport = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuBatchExtraction = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuCheckUpdate = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuLanguages = new System.Windows.Forms.ToolStripMenuItem();
            this.CombLanguages = new System.Windows.Forms.ToolStripComboBox();
            this.treeViewEngines = new System.Windows.Forms.TreeView();
            this.chkbxUnpack = new System.Windows.Forms.RadioButton();
            this.chkbxPack = new System.Windows.Forms.RadioButton();
            this.lbInput = new System.Windows.Forms.Label();
            this.txtOutputPath = new System.Windows.Forms.TextBox();
            this.txtInputPath = new System.Windows.Forms.TextBox();
            this.lbOutput = new System.Windows.Forms.Label();
            this.btSelInput = new System.Windows.Forms.Button();
            this.btSelOutput = new System.Windows.Forms.Button();
            this.btExecute = new System.Windows.Forms.Button();
            this.gbOptions = new System.Windows.Forms.GroupBox();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.gbLog = new System.Windows.Forms.GroupBox();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.lbStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.pBar = new System.Windows.Forms.ToolStripProgressBar();
            this.pnlOperation = new System.Windows.Forms.Panel();
            this.menuStrip.SuspendLayout();
            this.gbLog.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.pnlOperation.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            resources.ApplyResources(this.menuStrip, "menuStrip");
            this.menuStrip.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.menuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.optionMenuItem,
            this.MenuSettings,
            this.MenuHelp});
            this.menuStrip.Name = "menuStrip";
            // 
            // optionMenuItem
            // 
            this.optionMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuMatchPaths,
            this.MenuClearPaths,
            this.toolStripSeparator1,
            this.MenuReimport,
            this.toolStripSeparator2,
            this.MenuBatchExtraction});
            this.optionMenuItem.Name = "optionMenuItem";
            resources.ApplyResources(this.optionMenuItem, "optionMenuItem");
            // 
            // MenuMatchPaths
            // 
            this.MenuMatchPaths.CheckOnClick = true;
            this.MenuMatchPaths.Name = "MenuMatchPaths";
            resources.ApplyResources(this.MenuMatchPaths, "MenuMatchPaths");
            this.MenuMatchPaths.CheckedChanged += new System.EventHandler(this.MenuMatchPaths_CheckedChanged);
            // 
            // MenuClearPaths
            // 
            this.MenuClearPaths.Name = "MenuClearPaths";
            resources.ApplyResources(this.MenuClearPaths, "MenuClearPaths");
            this.MenuClearPaths.Click += new System.EventHandler(this.MenuClearPaths_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // MenuReimport
            // 
            this.MenuReimport.Name = "MenuReimport";
            resources.ApplyResources(this.MenuReimport, "MenuReimport");
            this.MenuReimport.Click += new System.EventHandler(this.MenuReimportSchemes_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            // 
            // MenuBatchExtraction
            // 
            this.MenuBatchExtraction.CheckOnClick = true;
            this.MenuBatchExtraction.Name = "MenuBatchExtraction";
            resources.ApplyResources(this.MenuBatchExtraction, "MenuBatchExtraction");
            this.MenuBatchExtraction.Click += new System.EventHandler(this.MenuBatchExtraction_Click);
            // 
            // MenuSettings
            // 
            this.MenuSettings.Name = "MenuSettings";
            resources.ApplyResources(this.MenuSettings, "MenuSettings");
            this.MenuSettings.Click += new System.EventHandler(this.MenuSettings_Click);
            // 
            // MenuHelp
            // 
            this.MenuHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuCheckUpdate,
            this.MenuAbout,
            this.MenuLanguages});
            this.MenuHelp.Name = "MenuHelp";
            resources.ApplyResources(this.MenuHelp, "MenuHelp");
            // 
            // MenuCheckUpdate
            // 
            this.MenuCheckUpdate.Name = "MenuCheckUpdate";
            resources.ApplyResources(this.MenuCheckUpdate, "MenuCheckUpdate");
            this.MenuCheckUpdate.Click += new System.EventHandler(this.MenuCheckUpdate_Click);
            // 
            // MenuAbout
            // 
            this.MenuAbout.Name = "MenuAbout";
            resources.ApplyResources(this.MenuAbout, "MenuAbout");
            this.MenuAbout.Click += new System.EventHandler(this.MenuAbout_Click);
            // 
            // MenuLanguages
            // 
            this.MenuLanguages.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CombLanguages});
            this.MenuLanguages.Name = "MenuLanguages";
            resources.ApplyResources(this.MenuLanguages, "MenuLanguages");
            // 
            // CombLanguages
            // 
            this.CombLanguages.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.CombLanguages, "CombLanguages");
            this.CombLanguages.Name = "CombLanguages";
            this.CombLanguages.SelectedIndexChanged += new System.EventHandler(this.CombLanguages_SelectedIndexChanged);
            // 
            // treeViewEngines
            // 
            resources.ApplyResources(this.treeViewEngines, "treeViewEngines");
            this.treeViewEngines.BackColor = System.Drawing.SystemColors.Control;
            this.treeViewEngines.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.treeViewEngines.HideSelection = false;
            this.treeViewEngines.HotTracking = true;
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
            resources.ApplyResources(this.txtOutputPath, "txtOutputPath");
            this.txtOutputPath.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtOutputPath.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            this.txtOutputPath.Name = "txtOutputPath";
            // 
            // txtInputPath
            // 
            this.txtInputPath.AllowDrop = true;
            resources.ApplyResources(this.txtInputPath, "txtInputPath");
            this.txtInputPath.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtInputPath.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
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
            // btExecute
            // 
            resources.ApplyResources(this.btExecute, "btExecute");
            this.btExecute.Name = "btExecute";
            this.btExecute.UseVisualStyleBackColor = true;
            this.btExecute.Click += new System.EventHandler(this.btExecute_Click);
            // 
            // gbOptions
            // 
            resources.ApplyResources(this.gbOptions, "gbOptions");
            this.gbOptions.Name = "gbOptions";
            this.gbOptions.TabStop = false;
            // 
            // txtLog
            // 
            this.txtLog.BackColor = System.Drawing.SystemColors.MenuText;
            this.txtLog.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.txtLog, "txtLog");
            this.txtLog.ForeColor = System.Drawing.SystemColors.Window;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.TabStop = false;
            // 
            // gbLog
            // 
            resources.ApplyResources(this.gbLog, "gbLog");
            this.gbLog.Controls.Add(this.txtLog);
            this.gbLog.Name = "gbLog";
            this.gbLog.TabStop = false;
            // 
            // statusStrip
            // 
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lbStatus,
            this.pBar});
            this.statusStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            resources.ApplyResources(this.statusStrip, "statusStrip");
            this.statusStrip.Name = "statusStrip";
            // 
            // lbStatus
            // 
            this.lbStatus.BorderStyle = System.Windows.Forms.Border3DStyle.Bump;
            this.lbStatus.Name = "lbStatus";
            resources.ApplyResources(this.lbStatus, "lbStatus");
            this.lbStatus.Spring = true;
            // 
            // pBar
            // 
            this.pBar.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.pBar.Name = "pBar";
            resources.ApplyResources(this.pBar, "pBar");
            // 
            // pnlOperation
            // 
            this.pnlOperation.Controls.Add(this.chkbxUnpack);
            this.pnlOperation.Controls.Add(this.chkbxPack);
            resources.ApplyResources(this.pnlOperation, "pnlOperation");
            this.pnlOperation.Name = "pnlOperation";
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.gbLog);
            this.Controls.Add(this.gbOptions);
            this.Controls.Add(this.lbOutput);
            this.Controls.Add(this.txtOutputPath);
            this.Controls.Add(this.txtInputPath);
            this.Controls.Add(this.lbInput);
            this.Controls.Add(this.treeViewEngines);
            this.Controls.Add(this.menuStrip);
            this.Controls.Add(this.btExecute);
            this.Controls.Add(this.btSelInput);
            this.Controls.Add(this.btSelOutput);
            this.Controls.Add(this.pnlOperation);
            this.MainMenuStrip = this.menuStrip;
            this.Name = "MainForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.gbLog.ResumeLayout(false);
            this.gbLog.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.pnlOperation.ResumeLayout(false);
            this.pnlOperation.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem optionMenuItem;
        private System.Windows.Forms.ToolStripMenuItem MenuHelp;
        private System.Windows.Forms.ToolStripMenuItem MenuCheckUpdate;
        private System.Windows.Forms.ToolStripMenuItem MenuAbout;
        internal System.Windows.Forms.TreeView treeViewEngines;
        private System.Windows.Forms.RadioButton chkbxUnpack;
        private System.Windows.Forms.RadioButton chkbxPack;
        private System.Windows.Forms.Label lbInput;
        private System.Windows.Forms.Label lbOutput;
        private System.Windows.Forms.Button btSelInput;
        private System.Windows.Forms.Button btSelOutput;
        internal System.Windows.Forms.Button btExecute;
        private System.Windows.Forms.GroupBox gbOptions;
        public System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.GroupBox gbLog;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lbStatus;
        private System.Windows.Forms.ToolStripProgressBar pBar;
        private System.Windows.Forms.Panel pnlOperation;
        private System.Windows.Forms.ToolStripMenuItem MenuReimport;
        private System.Windows.Forms.ToolStripMenuItem MenuSettings;
        private System.Windows.Forms.ToolStripMenuItem MenuLanguages;
        internal System.Windows.Forms.ToolStripComboBox CombLanguages;
        private System.Windows.Forms.ToolStripMenuItem MenuMatchPaths;
        private System.Windows.Forms.ToolStripMenuItem MenuClearPaths;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.TextBox txtOutputPath;
        private System.Windows.Forms.TextBox txtInputPath;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem MenuBatchExtraction;
    }
}

