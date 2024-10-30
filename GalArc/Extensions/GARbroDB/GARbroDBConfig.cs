using System;
using System.IO;

namespace GalArc.Extensions.GARbroDB
{
    public class GARbroDBConfig
    {
        public static bool IsGARbroDBEnabled { get; set; } = true;

        public static string GARbroDBPath { get; set; } = Path.Combine(Environment.CurrentDirectory, "Extensions\\GARbroDB\\Formats.json");

    }
}
