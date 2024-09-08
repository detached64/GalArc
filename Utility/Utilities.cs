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
using System.Linq;
using System.Text;

namespace Utility
{
    public class Utilities
    {
        /// <summary>
        /// Sort the file paths.Use string.CompareOrdinal() to avoid culture influence.
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
        /// Only used for continuous file names with a separator in between.
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
        /// <param name="fileSet"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static int GetNameLenSum(IEnumerable<string> fileSet, Encoding encoding)
        {
            int sum = 0;
            foreach (string s in fileSet)
            {
                sum += encoding.GetBytes(Path.GetFileName(s)).Length;
            }
            return sum;
        }

    }
}
