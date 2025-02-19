using System;
using System.IO;
using Utility;

namespace ArcFormats.Seraph
{
    internal static class SeraphUtils
    {
        public static byte[] LzDecompress(byte[] data)
        {
            uint unpacked_size = BitConverter.ToUInt32(data, 0);
            byte[] output = new byte[unpacked_size];
            int src = 4;
            int dst = 0;
            while (dst < unpacked_size)
            {
                if (src >= data.Length)
                {
                    throw new InvalidDataException("Unexpected end of input");
                }
                byte ctl = data[src++];
                if ((ctl & 0x80) != 0)
                {
                    ushort param = (ushort)(ctl << 8 | data[src++]);
                    int offset = ((param >> 5) & 0x3FF) + 1;
                    int count = (param & 0x1F) + 1;
                    if (dst - offset < 0 || dst - offset + count > output.Length)
                    {
                        throw new InvalidDataException("Invalid backreference");
                    }
                    Binary.CopyOverlapped(output, dst - offset, dst, count);
                    dst += count;
                }
                else
                {
                    int count = (ctl & 0x7F) + 1;
                    if (src + count > data.Length)
                    {
                        throw new InvalidDataException("Unexpected end of input");
                    }
                    Buffer.BlockCopy(data, src, output, dst, count);
                    dst += count;
                    src += count;
                }
            }
            if (dst != unpacked_size)
            {
                throw new InvalidDataException("Output size mismatch");
            }
            return output;
        }
    }
}
