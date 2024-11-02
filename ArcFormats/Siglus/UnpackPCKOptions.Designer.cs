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
            this.panel = new System.Windows.Forms.Panel();
            this.lbFromGameExe = new System.Windows.Forms.Label();
            this.btCheckExe = new System.Windows.Forms.Button();
            this.panel.SuspendLayout();
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
            // panel
            // 
            this.panel.Controls.Add(this.btCheckExe);
            this.panel.Controls.Add(this.lbFromGameExe);
            resources.ApplyResources(this.panel, "panel");
            this.panel.Name = "panel";
            // 
            // lbFromGameExe
            // 
            resources.ApplyResources(this.lbFromGameExe, "lbFromGameExe");
            this.lbFromGameExe.Name = "lbFromGameExe";
            // 
            // btCheckExe
            // 
            resources.ApplyResources(this.btCheckExe, "btCheckExe");
            this.btCheckExe.Name = "btCheckExe";
            this.btCheckExe.UseVisualStyleBackColor = true;
            this.btCheckExe.Click += new System.EventHandler(this.btCheckExe_Click);
            // 
            // UnpackPCKOptions
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel);
            this.Controls.Add(this.lbKey);
            this.Controls.Add(this.combSchemes);
            this.Controls.Add(this.lbChoose);
            this.Name = "UnpackPCKOptions";
            this.Load += new System.EventHandler(this.UnpackPCKOptions_Load);
            this.panel.ResumeLayout(false);
            this.panel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbChoose;
        private System.Windows.Forms.ComboBox combSchemes;
        private System.Windows.Forms.Label lbKey;
        private System.Windows.Forms.Panel panel;
        private System.Windows.Forms.Label lbFromGameExe;
        private System.Windows.Forms.Button btCheckExe;
    }
}
