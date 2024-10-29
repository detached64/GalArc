using System;
using System.IO;

namespace Utility
{
    public class Lz
    {
        public static byte[] Decompress(byte[] data)
        {
            using (MemoryStream input = new MemoryStream(data))
            {
                int unpacked_size = ReadInt32(input);
                var output = new byte[unpacked_size];
                int dst = 0;
                while (dst < unpacked_size)
                {
                    int ctl = input.ReadByte();
                    if (-1 == ctl)
                    {
                        throw new EndOfStreamException();
                    }

                    if (0 != (ctl & 0x80))
                    {
                        byte lo = (byte)input.ReadByte();
                        int offset = ((ctl << 3 | lo >> 5) & 0x3FF) + 1;
                        int count = (lo & 0x1F) + 1;
                        Binary.CopyOverlapped(output, dst - offset, dst, count);
                        dst += count;
                    }
                    else
                    {
                        int count = ctl + 1;
                        if (input.Read(output, dst, count) != count)
                        {
                            throw new EndOfStreamException();
                        }

                        dst += count;
                    }
                }
                return output;
            }
        }
        private static int ReadInt32(Stream stream)
        {
            byte[] buffer = new byte[4];
            if (stream.Read(buffer, 0, 4) != 4)
                throw new EndOfStreamException();
            return BitConverter.ToInt32(buffer, 0);
        }
    }
}
