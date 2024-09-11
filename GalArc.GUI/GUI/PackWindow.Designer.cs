namespace GalArc.GUI
{
    partial class PackWindow
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
            this.pa_FilePath = new System.Windows.Forms.TextBox();
            this.pa_lbOutputFile = new System.Windows.Forms.Label();
            this.pa_btnSelFolder = new System.Windows.Forms.Button();
            this.pa_btnSelFile = new System.Windows.Forms.Button();
            this.pa_FolderPath = new System.Windows.Forms.TextBox();
            this.pa_lbInputFolder = new System.Windows.Forms.Label();
            this.pa_ShowFormat = new System.Windows.Forms.ListBox();
            this.pa_lbFormat = new System.Windows.Forms.Label();
            this.pa_selEngine = new System.Windows.Forms.ComboBox();
            this.pa_lbEngine = new System.Windows.Forms.Label();
            this.pa_diaSelFile = new System.Windows.Forms.OpenFileDialog();
            this.pa_diaSelFolder = new System.Windows.Forms.FolderBrowserDialog();
            this.pa_btnClear = new System.Windows.Forms.Button();
            this.pa_chkbxMatch = new System.Windows.Forms.CheckBox();
            this.btnPack = new System.Windows.Forms.Button();
            this.pa_chkbxShowLog = new System.Windows.Forms.CheckBox();
            this.pa_gbOption = new System.Windows.Forms.GroupBox();
            this.pa_combVersion = new System.Windows.Forms.ComboBox();
            this.pa_lbPackVersion = new System.Windows.Forms.Label();
            this.pa_combPackFormat = new System.Windows.Forms.ComboBox();
            this.pa_lbPackFormat = new System.Windows.Forms.Label();
            this.pa_combEncoding = new System.Windows.Forms.ComboBox();
            this.pa_lbEncoding = new System.Windows.Forms.Label();
            this.pa_gbOption.SuspendLayout();
            this.SuspendLayout();
            // 
            // pa_FilePath
            // 
            this.pa_FilePath.AllowDrop = true;
            this.pa_FilePath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pa_FilePath.Location = new System.Drawing.Point(251, 186);
            this.pa_FilePath.Name = "pa_FilePath";
            this.pa_FilePath.Size = new System.Drawing.Size(724, 31);
            this.pa_FilePath.TabIndex = 19;
            // 
            // pa_lbOutputFile
            // 
            this.pa_lbOutputFile.AutoSize = true;
            this.pa_lbOutputFile.Location = new System.Drawing.Point(247, 150);
            this.pa_lbOutputFile.Name = "pa_lbOutputFile";
            this.pa_lbOutputFile.Size = new System.Drawing.Size(108, 24);
            this.pa_lbOutputFile.TabIndex = 18;
            this.pa_lbOutputFile.Text = "Output File";
            // 
            // pa_btnSelFolder
            // 
            this.pa_btnSelFolder.Location = new System.Drawing.Point(981, 90);
            this.pa_btnSelFolder.Name = "pa_btnSelFolder";
            this.pa_btnSelFolder.Size = new System.Drawing.Size(38, 32);
            this.pa_btnSelFolder.TabIndex = 17;
            this.pa_btnSelFolder.Text = "…";
            this.pa_btnSelFolder.UseVisualStyleBackColor = true;
            this.pa_btnSelFolder.Click += new System.EventHandler(this.pa_btnSelFolder_Click);
            // 
            // pa_btnSelFile
            // 
            this.pa_btnSelFile.Location = new System.Drawing.Point(981, 186);
            this.pa_btnSelFile.Name = "pa_btnSelFile";
            this.pa_btnSelFile.Size = new System.Drawing.Size(38, 32);
            this.pa_btnSelFile.TabIndex = 16;
            this.pa_btnSelFile.Text = "…";
            this.pa_btnSelFile.UseVisualStyleBackColor = true;
            this.pa_btnSelFile.Click += new System.EventHandler(this.pa_btnSelFile_Click);
            // 
            // pa_FolderPath
            // 
            this.pa_FolderPath.AllowDrop = true;
            this.pa_FolderPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pa_FolderPath.Location = new System.Drawing.Point(251, 91);
            this.pa_FolderPath.Name = "pa_FolderPath";
            this.pa_FolderPath.Size = new System.Drawing.Size(724, 31);
            this.pa_FolderPath.TabIndex = 15;
            this.pa_FolderPath.TextChanged += new System.EventHandler(this.pa_FilePath_TextChanged);
            // 
            // pa_lbInputFolder
            // 
            this.pa_lbInputFolder.AutoSize = true;
            this.pa_lbInputFolder.Location = new System.Drawing.Point(247, 54);
            this.pa_lbInputFolder.Name = "pa_lbInputFolder";
            this.pa_lbInputFolder.Size = new System.Drawing.Size(116, 24);
            this.pa_lbInputFolder.TabIndex = 14;
            this.pa_lbInputFolder.Text = "Input Folder";
            // 
            // pa_ShowFormat
            // 
            this.pa_ShowFormat.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pa_ShowFormat.FormattingEnabled = true;
            this.pa_ShowFormat.ItemHeight = 24;
            this.pa_ShowFormat.Location = new System.Drawing.Point(61, 204);
            this.pa_ShowFormat.Name = "pa_ShowFormat";
            this.pa_ShowFormat.Size = new System.Drawing.Size(121, 98);
            this.pa_ShowFormat.TabIndex = 13;
            this.pa_ShowFormat.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.pa_ShowFormat_MouseDoubleClick);
            // 
            // pa_lbFormat
            // 
            this.pa_lbFormat.AutoSize = true;
            this.pa_lbFormat.Location = new System.Drawing.Point(57, 166);
            this.pa_lbFormat.Name = "pa_lbFormat";
            this.pa_lbFormat.Size = new System.Drawing.Size(72, 24);
            this.pa_lbFormat.TabIndex = 12;
            this.pa_lbFormat.Text = "Format";
            // 
            // pa_selEngine
            // 
            this.pa_selEngine.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.pa_selEngine.FormattingEnabled = true;
            this.pa_selEngine.Location = new System.Drawing.Point(61, 90);
            this.pa_selEngine.Name = "pa_selEngine";
            this.pa_selEngine.Size = new System.Drawing.Size(121, 32);
            this.pa_selEngine.TabIndex = 11;
            this.pa_selEngine.SelectedIndexChanged += new System.EventHandler(this.pa_selEngine_SelectedIndexChanged);
            // 
            // pa_lbEngine
            // 
            this.pa_lbEngine.AutoSize = true;
            this.pa_lbEngine.Location = new System.Drawing.Point(57, 54);
            this.pa_lbEngine.Name = "pa_lbEngine";
            this.pa_lbEngine.Size = new System.Drawing.Size(69, 24);
            this.pa_lbEngine.TabIndex = 10;
            this.pa_lbEngine.Text = "Engine";
            // 
            // pa_btnClear
            // 
            this.pa_btnClear.Location = new System.Drawing.Point(689, 240);
            this.pa_btnClear.Name = "pa_btnClear";
            this.pa_btnClear.Size = new System.Drawing.Size(133, 38);
            this.pa_btnClear.TabIndex = 20;
            this.pa_btnClear.Text = "Clear";
            this.pa_btnClear.UseVisualStyleBackColor = true;
            this.pa_btnClear.Click += new System.EventHandler(this.pa_btnClear_Click);
            // 
            // pa_chkbxMatch
            // 
            this.pa_chkbxMatch.AutoSize = true;
            this.pa_chkbxMatch.Checked = true;
            this.pa_chkbxMatch.CheckState = System.Windows.Forms.CheckState.Checked;
            this.pa_chkbxMatch.Location = new System.Drawing.Point(565, 246);
            this.pa_chkbxMatch.Name = "pa_chkbxMatch";
            this.pa_chkbxMatch.Size = new System.Drawing.Size(91, 28);
            this.pa_chkbxMatch.TabIndex = 22;
            this.pa_chkbxMatch.Text = "Match";
            this.pa_chkbxMatch.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.pa_chkbxMatch.UseVisualStyleBackColor = true;
            this.pa_chkbxMatch.CheckedChanged += new System.EventHandler(this.pa_chkbxMatch_CheckedChanged);
            // 
            // btnPack
            // 
            this.btnPack.Location = new System.Drawing.Point(842, 240);
            this.btnPack.Name = "btnPack";
            this.btnPack.Size = new System.Drawing.Size(133, 38);
            this.btnPack.TabIndex = 21;
            this.btnPack.Text = "Pack";
            this.btnPack.UseVisualStyleBackColor = true;
            this.btnPack.Click += new System.EventHandler(this.btnPack_Click);
            // 
            // pa_chkbxShowLog
            // 
            this.pa_chkbxShowLog.Location = new System.Drawing.Point(61, 529);
            this.pa_chkbxShowLog.Name = "pa_chkbxShowLog";
            this.pa_chkbxShowLog.Size = new System.Drawing.Size(121, 38);
            this.pa_chkbxShowLog.TabIndex = 24;
            this.pa_chkbxShowLog.Text = "Log";
            this.pa_chkbxShowLog.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.pa_chkbxShowLog.UseVisualStyleBackColor = true;
            this.pa_chkbxShowLog.CheckedChanged += new System.EventHandler(this.pa_chkbxShowLog_CheckedChanged);
            // 
            // pa_gbOption
            // 
            this.pa_gbOption.Controls.Add(this.pa_combVersion);
            this.pa_gbOption.Controls.Add(this.pa_lbPackVersion);
            this.pa_gbOption.Controls.Add(this.pa_combPackFormat);
            this.pa_gbOption.Controls.Add(this.pa_lbPackFormat);
            this.pa_gbOption.Controls.Add(this.pa_combEncoding);
            this.pa_gbOption.Controls.Add(this.pa_lbEncoding);
            this.pa_gbOption.Location = new System.Drawing.Point(240, 302);
            this.pa_gbOption.Name = "pa_gbOption";
            this.pa_gbOption.Size = new System.Drawing.Size(779, 265);
            this.pa_gbOption.TabIndex = 25;
            this.pa_gbOption.TabStop = false;
            this.pa_gbOption.Text = "Pack Option";
            // 
            // pa_combVersion
            // 
            this.pa_combVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.pa_combVersion.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.pa_combVersion.FormattingEnabled = true;
            this.pa_combVersion.Location = new System.Drawing.Point(365, 98);
            this.pa_combVersion.Name = "pa_combVersion";
            this.pa_combVersion.Size = new System.Drawing.Size(127, 32);
            this.pa_combVersion.TabIndex = 3;
            // 
            // pa_lbPackVersion
            // 
            this.pa_lbPackVersion.AutoSize = true;
            this.pa_lbPackVersion.Location = new System.Drawing.Point(361, 61);
            this.pa_lbPackVersion.Name = "pa_lbPackVersion";
            this.pa_lbPackVersion.Size = new System.Drawing.Size(74, 24);
            this.pa_lbPackVersion.TabIndex = 2;
            this.pa_lbPackVersion.Text = "Version";
            // 
            // pa_combPackFormat
            // 
            this.pa_combPackFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.pa_combPackFormat.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.pa_combPackFormat.FormattingEnabled = true;
            this.pa_combPackFormat.Location = new System.Drawing.Point(209, 98);
            this.pa_combPackFormat.Name = "pa_combPackFormat";
            this.pa_combPackFormat.Size = new System.Drawing.Size(127, 32);
            this.pa_combPackFormat.TabIndex = 3;
            this.pa_combPackFormat.SelectedIndexChanged += new System.EventHandler(this.pa_combPackFormat_SelectedIndexChanged);
            // 
            // pa_lbPackFormat
            // 
            this.pa_lbPackFormat.AutoSize = true;
            this.pa_lbPackFormat.Location = new System.Drawing.Point(205, 61);
            this.pa_lbPackFormat.Name = "pa_lbPackFormat";
            this.pa_lbPackFormat.Size = new System.Drawing.Size(117, 24);
            this.pa_lbPackFormat.TabIndex = 2;
            this.pa_lbPackFormat.Text = "Pack Format";
            // 
            // pa_combEncoding
            // 
            this.pa_combEncoding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.pa_combEncoding.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.pa_combEncoding.FormattingEnabled = true;
            this.pa_combEncoding.Items.AddRange(new object[] {
            "UTF-8",
            "Shift-JIS",
            "GBK"});
            this.pa_combEncoding.Location = new System.Drawing.Point(53, 98);
            this.pa_combEncoding.Name = "pa_combEncoding";
            this.pa_combEncoding.Size = new System.Drawing.Size(127, 32);
            this.pa_combEncoding.TabIndex = 3;
            // 
            // pa_lbEncoding
            // 
            this.pa_lbEncoding.AutoSize = true;
            this.pa_lbEncoding.Location = new System.Drawing.Point(49, 61);
            this.pa_lbEncoding.Name = "pa_lbEncoding";
            this.pa_lbEncoding.Size = new System.Drawing.Size(91, 24);
            this.pa_lbEncoding.TabIndex = 2;
            this.pa_lbEncoding.Text = "Encoding";
            // 
            // PackWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pa_gbOption);
            this.Controls.Add(this.pa_chkbxShowLog);
            this.Controls.Add(this.pa_btnClear);
            this.Controls.Add(this.pa_chkbxMatch);
            this.Controls.Add(this.btnPack);
            this.Controls.Add(this.pa_FilePath);
            this.Controls.Add(this.pa_lbOutputFile);
            this.Controls.Add(this.pa_btnSelFolder);
            this.Controls.Add(this.pa_btnSelFile);
            this.Controls.Add(this.pa_FolderPath);
            this.Controls.Add(this.pa_lbInputFolder);
            this.Controls.Add(this.pa_ShowFormat);
            this.Controls.Add(this.pa_lbFormat);
            this.Controls.Add(this.pa_selEngine);
            this.Controls.Add(this.pa_lbEngine);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "PackWindow";
            this.Size = new System.Drawing.Size(1097, 639);
            this.Load += new System.EventHandler(this.PackWindow_Load);
            this.pa_gbOption.ResumeLayout(false);
            this.pa_gbOption.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.TextBox pa_FilePath;
        private System.Windows.Forms.Button pa_btnSelFolder;
        private System.Windows.Forms.Button pa_btnSelFile;
        public System.Windows.Forms.TextBox pa_FolderPath;
        private System.Windows.Forms.OpenFileDialog pa_diaSelFile;
        private System.Windows.Forms.FolderBrowserDialog pa_diaSelFolder;
        internal System.Windows.Forms.Label pa_lbOutputFile;
        internal System.Windows.Forms.Label pa_lbInputFolder;
        internal System.Windows.Forms.Label pa_lbFormat;
        internal System.Windows.Forms.Label pa_lbEngine;
        internal System.Windows.Forms.Button pa_btnClear;
        internal System.Windows.Forms.Button btnPack;
        internal System.Windows.Forms.CheckBox pa_chkbxMatch;
        internal System.Windows.Forms.ListBox pa_ShowFormat;
        internal System.Windows.Forms.ComboBox pa_selEngine;
        internal System.Windows.Forms.CheckBox pa_chkbxShowLog;
        internal System.Windows.Forms.ComboBox pa_combEncoding;
        internal System.Windows.Forms.ComboBox pa_combVersion;
        internal System.Windows.Forms.ComboBox pa_combPackFormat;
        internal System.Windows.Forms.GroupBox pa_gbOption;
        internal System.Windows.Forms.Label pa_lbEncoding;
        internal System.Windows.Forms.Label pa_lbPackVersion;
        internal System.Windows.Forms.Label pa_lbPackFormat;
    }
}