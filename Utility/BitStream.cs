// File: Utility/BitStream.cs
// Date: 2024/11/27
// Description: Bit stream.
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

namespace Utility
{
    public enum BitStreamMode
    {
        Read,
        Write
    }

    public enum BitStreamEndianness
    {
        Msb,
        Lsb
    }

    public class BitStream : IDisposable
    {
        private readonly Stream stream;
        private readonly BitStreamEndianness endianness;
        private readonly BitStreamMode mode;

        private byte currentByte;
        /// <summary>
        /// Bit position in the current byte.
        /// </summary>
        /// If bitPosition is 8, we need to read a new byte.
        private int bitPosition;

        public Stream BaseStream => stream;

        public BitStream(Stream stream, BitStreamMode mode, BitStreamEndianness endianness)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            this.mode = mode;
            this.endianness = endianness;
            currentByte = 0;
            bitPosition = mode == BitStreamMode.Write ? 0 : 8;

            if (mode == BitStreamMode.Read && !stream.CanRead)
            {
                throw new ArgumentException("Stream must be readable for Read mode.", nameof(stream));
            }
            if (mode == BitStreamMode.Write && !stream.CanWrite)
            {
                throw new ArgumentException("Stream must be writable for Write mode.", nameof(stream));
            }
        }

        public BitStream(Stream stream, BitStreamMode mode) : this(stream, mode, BitStreamEndianness.Msb)
        {
        }

        public int ReadBits(int bitCount)
        {
            if (mode != BitStreamMode.Read)
            {
                throw new InvalidOperationException("Stream is not in Read mode.");
            }
            if (bitCount < 1 || bitCount > 32)
            {
                throw new ArgumentOutOfRangeException(nameof(bitCount), "Bit count must be between 1 and 32.");
            }

            int result = 0;

            for (int i = 0; i < bitCount; i++)
            {
                if (bitPosition == 8)
                {
                    int readByte = stream.ReadByte();
                    if (readByte == -1)
                    {
                        throw new EndOfStreamException("Reached end of stream.");
                    }

                    currentByte = (byte)readByte;
                    bitPosition = 0;
                }

                int bit;
                if (endianness == BitStreamEndianness.Msb)
                {
                    bit = (currentByte >> (7 - bitPosition)) & 1;
                }
                else
                {
                    bit = (currentByte >> bitPosition) & 1;
                }

                result = (result << 1) | bit;
                bitPosition++;
            }

            return result;
        }

        public int ReadBit()
        {
            return ReadBits(1);
        }

        public void WriteBit(int bit)
        {
            if (mode != BitStreamMode.Write)
            {
                throw new InvalidOperationException("Stream is not in Write mode.");
            }
            if (bit != 0 && bit != 1)
            {
                throw new ArgumentOutOfRangeException(nameof(bit), "Bit must be 0 or 1.");
            }

            if (endianness == BitStreamEndianness.Msb)
            {
                currentByte = (byte)((currentByte & ~(1 << (7 - bitPosition))) | (bit << (7 - bitPosition)));
            }
            else
            {
                currentByte = (byte)((currentByte & ~(1 << bitPosition)) | (bit << bitPosition));
            }
            bitPosition++;

            if (bitPosition == 8)
            {
                stream.WriteByte(currentByte);
                currentByte = 0;
                bitPosition = 0;
            }
        }

        public void WriteBits(byte[] src, int bitCount)
        {
            if (mode != BitStreamMode.Write)
            {
                throw new InvalidOperationException("Stream is not in Write mode.");
            }
            if (endianness != BitStreamEndianness.Msb)
            {
                throw new NotImplementedException();
            }
            if (bitCount < 1 || bitCount > 32)
            {
                throw new ArgumentOutOfRangeException(nameof(bitCount), "Bit count must be between 1 and 32.");
            }

            int byteCount = bitCount / 8;
            int thisBitCount = bitCount % 8;
            for (int i = 0; i < thisBitCount; i++)
            {
                WriteBit((src[byteCount] >> (7 - (i % 8))) & 1);
            }
            byteCount--;
            thisBitCount = bitCount & ~7;
            for (int i = 0; i < thisBitCount; i++)
            {
                WriteBit((src[byteCount - i / 8] >> (7 - (i % 8))) & 1);
            }
        }

        public void Dispose()
        {
            if (mode == BitStreamMode.Write && bitPosition != 0)
            {
                stream.WriteByte(currentByte);
            }
            currentByte = 0;
            bitPosition = 0;
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                stream.Dispose();
            }
        }
    }
}
