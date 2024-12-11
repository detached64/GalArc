using System;

namespace GalArc.Extensions
{
    public static class ExtensionsConfig
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

    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class ExtensionAttribute : Attribute
    {
    }
}
