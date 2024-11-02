using System;
using System.IO;

namespace GalArc.Extensions.SiglusKeyFinder
{
    public class KeyFinderConfig
    {
        public static bool IsSiglusKeyFinderEnabled { get; set; } = true;

        private static string _SiglusKeyFinderPath;

        public static string SiglusKeyFinderPath
        {
            get
            {
                if (string.IsNullOrEmpty(_SiglusKeyFinderPath))
                {
                    return DefaultSiglusKeyFinderPath;
                }
                return _SiglusKeyFinderPath;
            }
            set
            {
                _SiglusKeyFinderPath = value;
            }
        }

        private static readonly string DefaultSiglusKeyFinderPath = Path.Combine(Environment.CurrentDirectory, "Extensions\\SiglusKeyFinder.exe");

    }
}
