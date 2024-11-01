using GalArc.DataBase.Siglus;
using System;
using System.Collections.Generic;
using System.IO;

namespace GalArc.DataBase
{
    public class DataBaseConfig
    {
        public static bool IsDataBaseEnabled { get; set; } = true;

        public static string DataBasePath { get; set; }

        public static string DefaultDataBasePath { get; } = Path.Combine(Environment.CurrentDirectory, "Database\\");

        internal static List<string> EnginesToLoad { get; } = new List<string>()
        {
            "Siglus",
        };

        internal static List<Scheme> InstanceToLoad { get; } = new List<Scheme>()
        {
            SiglusScheme.Instance,
        };
    }

    public class Scheme
    {
    }
}
