﻿namespace GalArc.GUI
{
    partial class LogSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogSettings));
            this.chkbxDebug = new System.Windows.Forms.CheckBox();
            this.chkbxSaveLog = new System.Windows.Forms.CheckBox();
            this.lbLogPath = new System.Windows.Forms.Label();
            this.txtLogPath = new System.Windows.Forms.TextBox();
            this.btSelInput = new System.Windows.Forms.Button();
            this.panel = new System.Windows.Forms.Panel();
            this.panel.SuspendLayout();
            this.SuspendLayout();
            // 
            // chkbxDebug
            // 
            resources.ApplyResources(this.chkbxDebug, "chkbxDebug");
            this.chkbxDebug.Name = "chkbxDebug";
            this.chkbxDebug.UseVisualStyleBackColor = true;
            this.chkbxDebug.CheckedChanged += new System.EventHandler(this.chkbxDebug_CheckedChanged);
            // 
            // chkbxSaveLog
            // 
            resources.ApplyResources(this.chkbxSaveLog, "chkbxSaveLog");
            this.chkbxSaveLog.Name = "chkbxSaveLog";
            this.chkbxSaveLog.UseVisualStyleBackColor = true;
            this.chkbxSaveLog.CheckedChanged += new System.EventHandler(this.chkbxSaveLog_CheckedChanged);
            // 
            // lbLogPath
            // 
            resources.ApplyResources(this.lbLogPath, "lbLogPath");
            this.lbLogPath.Name = "lbLogPath";
            // 
            // txtLogPath
            // 
            resources.ApplyResources(this.txtLogPath, "txtLogPath");
            this.txtLogPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtLogPath.Name = "txtLogPath";
            this.txtLogPath.TextChanged += new System.EventHandler(this.txtLogPath_TextChanged);
            // 
            // btSelInput
            // 
            resources.ApplyResources(this.btSelInput, "btSelInput");
            this.btSelInput.Name = "btSelInput";
            this.btSelInput.UseVisualStyleBackColor = true;
            this.btSelInput.Click += new System.EventHandler(this.btSelInput_Click);
            // 
            // panel
            // 
            resources.ApplyResources(this.panel, "panel");
            this.panel.Controls.Add(this.txtLogPath);
            this.panel.Controls.Add(this.btSelInput);
            this.panel.Controls.Add(this.lbLogPath);
            this.panel.Name = "panel";
            // 
            // LogSettings
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel);
            this.Controls.Add(this.chkbxSaveLog);
            this.Controls.Add(this.chkbxDebug);
            this.Name = "LogSettings";
            this.Load += new System.EventHandler(this.LogSettings_Load);
            this.panel.ResumeLayout(false);
            this.panel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkbxDebug;
        private System.Windows.Forms.CheckBox chkbxSaveLog;
        private System.Windows.Forms.Label lbLogPath;
        private System.Windows.Forms.TextBox txtLogPath;
        private System.Windows.Forms.Button btSelInput;
        private System.Windows.Forms.Panel panel;
    }
}
