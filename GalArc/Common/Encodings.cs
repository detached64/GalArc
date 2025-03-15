using System.Collections.Generic;
using System.Text;

namespace GalArc.Common
{
    public static class Encodings
    {
        public static IEnumerable<Encoding> SupportedEncodings => new[] {
            Encoding.GetEncoding(932),
            Encoding.UTF8,
            Encoding.GetEncoding(936),
        };
    }
}
