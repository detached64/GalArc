namespace ArcFormats.InnocentGrey
{
    partial class PackIGAWidget
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PackIGAWidget));
            this.chkbxEncScr = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // chkbxEncScr
            // 
            resources.ApplyResources(this.chkbxEncScr, "chkbxEncScr");
            this.chkbxEncScr.Checked = true;
            this.chkbxEncScr.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkbxEncScr.Name = "chkbxEncScr";
            this.chkbxEncScr.UseVisualStyleBackColor = true;
            this.chkbxEncScr.CheckedChanged += new System.EventHandler(this.chkbxEncScr_CheckedChanged);
            // 
            // PackIGAOptions
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chkbxEncScr);
            this.Name = "PackIGAOptions";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkbxEncScr;
    }
}
