// File: Utility/Binary.cs
// Date: 2024/09/20
// Description: Binary utility functions.
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

namespace Utility
{
    public static class Binary
    {
        public static uint RotL(uint v, int count)
        {
            count &= 0x1F;
            return v << count | v >> (32 - count);
        }

        public static uint RotR(uint v, int count)
        {
            count &= 0x1F;
            return v >> count | v << (32 - count);
        }

        public static byte RotByteL(byte v, int count)
        {
            count &= 7;
            return (byte)(v << count | v >> (8 - count));
        }

        public static byte RotByteR(byte v, int count)
        {
            count &= 7;
            return (byte)(v >> count | v << (8 - count));
        }

        /// <summary>
        /// When src and dst overlap and uncopied src is updated by dst in the process, this method copies updated bytes instead of original ones.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="count"></param>
        public static void CopyOverlapped(byte[] data, int src, int dst, int count)
        {
            if (dst > src)
            {
                while (count > 0)
                {
                    int preceding = Math.Min(dst - src, count);
                    Buffer.BlockCopy(data, src, data, dst, preceding);
                    dst += preceding;
                    count -= preceding;
                }
            }
            else
            {
                Buffer.BlockCopy(data, src, data, dst, count);
            }
        }

    }
}
