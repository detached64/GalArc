using System;
using System.IO;

namespace GalArc.DataBase
{
    public class DataBaseConfig
    {
        public static bool IsDataBaseEnabled { get; set; } = true;

        private static string _DataBasePath;

        public static string DataBasePath
        {
            get
            {
                if (string.IsNullOrEmpty(_DataBasePath))
                {
                    return DefaultDataBasePath;
                }
                return _DataBasePath;
            }
            set
            {
                _DataBasePath = value;
            }
        }

        private static string DefaultDataBasePath { get; } = Path.Combine(Environment.CurrentDirectory, "Database\\");
    }

    public class Scheme
    {
    }

    public class DatabaseSchemeAttribute : Attribute
    {
    }
}
