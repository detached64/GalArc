using GalArc.Properties;
using GalArc.Resource;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace GalArc.Controller
{
    internal class Localize
    {
        /// <summary>
        /// Set the culture according to the system language.
        /// </summary>
        /// Four child windows are in the same thread; main window is in another thread.
        /// Use this method in OptionWindow and main only.
        /// 
        internal static string GetLocalCulture()
        {
            string LocalCulture;
            if (string.IsNullOrEmpty(Settings.Default.lastLang))
            {
                LocalCulture = CultureInfo.CurrentCulture.Name;
            }
            else
            {
                LocalCulture = Settings.Default.lastLang;
            }
            if (!Languages.languages.Values.ToArray().Contains(LocalCulture))
            {
                LocalCulture = "en-US";
            }
            return LocalCulture;
        }

        internal static void SetLocalCulture(string LocalCulture)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(LocalCulture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(LocalCulture);
        }
    }
}
