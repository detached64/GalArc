namespace ArcFormats.Kirikiri
{
    partial class PackXP3Options
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PackXP3Options));
            this.lbVersion = new System.Windows.Forms.Label();
            this.combVersion = new System.Windows.Forms.ComboBox();
            this.chkbxComIndex = new System.Windows.Forms.CheckBox();
            this.chkbxComContents = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // lbVersion
            // 
            resources.ApplyResources(this.lbVersion, "lbVersion");
            this.lbVersion.Name = "lbVersion";
            // 
            // combVersion
            // 
            this.combVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.combVersion.FormattingEnabled = true;
            resources.ApplyResources(this.combVersion, "combVersion");
            this.combVersion.Name = "combVersion";
            this.combVersion.SelectedIndexChanged += new System.EventHandler(this.combVersion_SelectedIndexChanged);
            // 
            // chkbxComIndex
            // 
            resources.ApplyResources(this.chkbxComIndex, "chkbxComIndex");
            this.chkbxComIndex.Checked = true;
            this.chkbxComIndex.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkbxComIndex.Name = "chkbxComIndex";
            this.chkbxComIndex.UseVisualStyleBackColor = true;
            this.chkbxComIndex.CheckedChanged += new System.EventHandler(this.chkbxComIndex_CheckedChanged);
            // 
            // chkbxComContents
            // 
            resources.ApplyResources(this.chkbxComContents, "chkbxComContents");
            this.chkbxComContents.Checked = true;
            this.chkbxComContents.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkbxComContents.Name = "chkbxComContents";
            this.chkbxComContents.UseVisualStyleBackColor = true;
            this.chkbxComContents.CheckedChanged += new System.EventHandler(this.chkbxComContents_CheckedChanged);
            // 
            // PackXP3Options
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chkbxComContents);
            this.Controls.Add(this.chkbxComIndex);
            this.Controls.Add(this.combVersion);
            this.Controls.Add(this.lbVersion);
            this.Name = "PackXP3Options";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.Label lbVersion;
        internal System.Windows.Forms.ComboBox combVersion;
        internal System.Windows.Forms.CheckBox chkbxComIndex;
        internal System.Windows.Forms.CheckBox chkbxComContents;
    }
}
