// File: Utility/Utils.cs
// Date: 2024/08/26
// Description: Utility functions.
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

namespace Utility
{
    public static class Utils
    {
        /// <summary>
        /// Get file count in specified folder and all subfolders.
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public static int GetFileCount(string folderPath, SearchOption searchOption = SearchOption.AllDirectories)
        {
            DirectoryInfo dir = new DirectoryInfo(folderPath);
            FileInfo[] files = dir.GetFiles("*.*", searchOption);
            return files.Length;
        }

        /// <summary>
        /// Get all extensions among all files in specified folder and all subfolders.
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public static string[] GetFileExtensions(string folderPath)
        {
            HashSet<string> uniqueExtension = new HashSet<string>();
            DirectoryInfo d = new DirectoryInfo(folderPath);
            foreach (FileInfo file in d.GetFiles())
            {
                uniqueExtension.Add(file.Extension.Replace(".", string.Empty));
            }
            string[] ext = new string[uniqueExtension.Count];
            uniqueExtension.CopyTo(ext);
            return ext;
        }

        /// <summary>
        /// Get file name length sum among all files in specified folder and all subfolders.
        /// </summary>
        /// <param name="strings"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static int GetNameLengthSum(IEnumerable<string> strings, Encoding encoding)
        {
            int sum = 0;
            foreach (string s in strings)
            {
                sum += encoding.GetByteCount(Path.GetFileName(s));
            }
            return sum;
        }

        public static int GetNameLengthSum(FileInfo[] files, Encoding encoding)
        {
            int sum = 0;
            foreach (FileInfo file in files)
            {
                sum += encoding.GetByteCount(file.Name);
            }
            return sum;
        }

        public static int GetLengthSum(IEnumerable<string> strings, Encoding encoding)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var str in strings)
            {
                sb.Append(str);
            }
            return encoding.GetByteCount(sb.ToString());
        }

        public static string GetRelativePath(string fullPath, string basePath)
        {
            if (fullPath.StartsWith(basePath))
            {
                return fullPath.Substring(basePath.Length).TrimStart(Path.DirectorySeparatorChar);
            }
            throw new ArgumentException("fullPath does not start with basePath.");
        }

        public static string[] GetRelativePaths(string[] fullPaths, string basePath)
        {
            string[] results = new string[fullPaths.Length];
            for (int i = 0; i < fullPaths.Length; i++)
            {
                results[i] = GetRelativePath(fullPaths[i], basePath);
            }
            return results;
        }

        public static int GetRelativePathLenSum(string[] fullPaths, string basePath, Encoding encoding)
        {
            string[] relativePaths = GetRelativePaths(fullPaths, basePath);
            StringBuilder sb = new StringBuilder();
            foreach (string relativePath in relativePaths)
            {
                sb.Append(relativePath);
            }
            return encoding.GetByteCount(sb.ToString());
        }

        /// <summary>
        /// 123456 -> 0x12 0x34 0x56
        /// </summary>
        public static byte[] HexStringToByteArray(string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException("hexString length must be even.");
            }
            byte[] bytes = new byte[hexString.Length / 2];
            for (int i = 0; i < hexString.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            }
            return bytes;
        }

        /// <summary>
        /// 12-34-56 -> 0x12 0x34 0x56
        /// </summary>
        public static byte[] HexStringToByteArray(string hexString, char separator = '-')
        {
            string[] hexValues = hexString.Split(separator);
            byte[] bytes = new byte[hexValues.Length];
            for (int i = 0; i < hexValues.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexValues[i], 16);
            }
            return bytes;
        }

        public static byte[] GetPaddedBytes(string input, int length, char padChar, Encoding encoding)
        {
            byte[] bytes = encoding.GetBytes(input);
            if (length > bytes.Length)
            {
                byte[] paddedBytes = new byte[length];
                Array.Copy(bytes, paddedBytes, bytes.Length);
                if (padChar != '\0')
                {
                    byte[] padBytes = encoding.GetBytes(new string(padChar, length - bytes.Length));
                    Array.Copy(padBytes, 0, paddedBytes, bytes.Length, padBytes.Length);
                }
                bytes = null;
                return paddedBytes;
            }
            else
            {
                byte[] paddedBytes = new byte[length];
                Array.Copy(bytes, paddedBytes, length);
                bytes = null;
                return paddedBytes;
            }
        }

        public static byte[] GetPaddedBytes(string input, int length)
        {
            return GetPaddedBytes(input, length, '\0', ArcEncoding.Shift_JIS);
        }
    }
}
