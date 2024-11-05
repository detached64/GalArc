using System;

namespace GalArc.Extensions.SiglusKeyFinder
{
    [Extension]
    public class SiglusKeyFinderConfig : IExtension
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

        public static string DefaultPath { get; } = System.IO.Path.Combine(Environment.CurrentDirectory, "Extensions\\SiglusKeyFinder.exe");

        public string Description => "Extract xor key from SiglusEngine games.";
        public string OriginalAuthor => "yanhua0518";
        public string OriginalWebsite => "https://github.com/yanhua0518/GALgameScriptTools/tree/master/SiglusEngine";
        public string ExtensionWebsite => "https://github.com/detached64/SiglusKeyFinder";
    }
}
