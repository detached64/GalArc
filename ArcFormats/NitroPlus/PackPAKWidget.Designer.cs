namespace ArcFormats.NitroPlus
{
    partial class PackPAKWidget
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PackPAKWidget));
            this.lbOriginalArc = new System.Windows.Forms.Label();
            this.txtOriginalFilePath = new System.Windows.Forms.TextBox();
            this.btSelect = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lbOriginalArc
            // 
            resources.ApplyResources(this.lbOriginalArc, "lbOriginalArc");
            this.lbOriginalArc.Name = "lbOriginalArc";
            this.lbOriginalArc.SizeChanged += new System.EventHandler(this.lbOriginalArc_SizeChanged);
            // 
            // txtOriginalFilePath
            // 
            this.txtOriginalFilePath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.txtOriginalFilePath, "txtOriginalFilePath");
            this.txtOriginalFilePath.Name = "txtOriginalFilePath";
            // 
            // btSelect
            // 
            resources.ApplyResources(this.btSelect, "btSelect");
            this.btSelect.Name = "btSelect";
            this.btSelect.UseVisualStyleBackColor = true;
            this.btSelect.Click += new System.EventHandler(this.btSelect_Click);
            // 
            // PackPAKOptions
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btSelect);
            this.Controls.Add(this.txtOriginalFilePath);
            this.Controls.Add(this.lbOriginalArc);
            this.Name = "PackPAKOptions";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbOriginalArc;
        private System.Windows.Forms.TextBox txtOriginalFilePath;
        private System.Windows.Forms.Button btSelect;
    }
}
