using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcFormats.Cmvs
{
    internal class Utils
    {
        public static void Swap<T>(ref T a1, ref T a2)
        {
            (a2, a1) = (a1, a2);
        }
    }
}
