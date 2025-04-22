using System;

namespace GalArc.Templates
{
    public partial class Empty : WidgetTemplate
    {
        private static readonly Lazy<Empty> lazyInstance = new Lazy<Empty>(() => new Empty());

        public static Empty Instance => lazyInstance.Value;

        private Empty()
        {
            InitializeComponent();
        }
    }
}
