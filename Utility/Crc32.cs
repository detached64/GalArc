using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public class Crc32
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
                        entry = (entry >> 1) ^ Polynomial;
                    else
                        entry >>= 1;
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
                crc = (crc >> 8) ^ table[(crc & 0xff) ^ data[i]];
            return ~crc;
        }

    }
}
