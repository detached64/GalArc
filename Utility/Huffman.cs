// File: Utility/Huffman.cs
// Date: 2024/08/31
// Description: 对Huffman的解压缩进行封装
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

using GalArc.Logs;
using System;
using System.Collections.Generic;
using System.IO;

namespace Utility.Compression
{
    public class Huffman : IDisposable
    {
        private ushort token = 256;
        private const ushort max = 512;

        private ushort[] lct = new ushort[max];  // left child tree
        private ushort[] rct = new ushort[max];  // right child tree

        private Stream m_input;
        private BitStream m_bit_stream;
        private int decompressedLength;

        public Huffman(byte[] input, int length)
        {
            m_input = new MemoryStream(input);
            m_bit_stream = new BitStream(m_input);
            decompressedLength = length;
        }

        private ushort CreateTree()
        {
            int bit = m_bit_stream.ReadBit();
            switch (bit)
            {
                case 0:
                    return (ushort)m_bit_stream.ReadBits(8);
                case 1:
                    ushort v = token++;
                    if (v >= max)
                    {
                        Logger.Error("Exceeded huffman tree.");
                    }
                    lct[v] = CreateTree();
                    rct[v] = CreateTree();
                    return v;
                default:
                    throw new Exception("Invalid bit.");
            }
        }

        public byte[] Decompress()
        {
            ushort root = CreateTree();
            List<byte> decompressed = new List<byte>();

            while (true)
            {
                ushort value = root;
                while (value >= 256)
                {
                    int bit = m_bit_stream.ReadBit();
                    switch (bit)
                    {
                        case 0:
                            value = lct[value];
                            break;
                        case 1:
                            value = rct[value];
                            break;
                        case -1:
                            break;
                    }
                }
                decompressed.Add((byte)value);

                if (decompressed.Count == decompressedLength)
                {
                    return decompressed.ToArray();
                }
            }
        }

        public void Dispose()
        {
            m_input.Dispose();
            m_bit_stream.Dispose();
        }
    }

    public class HuffmanDecoder
    {
        public static byte[] Decompress(byte[] input, int length)
        {
            using (Huffman huffman = new Huffman(input, length))
            {
                return huffman.Decompress();
            }
        }
    }
}
