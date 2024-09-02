namespace GalArc.GUI
{
    partial class AboutWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dataGridViewEngines = new System.Windows.Forms.DataGridView();
            this.searchText = new System.Windows.Forms.TextBox();
            this.ab_gbFormat = new System.Windows.Forms.GroupBox();
            this.ab_lbSearch = new System.Windows.Forms.Label();
            this.ab_gbAbout = new System.Windows.Forms.GroupBox();
            this.ab_lbLicense = new System.Windows.Forms.Label();
            this.ab_linkIssue = new System.Windows.Forms.LinkLabel();
            this.ab_linkSite = new System.Windows.Forms.LinkLabel();
            this.ab_lbCopyright = new System.Windows.Forms.Label();
            this.ab_lbDescription = new System.Windows.Forms.Label();
            this.ab_gbUpdate = new System.Windows.Forms.GroupBox();
            this.ab_lbLatestVer = new System.Windows.Forms.Label();
            this.ab_lbCurrentVer = new System.Windows.Forms.Label();
            this.ab_lbLatestVersion = new System.Windows.Forms.Label();
            this.ab_lbCurrentVersion = new System.Windows.Forms.Label();
            this.ab_btnDownload = new System.Windows.Forms.Button();
            this.ab_btnCheckUpdate = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewEngines)).BeginInit();
            this.ab_gbFormat.SuspendLayout();
            this.ab_gbAbout.SuspendLayout();
            this.ab_gbUpdate.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridViewEngines
            // 
            this.dataGridViewEngines.AllowUserToAddRows = false;
            this.dataGridViewEngines.AllowUserToDeleteRows = false;
            this.dataGridViewEngines.AllowUserToResizeColumns = false;
            this.dataGridViewEngines.AllowUserToResizeRows = false;
            this.dataGridViewEngines.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dataGridViewEngines.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dataGridViewEngines.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewEngines.Location = new System.Drawing.Point(24, 46);
            this.dataGridViewEngines.Name = "dataGridViewEngines";
            this.dataGridViewEngines.ReadOnly = true;
            this.dataGridViewEngines.RowHeadersVisible = false;
            this.dataGridViewEngines.RowHeadersWidth = 62;
            this.dataGridViewEngines.RowTemplate.Height = 30;
            this.dataGridViewEngines.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dataGridViewEngines.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dataGridViewEngines.Size = new System.Drawing.Size(472, 422);
            this.dataGridViewEngines.TabIndex = 0;
            // 
            // searchText
            // 
            this.searchText.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.searchText.ImeMode = System.Windows.Forms.ImeMode.Hangul;
            this.searchText.Location = new System.Drawing.Point(97, 483);
            this.searchText.Name = "searchText";
            this.searchText.Size = new System.Drawing.Size(215, 31);
            this.searchText.TabIndex = 2;
            this.searchText.TextChanged += new System.EventHandler(this.searchText_TextChanged);
            // 
            // ab_gbFormat
            // 
            this.ab_gbFormat.Controls.Add(this.ab_lbSearch);
            this.ab_gbFormat.Controls.Add(this.dataGridViewEngines);
            this.ab_gbFormat.Controls.Add(this.searchText);
            this.ab_gbFormat.Location = new System.Drawing.Point(548, 38);
            this.ab_gbFormat.Name = "ab_gbFormat";
            this.ab_gbFormat.Size = new System.Drawing.Size(527, 540);
            this.ab_gbFormat.TabIndex = 4;
            this.ab_gbFormat.TabStop = false;
            this.ab_gbFormat.Text = "Supported Format";
            // 
            // ab_lbSearch
            // 
            this.ab_lbSearch.AutoSize = true;
            this.ab_lbSearch.Location = new System.Drawing.Point(20, 485);
            this.ab_lbSearch.Name = "ab_lbSearch";
            this.ab_lbSearch.Size = new System.Drawing.Size(71, 24);
            this.ab_lbSearch.TabIndex = 3;
            this.ab_lbSearch.Text = "Search:";
            this.ab_lbSearch.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ab_lbSearch.SizeChanged += new System.EventHandler(this.ab_lbSearch_SizeChanged);
            // 
            // ab_gbAbout
            // 
            this.ab_gbAbout.Controls.Add(this.ab_lbLicense);
            this.ab_gbAbout.Controls.Add(this.ab_linkIssue);
            this.ab_gbAbout.Controls.Add(this.ab_linkSite);
            this.ab_gbAbout.Controls.Add(this.ab_lbCopyright);
            this.ab_gbAbout.Controls.Add(this.ab_lbDescription);
            this.ab_gbAbout.Location = new System.Drawing.Point(22, 38);
            this.ab_gbAbout.Name = "ab_gbAbout";
            this.ab_gbAbout.Size = new System.Drawing.Size(520, 365);
            this.ab_gbAbout.TabIndex = 5;
            this.ab_gbAbout.TabStop = false;
            this.ab_gbAbout.Text = "About";
            // 
            // ab_lbLicense
            // 
            this.ab_lbLicense.AutoSize = true;
            this.ab_lbLicense.Location = new System.Drawing.Point(42, 123);
            this.ab_lbLicense.Name = "ab_lbLicense";
            this.ab_lbLicense.Size = new System.Drawing.Size(287, 24);
            this.ab_lbLicense.TabIndex = 5;
            this.ab_lbLicense.Text = "GNU General Public License v3.0";
            // 
            // ab_linkIssue
            // 
            this.ab_linkIssue.AutoSize = true;
            this.ab_linkIssue.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.ab_linkIssue.Location = new System.Drawing.Point(42, 276);
            this.ab_linkIssue.Name = "ab_linkIssue";
            this.ab_linkIssue.Size = new System.Drawing.Size(116, 24);
            this.ab_linkIssue.TabIndex = 4;
            this.ab_linkIssue.TabStop = true;
            this.ab_linkIssue.Text = "Report Issue";
            this.ab_linkIssue.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ab_linkIssue_LinkClicked);
            // 
            // ab_linkSite
            // 
            this.ab_linkSite.AutoSize = true;
            this.ab_linkSite.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.ab_linkSite.Location = new System.Drawing.Point(42, 232);
            this.ab_linkSite.Name = "ab_linkSite";
            this.ab_linkSite.Size = new System.Drawing.Size(163, 24);
            this.ab_linkSite.TabIndex = 4;
            this.ab_linkSite.TabStop = true;
            this.ab_linkSite.Text = "Development Site";
            this.ab_linkSite.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ab_linkSite_LinkClicked);
            // 
            // ab_lbCopyright
            // 
            this.ab_lbCopyright.AutoSize = true;
            this.ab_lbCopyright.Location = new System.Drawing.Point(42, 167);
            this.ab_lbCopyright.Name = "ab_lbCopyright";
            this.ab_lbCopyright.Size = new System.Drawing.Size(280, 24);
            this.ab_lbCopyright.TabIndex = 3;
            this.ab_lbCopyright.Text = "Copyright (c) 2024 detached64";
            // 
            // ab_lbDescription
            // 
            this.ab_lbDescription.AutoSize = true;
            this.ab_lbDescription.Location = new System.Drawing.Point(42, 62);
            this.ab_lbDescription.Name = "ab_lbDescription";
            this.ab_lbDescription.Size = new System.Drawing.Size(198, 24);
            this.ab_lbDescription.TabIndex = 0;
            this.ab_lbDescription.Text = "Galgame Archive Tool";
            // 
            // ab_gbUpdate
            // 
            this.ab_gbUpdate.Controls.Add(this.ab_lbLatestVer);
            this.ab_gbUpdate.Controls.Add(this.ab_lbCurrentVer);
            this.ab_gbUpdate.Controls.Add(this.ab_lbLatestVersion);
            this.ab_gbUpdate.Controls.Add(this.ab_lbCurrentVersion);
            this.ab_gbUpdate.Controls.Add(this.ab_btnDownload);
            this.ab_gbUpdate.Controls.Add(this.ab_btnCheckUpdate);
            this.ab_gbUpdate.Location = new System.Drawing.Point(22, 409);
            this.ab_gbUpdate.Name = "ab_gbUpdate";
            this.ab_gbUpdate.Size = new System.Drawing.Size(520, 169);
            this.ab_gbUpdate.TabIndex = 6;
            this.ab_gbUpdate.TabStop = false;
            this.ab_gbUpdate.Text = "Update";
            // 
            // ab_lbLatestVer
            // 
            this.ab_lbLatestVer.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.ab_lbLatestVer.AutoSize = true;
            this.ab_lbLatestVer.Location = new System.Drawing.Point(196, 109);
            this.ab_lbLatestVer.Name = "ab_lbLatestVer";
            this.ab_lbLatestVer.Size = new System.Drawing.Size(91, 24);
            this.ab_lbLatestVer.TabIndex = 3;
            this.ab_lbLatestVer.Text = "Unknown";
            // 
            // ab_lbCurrentVer
            // 
            this.ab_lbCurrentVer.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.ab_lbCurrentVer.AutoSize = true;
            this.ab_lbCurrentVer.Location = new System.Drawing.Point(196, 47);
            this.ab_lbCurrentVer.Name = "ab_lbCurrentVer";
            this.ab_lbCurrentVer.Size = new System.Drawing.Size(51, 24);
            this.ab_lbCurrentVer.TabIndex = 2;
            this.ab_lbCurrentVer.Text = "1.0.0";
            // 
            // ab_lbLatestVersion
            // 
            this.ab_lbLatestVersion.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.ab_lbLatestVersion.AutoSize = true;
            this.ab_lbLatestVersion.Location = new System.Drawing.Point(42, 109);
            this.ab_lbLatestVersion.Name = "ab_lbLatestVersion";
            this.ab_lbLatestVersion.Size = new System.Drawing.Size(134, 24);
            this.ab_lbLatestVersion.TabIndex = 1;
            this.ab_lbLatestVersion.Text = "Latest Version:";
            this.ab_lbLatestVersion.SizeChanged += new System.EventHandler(this.ab_lbLatestVersion_SizeChanged);
            // 
            // ab_lbCurrentVersion
            // 
            this.ab_lbCurrentVersion.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.ab_lbCurrentVersion.AutoSize = true;
            this.ab_lbCurrentVersion.Location = new System.Drawing.Point(42, 47);
            this.ab_lbCurrentVersion.Name = "ab_lbCurrentVersion";
            this.ab_lbCurrentVersion.Size = new System.Drawing.Size(148, 24);
            this.ab_lbCurrentVersion.TabIndex = 1;
            this.ab_lbCurrentVersion.Text = "Current Version:";
            this.ab_lbCurrentVersion.SizeChanged += new System.EventHandler(this.ab_lbCurrentVersion_SizeChanged);
            // 
            // ab_btnDownload
            // 
            this.ab_btnDownload.Enabled = false;
            this.ab_btnDownload.Location = new System.Drawing.Point(341, 104);
            this.ab_btnDownload.Name = "ab_btnDownload";
            this.ab_btnDownload.Size = new System.Drawing.Size(112, 35);
            this.ab_btnDownload.TabIndex = 0;
            this.ab_btnDownload.Text = "Download";
            this.ab_btnDownload.UseVisualStyleBackColor = true;
            this.ab_btnDownload.Click += new System.EventHandler(this.ab_btnDownload_Click);
            // 
            // ab_btnCheckUpdate
            // 
            this.ab_btnCheckUpdate.Location = new System.Drawing.Point(341, 41);
            this.ab_btnCheckUpdate.Name = "ab_btnCheckUpdate";
            this.ab_btnCheckUpdate.Size = new System.Drawing.Size(112, 35);
            this.ab_btnCheckUpdate.TabIndex = 0;
            this.ab_btnCheckUpdate.Text = "Check";
            this.ab_btnCheckUpdate.UseVisualStyleBackColor = true;
            this.ab_btnCheckUpdate.Click += new System.EventHandler(this.ab_btnCheckUpdate_Click);
            // 
            // AboutWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ab_gbUpdate);
            this.Controls.Add(this.ab_gbAbout);
            this.Controls.Add(this.ab_gbFormat);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "AboutWindow";
            this.Size = new System.Drawing.Size(1097, 639);
            this.Load += new System.EventHandler(this.AboutWindow_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewEngines)).EndInit();
            this.ab_gbFormat.ResumeLayout(false);
            this.ab_gbFormat.PerformLayout();
            this.ab_gbAbout.ResumeLayout(false);
            this.ab_gbAbout.PerformLayout();
            this.ab_gbUpdate.ResumeLayout(false);
            this.ab_gbUpdate.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        internal System.Windows.Forms.DataGridView dataGridViewEngines;
        internal System.Windows.Forms.Label ab_lbSearch;
        internal System.Windows.Forms.Label ab_lbCopyright;
        internal System.Windows.Forms.Label ab_lbDescription;
        internal System.Windows.Forms.LinkLabel ab_linkSite;
        internal System.Windows.Forms.Label ab_lbLicense;
        internal System.Windows.Forms.LinkLabel ab_linkIssue;
        internal System.Windows.Forms.GroupBox ab_gbUpdate;
        internal System.Windows.Forms.Button ab_btnCheckUpdate;
        internal System.Windows.Forms.Label ab_lbLatestVer;
        internal System.Windows.Forms.Label ab_lbCurrentVer;
        internal System.Windows.Forms.Label ab_lbLatestVersion;
        internal System.Windows.Forms.Label ab_lbCurrentVersion;
        internal System.Windows.Forms.Button ab_btnDownload;
        internal System.Windows.Forms.GroupBox ab_gbFormat;
        internal System.Windows.Forms.GroupBox ab_gbAbout;
        internal System.Windows.Forms.TextBox searchText;
    }
}