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
            this.pa_diaSelFile = new System.Windows.Forms.OpenFileDialog();
            this.pa_diaSelFolder = new System.Windows.Forms.FolderBrowserDialog();
            this.btnPack = new System.Windows.Forms.Button();
            this.pa_chkbxShowLog = new System.Windows.Forms.CheckBox();
            this.pa_gbOption = new System.Windows.Forms.GroupBox();
            this.pa_combVersion = new System.Windows.Forms.ComboBox();
            this.pa_lbPackVersion = new System.Windows.Forms.Label();
            this.pa_gbOption.SuspendLayout();
            this.SuspendLayout();
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
            // PackWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pa_gbOption);
            this.Controls.Add(this.pa_chkbxShowLog);
            this.Controls.Add(this.btnPack);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "PackWindow";
            this.Size = new System.Drawing.Size(1097, 639);
            this.Load += new System.EventHandler(this.PackWindow_Load);
            this.pa_gbOption.ResumeLayout(false);
            this.pa_gbOption.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.OpenFileDialog pa_diaSelFile;
        private System.Windows.Forms.FolderBrowserDialog pa_diaSelFolder;
        internal System.Windows.Forms.Button btnPack;
        internal System.Windows.Forms.CheckBox pa_chkbxShowLog;
        internal System.Windows.Forms.ComboBox pa_combVersion;
        internal System.Windows.Forms.GroupBox pa_gbOption;
        internal System.Windows.Forms.Label pa_lbPackVersion;
    }
}