namespace GalArc.GUI
{
    partial class UpdateBox
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpdateBox));
            this.lbNewUpdate = new System.Windows.Forms.Label();
            this.lbCurrentVersion = new System.Windows.Forms.Label();
            this.lbLatestVersion = new System.Windows.Forms.Label();
            this.btDown = new System.Windows.Forms.Button();
            this.table = new System.Windows.Forms.TableLayoutPanel();
            this.table.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbNewUpdate
            // 
            resources.ApplyResources(this.lbNewUpdate, "lbNewUpdate");
            this.lbNewUpdate.Name = "lbNewUpdate";
            // 
            // lbCurrentVersion
            // 
            resources.ApplyResources(this.lbCurrentVersion, "lbCurrentVersion");
            this.lbCurrentVersion.Name = "lbCurrentVersion";
            // 
            // lbLatestVersion
            // 
            resources.ApplyResources(this.lbLatestVersion, "lbLatestVersion");
            this.lbLatestVersion.Name = "lbLatestVersion";
            // 
            // btDown
            // 
            resources.ApplyResources(this.btDown, "btDown");
            this.btDown.Name = "btDown";
            this.btDown.UseVisualStyleBackColor = true;
            this.btDown.Click += new System.EventHandler(this.btDown_Click);
            // 
            // table
            // 
            resources.ApplyResources(this.table, "table");
            this.table.Controls.Add(this.lbLatestVersion, 0, 2);
            this.table.Controls.Add(this.btDown, 0, 3);
            this.table.Controls.Add(this.lbCurrentVersion, 0, 1);
            this.table.Controls.Add(this.lbNewUpdate, 0, 0);
            this.table.Name = "table";
            // 
            // UpdateBox
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.table);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UpdateBox";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Load += new System.EventHandler(this.UpdateBox_Load);
            this.table.ResumeLayout(false);
            this.table.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lbNewUpdate;
        private System.Windows.Forms.Label lbCurrentVersion;
        private System.Windows.Forms.Label lbLatestVersion;
        private System.Windows.Forms.Button btDown;
        private System.Windows.Forms.TableLayoutPanel table;
    }
}