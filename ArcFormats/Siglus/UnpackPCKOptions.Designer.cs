namespace ArcFormats.Siglus
{
    partial class UnpackPCKOptions
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UnpackPCKOptions));
            this.lbChoose = new System.Windows.Forms.Label();
            this.combSchemes = new System.Windows.Forms.ComboBox();
            this.lbKey = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lbChoose
            // 
            resources.ApplyResources(this.lbChoose, "lbChoose");
            this.lbChoose.Name = "lbChoose";
            // 
            // combSchemes
            // 
            this.combSchemes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.combSchemes.FormattingEnabled = true;
            resources.ApplyResources(this.combSchemes, "combSchemes");
            this.combSchemes.Name = "combSchemes";
            this.combSchemes.SelectedIndexChanged += new System.EventHandler(this.combSchemes_SelectedIndexChanged);
            // 
            // lbKey
            // 
            resources.ApplyResources(this.lbKey, "lbKey");
            this.lbKey.Name = "lbKey";
            // 
            // UnpackPCKOptions
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lbKey);
            this.Controls.Add(this.combSchemes);
            this.Controls.Add(this.lbChoose);
            this.Name = "UnpackPCKOptions";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbChoose;
        private System.Windows.Forms.ComboBox combSchemes;
        private System.Windows.Forms.Label lbKey;
    }
}
