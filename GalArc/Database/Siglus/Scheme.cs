using System;

namespace GalArc.DataBase.Siglus
{
    [DatabaseScheme]
    public class SiglusScheme : Scheme
    {
        public static string[] JsonNodeName = new string[] { "KnownSchemes" };

        public string KnownKey { get; set; }
    }
}
