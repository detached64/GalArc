using System.Collections.Generic;

namespace GalArc.Common
{
    public static class Languages
    {
        public static Dictionary<string, string> SupportedLanguages => new Dictionary<string, string>
        {
            { "简体中文" , "zh-CN" },
            { "English" , "en-US" }
        };
    }
}
