﻿using System;
using System.Windows.Forms;

namespace ArcFormats.Softpal
{
    public partial class PackPACOptions : UserControl
    {
        internal static bool toCompute = false;

        private static string versions = "1/2";

        public PackPACOptions()
        {
            InitializeComponent();
            this.combVersion.Items.AddRange(versions.Split('/'));
            if (this.combVersion.Items.Count > 0)
            {
                this.combVersion.SelectedIndex = 0;
            }
        }

        private void chkbxCompute_CheckedChanged(object sender, EventArgs e)
        {
            toCompute = this.chkbxCompute.Checked;
        }

        private void combVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            Global.Version = this.combVersion.Text;
            if (this.combVersion.Text == "1")
            {
                this.chkbxCompute.Enabled = false;
            }
            else
            {
                this.chkbxCompute.Enabled = true;
            }
        }
    }
}