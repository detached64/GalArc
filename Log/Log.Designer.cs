namespace Log
{
    partial class LogWindow
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
            this.log_txtLog = new System.Windows.Forms.TextBox();
            this.log_chkbxDebug = new System.Windows.Forms.CheckBox();
            this.log_btnClear = new System.Windows.Forms.Button();
            this.log_btnResize = new System.Windows.Forms.Button();
            this.log_chkbxSave = new System.Windows.Forms.CheckBox();
            this.bar = new System.Windows.Forms.ProgressBar();
            this.log_btnHide = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // log_txtLog
            // 
            this.log_txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.log_txtLog.BackColor = System.Drawing.SystemColors.MenuText;
            this.log_txtLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.log_txtLog.ForeColor = System.Drawing.SystemColors.Window;
            this.log_txtLog.Location = new System.Drawing.Point(16, 12);
            this.log_txtLog.Multiline = true;
            this.log_txtLog.Name = "log_txtLog";
            this.log_txtLog.ReadOnly = true;
            this.log_txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.log_txtLog.Size = new System.Drawing.Size(1097, 230);
            this.log_txtLog.TabIndex = 0;
            // 
            // log_chkbxDebug
            // 
            this.log_chkbxDebug.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.log_chkbxDebug.AutoSize = true;
            this.log_chkbxDebug.Location = new System.Drawing.Point(904, 252);
            this.log_chkbxDebug.Name = "log_chkbxDebug";
            this.log_chkbxDebug.Size = new System.Drawing.Size(95, 28);
            this.log_chkbxDebug.TabIndex = 1;
            this.log_chkbxDebug.Text = "Debug";
            this.log_chkbxDebug.UseVisualStyleBackColor = true;
            this.log_chkbxDebug.CheckedChanged += new System.EventHandler(this.log_chkbxDebug_CheckedChanged);
            // 
            // log_btnClear
            // 
            this.log_btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.log_btnClear.AutoSize = true;
            this.log_btnClear.Location = new System.Drawing.Point(230, 250);
            this.log_btnClear.Name = "log_btnClear";
            this.log_btnClear.Size = new System.Drawing.Size(101, 34);
            this.log_btnClear.TabIndex = 2;
            this.log_btnClear.Text = "Clear";
            this.log_btnClear.UseVisualStyleBackColor = true;
            this.log_btnClear.Click += new System.EventHandler(this.log_btnClear_Click);
            // 
            // log_btnResize
            // 
            this.log_btnResize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.log_btnResize.Location = new System.Drawing.Point(123, 250);
            this.log_btnResize.Name = "log_btnResize";
            this.log_btnResize.Size = new System.Drawing.Size(101, 34);
            this.log_btnResize.TabIndex = 3;
            this.log_btnResize.Text = "Resize";
            this.log_btnResize.UseVisualStyleBackColor = true;
            this.log_btnResize.Click += new System.EventHandler(this.log_btnResize_Click);
            // 
            // log_chkbxSave
            // 
            this.log_chkbxSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.log_chkbxSave.AutoSize = true;
            this.log_chkbxSave.Location = new System.Drawing.Point(1005, 252);
            this.log_chkbxSave.Name = "log_chkbxSave";
            this.log_chkbxSave.Size = new System.Drawing.Size(108, 28);
            this.log_chkbxSave.TabIndex = 4;
            this.log_chkbxSave.Text = "Save log";
            this.log_chkbxSave.UseVisualStyleBackColor = true;
            this.log_chkbxSave.CheckedChanged += new System.EventHandler(this.log_chkbxSave_CheckedChanged);
            // 
            // bar
            // 
            this.bar.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.bar.Location = new System.Drawing.Point(359, 252);
            this.bar.Name = "bar";
            this.bar.Size = new System.Drawing.Size(517, 30);
            this.bar.Step = 1;
            this.bar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.bar.TabIndex = 5;
            // 
            // log_btnHide
            // 
            this.log_btnHide.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.log_btnHide.AutoSize = true;
            this.log_btnHide.Location = new System.Drawing.Point(16, 250);
            this.log_btnHide.Name = "log_btnHide";
            this.log_btnHide.Size = new System.Drawing.Size(101, 34);
            this.log_btnHide.TabIndex = 2;
            this.log_btnHide.Text = "Hide";
            this.log_btnHide.UseVisualStyleBackColor = true;
            this.log_btnHide.Click += new System.EventHandler(this.log_btnHide_Click);
            // 
            // LogWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1129, 293);
            this.ControlBox = false;
            this.Controls.Add(this.bar);
            this.Controls.Add(this.log_chkbxSave);
            this.Controls.Add(this.log_btnResize);
            this.Controls.Add(this.log_btnHide);
            this.Controls.Add(this.log_btnClear);
            this.Controls.Add(this.log_chkbxDebug);
            this.Controls.Add(this.log_txtLog);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(1151, 200);
            this.Name = "LogWindow";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Log_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        public System.Windows.Forms.TextBox log_txtLog;
        public System.Windows.Forms.ProgressBar bar;
        public System.Windows.Forms.Button log_btnClear;
        public System.Windows.Forms.Button log_btnResize;
        public System.Windows.Forms.CheckBox log_chkbxDebug;
        public System.Windows.Forms.CheckBox log_chkbxSave;
        public System.Windows.Forms.Button log_btnHide;
    }
}