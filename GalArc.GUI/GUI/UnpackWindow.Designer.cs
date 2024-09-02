namespace GalArc.GUI
{
    partial class UnpackWindow
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
            this.un_lbEngine = new System.Windows.Forms.Label();
            this.un_selEngine = new System.Windows.Forms.ComboBox();
            this.un_lbFormat = new System.Windows.Forms.Label();
            this.un_ShowFormat = new System.Windows.Forms.ListBox();
            this.un_lbInputFile = new System.Windows.Forms.Label();
            this.un_FilePath = new System.Windows.Forms.TextBox();
            this.un_btnSelFile = new System.Windows.Forms.Button();
            this.un_btnSelFolder = new System.Windows.Forms.Button();
            this.un_diaSelFile = new System.Windows.Forms.OpenFileDialog();
            this.un_diaSelFolder = new System.Windows.Forms.FolderBrowserDialog();
            this.un_lbOutputFolder = new System.Windows.Forms.Label();
            this.un_FolderPath = new System.Windows.Forms.TextBox();
            this.un_gbOption = new System.Windows.Forms.GroupBox();
            this.un_combEncoding = new System.Windows.Forms.ComboBox();
            this.un_lbEncoding = new System.Windows.Forms.Label();
            this.btnUnpack = new System.Windows.Forms.Button();
            this.un_btnClear = new System.Windows.Forms.Button();
            this.un_chkbxMatch = new System.Windows.Forms.CheckBox();
            this.un_chkbxShowLog = new System.Windows.Forms.CheckBox();
            this.un_gbOption.SuspendLayout();
            this.SuspendLayout();
            // 
            // un_lbEngine
            // 
            this.un_lbEngine.AutoSize = true;
            this.un_lbEngine.Location = new System.Drawing.Point(57, 54);
            this.un_lbEngine.Name = "un_lbEngine";
            this.un_lbEngine.Size = new System.Drawing.Size(69, 24);
            this.un_lbEngine.TabIndex = 0;
            this.un_lbEngine.Text = "Engine";
            // 
            // un_selEngine
            // 
            this.un_selEngine.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.un_selEngine.FormattingEnabled = true;
            this.un_selEngine.Location = new System.Drawing.Point(61, 90);
            this.un_selEngine.Name = "un_selEngine";
            this.un_selEngine.Size = new System.Drawing.Size(121, 32);
            this.un_selEngine.TabIndex = 1;
            this.un_selEngine.SelectedIndexChanged += new System.EventHandler(this.un_selEngine_SelectedIndexChanged);
            // 
            // un_lbFormat
            // 
            this.un_lbFormat.AutoSize = true;
            this.un_lbFormat.Location = new System.Drawing.Point(57, 166);
            this.un_lbFormat.Name = "un_lbFormat";
            this.un_lbFormat.Size = new System.Drawing.Size(72, 24);
            this.un_lbFormat.TabIndex = 2;
            this.un_lbFormat.Text = "Format";
            // 
            // un_ShowFormat
            // 
            this.un_ShowFormat.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.un_ShowFormat.FormattingEnabled = true;
            this.un_ShowFormat.ItemHeight = 24;
            this.un_ShowFormat.Location = new System.Drawing.Point(61, 204);
            this.un_ShowFormat.Name = "un_ShowFormat";
            this.un_ShowFormat.Size = new System.Drawing.Size(121, 98);
            this.un_ShowFormat.TabIndex = 3;
            // 
            // un_lbInputFile
            // 
            this.un_lbInputFile.AutoSize = true;
            this.un_lbInputFile.Location = new System.Drawing.Point(247, 54);
            this.un_lbInputFile.Name = "un_lbInputFile";
            this.un_lbInputFile.Size = new System.Drawing.Size(91, 24);
            this.un_lbInputFile.TabIndex = 4;
            this.un_lbInputFile.Text = "Input File";
            // 
            // un_FilePath
            // 
            this.un_FilePath.AllowDrop = true;
            this.un_FilePath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.un_FilePath.Location = new System.Drawing.Point(251, 91);
            this.un_FilePath.Name = "un_FilePath";
            this.un_FilePath.Size = new System.Drawing.Size(724, 31);
            this.un_FilePath.TabIndex = 5;
            this.un_FilePath.TextChanged += new System.EventHandler(this.un_FilePath_TextChanged);
            // 
            // un_btnSelFile
            // 
            this.un_btnSelFile.Location = new System.Drawing.Point(981, 90);
            this.un_btnSelFile.Name = "un_btnSelFile";
            this.un_btnSelFile.Size = new System.Drawing.Size(38, 32);
            this.un_btnSelFile.TabIndex = 6;
            this.un_btnSelFile.Text = "…";
            this.un_btnSelFile.UseVisualStyleBackColor = true;
            this.un_btnSelFile.Click += new System.EventHandler(this.un_btnSelFile_Click);
            // 
            // un_btnSelFolder
            // 
            this.un_btnSelFolder.Location = new System.Drawing.Point(981, 186);
            this.un_btnSelFolder.Name = "un_btnSelFolder";
            this.un_btnSelFolder.Size = new System.Drawing.Size(38, 32);
            this.un_btnSelFolder.TabIndex = 7;
            this.un_btnSelFolder.Text = "…";
            this.un_btnSelFolder.UseVisualStyleBackColor = true;
            this.un_btnSelFolder.Click += new System.EventHandler(this.un_btnSelFolder_Click);
            // 
            // un_lbOutputFolder
            // 
            this.un_lbOutputFolder.AutoSize = true;
            this.un_lbOutputFolder.Location = new System.Drawing.Point(247, 150);
            this.un_lbOutputFolder.Name = "un_lbOutputFolder";
            this.un_lbOutputFolder.Size = new System.Drawing.Size(133, 24);
            this.un_lbOutputFolder.TabIndex = 8;
            this.un_lbOutputFolder.Text = "Output Folder";
            // 
            // un_FolderPath
            // 
            this.un_FolderPath.AllowDrop = true;
            this.un_FolderPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.un_FolderPath.Location = new System.Drawing.Point(251, 186);
            this.un_FolderPath.Name = "un_FolderPath";
            this.un_FolderPath.Size = new System.Drawing.Size(724, 31);
            this.un_FolderPath.TabIndex = 9;
            // 
            // un_gbOption
            // 
            this.un_gbOption.Controls.Add(this.un_combEncoding);
            this.un_gbOption.Controls.Add(this.un_lbEncoding);
            this.un_gbOption.Location = new System.Drawing.Point(240, 302);
            this.un_gbOption.Name = "un_gbOption";
            this.un_gbOption.Size = new System.Drawing.Size(779, 265);
            this.un_gbOption.TabIndex = 10;
            this.un_gbOption.TabStop = false;
            this.un_gbOption.Text = "Unpack Option";
            // 
            // un_combEncoding
            // 
            this.un_combEncoding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.un_combEncoding.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.un_combEncoding.FormattingEnabled = true;
            this.un_combEncoding.Items.AddRange(new object[] {
            "UTF-8",
            "Shift-JIS",
            "GBK"});
            this.un_combEncoding.Location = new System.Drawing.Point(53, 98);
            this.un_combEncoding.Name = "un_combEncoding";
            this.un_combEncoding.Size = new System.Drawing.Size(127, 32);
            this.un_combEncoding.TabIndex = 1;
            // 
            // un_lbEncoding
            // 
            this.un_lbEncoding.AutoSize = true;
            this.un_lbEncoding.Location = new System.Drawing.Point(49, 61);
            this.un_lbEncoding.Name = "un_lbEncoding";
            this.un_lbEncoding.Size = new System.Drawing.Size(91, 24);
            this.un_lbEncoding.TabIndex = 0;
            this.un_lbEncoding.Text = "Encoding";
            // 
            // btnUnpack
            // 
            this.btnUnpack.Location = new System.Drawing.Point(842, 240);
            this.btnUnpack.Name = "btnUnpack";
            this.btnUnpack.Size = new System.Drawing.Size(133, 38);
            this.btnUnpack.TabIndex = 11;
            this.btnUnpack.Text = "Unpack";
            this.btnUnpack.UseVisualStyleBackColor = true;
            this.btnUnpack.Click += new System.EventHandler(this.btnUnpack_Click);
            // 
            // un_btnClear
            // 
            this.un_btnClear.Location = new System.Drawing.Point(689, 240);
            this.un_btnClear.Name = "un_btnClear";
            this.un_btnClear.Size = new System.Drawing.Size(133, 38);
            this.un_btnClear.TabIndex = 11;
            this.un_btnClear.Text = "Clear";
            this.un_btnClear.UseVisualStyleBackColor = true;
            this.un_btnClear.Click += new System.EventHandler(this.un_btnClear_Click);
            // 
            // un_chkbxMatch
            // 
            this.un_chkbxMatch.AutoSize = true;
            this.un_chkbxMatch.Checked = true;
            this.un_chkbxMatch.CheckState = System.Windows.Forms.CheckState.Checked;
            this.un_chkbxMatch.Location = new System.Drawing.Point(565, 246);
            this.un_chkbxMatch.Name = "un_chkbxMatch";
            this.un_chkbxMatch.Size = new System.Drawing.Size(91, 28);
            this.un_chkbxMatch.TabIndex = 12;
            this.un_chkbxMatch.Text = "Match";
            this.un_chkbxMatch.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.un_chkbxMatch.UseVisualStyleBackColor = true;
            this.un_chkbxMatch.CheckedChanged += new System.EventHandler(this.un_chkbxMatch_CheckedChanged);
            // 
            // un_chkbxShowLog
            // 
            this.un_chkbxShowLog.Location = new System.Drawing.Point(61, 529);
            this.un_chkbxShowLog.Name = "un_chkbxShowLog";
            this.un_chkbxShowLog.Size = new System.Drawing.Size(121, 38);
            this.un_chkbxShowLog.TabIndex = 14;
            this.un_chkbxShowLog.Text = "Log";
            this.un_chkbxShowLog.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.un_chkbxShowLog.UseVisualStyleBackColor = true;
            this.un_chkbxShowLog.CheckedChanged += new System.EventHandler(this.un_chkbxShowLog_CheckedChanged);
            // 
            // UnpackWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.un_chkbxShowLog);
            this.Controls.Add(this.un_btnClear);
            this.Controls.Add(this.un_chkbxMatch);
            this.Controls.Add(this.btnUnpack);
            this.Controls.Add(this.un_gbOption);
            this.Controls.Add(this.un_FolderPath);
            this.Controls.Add(this.un_lbOutputFolder);
            this.Controls.Add(this.un_btnSelFolder);
            this.Controls.Add(this.un_btnSelFile);
            this.Controls.Add(this.un_FilePath);
            this.Controls.Add(this.un_lbInputFile);
            this.Controls.Add(this.un_ShowFormat);
            this.Controls.Add(this.un_lbFormat);
            this.Controls.Add(this.un_selEngine);
            this.Controls.Add(this.un_lbEngine);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "UnpackWindow";
            this.Size = new System.Drawing.Size(1097, 639);
            this.Load += new System.EventHandler(this.UnpackWindow_Load);
            this.un_gbOption.ResumeLayout(false);
            this.un_gbOption.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button un_btnSelFile;
        private System.Windows.Forms.Button un_btnSelFolder;
        public System.Windows.Forms.TextBox un_FilePath;
        private System.Windows.Forms.OpenFileDialog un_diaSelFile;
        private System.Windows.Forms.FolderBrowserDialog un_diaSelFolder;
        public System.Windows.Forms.TextBox un_FolderPath;
        internal System.Windows.Forms.Label un_lbEngine;
        internal System.Windows.Forms.Label un_lbFormat;
        internal System.Windows.Forms.Label un_lbInputFile;
        internal System.Windows.Forms.Label un_lbOutputFolder;
        internal System.Windows.Forms.Button btnUnpack;
        internal System.Windows.Forms.Button un_btnClear;
        internal System.Windows.Forms.CheckBox un_chkbxMatch;
        internal System.Windows.Forms.ComboBox un_selEngine;
        internal System.Windows.Forms.ListBox un_ShowFormat;
        internal System.Windows.Forms.CheckBox un_chkbxShowLog;
        internal System.Windows.Forms.ComboBox un_combEncoding;
        internal System.Windows.Forms.GroupBox un_gbOption;
        internal System.Windows.Forms.Label un_lbEncoding;
    }
}