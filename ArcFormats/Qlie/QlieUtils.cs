using System;
using System.IO;
using System.Linq;
using System.Text;
using Utility;

namespace ArcFormats.Qlie
{
    public partial class PACK
    {
        private class QlieHeader        // length: 0x1C
        {
            public string Magic { get; set; }   // "FilePackVer3.1"
            public int FileCount { get; set; }
            public long IndexOffset { get; set; }
        }

        private class QlieKey           // length: 0x20 + 0x4 + 0x400 = 0x424
        {
            public byte[] Magic { get; set; }
            public uint HashSize { get; set; }
            public byte[] Key { get; set; }    // 0x400, 0x100 key + 0x300 padding
        }

        private class QlieHash          // length: QlieKey.HashSize
        {
            public string Magic { get; set; }   // "HashVer1.4"
            public uint C { get; set; }         // 0x100
            public int FileCount { get; set; }
            public uint IndexSize { get; set; } // 4 * FileCount
            public uint DataSize { get; set; }
            public bool IsCompressed { get; set; }
            public byte[] Unknown { get; set; } // length: 0x20
            public byte[] HashData { get; set; }// length: QlieKey.HashSize - 0x44
        }

        private class QlieEntry : PackedEntry
        {
            public new long Offset { get; set; }
            public uint Hash { get; set; }       // for check, not necessary
            public uint IsEncrypted { get; set; }
        }

        private uint ComputeHash(byte[] data, int length)
        {
            if (length > data.Length)
            {
                throw new ArgumentException("Invalid length");
            }
            int round = length >> 3;
            ulong c = MMX.PUNPCKLDQ(0xA35793A7);
            ulong hash = 0;
            ulong key = 0;
            int index = 0;
            for (int i = 0; i < round; i++)
            {
                hash = MMX.PAddW(hash, c);
                ulong v = BitConverter.ToUInt64(data, index);
                ulong temp = MMX.PAddW(key, hash ^ v);
                index += 8;
                key = MMX.PSllD(temp, 3) | MMX.PSrlD(temp, 0x1D);
            }
            return (uint)((short)key * (short)(key >> 32) + (short)(key >> 16) * (short)(key >> 48));       // _mm_cvtsi64_si32(_m_pmaddwd(key, _m_psrlqi(key, 0x20u)))
        }

        private string DecryptName(byte[] name, uint hash)
        {
            int nameLength = name.Length;
            int charLength = nameLength / 2;        // Unicode
            int temp = ((charLength * charLength) ^ charLength ^ 0x3E13 ^ ((int)hash >> 16) ^ (int)hash) & 0xFFFF;       // unsigned __int16
            int key = temp;
            for (int i = 0; i < charLength; i++)
            {
                key = temp + i + 8 * key;
                name[2 * i] ^= (byte)key;
                name[2 * i + 1] ^= (byte)(key >> 8);
                // * v16++ = v6 ^ * v7++          _WORD * v16, * v7;
            }
            return Encoding.Unicode.GetString(name, 0, nameLength);
        }

        private void DecryptV1(byte[] data, string name, uint key)
        {
            uint v1 = 0x85F532;
            uint v2 = 0x33F641;
            uint length = (uint)data.Length;
            for (int i = 0; i < name.Length; i++)
            {
                v1 += (uint)(name[i] << (i & 7));
                v2 ^= v1;
            }
            v2 += key ^ (7 * (length & 0xFFFFFF) + length + v1 + (v1 ^ length ^ 0x8F32DC));
            v2 = 9 * (v2 & 0xFFFFFF);
            byte[] table = CreateTable(0x40, v2, true).SelectMany(BitConverter.GetBytes).ToArray();
            int round = data.Length >> 3;
            if (round > 0)
            {
                unsafe
                {
                    fixed (byte* p = data)
                    {
                        ulong* data64 = (ulong*)p;
                        uint v4 = 8 * (BitConverter.ToUInt32(table, 52) & 0xF);
                        ulong v6 = BitConverter.ToUInt64(table, 24);
                        for (int i = 0; i < round; i++)
                        {
                            ulong temp = BitConverter.ToUInt64(table, (int)v4);
                            ulong v7 = MMX.PAddD(v6 ^ temp, temp);
                            ulong v8 = data64[i] ^ v7;
                            data64[i] = v8;
                            v6 = MMX.PAddW(MMX.PSllD(MMX.PAddB(v7, v8) ^ v8, 1), v8);
                            v4 = (v4 + 8) & 0x7F;
                        }
                    }
                }
            }
        }

        private void DecryptV2(byte[] data, string name, uint key, byte[] common_key)
        {
            uint v1 = 0x86F7E2;
            uint v2 = 0x4437F1;
            uint length = (uint)data.Length;
            for (int i = 0; i < name.Length; i++)
            {
                v1 += (uint)(name[i] << (i & 7));
                v2 ^= v1;
            }
            v2 += key ^ (13 * (length & 0xFFFFFF) + length + v1 + (v1 ^ length ^ 0x56E213));
            v2 = 13 * (v2 & 0xFFFFFF);
            byte[] table = CreateTable(0x40, v2, false).SelectMany(BitConverter.GetBytes).ToArray();
            int round = data.Length >> 3;
            if (round > 0)
            {
                unsafe
                {
                    fixed (byte* p = data)
                    {
                        ulong* data64 = (ulong*)p;
                        uint v4 = 8 * (BitConverter.ToUInt32(table, 32) & 0xD);
                        ulong v6 = BitConverter.ToUInt64(table, 24);
                        for (int i = 0; i < round; i++)
                        {
                            ulong temp = BitConverter.ToUInt64(table, (int)(8 * (v4 & 0xF))) ^ BitConverter.ToUInt64(common_key, (int)(8 * (v4 & 0x7F)));
                            ulong v7 = MMX.PAddD(v6 ^ temp, temp);
                            ulong v8 = data64[i] ^ v7;
                            data64[i] = v8;
                            v6 = MMX.PAddW(MMX.PSllD(MMX.PAddB(v7, v8) ^ v8, 1), v8);
                            v4 = (v4 + 1) & 0x7F;
                        }
                    }
                }
            }
        }

        private uint[] CreateTable(int length, uint value, bool isV1)
        {
            uint[] table = new uint[length];
            uint key = isV1 ? 0x8DF21431 : 0x8A77F473;
            for (int i = 0; i < length; i++)
            {
                ulong t = key * ((ulong)value ^ key);
                value = (uint)((t >> 32) + t);
                table[i] = value;
            }
            return table;
        }

        /// <summary>
        /// Used to decrypt hash data and key data
        /// </summary>
        private void Decrypt(byte[] data, int length, uint key = 0x428)
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

        private byte[] Decompress(byte[] input)
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

        /// <summary>
        /// Get common key from key data for decryption v2.
        /// </summary>
        private unsafe byte[] GetCommonKey(byte[] data)
        {
            byte[] key = new byte[0x400];
            fixed (byte* p = key)
            {
                uint* key32 = (uint*)p;
                for (int i = 0; i < 0x100; ++i)
                {
                    int temp = 0;
                    if ((i % 3) != 0)
                    {
                        temp = (i + 7) * -(i + 3);
                    }
                    else
                    {
                        temp = (i + 7) * (i + 3);
                    }
                    key32[i] = (uint)temp;
                }
                int v1 = data[49] % 0x49 + 128;
                int v2 = data[79] % 7 + 7;
                for (int i = 0; i < key.Length; ++i)
                {
                    v1 = (v1 + v2) % data.Length;
                    key[i] ^= data[v1];
                }
                return key;
            }
        }

        private byte[] GetResourceKey(string archive_path)
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
    }
}
