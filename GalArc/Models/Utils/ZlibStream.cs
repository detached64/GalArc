using System;
using System.IO;
using System.IO.Compression;

namespace GalArc.Models.Utils;

internal sealed class ZlibStream : DeflateStream
{
    private readonly CompressionMode Mode;

    private readonly Adler32 adler32 = new();

    public ZlibStream(Stream stream, CompressionMode mode) : base(stream, mode, true)
    {
        Mode = mode;
        byte m1 = (byte)stream.ReadByte();
        byte m2 = (byte)stream.ReadByte();
        if (m1 != 0x78 || (m2 != 0x01 && m2 != 0x9c && m2 != 0xda))
        {
            stream.Position -= 2;
        }
    }

    public ZlibStream(Stream Stream, CompressionLevel level = CompressionLevel.Optimal) : base(Stream, level, true)
    {
        Mode = CompressionMode.Compress;
        byte[] header = new byte[2];
        header[0] = 0x78;

        switch (level)
        {
            case CompressionLevel.NoCompression:
                header[1] = 0x01;
                break;
            case CompressionLevel.Fastest:
                header[1] = 0x9c;
                break;
            case CompressionLevel.Optimal:
                header[1] = 0xDA;
                break;
        }

        BaseStream.Write(header, 0, 2);
    }

    public override void Write(byte[] array, int offset, int count)
    {
        base.Write(array, offset, count);
        adler32.Update(array, offset, count);
    }

    protected override void Dispose(bool disposing)
    {
        Stream baseStream = BaseStream;
        base.Dispose(disposing);

        if (Mode == CompressionMode.Compress)
        {
            byte[] checksum = BitConverter.GetBytes(adler32.Checksum);
            for (int i = 3; i >= 0; i--)
            {
                baseStream.WriteByte(checksum[i]);
            }
        }
    }
}

internal static class ZlibHelper
{
    public static byte[] Compress(byte[] input, CompressionLevel level = CompressionLevel.Optimal)
    {
        using MemoryStream output = new();
        using (ZlibStream zs = new(output, level))
        {
            zs.Write(input, 0, input.Length);
        }
        return output.ToArray();
    }

    public static byte[] Compress(string path, CompressionLevel level = CompressionLevel.Optimal)
    {
        using FileStream fs = File.OpenRead(path);
        using MemoryStream output = new();
        using (ZlibStream compressionStream = new(output, level))
        {
            fs.CopyTo(compressionStream);
        }
        return output.ToArray();
    }

    public static byte[] Decompress(byte[] input)
    {
        using MemoryStream compressed = new(input);
        using MemoryStream decompressed = new();
        using (ZlibStream decompressionStream = new(compressed, CompressionMode.Decompress))
        {
            decompressionStream.CopyTo(decompressed);
        }
        return decompressed.ToArray();
    }
}

internal static class DeflateHelper
{
    public static byte[] Compress(byte[] input, CompressionLevel level = CompressionLevel.Optimal)
    {
        using MemoryStream output = new();
        using (DeflateStream ds = new(output, level))
        {
            ds.Write(input, 0, input.Length);
        }
        return output.ToArray();
    }
}
