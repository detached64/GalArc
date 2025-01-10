using System;
using System.IO;
using Utility;

namespace ArcFormats.Qlie
{
    internal abstract class QlieEncryption : ArchiveFormat
    {
        protected readonly string HashMagic1_4 = "HashVer1.4";       // length 16, padded with nulls
        protected readonly string HashMagic1_3 = "HashVer1.3";

        protected static byte[] GetResourceKey(string archive_path)
        {
            string exe_path = Path.GetDirectoryName(Path.GetDirectoryName(archive_path));
            foreach (string file_path in Directory.GetFiles(exe_path, "*.exe", SearchOption.TopDirectoryOnly))
            {
                using (ResourceReader reader = new ResourceReader(file_path))
                {
                    byte[] key = reader.ReadResource("RESKEY");
                    if (key != null)
                    {
                        return key;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Used to decrypt hash & key data and to decrypt entry in FilePackVer2.0.
        /// </summary>
        protected static void Decrypt(byte[] data, int length, uint key = 0x428)
        {
            if (length < 8)
            {
                return;
            }
            const ulong c1 = 0xA73C5F9D;
            const ulong c2 = 0xCE24F523;
            const ulong c3 = 0xFEC9753E;
            ulong v5 = MMX.PUNPCKLDQ(c1);
            ulong v7 = MMX.PUNPCKLDQ(c2);
            ulong v9 = MMX.PUNPCKLDQ((ulong)(length + key) ^ c3);
            unsafe
            {
                fixed (byte* raw = data)
                {
                    ulong* d = (ulong*)raw;
                    for (int i = 0; i < length / 8; ++i)
                    {
                        v5 = MMX.PAddD(v5, v7) ^ v9;
                        v9 = *d ^ v5;
                        *d++ = v9;
                    }
                }
            }
        }

        protected static byte[] Decompress(byte[] input)
        {
            if (input.Length < 8 || BitConverter.ToUInt32(input, 0) != 0xFF435031)  // "1PC\xFF"
            {
                return input;
            }
            int unpacked_size = BitConverter.ToInt32(input, 8);
            byte[] output = new byte[unpacked_size];
            byte[,] table = new byte[0x100, 2];
            byte[] temp = new byte[0x1000];
            bool is_16bit = (BitConverter.ToUInt32(input, 4) & 1) != 0;
            int src = 12;
            int dst = 0;
            while (src < input.Length)
            {
                for (uint i = 0; i < 256;)
                {
                    uint c = input[src++];
                    if (c > 127)
                    {
                        for (c -= 127; c > 0; c--, i++)
                        {
                            table[i, 0] = (byte)i;
                        }
                    }
                    for (c++; c > 0 && i < 256; c--, i++)
                    {
                        table[i, 0] = input[src++];
                        if (i != table[i, 0])
                        {
                            table[i, 1] = input[src++];
                        }
                    }
                }

                uint block_length = 0;
                uint temp_length = 0;
                if (is_16bit)
                {
                    block_length = BitConverter.ToUInt16(input, src);
                    src += 2;
                }
                else
                {
                    block_length = BitConverter.ToUInt32(input, src);
                    src += 4;
                }
                while (block_length > 0 || temp_length > 0)
                {
                    byte c = 0;
                    if (temp_length != 0)
                    {
                        c = temp[--temp_length];
                    }
                    else
                    {
                        c = input[src++];
                        block_length--;
                    }
                    if (c == table[c, 0])
                    {
                        output[dst++] = c;
                    }
                    else
                    {
                        temp[temp_length++] = table[c, 1];
                        temp[temp_length++] = table[c, 0];
                    }
                }
            }
            return output;
        }
    }
}
