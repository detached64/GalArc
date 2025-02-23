namespace GalArc.GUI
{
    partial class PreferenceSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PreferenceSettings));
            this.lbDefaultEncoding = new System.Windows.Forms.Label();
            this.combEncoding = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // lbDefaultEncoding
            // 
            resources.ApplyResources(this.lbDefaultEncoding, "lbDefaultEncoding");
            this.lbDefaultEncoding.Name = "lbDefaultEncoding";
            // 
            // combEncoding
            // 
            this.combEncoding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.combEncoding, "combEncoding");
            this.combEncoding.FormattingEnabled = true;
            this.combEncoding.Name = "combEncoding";
            this.combEncoding.SelectedIndexChanged += new System.EventHandler(this.combEncoding_SelectedIndexChanged);
            // 
            // PreferenceSettings
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.combEncoding);
            this.Controls.Add(this.lbDefaultEncoding);
            this.Name = "PreferenceSettings";
            this.Load += new System.EventHandler(this.PreferenceSettings_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbDefaultEncoding;
        internal System.Windows.Forms.ComboBox combEncoding;
    }
}
