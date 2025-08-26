using System.IO;
using ZstdSharp;

namespace GalArc.Models.Utils;

internal static class Zstd
{
    public static byte[] Decompress(byte[] data)
    {
        using MemoryStream input = new(data);
        using MemoryStream output = new();
        using DecompressionStream dec = new(input);
        dec.CopyTo(output);
        return output.ToArray();
    }

    public static byte[] Compress(byte[] data)
    {
        using MemoryStream input = new(data);
        using MemoryStream output = new();
        using CompressionStream compressionStream = new(output);
        input.CopyTo(compressionStream);
        return output.ToArray();
    }
}
