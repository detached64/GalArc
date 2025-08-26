using System.Collections.Generic;
using System.Text;

namespace GalArc.Models.Utils;

internal static class ArcEncoding
{
    public static readonly Encoding Shift_JIS;

    public static readonly IReadOnlyList<Encoding> SupportedEncodings;

    static ArcEncoding()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Shift_JIS = Encoding.GetEncoding(932);
        SupportedEncodings = [Encoding.UTF8, Shift_JIS];
    }
}
