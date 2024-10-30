using System;
using System.IO;

namespace GalArc.Extensions.GARbroDB
{
    public class GARbroDBConfig
    {
        public static bool IsGARbroDBEnabled { get; set; } = true;

        public static string GARbroDBPath { get; set; }

        public static string DefaultGARbroDBPath { get; } = Path.Combine(Environment.CurrentDirectory, "Extensions\\Formats.json");

    }
}
