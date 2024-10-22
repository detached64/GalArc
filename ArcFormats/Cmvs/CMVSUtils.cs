using System;

namespace ArcFormats.Cmvs
{
    internal class CMVSUtils
    {
        public static void Swap<T>(ref T a1, ref T a2)
        {
            (a2, a1) = (a1, a2);
        }
    }
}
