namespace GalArc.GUI
{
    partial class ExtensionSiglusKeyFinder
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

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExtensionSiglusKeyFinder));
            this.chkbxEnableGARbroDB = new System.Windows.Forms.CheckBox();
            this.lbKeyFinderPath = new System.Windows.Forms.Label();
            this.txtExePath = new System.Windows.Forms.TextBox();
            this.btSelect = new System.Windows.Forms.Button();
            this.panel = new System.Windows.Forms.Panel();
            this.panel.SuspendLayout();
            this.SuspendLayout();
            // 
            // chkbxEnableGARbroDB
            // 
            resources.ApplyResources(this.chkbxEnableGARbroDB, "chkbxEnableGARbroDB");
            this.chkbxEnableGARbroDB.Checked = true;
            this.chkbxEnableGARbroDB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkbxEnableGARbroDB.Name = "chkbxEnableGARbroDB";
            this.chkbxEnableGARbroDB.UseVisualStyleBackColor = true;
            this.chkbxEnableGARbroDB.CheckedChanged += new System.EventHandler(this.chkbxEnableGARbroDB_CheckedChanged);
            // 
            // lbKeyFinderPath
            // 
            resources.ApplyResources(this.lbKeyFinderPath, "lbKeyFinderPath");
            this.lbKeyFinderPath.Name = "lbKeyFinderPath";
            // 
            // txtExePath
            // 
            resources.ApplyResources(this.txtExePath, "txtExePath");
            this.txtExePath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtExePath.Name = "txtExePath";
            this.txtExePath.TextChanged += new System.EventHandler(this.txtExePath_TextChanged);
            // 
            // btSelect
            // 
            resources.ApplyResources(this.btSelect, "btSelect");
            this.btSelect.Name = "btSelect";
            this.btSelect.UseVisualStyleBackColor = true;
            this.btSelect.Click += new System.EventHandler(this.btSelect_Click);
            // 
            // panel
            // 
            resources.ApplyResources(this.panel, "panel");
            this.panel.Controls.Add(this.txtExePath);
            this.panel.Controls.Add(this.lbKeyFinderPath);
            this.panel.Controls.Add(this.btSelect);
            this.panel.Name = "panel";
            // 
            // ExtensionSiglusKeyFinder
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel);
            this.Controls.Add(this.chkbxEnableGARbroDB);
            this.Name = "ExtensionSiglusKeyFinder";
            this.Load += new System.EventHandler(this.ExtensionSiglusKeyFinder_Load);
            this.panel.ResumeLayout(false);
            this.panel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkbxEnableGARbroDB;
        private System.Windows.Forms.Label lbKeyFinderPath;
        private System.Windows.Forms.TextBox txtExePath;
        private System.Windows.Forms.Button btSelect;
        private System.Windows.Forms.Panel panel;
    }
}
