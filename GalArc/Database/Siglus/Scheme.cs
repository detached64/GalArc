using System;

namespace GalArc.DataBase.Siglus
{
    public class SiglusScheme : Scheme
    {
        public static string EngineName = "Siglus";
        public static string[] JsonNodeName = new string[] { "KnownSchemes" };
        public static Scheme Instance = new SiglusScheme();

        public string KnownKey { get; set; }
    }
}
