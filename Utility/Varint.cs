// File: Utility/Varint.cs
// Date: 2024/07/20
// Description: 对Varint变长编码的打包和解包进行封装
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Utility
{
    public class Varint
    {
        public static uint UnpackUint(BinaryReader br)
        {
            uint value = 0;
            while ((value & 1) == 0)
            {
                value = value << 7 | br.ReadByte();
            }
            return value >> 1;
        }
        public static string UnpackString(BinaryReader br, uint length)
        {
            var bytes = new byte[length];
            for (uint i = 0; i < length; ++i)
            {
                bytes[i] = (byte)UnpackUint(br);
            }
            return Encoding.GetEncoding(932).GetString(bytes);
        }

        public static byte[] PackUint(uint a)
        {
            List<byte> result = new List<byte>();
            uint v = a;

            if (v == 0)
            {
                result.Add(0x01);
                return result.ToArray();
            }

            v = (v << 1) + 1;
            byte curByte = (byte)(v & 0xFF);
            while ((v & 0xFFFFFFFFFFFFFFFE) != 0)
            {
                result.Add(curByte);
                v >>= 7;
                curByte = (byte)(v & 0xFE);
            }

            result.Reverse();
            return result.ToArray();
        }
        public static byte[] PackString(string s)
        {
            byte[] bytes = Encoding.GetEncoding(932).GetBytes(s);
            List<byte> rst = new List<byte>();
            foreach (byte b in bytes)
            {
                rst.AddRange(PackUint(b));
            }
            return rst.ToArray();
        }

    }
}
