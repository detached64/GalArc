using System;

namespace GalArc.Extensions.GARbroDB
{
    public class Scheme
    {
    }

    public class SeraphScheme : Scheme
    {
        public static string JsonEngineName = "SERAPH/ARCH";
        public static string[] JsonNodeName = new string[] { "KnownSchemes" };

        public long IndexOffset { get; set; }
        public short EventDir { get; set; }
        public object EventMap { get; set; }
    }
}
