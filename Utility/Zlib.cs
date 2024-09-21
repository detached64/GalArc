// File: Utility/Zlib.cs
// Date: 2024/08/26
// Description: 基于arcusmaximus的相关代码，对Zlib的压缩和解压缩进行了封装
//
// Copyright (c) 2018 arcusmaximus
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

using Log;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Utility.Compression
{
    /// <summary>
    /// Full zlib compression&decompression method.
    /// </summary>
    public class ZlibCompressStream : DeflateStream
    {
        private readonly Adler32 _adler32 = new Adler32();

        public ZlibCompressStream(Stream Stream, CompressionLevel level = CompressionLevel.Optimal)
            : base(Stream, level, true)
        {
            byte[] header = new byte[2];
            header[0] = 0x78;

            if (level == CompressionLevel.NoCompression)
            {
                header[1] = 0x01;
            }
            else if (level == CompressionLevel.Fastest)
            {
                header[1] = 0x9C;
            }
            else
            {
                header[1] = 0xDA;
            }

            BaseStream.Write(header, 0, 2);
        }

        public override void Write(byte[] array, int offset, int count)
        {
            base.Write(array, offset, count);
            _adler32.Update(array, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            Stream baseStream = BaseStream;
            base.Dispose(disposing);

            byte[] checksum = BitConverter.GetBytes(_adler32.Checksum);
            for (int i = 3; i >= 0; i--)
            {
                baseStream.WriteByte(checksum[i]);
            }
        }
    }

    public class Zlib
    {
        /// <summary>
        /// Compress file and append it to given Stream. Used for continuous compressed files.
        /// </summary>
        /// <param name="Stream"></param>
        /// <param name="inputFilePath"></param>
        /// <param name="originalSize"></param>
        /// <param name="compressedSize"></param>
        public static void AppendCompressedFile(Stream Stream, string inputFilePath, out long originalSize, out long compressedSize, CompressionLevel compressionLevel = CompressionLevel.Optimal)
        {
            using (Stream fileStream = File.OpenRead(inputFilePath))
            {
                originalSize = fileStream.Length;
                long startPos = Stream.Position;
                using (ZlibCompressStream compressionStream = new ZlibCompressStream(Stream, compressionLevel))
                {
                    fileStream.CopyTo(compressionStream);
                }

                compressedSize = Stream.Position - startPos;
            }
        }

        /// <summary>
        /// Compress byte array.
        /// </summary>
        /// <param name="inputData"></param>
        /// <returns></returns>
        public static byte[] CompressBytes(byte[] inputData, CompressionLevel compressionLevel = CompressionLevel.Optimal)
        {
            using (MemoryStream outputStream = new MemoryStream())
            {
                using (MemoryStream fileStream = new MemoryStream(inputData))
                {
                    using (ZlibCompressStream compressionStream = new ZlibCompressStream(outputStream, compressionLevel))
                    {
                        fileStream.CopyTo(compressionStream);
                    }
                    return outputStream.ToArray();
                }
            }
        }

        /// <summary>
        /// Compress one file.
        /// </summary>
        /// <param name="inputFilePath"></param>
        /// <returns></returns>
        public static byte[] CompressFile(string inputFilePath, CompressionLevel compressionLevel = CompressionLevel.Optimal)
        {
            using (MemoryStream outputStream = new MemoryStream())
            {
                using (FileStream fileStream = File.OpenRead(inputFilePath))
                {
                    using (ZlibCompressStream compressionStream = new ZlibCompressStream(outputStream, compressionLevel))
                    {
                        fileStream.CopyTo(compressionStream);
                    }
                    return outputStream.ToArray();
                }
            }
        }

        /// <summary>
        /// Decompress byte array.
        /// </summary>
        /// <param name="inputData"></param>
        /// <returns></returns>
        public static byte[] DecompressBytes(byte[] inputData)
        {
            using (MemoryStream inputStream = new MemoryStream(inputData))
            using (BinaryReader br = new BinaryReader(inputStream))
            {
                byte magic1 = br.ReadByte();
                byte magic2 = br.ReadByte();
                using (MemoryStream outputStream = new MemoryStream())
                {
                    if (magic1 != 0x78 || (magic2 != 0x01 && magic2 != 0x9c && magic2 != 0xda))
                    {
                        //raw deflate
                        inputStream.Position -= 2;
                        try
                        {
                            using (DeflateStream decompressed = new DeflateStream(inputStream, CompressionMode.Decompress, true))
                            {
                                decompressed.CopyTo(outputStream);
                            }
                        }
                        catch (Exception e)
                        {
                            LogUtility.Error(e.Message);
                        }
                    }
                    else
                    {
                        //zlib
                        inputStream.SetLength(inputStream.Length - 4);          //remove checksum
                        using (DeflateStream decompressed = new DeflateStream(inputStream, CompressionMode.Decompress, true))
                        {
                            decompressed.CopyTo(outputStream);
                        }
                    }
                    return outputStream.ToArray();
                }
            }
        }
    }
}
