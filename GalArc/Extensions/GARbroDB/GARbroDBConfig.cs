using System;

namespace GalArc.Extensions.GARbroDB
{
    [Extension]
    public class GARbroDBConfig : IExtension
    {
        public static string Path
        {
            get
            {
                if (string.IsNullOrEmpty(BaseSettings.Default.GARbroDBPath))
                {
                    BaseSettings.Default.GARbroDBPath = DefaultPath;
                    BaseSettings.Default.Save();
                    return DefaultPath;
                }
                return BaseSettings.Default.GARbroDBPath;
            }
            set
            {
                BaseSettings.Default.GARbroDBPath = value;
                BaseSettings.Default.Save();
            }
        }

        public static string DefaultPath => System.IO.Path.Combine(Environment.CurrentDirectory, "Extensions\\Formats.json");

        public string Description => "Database of GARbro.";

        public string OriginalAuthor => "morkt/Crsky";

        public string OriginalWebsite => "https://github.com/crskycode/GARbro";

        public string ExtensionWebsite => "https://github.com/detached64/GARbroSchemeDumper";
    }
}
