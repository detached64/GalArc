// File: Utility/HuffmanCompression.cs
// Date: 2024/11/28
// Description: Standard Huffman decompression algorithm.
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
    public class HuffmanCompression : IDisposable
    {
        private const ushort MAX = 512;
        private ushort index = 256;

        private ushort[,] children = new ushort[MAX, 2];

        private BitStream input;
        private CompressionMode mode;
        private int decompressedLength;
        private MemoryStream output;

        public HuffmanCompression(Stream input, CompressionMode mode, int length)
        {
            if (mode == CompressionMode.Decompress)
            {
                this.input = new BitStream(input, BitStreamMode.Read);
                decompressedLength = length;
            }
            else
            {
                this.input = new BitStream(input, BitStreamMode.Write);
            }
            this.mode = mode;
        }

        public HuffmanCompression(Stream input, int length) : this(input, CompressionMode.Decompress, length)
        { }

        public HuffmanCompression(Stream input) : this(input, CompressionMode.Compress, 0)
        { }

        private ushort CreateTree()
        {
            switch (input.ReadBit())
            {
                case 0:
                    return (ushort)input.ReadBits(8);
                case 1:
                    ushort parent = index++;
                    if (parent >= MAX)
                    {
                        throw new Exception("Exceeded huffman tree.");
                    }
                    children[parent, 0] = CreateTree();
                    children[parent, 1] = CreateTree();
                    return parent;
                default:
                    throw new Exception("Invalid bit.");
            }
        }

        public byte[] Decompress()
        {
            if (mode != CompressionMode.Decompress)
            {
                throw new InvalidOperationException("Not in decompression mode.");
            }

            ushort root = CreateTree();
            output = new MemoryStream();

            while (true)
            {
                ushort value = root;
                while (value >= 256)
                {
                    int bit = input.ReadBit();
                    if (bit != -1)
                    {
                        value = children[value, bit];
                    }
                }
                output.WriteByte((byte)value);

                if (output.Length == decompressedLength)
                {
                    return output.ToArray();
                }
            }
        }

        public void Dispose()
        {
            input.Dispose();
            output.Dispose();
        }
    }

    public static class HuffmanDecoder
    {
        public static byte[] Decompress(byte[] input, int length)
        {
            using (MemoryStream stream = new MemoryStream(input))
            {
                using (HuffmanCompression huffman = new HuffmanCompression(stream, length))
                {
                    return huffman.Decompress();
                }
            }
        }
    }
}
