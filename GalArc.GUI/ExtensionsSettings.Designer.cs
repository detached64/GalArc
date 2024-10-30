namespace GalArc.GUI
{
    partial class ExtensionsSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExtensionsSettings));
            this.chkbxEnableExtensions = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // chkbxEnableExtensions
            // 
            resources.ApplyResources(this.chkbxEnableExtensions, "chkbxEnableExtensions");
            this.chkbxEnableExtensions.Checked = true;
            this.chkbxEnableExtensions.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkbxEnableExtensions.Name = "chkbxEnableExtensions";
            this.chkbxEnableExtensions.UseVisualStyleBackColor = true;
            this.chkbxEnableExtensions.CheckedChanged += new System.EventHandler(this.chkbxEnableExtensions_CheckedChanged);
            // 
            // ExtensionsSettings
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chkbxEnableExtensions);
            this.Name = "ExtensionsSettings";
            this.Load += new System.EventHandler(this.ExtensionsSettings_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkbxEnableExtensions;
    }
}
