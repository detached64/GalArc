namespace GalArc.GUI
{
    partial class DataBaseSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DataBaseSettings));
            this.chkbxEnableDataBase = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lbDBPath = new System.Windows.Forms.Label();
            this.lbDBInfo = new System.Windows.Forms.Label();
            this.txtDBPath = new System.Windows.Forms.TextBox();
            this.txtDBInfo = new System.Windows.Forms.TextBox();
            this.btSelect = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // chkbxEnableDataBase
            // 
            resources.ApplyResources(this.chkbxEnableDataBase, "chkbxEnableDataBase");
            this.chkbxEnableDataBase.Checked = true;
            this.chkbxEnableDataBase.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkbxEnableDataBase.Name = "chkbxEnableDataBase";
            this.chkbxEnableDataBase.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lbDBPath);
            this.panel1.Controls.Add(this.lbDBInfo);
            this.panel1.Controls.Add(this.txtDBPath);
            this.panel1.Controls.Add(this.txtDBInfo);
            this.panel1.Controls.Add(this.btSelect);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // lbDBPath
            // 
            resources.ApplyResources(this.lbDBPath, "lbDBPath");
            this.lbDBPath.Name = "lbDBPath";
            // 
            // lbDBInfo
            // 
            resources.ApplyResources(this.lbDBInfo, "lbDBInfo");
            this.lbDBInfo.Name = "lbDBInfo";
            // 
            // txtDBPath
            // 
            resources.ApplyResources(this.txtDBPath, "txtDBPath");
            this.txtDBPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDBPath.Name = "txtDBPath";
            this.txtDBPath.TextChanged += new System.EventHandler(this.txtDBPath_TextChanged);
            // 
            // txtDBInfo
            // 
            resources.ApplyResources(this.txtDBInfo, "txtDBInfo");
            this.txtDBInfo.BackColor = System.Drawing.Color.White;
            this.txtDBInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDBInfo.Name = "txtDBInfo";
            this.txtDBInfo.ReadOnly = true;
            // 
            // btSelect
            // 
            resources.ApplyResources(this.btSelect, "btSelect");
            this.btSelect.Name = "btSelect";
            this.btSelect.UseVisualStyleBackColor = true;
            this.btSelect.Click += new System.EventHandler(this.btSelect_Click);
            // 
            // DataBaseSettings
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.chkbxEnableDataBase);
            this.Name = "DataBaseSettings";
            this.Load += new System.EventHandler(this.DataBaseSettings_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkbxEnableDataBase;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lbDBPath;
        private System.Windows.Forms.Label lbDBInfo;
        private System.Windows.Forms.TextBox txtDBPath;
        private System.Windows.Forms.TextBox txtDBInfo;
        private System.Windows.Forms.Button btSelect;
    }
}
