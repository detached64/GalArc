// File: Utility/Xor.cs
// Date: 2024/08/26
// Description: xor封装
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

namespace Utility
{
    public class Xor
    {
        public static byte[] xor(byte[] data, byte[] key)
        {
            byte[] result = new byte[data.Length];
            if (key.Length == 1)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    result[i] = (byte)(data[i] ^ key[0]);
                }
            }
            else
            {
                for (int i = 0; i < data.Length; i++)
                {
                    result[i] = (byte)(data[i] ^ key[i % key.Length]);
                }
            }
            data = null;
            return result;
        }

        public static byte[] xor(byte[] data, byte key)
        {
            byte[] result = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (byte)(data[i] ^ key);
            }
            data = null;
            return result;
        }
    }
}
