namespace ArcFormats.Qlie
{
    partial class UnpackPACKOptions
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UnpackPACKOptions));
            this.combSchemes = new System.Windows.Forms.ComboBox();
            this.lbChoose = new System.Windows.Forms.Label();
            this.chkbxSaveHash = new System.Windows.Forms.CheckBox();
            this.lbKeyPath = new System.Windows.Forms.Label();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.chkbxSaveKey = new System.Windows.Forms.CheckBox();
            this.btSelect = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // combSchemes
            // 
            this.combSchemes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.combSchemes.FormattingEnabled = true;
            this.combSchemes.Items.AddRange(new object[] {
            resources.GetString("combSchemes.Items")});
            resources.ApplyResources(this.combSchemes, "combSchemes");
            this.combSchemes.Name = "combSchemes";
            this.combSchemes.SelectedIndexChanged += new System.EventHandler(this.combSchemes_SelectedIndexChanged);
            // 
            // lbChoose
            // 
            resources.ApplyResources(this.lbChoose, "lbChoose");
            this.lbChoose.Name = "lbChoose";
            // 
            // chkbxSaveHash
            // 
            resources.ApplyResources(this.chkbxSaveHash, "chkbxSaveHash");
            this.chkbxSaveHash.Name = "chkbxSaveHash";
            this.chkbxSaveHash.UseVisualStyleBackColor = true;
            this.chkbxSaveHash.CheckedChanged += new System.EventHandler(this.chkbxSaveHash_CheckedChanged);
            // 
            // lbKeyPath
            // 
            resources.ApplyResources(this.lbKeyPath, "lbKeyPath");
            this.lbKeyPath.Name = "lbKeyPath";
            // 
            // txtPath
            // 
            this.txtPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.txtPath, "txtPath");
            this.txtPath.Name = "txtPath";
            this.txtPath.TextChanged += new System.EventHandler(this.txtPath_TextChanged);
            // 
            // chkbxSaveKey
            // 
            resources.ApplyResources(this.chkbxSaveKey, "chkbxSaveKey");
            this.chkbxSaveKey.Name = "chkbxSaveKey";
            this.chkbxSaveKey.UseVisualStyleBackColor = true;
            this.chkbxSaveKey.CheckedChanged += new System.EventHandler(this.lbSaveKey_CheckedChanged);
            // 
            // btSelect
            // 
            resources.ApplyResources(this.btSelect, "btSelect");
            this.btSelect.Name = "btSelect";
            this.btSelect.UseVisualStyleBackColor = true;
            this.btSelect.Click += new System.EventHandler(this.btSelect_Click);
            // 
            // UnpackPACKOptions
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btSelect);
            this.Controls.Add(this.txtPath);
            this.Controls.Add(this.lbKeyPath);
            this.Controls.Add(this.chkbxSaveKey);
            this.Controls.Add(this.chkbxSaveHash);
            this.Controls.Add(this.combSchemes);
            this.Controls.Add(this.lbChoose);
            this.Name = "UnpackPACKOptions";
            this.Load += new System.EventHandler(this.UnpackPACKOptions_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox combSchemes;
        private System.Windows.Forms.Label lbChoose;
        private System.Windows.Forms.CheckBox chkbxSaveHash;
        private System.Windows.Forms.Label lbKeyPath;
        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.CheckBox chkbxSaveKey;
        private System.Windows.Forms.Button btSelect;
    }
}
