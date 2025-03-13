using System.Collections.Generic;

namespace GalArc.Database
{
    public class AGSScheme : ArcScheme
    {
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

    public class Ns2Scheme : ArcScheme
    {
        public Dictionary<string, string> KnownKeys { get; set; }
    }

    public class QlieScheme : ArcScheme
    {
        public Dictionary<string, byte[]> KnownKeys { get; set; }
    }

    public class SeraphScheme : ArcScheme
    {
        public List<long> KnownOffsets { get; set; }
    }

    public class SiglusScheme : ArcScheme
    {
        public Dictionary<string, byte[]> KnownKeys { get; set; }
    }

    public abstract class ArcScheme
    {
    }
}
