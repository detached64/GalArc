using System;
using System.Collections.Generic;

namespace GalArc.Common
{
    public static class Encodings
    {
        static Encodings()
        {
            CodePages = new Dictionary<string, int>
            {
                { "Shift-JIS", 932 },
                { "UTF-8", 65001 },
                { "GBK", 936 },
            };
        }

        public static Dictionary<string, int> CodePages { get; }
    }
}
