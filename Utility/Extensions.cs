// File: Utility/Crc32.cs
// Date: 2024/10/09
// Description: 提供一系列扩展方法
//
// Copyright (C) 2024 detached64
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
        public static bool HasAnyOfExtensions(this string source, params string[] extensions)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            string thisExtension = source[0] == '.' ? source.TrimStart('.') : Path.GetExtension(source).TrimStart('.');
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

    public static class BinaryReaderExtensions
    {
        public static string ReadCString(this BinaryReader br, Encoding encoding, byte toThis = 0x00)
        {
            List<byte> byteList = new List<byte>();
            bool isUnicode = encoding == Encoding.Unicode;

            if (isUnicode)
            {
                while (true)
                {
                    byte b1 = br.ReadByte();
                    byte b2 = br.ReadByte();
                    if (b1 == 0 && b2 == 0)
                    {
                        break;
                    }
                    byteList.Add(b1);
                    byteList.Add(b2);

                }
            }
            else
            {
                while (true)
                {
                    byte b = br.ReadByte();
                    if (b == toThis)
                    {
                        break;
                    }
                    byteList.Add(b);
                }
            }

            return encoding.GetString(byteList.ToArray());
        }

        public static void WritePaddedString(this BinaryWriter bw, string input, int length, char padChar, Encoding encoding)
        {
            bw.Write(Utils.PaddedBytes(input, length, padChar, encoding));
        }

        public static void WritePaddedString(this BinaryWriter bw, string input, int length)
        {
            WritePaddedString(bw, input, length, '\0', ArcEncoding.Shift_JIS);
        }

    }
}
