using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GalArc.Infrastructure.Cultures;

internal static class CultureManager
{
    private static readonly CultureInfo DefaultCulture = new("en-US");

    private static readonly CultureInfo SimplifiedChinese = new("zh-Hans");

    private static readonly CultureInfo TraditionalChinese = new("zh-Hant");

    public static readonly IReadOnlyList<CultureInfo> SupportedCultures =
        [DefaultCulture, SimplifiedChinese, TraditionalChinese];

    public static CultureInfo InitCulture(CultureInfo culture)
    {
        if (culture.Name is "zh-TW" or "zh-HK" or "zh-MO") return TraditionalChinese;
        return culture.Name == "zh-CN" ? SimplifiedChinese : SupportedCultures.Contains(culture) ? culture : DefaultCulture;
    }
}
