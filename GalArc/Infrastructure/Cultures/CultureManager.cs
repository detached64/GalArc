using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GalArc.Infrastructure.Cultures;

internal static class CultureManager
{
    private static readonly CultureInfo DefaultCulture = new("en-US");

    private static readonly CultureInfo SimplifiedChinese = new("zh-Hans");

    public static readonly IReadOnlyList<CultureInfo> SupportedCultures =
        [DefaultCulture, SimplifiedChinese];

    public static CultureInfo InitCulture(CultureInfo culture)
    {
        return culture.Name == "zh-CN" ? SimplifiedChinese : SupportedCultures.Contains(culture) ? culture : DefaultCulture;
    }
}
