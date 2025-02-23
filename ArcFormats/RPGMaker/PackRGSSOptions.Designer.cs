namespace ArcFormats.RPGMaker
{
    partial class PackRGSSOptions
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PackRGSSOptions));
            this.combVersion = new System.Windows.Forms.ComboBox();
            this.lbVersion = new System.Windows.Forms.Label();
            this.txtSeed = new System.Windows.Forms.TextBox();
            this.lbSeed = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // combVersion
            // 
            this.combVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.combVersion.FormattingEnabled = true;
            resources.ApplyResources(this.combVersion, "combVersion");
            this.combVersion.Name = "combVersion";
            this.combVersion.SelectedIndexChanged += new System.EventHandler(this.combVersion_SelectedIndexChanged);
            // 
            // lbVersion
            // 
            resources.ApplyResources(this.lbVersion, "lbVersion");
            this.lbVersion.Name = "lbVersion";
            // 
            // txtSeed
            // 
            this.txtSeed.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.txtSeed, "txtSeed");
            this.txtSeed.Name = "txtSeed";
            this.txtSeed.TextChanged += new System.EventHandler(this.txtSeed_TextChanged);
            // 
            // lbSeed
            // 
            resources.ApplyResources(this.lbSeed, "lbSeed");
            this.lbSeed.Name = "lbSeed";
            this.lbSeed.SizeChanged += new System.EventHandler(this.lbSeed_SizeChanged);
            // 
            // PackRGSSOptions
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lbSeed);
            this.Controls.Add(this.txtSeed);
            this.Controls.Add(this.combVersion);
            this.Controls.Add(this.lbVersion);
            this.Name = "PackRGSSOptions";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox combVersion;
        private System.Windows.Forms.Label lbVersion;
        private System.Windows.Forms.TextBox txtSeed;
        private System.Windows.Forms.Label lbSeed;
    }
}
