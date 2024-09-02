// File: Utility/Lzss.cs
// Date: 2024/07/28
// Description: 对Lzss的解压缩进行封装；压缩Lzss暂未实现
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
using System.Linq;

namespace Utility.Compression
{
    public class Lzss
    {
        public static MemoryStream Decompress(MemoryStream ms)
        {
            const int N = 4096;
            const int F = 18;
            const int THRESHOLD = 2;
            int[] lzBuffer = new int[N + F - 1];
            int r = N - F;
            int flags = 0;

            MemoryStream output = new MemoryStream();
            int bufferIndex = 0;

            while (true)
            {
                if (((flags >>= 1) & 256) == 0)
                {
                    if (bufferIndex >= ms.Length)
                    {
                        break;
                    }

                    flags = ms.ReadByte() | 0xff00;
                    bufferIndex++;
                }

                if ((flags & 1) != 0)
                {
                    if (bufferIndex >= ms.Length)
                    {
                        break;
                    }

                    int c = ms.ReadByte();
                    bufferIndex++;
                    output.WriteByte((byte)c);
                    lzBuffer[r++] = c;
                    r &= (N - 1);
                }
                else
                {
                    if (bufferIndex >= ms.Length)
                    {
                        break;
                    }

                    int i = ms.ReadByte();
                    bufferIndex++;
                    if (bufferIndex >= ms.Length)
                    {
                        break;
                    }

                    int j = ms.ReadByte();
                    bufferIndex++;
                    i |= ((j & 0xf0) << 4);
                    j = (j & 0x0f) + THRESHOLD;

                    for (int k = 0; k <= j; k++)
                    {
                        int c = lzBuffer[(i + k) & (N - 1)];
                        output.WriteByte((byte)c);
                        lzBuffer[r++] = c;
                        r &= (N - 1);
                    }
                }
            }
            return output;
        }

        public static byte[] Decompress(byte[] data)
        {
            const int N = 4096;
            const int F = 18;
            const int THRESHOLD = 2;
            int[] lzBuffer = new int[N + F - 1];
            int r = N - F;
            int flags = 0;

            MemoryStream ms = new MemoryStream(data);
            MemoryStream output = new MemoryStream();
            int bufferIndex = 0;

            while (true)
            {
                if (((flags >>= 1) & 256) == 0)
                {
                    if (bufferIndex >= ms.Length)
                    {
                        break;
                    }

                    flags = ms.ReadByte() | 0xff00;
                    bufferIndex++;
                }

                if ((flags & 1) != 0)
                {
                    if (bufferIndex >= ms.Length)
                    {
                        break;
                    }

                    int c = ms.ReadByte();
                    bufferIndex++;
                    output.WriteByte((byte)c);
                    lzBuffer[r++] = c;
                    r &= (N - 1);
                }
                else
                {
                    if (bufferIndex >= ms.Length)
                    {
                        break;
                    }

                    int i = ms.ReadByte();
                    bufferIndex++;
                    if (bufferIndex >= ms.Length)
                    {
                        break;
                    }

                    int j = ms.ReadByte();
                    bufferIndex++;
                    i |= ((j & 0xf0) << 4);
                    j = (j & 0x0f) + THRESHOLD;

                    for (int k = 0; k <= j; k++)
                    {
                        int c = lzBuffer[(i + k) & (N - 1)];
                        output.WriteByte((byte)c);
                        lzBuffer[r++] = c;
                        r &= (N - 1);
                    }
                }
            }
            return output.ToArray();
        }

    }
}
