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
            this.lbCurrentVer = new System.Windows.Forms.Label();
            this.lbLatestVer = new System.Windows.Forms.Label();
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
            this.lbCurrentVersion.SizeChanged += new System.EventHandler(this.lbCurrentVersion_SizeChanged);
            // 
            // lbLatestVersion
            // 
            resources.ApplyResources(this.lbLatestVersion, "lbLatestVersion");
            this.lbLatestVersion.Name = "lbLatestVersion";
            this.lbLatestVersion.SizeChanged += new System.EventHandler(this.lbLatestVersion_SizeChanged);
            // 
            // btDown
            // 
            resources.ApplyResources(this.btDown, "btDown");
            this.btDown.Name = "btDown";
            this.btDown.UseVisualStyleBackColor = true;
            this.btDown.Click += new System.EventHandler(this.btDown_Click);
            // 
            // lbCurrentVer
            // 
            resources.ApplyResources(this.lbCurrentVer, "lbCurrentVer");
            this.lbCurrentVer.Name = "lbCurrentVer";
            // 
            // lbLatestVer
            // 
            resources.ApplyResources(this.lbLatestVer, "lbLatestVer");
            this.lbLatestVer.Name = "lbLatestVer";
            // 
            // UpdateBox
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lbLatestVer);
            this.Controls.Add(this.lbCurrentVer);
            this.Controls.Add(this.btDown);
            this.Controls.Add(this.lbLatestVersion);
            this.Controls.Add(this.lbCurrentVersion);
            this.Controls.Add(this.lbNewUpdate);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UpdateBox";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbNewUpdate;
        private System.Windows.Forms.Label lbCurrentVersion;
        private System.Windows.Forms.Label lbLatestVersion;
        private System.Windows.Forms.Button btDown;
        private System.Windows.Forms.Label lbCurrentVer;
        private System.Windows.Forms.Label lbLatestVer;
    }
}