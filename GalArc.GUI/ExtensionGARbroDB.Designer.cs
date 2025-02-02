namespace GalArc.GUI
{
    partial class ExtensionGARbroDB
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExtensionGARbroDB));
            this.lbFormatsJson = new System.Windows.Forms.Label();
            this.btSelect = new System.Windows.Forms.Button();
            this.txtJsonPath = new System.Windows.Forms.TextBox();
            this.txtDBInfo = new System.Windows.Forms.TextBox();
            this.lbDBInfo = new System.Windows.Forms.Label();
            this.panel = new System.Windows.Forms.Panel();
            this.chkbxEnableGARbroDB = new System.Windows.Forms.CheckBox();
            this.panel.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbFormatsJson
            // 
            resources.ApplyResources(this.lbFormatsJson, "lbFormatsJson");
            this.lbFormatsJson.Name = "lbFormatsJson";
            // 
            // btSelect
            // 
            resources.ApplyResources(this.btSelect, "btSelect");
            this.btSelect.Name = "btSelect";
            this.btSelect.UseVisualStyleBackColor = true;
            this.btSelect.Click += new System.EventHandler(this.btSelect_Click);
            // 
            // txtJsonPath
            // 
            resources.ApplyResources(this.txtJsonPath, "txtJsonPath");
            this.txtJsonPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtJsonPath.Name = "txtJsonPath";
            this.txtJsonPath.TextChanged += new System.EventHandler(this.txtJsonPath_TextChanged);
            // 
            // txtDBInfo
            // 
            resources.ApplyResources(this.txtDBInfo, "txtDBInfo");
            this.txtDBInfo.BackColor = System.Drawing.SystemColors.Control;
            this.txtDBInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDBInfo.Name = "txtDBInfo";
            this.txtDBInfo.ReadOnly = true;
            // 
            // lbDBInfo
            // 
            resources.ApplyResources(this.lbDBInfo, "lbDBInfo");
            this.lbDBInfo.Name = "lbDBInfo";
            // 
            // panel
            // 
            resources.ApplyResources(this.panel, "panel");
            this.panel.Controls.Add(this.lbFormatsJson);
            this.panel.Controls.Add(this.lbDBInfo);
            this.panel.Controls.Add(this.txtJsonPath);
            this.panel.Controls.Add(this.txtDBInfo);
            this.panel.Controls.Add(this.btSelect);
            this.panel.Name = "panel";
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
            // ExtensionGARbroDB
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chkbxEnableGARbroDB);
            this.Controls.Add(this.panel);
            this.Name = "ExtensionGARbroDB";
            this.Load += new System.EventHandler(this.ExtensionGARbroDB_Load);
            this.panel.ResumeLayout(false);
            this.panel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbFormatsJson;
        private System.Windows.Forms.Button btSelect;
        private System.Windows.Forms.TextBox txtJsonPath;
        private System.Windows.Forms.TextBox txtDBInfo;
        private System.Windows.Forms.Label lbDBInfo;
        private System.Windows.Forms.Panel panel;
        private System.Windows.Forms.CheckBox chkbxEnableGARbroDB;
    }
}
