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
        // 3. 處理簡體中文變體
        if (culture.Name == "zh-CN") return SimplifiedChinese;

        // 4. 增加正體中文變體處理 (讓 zh-TW/zh-HK 也能正確顯示正體中文)
        if (culture.Name is "zh-TW" or "zh-HK" or "zh-MO") return TraditionalChinese;

        // 檢查是否在支援清單中，否則回退到預設(英文)
        return SupportedCultures.Contains(culture) ? culture : DefaultCulture;
    }
}
