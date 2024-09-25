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
            this.un_diaSelFile = new System.Windows.Forms.OpenFileDialog();
            this.un_diaSelFolder = new System.Windows.Forms.FolderBrowserDialog();
            this.btnUnpack = new System.Windows.Forms.Button();
            this.un_chkbxShowLog = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
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
            this.Controls.Add(this.btnUnpack);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "UnpackWindow";
            this.Size = new System.Drawing.Size(1097, 639);
            this.Load += new System.EventHandler(this.UnpackWindow_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.OpenFileDialog un_diaSelFile;
        private System.Windows.Forms.FolderBrowserDialog un_diaSelFolder;
        internal System.Windows.Forms.Button btnUnpack;
        internal System.Windows.Forms.CheckBox un_chkbxShowLog;
    }
}