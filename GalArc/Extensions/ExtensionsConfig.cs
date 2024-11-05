using System;

namespace GalArc.Extensions
{
    public class ExtensionsConfig
    {
        public static bool IsEnabled { get; set; } = true;
    }

    public interface IExtension
    {
        string Description { get; }
        string OriginalAuthor { get; }
        string OriginalWebsite { get; }
        string ExtensionWebsite { get; }
    }

    public class ExtensionAttribute : Attribute
    {
    }
}
