using System;

namespace GalArc.Controls
{
    public partial class Empty : OptionsTemplate
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
