namespace GalArc.GUI
{
    partial class Log
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
            this.log_chkbxVerbose = new System.Windows.Forms.CheckBox();
            this.log_btnClear = new System.Windows.Forms.Button();
            this.log_btnHide = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // log_txtLog
            // 
            this.log_txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.log_txtLog.BackColor = System.Drawing.SystemColors.MenuText;
            this.log_txtLog.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.log_txtLog.ForeColor = System.Drawing.SystemColors.Window;
            this.log_txtLog.Location = new System.Drawing.Point(12, 12);
            this.log_txtLog.Multiline = true;
            this.log_txtLog.Name = "log_txtLog";
            this.log_txtLog.ReadOnly = true;
            this.log_txtLog.Size = new System.Drawing.Size(1105, 281);
            this.log_txtLog.TabIndex = 0;
            // 
            // log_chkbxVerbose
            // 
            this.log_chkbxVerbose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.log_chkbxVerbose.AutoSize = true;
            this.log_chkbxVerbose.Location = new System.Drawing.Point(882, 303);
            this.log_chkbxVerbose.Name = "log_chkbxVerbose";
            this.log_chkbxVerbose.Size = new System.Drawing.Size(106, 28);
            this.log_chkbxVerbose.TabIndex = 1;
            this.log_chkbxVerbose.Text = "Verbose";
            this.log_chkbxVerbose.UseVisualStyleBackColor = true;
            // 
            // log_btnClear
            // 
            this.log_btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.log_btnClear.AutoSize = true;
            this.log_btnClear.Location = new System.Drawing.Point(1016, 299);
            this.log_btnClear.Name = "log_btnClear";
            this.log_btnClear.Size = new System.Drawing.Size(101, 34);
            this.log_btnClear.TabIndex = 2;
            this.log_btnClear.Text = "Clear";
            this.log_btnClear.UseVisualStyleBackColor = true;
            // 
            // log_btnHide
            // 
            this.log_btnHide.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.log_btnHide.AutoSize = true;
            this.log_btnHide.Location = new System.Drawing.Point(12, 299);
            this.log_btnHide.Name = "log_btnHide";
            this.log_btnHide.Size = new System.Drawing.Size(101, 34);
            this.log_btnHide.TabIndex = 2;
            this.log_btnHide.Text = "Hide";
            this.log_btnHide.UseVisualStyleBackColor = true;
            this.log_btnHide.Click += new System.EventHandler(this.log_btnHide_Click);
            // 
            // Log
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1129, 344);
            this.Controls.Add(this.log_btnHide);
            this.Controls.Add(this.log_btnClear);
            this.Controls.Add(this.log_chkbxVerbose);
            this.Controls.Add(this.log_txtLog);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Log";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Log_FormClosing);
            this.Load += new System.EventHandler(this.Log_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.CheckBox log_chkbxVerbose;
        public System.Windows.Forms.TextBox log_txtLog;
        private System.Windows.Forms.Button log_btnClear;
        private System.Windows.Forms.Button log_btnHide;
    }
}