using System;
using System.IO;

namespace Utility.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Invalid path chars in Windows.
        /// </summary>
        /// Discard path separators \ , / and :.
        private static readonly char[] InvalidPathChars = new char[]
        {
            '<', '>', '"', '|', '?', '*'
        };

        /// <summary>
        /// Returns true if the string contains invalid path chars('\' , '/' and ':' are not included).
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool ContainsInvalidChars(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return true;
            }
            return str.IndexOfAny(InvalidPathChars) != -1;
        }

        /// <summary>
        /// Returns true if the string contains any of given extensions.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="extensions"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool HaveAnyOfExtensions(this string source, params string[] extensions)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            string thisExtension = Path.GetExtension(source).TrimStart('.');

            foreach (var extension in extensions)
            {
                if (string.Equals(thisExtension, extension, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
