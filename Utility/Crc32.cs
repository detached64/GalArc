// File: Utility/Crc32.cs
// Date: 2024/09/04
// Description: Crc32 checksum algorithm.
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
    public static class Crc32
    {
        private const uint Polynomial = 0xedb88320;

        private static uint[] InitializeTable()
        {
            uint[] table = new uint[256];
            for (uint i = 0; i < 256; i++)
            {
                uint entry = i;
                for (int j = 0; j < 8; j++)
                {
                    if (0 != (entry & 1))
                    {
                        entry = (entry >> 1) ^ Polynomial;
                    }
                    else
                    {
                        entry >>= 1;
                    }
                }
                table[i] = entry;
            }
            return table;
        }

        public static uint Calculate(byte[] data)
        {
            uint[] table = InitializeTable();
            uint crc = 0xffffffff;
            for (int i = 0; i < data.Length; i++)
            {
                crc = (crc >> 8) ^ table[(crc & 0xff) ^ data[i]];
            }

            return ~crc;
        }
    }
}
