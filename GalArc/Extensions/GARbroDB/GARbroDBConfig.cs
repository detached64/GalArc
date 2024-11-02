using System;
using System.IO;

namespace GalArc.Extensions.GARbroDB
{
    public class GARbroDBConfig
    {
        public static bool IsGARbroDBEnabled { get; set; } = true;

        private static string _GARbroDBPath;

        public static string GARbroDBPath
        {
            get
            {
                if (string.IsNullOrEmpty(_GARbroDBPath))
                {
                    return DefaultGARbroDBPath;
                }
                return _GARbroDBPath;
            }
            set
            {
                _GARbroDBPath = value;
            }
        }

        private static readonly string DefaultGARbroDBPath = Path.Combine(Environment.CurrentDirectory, "Extensions\\Formats.json");

    }
}
