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

using Log;
using System;
using System.Collections.Generic;

namespace Utility.Compression
{
    public class Huffman
    {
        private static ushort token = 256;
        private const ushort max = 512;
        private static int offset = 0;

        private static ushort[] lct = new ushort[max];  // left child tree
        private static ushort[] rct = new ushort[max];  // right child tree

        private static byte[] data;

        private static void Reset()
        {
            token = 256;
            offset = 0;
            lct = new ushort[max];
            rct = new ushort[max];
        }

        private static ushort CreateTree()
        {
            int bit = Bits.ReadBits(data, ref offset, 1);
            switch (bit)
            {
                case 0:
                    return (ushort)Bits.ReadBits(data, ref offset, 8);
                case 1:
                    ushort v = token++;
                    if (v >= max)
                    {
                        LogUtility.Error("Exceeded huffman tree.");
                    }
                    lct[v] = CreateTree();
                    rct[v] = CreateTree();
                    return v;
                default:
                    throw new Exception("Invalid bit.");
            }
        }

        public static byte[] Decompress(byte[] Data, int decompressedLen)
        {
            Reset();
            data = Data;
            ushort root = CreateTree();

            List<byte> decompressed = new List<byte>();

            while (true)
            {
                ushort value = root;
                while (value >= 256)
                {
                    int bit = Bits.ReadBits(data, ref offset, 1);
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

                if (decompressed.Count == decompressedLen)
                {
                    return decompressed.ToArray();
                }
            }
        }
    }
}
