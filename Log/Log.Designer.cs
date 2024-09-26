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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogWindow));
            this.log_txtLog = new System.Windows.Forms.TextBox();
            this.log_chkbxDebug = new System.Windows.Forms.CheckBox();
            this.log_btnClear = new System.Windows.Forms.Button();
            this.log_btnResize = new System.Windows.Forms.Button();
            this.log_chkbxSave = new System.Windows.Forms.CheckBox();
            this.bar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // log_txtLog
            // 
            resources.ApplyResources(this.log_txtLog, "log_txtLog");
            this.log_txtLog.BackColor = System.Drawing.SystemColors.MenuText;
            this.log_txtLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.log_txtLog.ForeColor = System.Drawing.SystemColors.Window;
            this.log_txtLog.Name = "log_txtLog";
            this.log_txtLog.ReadOnly = true;
            // 
            // log_chkbxDebug
            // 
            resources.ApplyResources(this.log_chkbxDebug, "log_chkbxDebug");
            this.log_chkbxDebug.Name = "log_chkbxDebug";
            this.log_chkbxDebug.UseVisualStyleBackColor = true;
            this.log_chkbxDebug.CheckedChanged += new System.EventHandler(this.log_chkbxDebug_CheckedChanged);
            // 
            // log_btnClear
            // 
            resources.ApplyResources(this.log_btnClear, "log_btnClear");
            this.log_btnClear.Name = "log_btnClear";
            this.log_btnClear.UseVisualStyleBackColor = true;
            this.log_btnClear.Click += new System.EventHandler(this.log_btnClear_Click);
            // 
            // log_btnResize
            // 
            resources.ApplyResources(this.log_btnResize, "log_btnResize");
            this.log_btnResize.Name = "log_btnResize";
            this.log_btnResize.UseVisualStyleBackColor = true;
            this.log_btnResize.Click += new System.EventHandler(this.log_btnResize_Click);
            // 
            // log_chkbxSave
            // 
            resources.ApplyResources(this.log_chkbxSave, "log_chkbxSave");
            this.log_chkbxSave.Name = "log_chkbxSave";
            this.log_chkbxSave.UseVisualStyleBackColor = true;
            this.log_chkbxSave.CheckedChanged += new System.EventHandler(this.log_chkbxSave_CheckedChanged);
            // 
            // bar
            // 
            resources.ApplyResources(this.bar, "bar");
            this.bar.Name = "bar";
            this.bar.Step = 1;
            this.bar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            // 
            // LogWindow
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ControlBox = false;
            this.Controls.Add(this.bar);
            this.Controls.Add(this.log_chkbxSave);
            this.Controls.Add(this.log_btnResize);
            this.Controls.Add(this.log_btnClear);
            this.Controls.Add(this.log_chkbxDebug);
            this.Controls.Add(this.log_txtLog);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LogWindow";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Log_FormClosing);
            this.Load += new System.EventHandler(this.LogWindow_Load);
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
    }
}