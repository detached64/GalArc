namespace GalArc.GUI
{
    partial class ExtensionsSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExtensionsSettings));
            this.chkbxEnableExtensions = new System.Windows.Forms.CheckBox();
            this.lbExtensionsInfo = new System.Windows.Forms.Label();
            this.dataGridViewInfos = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewInfos)).BeginInit();
            this.SuspendLayout();
            // 
            // chkbxEnableExtensions
            // 
            resources.ApplyResources(this.chkbxEnableExtensions, "chkbxEnableExtensions");
            this.chkbxEnableExtensions.Checked = true;
            this.chkbxEnableExtensions.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkbxEnableExtensions.Name = "chkbxEnableExtensions";
            this.chkbxEnableExtensions.UseVisualStyleBackColor = true;
            this.chkbxEnableExtensions.CheckedChanged += new System.EventHandler(this.chkbxEnableExtensions_CheckedChanged);
            // 
            // lbExtensionsInfo
            // 
            resources.ApplyResources(this.lbExtensionsInfo, "lbExtensionsInfo");
            this.lbExtensionsInfo.Name = "lbExtensionsInfo";
            // 
            // dataGridViewInfos
            // 
            this.dataGridViewInfos.AllowUserToAddRows = false;
            this.dataGridViewInfos.AllowUserToDeleteRows = false;
            resources.ApplyResources(this.dataGridViewInfos, "dataGridViewInfos");
            this.dataGridViewInfos.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewInfos.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dataGridViewInfos.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
            this.dataGridViewInfos.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dataGridViewInfos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewInfos.Name = "dataGridViewInfos";
            this.dataGridViewInfos.ReadOnly = true;
            this.dataGridViewInfos.RowHeadersVisible = false;
            this.dataGridViewInfos.RowTemplate.Height = 30;
            this.dataGridViewInfos.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dataGridViewInfos.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewInfos_CellContentClick);
            // 
            // ExtensionsSettings
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dataGridViewInfos);
            this.Controls.Add(this.lbExtensionsInfo);
            this.Controls.Add(this.chkbxEnableExtensions);
            this.Name = "ExtensionsSettings";
            this.Load += new System.EventHandler(this.ExtensionsSettings_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewInfos)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkbxEnableExtensions;
        private System.Windows.Forms.Label lbExtensionsInfo;
        private System.Windows.Forms.DataGridView dataGridViewInfos;
    }
}
