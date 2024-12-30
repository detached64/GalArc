using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace GalArc.Extensions.GARbroDB
{
    public class SeraphScheme : IScheme
    {
        public static string JsonEngineName = "SERAPH/ARCH";

        public class ArchPacScheme
        {
            public long IndexOffset { get; set; }
            public short EventDir { get; set; }
            public Dictionary<short, short> EventMap { get; set; }
        }

        [JsonProperty("KnownSchemes")]
        public Dictionary<string, ArchPacScheme> KnownSchemes { get; set; }
    }

    public class AGSScheme : IScheme
    {
        public static string JsonEngineName = "DAT/AGS";

        public class Key
        {
            public int Initial { get; set; }
            public int Increment { get; set; }
        }

        public class AGSFileMap
        {
            [JsonProperty("FileMap")]
            public Dictionary<string, Key> FileMap { get; set; }
        }

        [JsonProperty("KnownSchemes")]
        public Dictionary<string, AGSFileMap> KnownSchemes { get; set; }

        [JsonProperty("EncryptedArchives")]
        public List<string> EncryptedArchives { get; set; }
    }

    public interface IScheme
    {
    }
}
