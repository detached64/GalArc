using System;
using System.Windows.Forms;

namespace ArcFormats.Templates
{
    public partial class Empty : UserControl
    {
        private static Empty _instance;
        public static Empty Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Empty();
                }
                return _instance;
            }
        }

        public Empty()
        {
            InitializeComponent();
        }
    }
}
