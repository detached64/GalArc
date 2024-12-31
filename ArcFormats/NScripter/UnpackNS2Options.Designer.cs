namespace ArcFormats.NScripter
{
    partial class UnpackNS2Options
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UnpackNS2Options));
            this.combSchemes = new System.Windows.Forms.ComboBox();
            this.lbChooseOrInput = new System.Windows.Forms.Label();
            this.txtKey = new System.Windows.Forms.TextBox();
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
            // lbChooseOrInput
            // 
            resources.ApplyResources(this.lbChooseOrInput, "lbChooseOrInput");
            this.lbChooseOrInput.Name = "lbChooseOrInput";
            // 
            // txtKey
            // 
            this.txtKey.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.txtKey, "txtKey");
            this.txtKey.Name = "txtKey";
            this.txtKey.TextChanged += new System.EventHandler(this.txtKey_TextChanged);
            // 
            // UnpackNS2Options
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtKey);
            this.Controls.Add(this.combSchemes);
            this.Controls.Add(this.lbChooseOrInput);
            this.Name = "UnpackNS2Options";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ComboBox combSchemes;
        private System.Windows.Forms.Label lbChooseOrInput;
        private System.Windows.Forms.TextBox txtKey;
    }
}
