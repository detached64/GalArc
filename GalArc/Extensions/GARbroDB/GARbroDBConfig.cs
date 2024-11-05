using System;

namespace GalArc.Extensions.GARbroDB
{
    [Extension]
    public class GARbroDBConfig : IExtension
    {
        public static bool IsEnabled { get; set; } = true;

        private static string _Path;

        public static string Path
        {
            get
            {
                if (string.IsNullOrEmpty(_Path))
                {
                    return DefaultPath;
                }
                return _Path;
            }
            set
            {
                _Path = value;
            }
        }

        public static string DefaultPath { get; } = System.IO.Path.Combine(Environment.CurrentDirectory, "Extensions\\Formats.json");

        public string Description => "Database of GARbro.";

        public string OriginalAuthor => "morkt/Crsky";

        public string OriginalWebsite => "https://github.com/crskycode/GARbro";

        public string ExtensionWebsite => "https://github.com/detached64/GARbroSchemeDumper";
    }
}
