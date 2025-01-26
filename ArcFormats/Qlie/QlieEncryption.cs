﻿using GalArc.Logs;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Utility;

namespace ArcFormats.Qlie
{
    /// <summary>
    /// Base class for Qlie encryption. Contains common methods for decryption and decompression of Qlie archives.
    /// </summary>
    internal abstract class QlieEncryption
    {
        public abstract int Version { get; }

        public virtual bool IsUnicode { get; } = false;

        public abstract string DecryptName(byte[] name, int name_length, uint hash);

        public abstract void DecryptEntry(byte[] data, QlieEntry entry);

        /// <summary>
        /// Get the resource key from the game executable. Used by 3.0 and above encryption.
        /// </summary>
        public static byte[] GetResourceKey(string archive_path)
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

        public static uint ComputeHash(byte[] data, int length)
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

        public static unsafe byte[] GetCommonKey(byte[] data)
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

        /// <summary>
        /// Widely used decryption method. Used to decrypt hash, key data and entry in FilePackVer2.0.
        /// </summary>
        public static void Decrypt(byte[] data, int length, uint key)
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

        public static byte[] Decompress(byte[] input)
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

    internal class Encryption10 : QlieEncryption
    {
        public override int Version => 10;

        public override string DecryptName(byte[] name, int name_length, uint hash)
        {
            uint key = 0xFA;        // 0xC4 ^ 0x3E
            for (int i = 1; i <= name_length; i++)
            {
                name[i - 1] ^= (byte)((key ^ i) + i);
            }
            return ArcEncoding.Shift_JIS.GetString(name);
        }

        public override void DecryptEntry(byte[] data, QlieEntry entry)
        {
            if (entry.IsEncrypted != 0)
            {
                Decrypt(data, data.Length, 0);
            }
        }
    }

    internal class Encryption20 : Encryption10
    {
        public override int Version => 20;

        public override string DecryptName(byte[] input, int name_length, uint hash = 0)
        {
            uint key = 0xFA + (uint)name_length;        // 0xC4 ^ 0x3E
            for (int i = 1; i <= name_length; i++)
            {
                input[i - 1] ^= (byte)((key ^ i) + i);
            }
            return ArcEncoding.Shift_JIS.GetString(input);
        }
    }

    internal class Encryption31 : QlieEncryption
    {
        public override int Version => 31;

        public override bool IsUnicode => true;

        public override string DecryptName(byte[] name, int name_length, uint hash)
        {
            int char_length = name_length / 2;        // Unicode
            int temp = ((char_length * char_length) ^ char_length ^ 0x3E13 ^ ((int)hash >> 16) ^ (int)hash) & 0xFFFF;       // unsigned __int16
            int key = temp;
            for (int i = 0; i < char_length; i++)
            {
                key = temp + i + 8 * key;
                name[2 * i] ^= (byte)key;
                name[2 * i + 1] ^= (byte)(key >> 8);
                // * v16++ = v6 ^ * v7++          _WORD * v16, * v7;
            }
            return Encoding.Unicode.GetString(name, 0, name_length);
        }

        public override void DecryptEntry(byte[] data, QlieEntry entry)
        {
            switch (entry.IsEncrypted)
            {
                case 0:
                    break;
                case 1:
                    DecryptV1(data, entry.Name, entry.Key);
                    break;
                case 2:
                    DecryptV2(data, entry.Name, entry.Key, entry.CommmonKey);
                    break;
                default:
                    Logger.Error("Unknown encryption method");
                    break;
            }
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
    }
}
