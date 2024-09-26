using System;
using System.Collections.Generic;

namespace GalArc.Resource
{
    internal class Encodings
    {
        public static Dictionary<string, int> CodePages = new Dictionary<string, int>
        {
            { "Shift-JIS",932 },
            { "UTF-8",65001 },
            { "GBK",936 },
        };
    }
}
