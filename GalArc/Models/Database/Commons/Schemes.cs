using System.Collections.Generic;

namespace GalArc.Models.Database.Commons;

internal sealed class AgsScheme : ArcScheme
{
    public sealed class AgsKey
    {
        public int Initial { get; set; }
        public int Increment { get; set; }
    }

    public Dictionary<string, Dictionary<string, AgsKey>> KnownSchemes { get; set; }

    public List<string> EncryptedArchives { get; set; }
}

internal sealed class NitroPlusScheme : ArcScheme
{
    public Dictionary<string, byte[]> KnownSchemes { get; set; }
}

internal sealed class Ns2Scheme : ArcScheme
{
    public Dictionary<string, string> KnownSchemes { get; set; }
}

internal sealed class QlieScheme : ArcScheme
{
    public Dictionary<string, byte[]> KnownSchemes { get; set; }
}

internal sealed class SeraphScheme : ArcScheme
{
    public List<long> KnownOffsets { get; set; }
}

internal sealed class SiglusScheme : ArcScheme
{
    public Dictionary<string, byte[]> KnownSchemes { get; set; }
}

internal sealed class VinosScheme : ArcScheme
{
    public Dictionary<string, Dictionary<string, string>> KnownSchemes { get; set; }
}

internal abstract class ArcScheme;
