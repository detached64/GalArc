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
            this.tableFormats = new System.Windows.Forms.TableLayoutPanel();
            this.tableInfos = new System.Windows.Forms.TableLayoutPanel();
            this.table = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewEngines)).BeginInit();
            this.tabControl.SuspendLayout();
            this.pageEngines.SuspendLayout();
            this.pageLicense.SuspendLayout();
            this.tableFormats.SuspendLayout();
            this.tableInfos.SuspendLayout();
            this.table.SuspendLayout();
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
            this.txtSearchText.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.txtSearchText, "txtSearchText");
            this.txtSearchText.Name = "txtSearchText";
            this.txtSearchText.TextChanged += new System.EventHandler(this.txtSearchText_TextChanged);
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
            this.pageEngines.Controls.Add(this.tableFormats);
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
            // tableFormats
            // 
            resources.ApplyResources(this.tableFormats, "tableFormats");
            this.tableFormats.Controls.Add(this.txtSearchText, 0, 1);
            this.tableFormats.Controls.Add(this.dataGridViewEngines, 0, 0);
            this.tableFormats.Name = "tableFormats";
            // 
            // tableInfos
            // 
            resources.ApplyResources(this.tableInfos, "tableInfos");
            this.tableInfos.Controls.Add(this.lbDescription, 0, 0);
            this.tableInfos.Controls.Add(this.lbCurrentVer, 0, 1);
            this.tableInfos.Controls.Add(this.linkIssue, 0, 5);
            this.tableInfos.Controls.Add(this.lbLicense, 0, 2);
            this.tableInfos.Controls.Add(this.linkSite, 0, 4);
            this.tableInfos.Controls.Add(this.lbCopyright, 0, 3);
            this.tableInfos.Name = "tableInfos";
            // 
            // table
            // 
            resources.ApplyResources(this.table, "table");
            this.table.Controls.Add(this.tabControl, 1, 0);
            this.table.Controls.Add(this.tableInfos, 0, 0);
            this.table.Name = "table";
            // 
            // AboutBox
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.table);
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
            this.pageLicense.ResumeLayout(false);
            this.pageLicense.PerformLayout();
            this.tableFormats.ResumeLayout(false);
            this.tableFormats.PerformLayout();
            this.tableInfos.ResumeLayout(false);
            this.tableInfos.PerformLayout();
            this.table.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        internal System.Windows.Forms.DataGridView dataGridViewEngines;
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
        private System.Windows.Forms.TableLayoutPanel tableFormats;
        private System.Windows.Forms.TableLayoutPanel tableInfos;
        private System.Windows.Forms.TableLayoutPanel table;
    }
}