using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace GalArc.Database.Siglus
{
    [DatabaseScheme]
    public class SiglusScheme : Scheme
    {
        public class SiglusKey
        {
            public string KnownKey { get; set; }
        }

        [JsonProperty("KnownSchemes")]
        public Dictionary<string, SiglusKey> KnownSchemes { get; set; }

        [JsonProperty("Version")]
        public int Version { get; set; }
    }
}
