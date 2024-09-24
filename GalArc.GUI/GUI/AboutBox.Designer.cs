namespace GalArc.GUI
{
    partial class AboutBox
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
            this.ab_lbSearch = new System.Windows.Forms.Label();
            this.ab_lbLicense = new System.Windows.Forms.Label();
            this.ab_linkIssue = new System.Windows.Forms.LinkLabel();
            this.ab_linkSite = new System.Windows.Forms.LinkLabel();
            this.ab_lbCopyright = new System.Windows.Forms.Label();
            this.lbDescription = new System.Windows.Forms.Label();
            this.lbCurrentVer = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.pageEngines = new System.Windows.Forms.TabPage();
            this.pageLicense = new System.Windows.Forms.TabPage();
            this.txtLicense = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewEngines)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.pageEngines.SuspendLayout();
            this.pageLicense.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridViewEngines
            // 
            this.dataGridViewEngines.AllowUserToAddRows = false;
            this.dataGridViewEngines.AllowUserToDeleteRows = false;
            this.dataGridViewEngines.AllowUserToResizeColumns = false;
            this.dataGridViewEngines.AllowUserToResizeRows = false;
            this.dataGridViewEngines.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewEngines.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dataGridViewEngines.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dataGridViewEngines.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
            this.dataGridViewEngines.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewEngines.Location = new System.Drawing.Point(-1, 0);
            this.dataGridViewEngines.Name = "dataGridViewEngines";
            this.dataGridViewEngines.ReadOnly = true;
            this.dataGridViewEngines.RowHeadersVisible = false;
            this.dataGridViewEngines.RowHeadersWidth = 62;
            this.dataGridViewEngines.RowTemplate.Height = 30;
            this.dataGridViewEngines.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dataGridViewEngines.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dataGridViewEngines.Size = new System.Drawing.Size(463, 358);
            this.dataGridViewEngines.TabIndex = 0;
            // 
            // searchText
            // 
            this.searchText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.searchText.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.searchText.ImeMode = System.Windows.Forms.ImeMode.Hangul;
            this.searchText.Location = new System.Drawing.Point(78, 364);
            this.searchText.Name = "searchText";
            this.searchText.Size = new System.Drawing.Size(377, 31);
            this.searchText.TabIndex = 2;
            this.searchText.TextChanged += new System.EventHandler(this.searchText_TextChanged);
            // 
            // ab_lbSearch
            // 
            this.ab_lbSearch.AutoSize = true;
            this.ab_lbSearch.Location = new System.Drawing.Point(3, 366);
            this.ab_lbSearch.Name = "ab_lbSearch";
            this.ab_lbSearch.Size = new System.Drawing.Size(71, 24);
            this.ab_lbSearch.TabIndex = 3;
            this.ab_lbSearch.Text = "Search:";
            this.ab_lbSearch.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ab_lbSearch.SizeChanged += new System.EventHandler(this.ab_lbSearch_SizeChanged);
            // 
            // ab_lbLicense
            // 
            this.ab_lbLicense.AutoSize = true;
            this.ab_lbLicense.Location = new System.Drawing.Point(37, 129);
            this.ab_lbLicense.Name = "ab_lbLicense";
            this.ab_lbLicense.Size = new System.Drawing.Size(287, 24);
            this.ab_lbLicense.TabIndex = 5;
            this.ab_lbLicense.Text = "GNU General Public License v3.0";
            // 
            // ab_linkIssue
            // 
            this.ab_linkIssue.AutoSize = true;
            this.ab_linkIssue.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.ab_linkIssue.Location = new System.Drawing.Point(37, 279);
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
            this.ab_linkSite.Location = new System.Drawing.Point(37, 239);
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
            this.ab_lbCopyright.Location = new System.Drawing.Point(37, 169);
            this.ab_lbCopyright.Name = "ab_lbCopyright";
            this.ab_lbCopyright.Size = new System.Drawing.Size(280, 24);
            this.ab_lbCopyright.TabIndex = 3;
            this.ab_lbCopyright.Text = "Copyright (c) 2024 detached64";
            // 
            // lbDescription
            // 
            this.lbDescription.AutoSize = true;
            this.lbDescription.Location = new System.Drawing.Point(37, 49);
            this.lbDescription.Name = "lbDescription";
            this.lbDescription.Size = new System.Drawing.Size(198, 24);
            this.lbDescription.TabIndex = 0;
            this.lbDescription.Text = "Galgame Archive Tool";
            // 
            // lbCurrentVer
            // 
            this.lbCurrentVer.AutoSize = true;
            this.lbCurrentVer.Location = new System.Drawing.Point(37, 89);
            this.lbCurrentVer.Name = "lbCurrentVer";
            this.lbCurrentVer.Size = new System.Drawing.Size(0, 24);
            this.lbCurrentVer.TabIndex = 6;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.pageEngines);
            this.tabControl1.Controls.Add(this.pageLicense);
            this.tabControl1.Location = new System.Drawing.Point(366, 41);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(471, 440);
            this.tabControl1.TabIndex = 7;
            // 
            // pageEngines
            // 
            this.pageEngines.BackColor = System.Drawing.Color.White;
            this.pageEngines.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pageEngines.Controls.Add(this.dataGridViewEngines);
            this.pageEngines.Controls.Add(this.ab_lbSearch);
            this.pageEngines.Controls.Add(this.searchText);
            this.pageEngines.Location = new System.Drawing.Point(4, 33);
            this.pageEngines.Name = "pageEngines";
            this.pageEngines.Padding = new System.Windows.Forms.Padding(3);
            this.pageEngines.Size = new System.Drawing.Size(463, 403);
            this.pageEngines.TabIndex = 0;
            this.pageEngines.Text = "Supported Formats";
            // 
            // pageLicense
            // 
            this.pageLicense.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pageLicense.Controls.Add(this.txtLicense);
            this.pageLicense.Location = new System.Drawing.Point(4, 33);
            this.pageLicense.Name = "pageLicense";
            this.pageLicense.Padding = new System.Windows.Forms.Padding(3);
            this.pageLicense.Size = new System.Drawing.Size(463, 403);
            this.pageLicense.TabIndex = 1;
            this.pageLicense.Text = "License";
            this.pageLicense.UseVisualStyleBackColor = true;
            // 
            // txtLicense
            // 
            this.txtLicense.Location = new System.Drawing.Point(-1, -1);
            this.txtLicense.Multiline = true;
            this.txtLicense.Name = "txtLicense";
            this.txtLicense.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLicense.Size = new System.Drawing.Size(463, 403);
            this.txtLicense.TabIndex = 0;
            // 
            // AboutBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(875, 522);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.lbCurrentVer);
            this.Controls.Add(this.ab_lbLicense);
            this.Controls.Add(this.ab_linkIssue);
            this.Controls.Add(this.ab_linkSite);
            this.Controls.Add(this.ab_lbCopyright);
            this.Controls.Add(this.lbDescription);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutBox";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About";
            this.Load += new System.EventHandler(this.AboutBox_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewEngines)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.pageEngines.ResumeLayout(false);
            this.pageEngines.PerformLayout();
            this.pageLicense.ResumeLayout(false);
            this.pageLicense.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        internal System.Windows.Forms.DataGridView dataGridViewEngines;
        internal System.Windows.Forms.Label ab_lbSearch;
        internal System.Windows.Forms.Label ab_lbCopyright;
        internal System.Windows.Forms.Label lbDescription;
        internal System.Windows.Forms.LinkLabel ab_linkSite;
        internal System.Windows.Forms.Label ab_lbLicense;
        internal System.Windows.Forms.LinkLabel ab_linkIssue;
        internal System.Windows.Forms.TextBox searchText;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage pageEngines;
        private System.Windows.Forms.TabPage pageLicense;
        internal System.Windows.Forms.Label lbCurrentVer;
        internal System.Windows.Forms.TextBox txtLicense;
    }
}