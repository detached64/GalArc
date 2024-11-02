using System;
using System.IO;

namespace GalArc.Extensions.SiglusKeyFinder
{
    public class KeyFinderConfig
    {
        public static bool IsSiglusKeyFinderEnabled { get; set; } = true;

        public static string SiglusKeyFinderPath { get; set; }

        public static string DefaultSiglusKeyFinderPath { get; } = Path.Combine(Environment.CurrentDirectory, "Extensions\\SiglusKeyFinder.exe");

    }
}
