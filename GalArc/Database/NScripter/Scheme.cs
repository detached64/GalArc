using System;
using System.Collections.Generic;

namespace GalArc.Database.NScripter
{
    [DatabaseScheme]
    public class NS2Scheme : Scheme
    {
        public Dictionary<string, string> KnownKeys { get; set; }
        public int Version { get; set; }
    }
}
