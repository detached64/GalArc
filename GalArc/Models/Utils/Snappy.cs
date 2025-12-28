namespace GalArc.Models.Utils;

internal static class Snappy
{
    public static byte[] Decompress(byte[] data)
    {
        return Snappier.Snappy.DecompressToArray(data);
    }

    public static byte[] Compress(byte[] data)
    {
        return Snappier.Snappy.CompressToArray(data);
    }
}
