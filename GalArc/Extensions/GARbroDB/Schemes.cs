using System;
using System.Collections.Generic;

namespace GalArc.Extensions.GARbroDB
{
    public class SeraphScheme : IGARbroScheme
    {
        public static string JsonEngineName = "SERAPH/ARCH";

        public class ArchPacScheme
        {
            public long IndexOffset { get; set; }
            public short EventDir { get; set; }
            public Dictionary<short, short> EventMap { get; set; }
        }

        public Dictionary<string, ArchPacScheme> KnownSchemes { get; set; }
    }

    public class AGSScheme : IGARbroScheme
    {
        public static string JsonEngineName = "DAT/AGS";

        public class Key
        {
            public int Initial { get; set; }
            public int Increment { get; set; }
        }

        public class AGSFileMap
        {
            public Dictionary<string, Key> FileMap { get; set; }
        }

        public Dictionary<string, AGSFileMap> KnownSchemes { get; set; }

        public List<string> EncryptedArchives { get; set; }
    }

    public interface IGARbroScheme
    {
    }
}
