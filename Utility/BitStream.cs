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
    public class BitStream : IDisposable
    {
        private readonly Stream _stream;
        private byte _currentByte;
        private int _bitPosition;
        private readonly bool _isMsb;

        public BitStream(Stream stream, bool isMsb = true)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            _isMsb = isMsb;
            _bitPosition = 8;
        }

        public int ReadBits(int bitCount)
        {
            if (bitCount < 1 || bitCount > 32)
                throw new ArgumentOutOfRangeException(nameof(bitCount), "Bit count must be between 1 and 32.");

            int result = 0;

            for (int i = 0; i < bitCount; i++)
            {
                if (_bitPosition == 8)
                {
                    int readByte = _stream.ReadByte();
                    if (readByte == -1)
                        throw new EndOfStreamException("End of stream.");

                    _currentByte = (byte)readByte;
                    _bitPosition = 0;
                }

                int bit;
                if (_isMsb)
                {
                    bit = (_currentByte >> (7 - _bitPosition)) & 1;
                }
                else
                {
                    bit = (_currentByte >> _bitPosition) & 1;
                }

                result = (result << 1) | bit;
                _bitPosition++;
            }

            return result;
        }

        public int ReadBit()
        {
            return ReadBits(1);
        }

        public void Dispose()
        {
            _currentByte = 0;
            _bitPosition = 0;
            _stream?.Dispose();
        }
    }

}
