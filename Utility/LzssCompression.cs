// File: Utility/LzssCompression.cs
// Date: 2024/11/29
// Description: C# port of the Lzss compression algorithm, based on
// the original code by Haruhiko Okumura, 1989, licensed under MIT License.
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
    public class LzssSettings
    {
        /// <summary>
        /// The size of the sliding window.
        /// </summary>
        /// "N" in the original code. Original value is 0x1000.
        public int FrameSize { get; set; }
        /// <summary>
        /// The value used to fill the sliding window.
        /// </summary>
        /// Original value is 0x20.
        public byte FrameFill { get; set; }
        /// <summary>
        /// The initial position of the sliding window.
        /// </summary>
        /// "N-F" in the original code. Original value is 0xFEE.
        public int FrameInitPos { get; set; }
        /// <summary>
        /// Maximum match length.
        /// </summary>
        /// "F" in the original code. Original value is 0x12.
        public int MaxMatchLength { get; set; }
        /// <summary>
        /// Minimum match length.
        /// </summary>
        /// "THRESHOLD" in the original code. Original value is 2.
        public int MinMatchLength { get; set; }
    }

    public class LzssCompression : IDisposable
    {
        private Stream input;
        private CompressionMode mode;
        private LzssSettings settings;
        private MemoryStream output;

        public LzssCompression(Stream input, CompressionMode mode, LzssSettings settings)
        {
            this.input = input;
            this.mode = mode;
            this.settings = settings;
            this.output = new MemoryStream();
            this.buffer = new byte[settings.FrameSize + settings.MaxMatchLength - 1];
            if (settings.FrameFill != 0)
            {
                for (int j = 0; j < settings.FrameSize; ++j)
                {
                    buffer[j] = settings.FrameFill;
                }
            }
            if (mode == CompressionMode.Compress)
            {
                InitCompress();
            }
        }

        public LzssCompression(Stream input, CompressionMode mode)
            : this(input, mode, new LzssSettings
            {
                FrameSize = 0x1000,
                FrameFill = 0,
                FrameInitPos = 0xFEE,
                MaxMatchLength = 0x12,
                MinMatchLength = 2
            })
        { }

        public LzssCompression(Stream input) : this(input, CompressionMode.Decompress)
        { }

        private byte[] buffer;

        public byte[] Decompress()
        {
            if (mode != CompressionMode.Decompress)
            {
                throw new InvalidOperationException("Not in decompression mode");
            }

            uint flag = 0;
            int byteRead, distance, length;
            while (true)
            {
                if (((flag >>= 1) & 256) == 0)
                {
                    if ((byteRead = input.ReadByte()) == -1)
                    {
                        break;
                    }
                    flag = (uint)(byteRead | 0xff00);
                }

                if ((flag & 1) != 0)
                {
                    if ((byteRead = input.ReadByte()) == -1)
                    {
                        break;
                    }
                    output.WriteByte((byte)byteRead);
                    buffer[settings.FrameInitPos++] = (byte)byteRead;
                    settings.FrameInitPos &= settings.FrameSize - 1;
                }
                else
                {
                    if ((distance = input.ReadByte()) == -1)
                    {
                        break;
                    }
                    if ((length = input.ReadByte()) == -1)
                    {
                        break;
                    }
                    distance |= (length & 0xf0) << 4;
                    length = (length & 0x0f) + settings.MinMatchLength;

                    for (int k = 0; k <= length; k++)
                    {
                        byteRead = buffer[(distance + k) & (settings.FrameSize - 1)];
                        output.WriteByte((byte)byteRead);
                        buffer[settings.FrameInitPos++] = (byte)byteRead;
                        settings.FrameInitPos &= settings.FrameSize - 1;
                    }
                }
            }
            return output.ToArray();
        }

        private int[] lc;           // left children
        private int[] rc;           // right children
        private int[] parents;
        private int matchLength;
        private int matchPosition;

        private void InitCompress()
        {
            lc = new int[settings.FrameSize + 1];
            rc = new int[settings.FrameSize + 257];
            parents = new int[settings.FrameSize + 1];
            matchLength = 0;
            matchPosition = 0;
        }

        private void InitTree()
        {
            for (int i = settings.FrameSize + 1; i <= settings.FrameSize + 256; i++)
            {
                rc[i] = settings.FrameSize;
            }
            for (int i = 0; i < settings.FrameSize; i++)
            {
                parents[i] = settings.FrameSize;
            }
        }

        private void InsertNode(int r)
        {
            int i = 0;
            int p = settings.FrameSize + 1 + buffer[r];
            int cmp = 1;
            rc[r] = lc[r] = settings.FrameSize;
            matchLength = 0;

            while (true)
            {
                if (cmp >= 0)
                {
                    if (rc[p] != settings.FrameSize)
                    {
                        p = rc[p];
                    }
                    else
                    {
                        rc[p] = r;
                        parents[r] = p;
                        return;
                    }
                }
                else
                {
                    if (lc[p] != settings.FrameSize)
                    {
                        p = lc[p];
                    }
                    else
                    {
                        lc[p] = r;
                        parents[r] = p;
                        return;
                    }
                }

                for (i = 1; i < settings.MaxMatchLength; i++)
                {
                    if ((cmp = buffer[r + i] - buffer[p + i]) != 0)
                    {
                        break;
                    }
                }

                if (i > matchLength)
                {
                    matchPosition = p;
                    if ((matchLength = i) >= settings.MaxMatchLength)
                    {
                        break;
                    }
                }
            }
            parents[r] = parents[p];
            lc[r] = lc[p];
            rc[r] = rc[p];
            parents[lc[p]] = r;
            parents[rc[p]] = r;
            if (rc[parents[p]] == p)
            {
                rc[parents[p]] = r;
            }
            else
            {
                lc[parents[p]] = r;
            }
            parents[p] = settings.FrameSize;
        }

        private void DeleteNode(int p)
        {
            int q = 0;
            if (parents[p] == settings.FrameSize)
            {
                return;
            }
            if (rc[p] == settings.FrameSize)
            {
                q = lc[p];
            }
            else if (lc[p] == settings.FrameSize)
            {
                q = rc[p];
            }
            else
            {
                q = lc[p];
                if (rc[q] != settings.FrameSize)
                {
                    do
                    {
                        q = rc[q];
                    } while (rc[q] != settings.FrameSize);
                    rc[parents[q]] = lc[q];
                    parents[lc[q]] = parents[q];
                    lc[q] = lc[p];
                    parents[lc[p]] = q;
                }
                rc[q] = rc[p];
                parents[rc[p]] = q;
            }
            parents[q] = parents[p];
            if (rc[parents[p]] == p)
            {
                rc[parents[p]] = q;
            }
            else
            {
                lc[parents[p]] = q;
            }
            parents[p] = settings.FrameSize;
        }

        public byte[] Compress()
        {
            if (mode != CompressionMode.Compress)
            {
                throw new InvalidOperationException("Not in compression mode");
            }

            int r = settings.FrameInitPos;
            int s = 0;
            int len = 0;
            int i;
            int c, lastMatchLength, codeBufPtr;
            byte[] codeBuf = new byte[17];
            byte mask;

            InitTree();
            codeBuf[0] = 0;
            codeBufPtr = mask = 1;

            for (len = 0; len < settings.MaxMatchLength && (c = input.ReadByte()) != -1; len++)
            {
                buffer[r + len] = (byte)c;
            }
            if (len == 0)
            {
                return new byte[0];
            }
            for (i = 1; i <= settings.MaxMatchLength; i++)
            {
                InsertNode(r - i);
            }
            InsertNode(r);
            do
            {
                if (matchLength > len)
                {
                    matchLength = len;
                }
                if (matchLength <= settings.MinMatchLength)
                {
                    matchLength = 1;
                    codeBuf[0] |= mask;
                    codeBuf[codeBufPtr++] = buffer[r];
                }
                else
                {
                    codeBuf[codeBufPtr++] = (byte)matchPosition;
                    codeBuf[codeBufPtr++] = (byte)(((matchPosition >> 4) & 0xf0) | (matchLength - (settings.MinMatchLength + 1)));
                }
                if ((mask <<= 1) == 0)
                {
                    for (i = 0; i < codeBufPtr; i++)
                    {
                        output.WriteByte(codeBuf[i]);
                    }
                    codeBuf[0] = 0;
                    codeBufPtr = mask = 1;
                }
                lastMatchLength = matchLength;
                for (i = 0; i < lastMatchLength && (c = input.ReadByte()) != -1; i++)
                {
                    DeleteNode(s);
                    buffer[s] = (byte)c;
                    if (s < settings.MaxMatchLength - 1)
                    {
                        buffer[s + settings.FrameSize] = (byte)c;
                    }
                    s = (s + 1) & (settings.FrameSize - 1);
                    r = (r + 1) & (settings.FrameSize - 1);
                    InsertNode(r);
                }
                while (i++ < lastMatchLength)
                {
                    DeleteNode(s);
                    s = (s + 1) & (settings.FrameSize - 1);
                    r = (r + 1) & (settings.FrameSize - 1);
                    if (--len != 0)
                    {
                        InsertNode(r);
                    }
                }
            }
            while (len > 0);
            if (codeBufPtr > 1)
            {
                for (i = 0; i < codeBufPtr; i++)
                {
                    output.WriteByte(codeBuf[i]);
                }
            }
            return output.ToArray();
        }

        public void Dispose()
        {
            input.Dispose();
            output.Dispose();
        }
    }

    public static class LzssHelper
    {
        public static byte[] Decompress(byte[] input)
        {
            using (MemoryStream ms = new MemoryStream(input))
            {
                using (LzssCompression lzss = new LzssCompression(ms))
                {
                    return lzss.Decompress();
                }
            }
        }

        public static byte[] Compress(byte[] input)
        {
            using (MemoryStream ms = new MemoryStream(input))
            {
                using (LzssCompression lzss = new LzssCompression(ms, CompressionMode.Compress))
                {
                    return lzss.Compress();
                }
            }
        }
    }
}
