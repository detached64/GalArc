// File: Utility/ZlibStream.cs
// Date: 2024/12/02
// Description: Standard Zlib compression method based on DeflateStream.
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
using System.IO;
using System.IO.Compression;

namespace Utility.Compression
{
    public class ZlibStream : DeflateStream
    {
        private CompressionMode mode;

        private readonly Adler32 adler32 = new Adler32();

        public ZlibStream(Stream stream, CompressionMode mode)
            : base(stream, mode, true)
        {
            this.mode = mode;
            byte m1 = (byte)stream.ReadByte();
            byte m2 = (byte)stream.ReadByte();
            if (m1 != 0x78 || (m2 != 0x01 && m2 != 0x9c && m2 != 0xda))
            {
                stream.Position -= 2;
            }
        }

        public ZlibStream(Stream Stream, CompressionLevel level = CompressionLevel.Optimal)
            : base(Stream, level, true)
        {
            this.mode = CompressionMode.Compress;
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

        public ZlibStream(Stream stream, CompressionMode mode, CompressionLevel level)
            : this(stream, level)
        { }

        public override void Write(byte[] array, int offset, int count)
        {
            base.Write(array, offset, count);
            adler32.Update(array, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            Stream baseStream = BaseStream;
            base.Dispose(disposing);

            if (mode == CompressionMode.Compress)
            {
                byte[] checksum = BitConverter.GetBytes(adler32.Checksum);
                for (int i = 3; i >= 0; i--)
                {
                    baseStream.WriteByte(checksum[i]);
                }
            }
        }
    }

    public static class ZlibHelper
    {
        public static byte[] Compress(byte[] input, CompressionLevel level = CompressionLevel.Optimal)
        {
            using (MemoryStream output = new MemoryStream())
            {
                using (ZlibStream zs = new ZlibStream(output, level))
                {
                    zs.Write(input, 0, input.Length);
                }
                return output.ToArray();
            }
        }

        public static byte[] Compress(string path, CompressionLevel level = CompressionLevel.Optimal)
        {
            using (FileStream fs = File.OpenRead(path))
            {
                using (MemoryStream output = new MemoryStream())
                {
                    using (ZlibStream compressionStream = new ZlibStream(output, level))
                    {
                        fs.CopyTo(compressionStream);
                    }
                    return output.ToArray();
                }
            }
        }

        public static byte[] Decompress(byte[] input)
        {
            using (MemoryStream compressed = new MemoryStream(input))
            {
                using (MemoryStream decompressed = new MemoryStream())
                {
                    using (ZlibStream decompressionStream = new ZlibStream(compressed, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressed);
                    }
                    return decompressed.ToArray();
                }
            }
        }
    }
}
