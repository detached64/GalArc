namespace GalArc.GUI
{
    partial class OptionWindow
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
            this.op_lbLang = new System.Windows.Forms.Label();
            this.op_cbLang = new System.Windows.Forms.ComboBox();
            this.op_chkbxOnTop = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // op_lbLang
            // 
            this.op_lbLang.AutoSize = true;
            this.op_lbLang.Location = new System.Drawing.Point(74, 58);
            this.op_lbLang.Name = "op_lbLang";
            this.op_lbLang.Size = new System.Drawing.Size(95, 24);
            this.op_lbLang.TabIndex = 0;
            this.op_lbLang.Text = "Language";
            // 
            // op_cbLang
            // 
            this.op_cbLang.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.op_cbLang.FormattingEnabled = true;
            this.op_cbLang.Location = new System.Drawing.Point(78, 96);
            this.op_cbLang.Name = "op_cbLang";
            this.op_cbLang.Size = new System.Drawing.Size(129, 32);
            this.op_cbLang.TabIndex = 1;
            this.op_cbLang.SelectedIndexChanged += new System.EventHandler(this.op_cbLang_SelectedIndexChanged);
            // 
            // op_chkbxOnTop
            // 
            this.op_chkbxOnTop.AutoSize = true;
            this.op_chkbxOnTop.Location = new System.Drawing.Point(78, 177);
            this.op_chkbxOnTop.Name = "op_chkbxOnTop";
            this.op_chkbxOnTop.Size = new System.Drawing.Size(158, 28);
            this.op_chkbxOnTop.TabIndex = 2;
            this.op_chkbxOnTop.Text = "Always on top";
            this.op_chkbxOnTop.UseVisualStyleBackColor = true;
            this.op_chkbxOnTop.CheckedChanged += new System.EventHandler(this.op_chkbxTopMost_CheckedChanged);
            // 
            // OptionWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.op_chkbxOnTop);
            this.Controls.Add(this.op_cbLang);
            this.Controls.Add(this.op_lbLang);
            this.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "OptionWindow";
            this.Size = new System.Drawing.Size(1097, 639);
            this.Load += new System.EventHandler(this.OptionWindow_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        internal System.Windows.Forms.Label op_lbLang;
        internal System.Windows.Forms.CheckBox op_chkbxOnTop;
        internal System.Windows.Forms.ComboBox op_cbLang;
    }
}