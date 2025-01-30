using System;
using System.Collections.Generic;

namespace GalArc.Database
{
    public class Ns2Scheme : IScheme
    {
        public Dictionary<string, string> KnownKeys { get; set; }

        public int Version { get; set; }
    }

    public class SeraphScheme : IScheme
    {
        public List<long> KnownOffsets { get; set; }

        public int Version { get; set; }
    }

    public class SiglusScheme : IScheme
    {
        public class SiglusKey
        {
            public byte[] KnownKey { get; set; }
        }

        public Dictionary<string, SiglusKey> KnownSchemes { get; set; }

        public int Version { get; set; }
    }

    public interface IScheme
    {
        int Version { get; set; }
    }
}
