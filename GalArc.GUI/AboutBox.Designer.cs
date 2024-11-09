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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutBox));
            this.dataGridViewEngines = new System.Windows.Forms.DataGridView();
            this.txtSearchText = new System.Windows.Forms.TextBox();
            this.lbSearch = new System.Windows.Forms.Label();
            this.lbLicense = new System.Windows.Forms.Label();
            this.linkIssue = new System.Windows.Forms.LinkLabel();
            this.linkSite = new System.Windows.Forms.LinkLabel();
            this.lbCopyright = new System.Windows.Forms.Label();
            this.lbDescription = new System.Windows.Forms.Label();
            this.lbCurrentVer = new System.Windows.Forms.Label();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.pageEngines = new System.Windows.Forms.TabPage();
            this.pageLicense = new System.Windows.Forms.TabPage();
            this.txtLicense = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewEngines)).BeginInit();
            this.tabControl.SuspendLayout();
            this.pageEngines.SuspendLayout();
            this.pageLicense.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridViewEngines
            // 
            this.dataGridViewEngines.AllowUserToAddRows = false;
            this.dataGridViewEngines.AllowUserToDeleteRows = false;
            this.dataGridViewEngines.AllowUserToResizeRows = false;
            this.dataGridViewEngines.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewEngines.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dataGridViewEngines.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
            this.dataGridViewEngines.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dataGridViewEngines.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            resources.ApplyResources(this.dataGridViewEngines, "dataGridViewEngines");
            this.dataGridViewEngines.Name = "dataGridViewEngines";
            this.dataGridViewEngines.ReadOnly = true;
            this.dataGridViewEngines.RowHeadersVisible = false;
            this.dataGridViewEngines.RowTemplate.Height = 30;
            this.dataGridViewEngines.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dataGridViewEngines.RowPrePaint += new System.Windows.Forms.DataGridViewRowPrePaintEventHandler(this.dataGridViewEngines_RowPrePaint);
            // 
            // txtSearchText
            // 
            resources.ApplyResources(this.txtSearchText, "txtSearchText");
            this.txtSearchText.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSearchText.Name = "txtSearchText";
            this.txtSearchText.TextChanged += new System.EventHandler(this.txtSearchText_TextChanged);
            // 
            // lbSearch
            // 
            resources.ApplyResources(this.lbSearch, "lbSearch");
            this.lbSearch.Name = "lbSearch";
            this.lbSearch.SizeChanged += new System.EventHandler(this.lbSearch_SizeChanged);
            // 
            // lbLicense
            // 
            resources.ApplyResources(this.lbLicense, "lbLicense");
            this.lbLicense.Name = "lbLicense";
            // 
            // linkIssue
            // 
            resources.ApplyResources(this.linkIssue, "linkIssue");
            this.linkIssue.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.linkIssue.Name = "linkIssue";
            this.linkIssue.TabStop = true;
            this.linkIssue.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkIssue_LinkClicked);
            // 
            // linkSite
            // 
            resources.ApplyResources(this.linkSite, "linkSite");
            this.linkSite.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.linkSite.Name = "linkSite";
            this.linkSite.TabStop = true;
            this.linkSite.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkSite_LinkClicked);
            // 
            // lbCopyright
            // 
            resources.ApplyResources(this.lbCopyright, "lbCopyright");
            this.lbCopyright.Name = "lbCopyright";
            // 
            // lbDescription
            // 
            resources.ApplyResources(this.lbDescription, "lbDescription");
            this.lbDescription.Name = "lbDescription";
            // 
            // lbCurrentVer
            // 
            resources.ApplyResources(this.lbCurrentVer, "lbCurrentVer");
            this.lbCurrentVer.Name = "lbCurrentVer";
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.pageEngines);
            this.tabControl.Controls.Add(this.pageLicense);
            resources.ApplyResources(this.tabControl, "tabControl");
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            // 
            // pageEngines
            // 
            this.pageEngines.BackColor = System.Drawing.SystemColors.Control;
            this.pageEngines.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pageEngines.Controls.Add(this.dataGridViewEngines);
            this.pageEngines.Controls.Add(this.lbSearch);
            this.pageEngines.Controls.Add(this.txtSearchText);
            resources.ApplyResources(this.pageEngines, "pageEngines");
            this.pageEngines.Name = "pageEngines";
            // 
            // pageLicense
            // 
            this.pageLicense.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pageLicense.Controls.Add(this.txtLicense);
            resources.ApplyResources(this.pageLicense, "pageLicense");
            this.pageLicense.Name = "pageLicense";
            this.pageLicense.UseVisualStyleBackColor = true;
            // 
            // txtLicense
            // 
            this.txtLicense.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.txtLicense, "txtLicense");
            this.txtLicense.Name = "txtLicense";
            this.txtLicense.ReadOnly = true;
            // 
            // AboutBox
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.lbCurrentVer);
            this.Controls.Add(this.lbLicense);
            this.Controls.Add(this.linkIssue);
            this.Controls.Add(this.linkSite);
            this.Controls.Add(this.lbCopyright);
            this.Controls.Add(this.lbDescription);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutBox";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Load += new System.EventHandler(this.AboutBox_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewEngines)).EndInit();
            this.tabControl.ResumeLayout(false);
            this.pageEngines.ResumeLayout(false);
            this.pageEngines.PerformLayout();
            this.pageLicense.ResumeLayout(false);
            this.pageLicense.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        internal System.Windows.Forms.DataGridView dataGridViewEngines;
        internal System.Windows.Forms.Label lbSearch;
        internal System.Windows.Forms.Label lbCopyright;
        internal System.Windows.Forms.Label lbDescription;
        internal System.Windows.Forms.LinkLabel linkSite;
        internal System.Windows.Forms.Label lbLicense;
        internal System.Windows.Forms.LinkLabel linkIssue;
        internal System.Windows.Forms.TextBox txtSearchText;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage pageEngines;
        private System.Windows.Forms.TabPage pageLicense;
        internal System.Windows.Forms.TextBox txtLicense;
        private System.Windows.Forms.Label lbCurrentVer;
    }
}