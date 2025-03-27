using EasyCompressor;

namespace Utility.Compression
{
    public static class Snappy
    {
        public static byte[] Decompress(byte[] data)
        {
            ICompressor compressor = new SnappierCompressor();
            return compressor.Decompress(data);
        }

        public static byte[] Compress(byte[] data)
        {
            ICompressor compressor = new SnappierCompressor();
            return compressor.Compress(data);
        }
    }
}
