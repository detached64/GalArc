using System;

namespace GalArc.DataBase
{
    public static class DataBaseConfig
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

        private static string DefaultPath { get; } = System.IO.Path.Combine(Environment.CurrentDirectory, "Database\\");
    }

    public class Scheme
    {
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class DatabaseSchemeAttribute : Attribute
    {
    }
}
