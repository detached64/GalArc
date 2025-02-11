using System;
using System.Windows.Forms;

namespace GalArc.Controls
{
    /// <summary>
    /// The template for the extra options.
    /// </summary>
    public partial class OptionsTemplate : UserControl
    {
        public string Version { get; set; }

        public OptionsTemplate()
        {
            InitializeComponent();
        }
    }
}
