namespace ArcFormats.Pkware
{
    partial class UnpackPkwareWidget
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UnpackPkwareWidget));
            this.combSchemes = new System.Windows.Forms.ComboBox();
            this.lbChoose = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // combSchemes
            // 
            this.combSchemes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.combSchemes.FormattingEnabled = true;
            resources.ApplyResources(this.combSchemes, "combSchemes");
            this.combSchemes.Name = "combSchemes";
            this.combSchemes.SelectedIndexChanged += new System.EventHandler(this.combSchemes_SelectedIndexChanged);
            // 
            // lbChoose
            // 
            resources.ApplyResources(this.lbChoose, "lbChoose");
            this.lbChoose.Name = "lbChoose";
            // 
            // UnpackPkwareOptions
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.combSchemes);
            this.Controls.Add(this.lbChoose);
            this.Name = "UnpackPkwareOptions";
            this.Load += new System.EventHandler(this.UnpackPkwareOptions_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ComboBox combSchemes;
        private System.Windows.Forms.Label lbChoose;
    }
}
