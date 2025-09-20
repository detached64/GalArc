using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GalArc.Models.Utils;

internal static class Utility
{
    public static int GetFileCount(string folderPath, SearchOption searchOption = SearchOption.AllDirectories)
    {
        DirectoryInfo dir = new(folderPath);
        FileInfo[] files = dir.GetFiles("*.*", searchOption);
        return files.Length;
    }

    public static string[] GetFileExtensions(string folderPath)
    {
        HashSet<string> uniqueExtension = [];
        DirectoryInfo d = new(folderPath);
        foreach (FileInfo file in d.GetFiles())
        {
            uniqueExtension.Add(file.Extension.Replace(".", string.Empty));
        }
        string[] ext = new string[uniqueExtension.Count];
        uniqueExtension.CopyTo(ext);
        return ext;
    }

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
        StringBuilder sb = new();
        foreach (string str in strings)
        {
            sb.Append(str);
        }
        return encoding.GetByteCount(sb.ToString());
    }

    public static string[] GetRelativePaths(string[] fullPaths, string basePath)
    {
        string[] results = new string[fullPaths.Length];
        for (int i = 0; i < fullPaths.Length; i++)
        {
            results[i] = Path.GetRelativePath(basePath, fullPaths[i]);
        }
        return results;
    }

    public static int GetRelativePathLenSum(string[] fullPaths, string basePath, Encoding encoding)
    {
        string[] relativePaths = GetRelativePaths(fullPaths, basePath);
        StringBuilder sb = new();
        foreach (string relativePath in relativePaths)
        {
            sb.Append(relativePath);
        }
        return encoding.GetByteCount(sb.ToString());
    }

    public static byte[] HexStringToByteArray(string hexString)
    {
        return Convert.FromHexString(hexString);
    }

    public static byte[] GetPaddedBytes(string input, int length, char padChar, Encoding encoding)
    {
        byte[] bytes = encoding.GetBytes(input);
        byte[] result = new byte[length];
        int bytesToCopy = Math.Min(length, bytes.Length);
        Buffer.BlockCopy(bytes, 0, result, 0, bytesToCopy);

        if (length > bytes.Length && padChar != '\0')
        {
            byte[] padBytes = Encoding.ASCII.GetBytes(new string(padChar, length - bytes.Length));
            Buffer.BlockCopy(padBytes, 0, result, bytes.Length, padBytes.Length);
        }
        return result;
    }

    public static byte[] GetPaddedBytes(string input, int length)
    {
        return GetPaddedBytes(input, length, '\0', ArcEncoding.Shift_JIS);
    }
}
