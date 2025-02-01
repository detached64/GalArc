using System;
using System.Windows.Forms;

namespace ArcFormats.Templates
{
    public partial class Empty : UserControl
    {
        private static readonly Lazy<Empty> lazyInstance =
                new Lazy<Empty>(() => new Empty());

        public static Empty Instance => lazyInstance.Value;

        private Empty()
        {
            InitializeComponent();
        }
    }
}
