namespace GalArc
{
    partial class main
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

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.pages = new System.Windows.Forms.TabControl();
            this.unpackPage = new System.Windows.Forms.TabPage();
            this.packPage = new System.Windows.Forms.TabPage();
            this.optionPage = new System.Windows.Forms.TabPage();
            this.aboutPage = new System.Windows.Forms.TabPage();
            this.main_statusLabel = new System.Windows.Forms.Label();
            this.pages.SuspendLayout();
            this.SuspendLayout();
            // 
            // pages
            // 
            this.pages.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.pages.Controls.Add(this.unpackPage);
            this.pages.Controls.Add(this.packPage);
            this.pages.Controls.Add(this.optionPage);
            this.pages.Controls.Add(this.aboutPage);
            this.pages.Location = new System.Drawing.Point(12, 12);
            this.pages.Name = "pages";
            this.pages.SelectedIndex = 0;
            this.pages.Size = new System.Drawing.Size(1105, 676);
            this.pages.TabIndex = 0;
            this.pages.SelectedIndexChanged += new System.EventHandler(this.pages_SelectedIndexChanged);
            // 
            // unpackPage
            // 
            this.unpackPage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.unpackPage.Location = new System.Drawing.Point(4, 36);
            this.unpackPage.Name = "unpackPage";
            this.unpackPage.Padding = new System.Windows.Forms.Padding(3);
            this.unpackPage.Size = new System.Drawing.Size(1097, 636);
            this.unpackPage.TabIndex = 0;
            this.unpackPage.Text = "Unpack";
            this.unpackPage.UseVisualStyleBackColor = true;
            // 
            // packPage
            // 
            this.packPage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.packPage.Location = new System.Drawing.Point(4, 36);
            this.packPage.Name = "packPage";
            this.packPage.Padding = new System.Windows.Forms.Padding(3);
            this.packPage.Size = new System.Drawing.Size(1097, 636);
            this.packPage.TabIndex = 1;
            this.packPage.Text = "Pack";
            this.packPage.UseVisualStyleBackColor = true;
            // 
            // optionPage
            // 
            this.optionPage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.optionPage.Location = new System.Drawing.Point(4, 36);
            this.optionPage.Name = "optionPage";
            this.optionPage.Padding = new System.Windows.Forms.Padding(3);
            this.optionPage.Size = new System.Drawing.Size(1097, 636);
            this.optionPage.TabIndex = 2;
            this.optionPage.Text = "Option";
            this.optionPage.UseVisualStyleBackColor = true;
            // 
            // aboutPage
            // 
            this.aboutPage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.aboutPage.Location = new System.Drawing.Point(4, 36);
            this.aboutPage.Name = "aboutPage";
            this.aboutPage.Padding = new System.Windows.Forms.Padding(3);
            this.aboutPage.Size = new System.Drawing.Size(1097, 636);
            this.aboutPage.TabIndex = 3;
            this.aboutPage.Text = "About";
            this.aboutPage.UseVisualStyleBackColor = true;
            // 
            // main_statusLabel
            // 
            this.main_statusLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.main_statusLabel.Location = new System.Drawing.Point(16, 691);
            this.main_statusLabel.Name = "main_statusLabel";
            this.main_statusLabel.Size = new System.Drawing.Size(1097, 27);
            this.main_statusLabel.TabIndex = 2;
            this.main_statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1129, 732);
            this.Controls.Add(this.main_statusLabel);
            this.Controls.Add(this.pages);
            this.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "GalArc";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.main_FormClosing);
            this.Load += new System.EventHandler(this.main_Load);
            this.LocationChanged += new System.EventHandler(this.main_LocationChanged);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.main_KeyDown);
            this.pages.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        public System.Windows.Forms.TabPage unpackPage;
        public System.Windows.Forms.TabPage packPage;
        public System.Windows.Forms.TabPage optionPage;
        public System.Windows.Forms.TabPage aboutPage;
        public System.Windows.Forms.TabControl pages;
        internal System.Windows.Forms.Label main_statusLabel;
    }
}

