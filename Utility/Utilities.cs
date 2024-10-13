// File: Utility/Utilities.cs
// Date: 2024/08/26
// Description: 一些常用的工具函数
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
    public class Utilities
    {
        /// <summary>
        /// Sort the file paths. Use string.CompareOrdinal() to avoid culture influence.
        /// </summary>
        /// <param name="pathString"></param>
        public static void InsertSort(string[] pathString)
        {
            for (int i = 1; i < pathString.Length; i++)
            {
                string insrtVal = pathString[i];
                int insertIndex = i - 1;

                while (insertIndex >= 0 && string.CompareOrdinal(insrtVal, pathString[insertIndex]) < 0)
                {
                    string temp;
                    temp = pathString[insertIndex + 1];
                    pathString[insertIndex + 1] = pathString[insertIndex];
                    pathString[insertIndex] = temp;
                    insertIndex--;
                }
                pathString[insertIndex + 1] = insrtVal;
            }
        }

        /// <summary>
        /// Used for continuous file names with a separator <paramref name="toThis"/> in between.
        /// </summary>
        /// <param name="br"></param>
        /// <param name="encoding"></param>
        /// <param name="toThis"></param>
        /// <returns></returns>
        public static string ReadCString(BinaryReader br, Encoding encoding, byte toThis = 0x00)
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

        public static string GetCString(string str, Encoding encoding)
        {
            byte[] bytes = encoding.GetBytes(str);
            byte[] cStringBytes = new byte[bytes.Length + 1];
            Array.Copy(bytes, cStringBytes, bytes.Length);
            cStringBytes[bytes.Length] = 0x00;
            return encoding.GetString(cStringBytes);
        }

        /// <summary>
        /// Get file count in specified folder and all subfolders.
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public static int GetFileCount_All(string folderPath)
        {
            string[] allFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
            return allFiles.Length;
        }

        /// <summary>
        /// Get file count in specified folder only.
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public static int GetFileCount_TopOnly(string folderPath)
        {
            string[] allFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly);
            return allFiles.Length;
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
    }
}
