using System;
using System.Windows.Forms;

namespace ArcFormats.Templates
{
    public partial class Empty : UserControl
    {
        private static Empty instance;
        public static Empty Instance
        {
            get
            {
                return instance ?? (instance = new Empty());
            }
        }

        public Empty()
        {
            InitializeComponent();
        }
    }
}
