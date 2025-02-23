namespace GalArc.GUI
{
    partial class GeneralSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GeneralSettings));
            this.chkbxAutoSave = new System.Windows.Forms.CheckBox();
            this.chkbxTopMost = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // chkbxAutoSave
            // 
            resources.ApplyResources(this.chkbxAutoSave, "chkbxAutoSave");
            this.chkbxAutoSave.Checked = true;
            this.chkbxAutoSave.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkbxAutoSave.Name = "chkbxAutoSave";
            this.chkbxAutoSave.UseVisualStyleBackColor = true;
            this.chkbxAutoSave.CheckedChanged += new System.EventHandler(this.chkbxAutoSave_CheckedChanged);
            // 
            // chkbxTopMost
            // 
            resources.ApplyResources(this.chkbxTopMost, "chkbxTopMost");
            this.chkbxTopMost.Name = "chkbxTopMost";
            this.chkbxTopMost.UseVisualStyleBackColor = true;
            this.chkbxTopMost.CheckedChanged += new System.EventHandler(this.chkbxTopMost_CheckedChanged);
            // 
            // GeneralSettings
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chkbxTopMost);
            this.Controls.Add(this.chkbxAutoSave);
            this.Name = "GeneralSettings";
            this.Load += new System.EventHandler(this.GeneralSettings_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkbxAutoSave;
        private System.Windows.Forms.CheckBox chkbxTopMost;
    }
}
