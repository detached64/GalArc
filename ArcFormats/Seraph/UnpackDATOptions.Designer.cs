namespace ArcFormats.Seraph
{
    partial class UnpackDATOptions
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UnpackDATOptions));
            this.chkbxSpecifyIndex = new System.Windows.Forms.RadioButton();
            this.chkbxBrutalForce = new System.Windows.Forms.RadioButton();
            this.txtIndexOffset = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // chkbxSpecifyIndex
            // 
            resources.ApplyResources(this.chkbxSpecifyIndex, "chkbxSpecifyIndex");
            this.chkbxSpecifyIndex.Name = "chkbxSpecifyIndex";
            this.chkbxSpecifyIndex.TabStop = true;
            this.chkbxSpecifyIndex.UseVisualStyleBackColor = true;
            this.chkbxSpecifyIndex.CheckedChanged += new System.EventHandler(this.chkbxSpecifyIndex_CheckedChanged);
            this.chkbxSpecifyIndex.SizeChanged += new System.EventHandler(this.chkbxSpecifyIndex_SizeChanged);
            // 
            // chkbxBrutalForce
            // 
            resources.ApplyResources(this.chkbxBrutalForce, "chkbxBrutalForce");
            this.chkbxBrutalForce.Checked = true;
            this.chkbxBrutalForce.Name = "chkbxBrutalForce";
            this.chkbxBrutalForce.TabStop = true;
            this.chkbxBrutalForce.UseVisualStyleBackColor = true;
            this.chkbxBrutalForce.CheckedChanged += new System.EventHandler(this.chkbxBrutalForce_CheckedChanged);
            // 
            // txtIndexOffset
            // 
            this.txtIndexOffset.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.txtIndexOffset, "txtIndexOffset");
            this.txtIndexOffset.Name = "txtIndexOffset";
            this.txtIndexOffset.TextChanged += new System.EventHandler(this.txtIndexOffset_TextChanged);
            // 
            // UnpackDATOptions
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtIndexOffset);
            this.Controls.Add(this.chkbxBrutalForce);
            this.Controls.Add(this.chkbxSpecifyIndex);
            this.Name = "UnpackDATOptions";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton chkbxSpecifyIndex;
        private System.Windows.Forms.RadioButton chkbxBrutalForce;
        private System.Windows.Forms.TextBox txtIndexOffset;
    }
}
