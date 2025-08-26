using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GalArc.Models.Utils;

internal static class StringExtensions
{
    private static readonly SearchValues<char> InvalidPathChars = SearchValues.Create("<>\"|?*");

    public static bool ContainsInvalidChars(this string str)
    {
        return string.IsNullOrEmpty(str) || str.AsSpan().IndexOfAny(InvalidPathChars) != -1;
    }

    public static bool HasAnyOfExtensions(this string source, params string[] extensions)
    {
        ArgumentNullException.ThrowIfNull(source);

        string thisExtension = source[0] == '.' ? source.TrimStart('.') : Path.GetExtension(source).TrimStart('.');
        foreach (string extension in extensions)
        {
            if (string.Equals(thisExtension, extension, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }
}

internal static class BinaryReaderExtensions
{
    public static string ReadCString(this BinaryReader br, Encoding encoding, byte terminator = 0x00)
    {
        bool isUnicode = encoding == Encoding.Unicode;
        int bytesToRead = isUnicode ? 2 : 1;
        byte[] buffer = new byte[bytesToRead];
        List<byte> byteList = [];

        while (true)
        {
            int bytesRead = br.Read(buffer, 0, bytesToRead);

            if (bytesRead != bytesToRead)
            {
                break;
            }

            if (isUnicode)
            {
                if (buffer[0] == 0 && buffer[1] == 0)
                {
                    break;
                }
            }
            else
            {
                if (buffer[0] == terminator)
                {
                    break;
                }
            }
            byteList.AddRange(buffer);
        }
        return encoding.GetString([.. byteList]);
    }

    public static string ReadCString(this BinaryReader br)
    {
        return ReadCString(br, ArcEncoding.Shift_JIS, 0x00);
    }

    public static string ReadCString(this BinaryReader br, byte terminator)
    {
        return ReadCString(br, ArcEncoding.Shift_JIS, terminator);
    }
}

internal static class BinaryWriterExtensions
{
    public static void WritePaddedString(this BinaryWriter bw, string input, int length, char padChar, Encoding encoding)
    {
        bw.Write(Utility.GetPaddedBytes(input, length, padChar, encoding));
    }

    public static void WritePaddedString(this BinaryWriter bw, string input, int length)
    {
        WritePaddedString(bw, input, length, '\0', ArcEncoding.Shift_JIS);
    }
}

internal static class ByteArrayExtensions
{
    public static string ToHexString(this byte[] bytes)
    {
        char[] c = new char[bytes.Length * 2];
        byte b;
        for (int i = 0; i < bytes.Length; i++)
        {
            b = ((byte)(bytes[i] >> 4));
            c[i * 2] = (char)(b > 9 ? b + 0x37 : b + 0x30);
            b = ((byte)(bytes[i] & 0xF));
            c[(i * 2) + 1] = (char)(b > 9 ? b + 0x37 : b + 0x30);
        }
        return new string(c);
    }

    public static string GetCString(this byte[] bytes, int offset, int maxLength, Encoding encoding, byte separator)
    {
        int nameLength = 0;
        while (nameLength < maxLength && bytes[offset + nameLength] != separator)
        {
            nameLength++;
        }
        return encoding.GetString(bytes, offset, nameLength);
    }

    public static string GetCString(this byte[] bytes, int offset, int maxLength)
    {
        return GetCString(bytes, offset, maxLength, ArcEncoding.Shift_JIS, 0x00);
    }

    public static string GetCString(this byte[] bytes, int offset)
    {
        return GetCString(bytes, offset, bytes.Length - offset);
    }

    public static string GetCString(this byte[] bytes)
    {
        return GetCString(bytes, 0);
    }
}
