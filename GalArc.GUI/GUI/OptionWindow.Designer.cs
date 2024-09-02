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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OptionWindow));
            this.op_lbLang = new System.Windows.Forms.Label();
            this.op_cbLang = new System.Windows.Forms.ComboBox();
            this.op_chkbxOnTop = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // op_lbLang
            // 
            resources.ApplyResources(this.op_lbLang, "op_lbLang");
            this.op_lbLang.Name = "op_lbLang";
            // 
            // op_cbLang
            // 
            this.op_cbLang.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.op_cbLang.FormattingEnabled = true;
            this.op_cbLang.Items.AddRange(new object[] {
            resources.GetString("op_cbLang.Items"),
            resources.GetString("op_cbLang.Items1")});
            resources.ApplyResources(this.op_cbLang, "op_cbLang");
            this.op_cbLang.Name = "op_cbLang";
            this.op_cbLang.SelectedIndexChanged += new System.EventHandler(this.op_cbLang_SelectedIndexChanged);
            // 
            // op_chkbxOnTop
            // 
            resources.ApplyResources(this.op_chkbxOnTop, "op_chkbxOnTop");
            this.op_chkbxOnTop.Name = "op_chkbxOnTop";
            this.op_chkbxOnTop.UseVisualStyleBackColor = true;
            this.op_chkbxOnTop.CheckedChanged += new System.EventHandler(this.op_chkbxTopMost_CheckedChanged);
            // 
            // OptionWindow
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.op_chkbxOnTop);
            this.Controls.Add(this.op_cbLang);
            this.Controls.Add(this.op_lbLang);
            this.Name = "OptionWindow";
            this.Load += new System.EventHandler(this.OptionWindow_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ComboBox op_cbLang;
        internal System.Windows.Forms.Label op_lbLang;
        internal System.Windows.Forms.CheckBox op_chkbxOnTop;
    }
}