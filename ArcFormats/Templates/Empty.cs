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
                if (instance == null)
                {
                    instance = new Empty();
                }
                return instance;
            }
        }

        public Empty()
        {
            InitializeComponent();
        }
    }
}
