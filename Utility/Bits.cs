// File: Utility/Bits.cs
// Date: 2024/08/31
// Description: 按位读取字节数组中的数据
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
using System.Linq;

namespace Utility
{
    public class Bits
    {
        public static int ReadBits(byte[] byteArray, ref int bitPosition, int bitCount)
        {
            if (byteArray == null || bitPosition < 0 || bitCount <= 0)
            {
                throw new ArgumentException("Invalid input.");
            }

            int result = 0;

            for (int i = 0; i < bitCount; i++)
            {
                int currentBitPosition = bitPosition + i;
                int byteIndex = currentBitPosition / 8;
                int bitIndex = currentBitPosition % 8;

                //if (byteIndex >= byteArray.Length)
                //    throw new ArgumentOutOfRangeException("Bit position exceeds byte array length.");

                byte targetByte = byteArray[byteIndex];
                int mask = 1 << (7 - bitIndex);
                int bitValue = (targetByte & mask) != 0 ? 1 : 0;

                result = (result << 1) | bitValue;
            }
            bitPosition += bitCount;
            return result;
        }

    }
}
