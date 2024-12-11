using System;

namespace GalArc.Extensions
{
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
