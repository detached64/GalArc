namespace ArcFormats.Softpal
{
    partial class UnpackPACOptions
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UnpackPACOptions));
            this.chkbxDecScr = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // chkbxDecScr
            // 
            resources.ApplyResources(this.chkbxDecScr, "chkbxDecScr");
            this.chkbxDecScr.Checked = true;
            this.chkbxDecScr.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkbxDecScr.Name = "chkbxDecScr";
            this.chkbxDecScr.UseVisualStyleBackColor = true;
            this.chkbxDecScr.CheckedChanged += new System.EventHandler(this.chkbxDecScr_CheckedChanged);
            // 
            // UnpackPACOptions
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chkbxDecScr);
            this.Name = "UnpackPACOptions";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkbxDecScr;
    }
}
