using GalArc.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Utility;

namespace ArcFormats.Qlie
{
    internal class Encryption31 : QileEncryption
    {
        public Encryption31(QlieHeader qheader)
        {
            header = qheader;
            entries = new List<QlieEntry>(header.FileCount);
        }
        private List<QlieEntry> entries;
        private QlieHeader header;

        public override void Unpack(string input, string output)
        {
            FileStream fs = File.OpenRead(input);
            BinaryReader br = new BinaryReader(fs);
            // Init & check
            fs.Position = fs.Length - 0x440;
            QlieKey qkey = new QlieKey()
            {
                Magic = br.ReadBytes(32),
                HashSize = br.ReadUInt32(),
                Key = br.ReadBytes(0x400)
            };
            if (!qkey.Magic.SequenceEqual(KeyMagic) || qkey.HashSize > fs.Length || qkey.HashSize < 0x44)
            {
                Logger.Error("Invalid key");
            }

            fs.Position = fs.Length - 0x440 - qkey.HashSize;
            QlieHash qhash = new QlieHash()
            {
                Magic = Encoding.ASCII.GetString(br.ReadBytes(16)).TrimEnd('\0'),
                C = br.ReadUInt32(),
                FileCount = br.ReadInt32(),
                IndexSize = br.ReadUInt32(),
                DataSize = br.ReadUInt32(),
                IsCompressed = br.ReadInt32() != 0,
                Unknown = br.ReadBytes(0x20),
                HashData = br.ReadBytes((int)qkey.HashSize - 0x44)
            };
            if (!string.Equals(qhash.Magic, HashMagic1_4) || qhash.C != 0x100 || qhash.FileCount != header.FileCount || qhash.IndexSize != 4 * header.FileCount)
            {
                Logger.Error("Invalid hash");
            }
            if (qhash.DataSize != qkey.HashSize - 0x44)
            {
                Logger.Info("Invalid hash data size");
            }
            // You can also read names from hashdata.
            // Skip hashdata for now for speed.
            //Decrypt(qhash.HashData, qhash.HashData.Length);
            //File.WriteAllBytes(Path.Combine(output, "hash.bin"), Decompress(hash.HashData));

            uint key = ComputeHash(qkey.Key, 256) & 0xFFFFFFF;
            Decrypt(KeyMagic, KeyMagic.Length, key);
            if (!string.Equals(Encoding.ASCII.GetString(KeyMagic), KeyMagicStr))
            {
                Logger.Error("Invalid key magic");
            }

            fs.Position = header.IndexOffset;
            for (int i = 0; i < header.FileCount; i++)
            {
                QlieEntry entry = new QlieEntry();
                int charLength = br.ReadInt16();
                int nameLength = 2 * charLength;
                entry.Name = DecryptName(br.ReadBytes(nameLength), key);
                entry.Path = Path.Combine(output, entry.Name);
                entry.Offset = br.ReadInt64();
                entry.Size = br.ReadUInt32();
                entry.UnpackedSize = br.ReadUInt32();
                entry.IsPacked = br.ReadInt32() != 0 && entry.Size != entry.UnpackedSize;
                entry.IsEncrypted = br.ReadUInt32();
                entry.Hash = br.ReadUInt32();
                entries.Add(entry);
            }
            Logger.InitBar(entries.Count);
            bool is_common_key_obtained = false;
            byte[] common_key = new byte[64];
            foreach (QlieEntry entry in entries)
            {
                br.BaseStream.Position = entry.Offset;
                byte[] data = br.ReadBytes((int)entry.Size);
                // Check hash. Ignore hash check for speed.
                //if (ComputeHash(data, data.Length) != entry.Hash)
                //{
                //    Logger.Info("Hash failed to match");
                //}
                // Decrypt data
                switch (entry.IsEncrypted)
                {
                    case 0:
                        break;
                    case 1:
                        DecryptV1(data, entry.Name, key);
                        break;
                    case 2:
                        DecryptV2(data, entry.Name, key, common_key);
                        break;
                    default:
                        Logger.Error("Unknown encryption method");
                        break;
                }
                // Decompress data
                if (entry.IsPacked)
                {
                    data = Decompress(data);
                }
                // Get common key for decryption v2
                // Also, we can obtain the decrypted key from exe resources.
                if (!is_common_key_obtained && string.Equals(entry.Name, KeyFileName))
                {
                    // Ignore for now for speed.
                    //byte[] res_key = GetResourceKey(input);
                    //if (res_key != null && !res_key.SequenceEqual(data))
                    //{
                    //    Logger.Info("Key mismatch");
                    //}
                    common_key = GetCommonKey(data);
                    is_common_key_obtained = true;
                }
                Directory.CreateDirectory(Path.GetDirectoryName(entry.Path));
                File.WriteAllBytes(entry.Path, data);
                Logger.UpdateBar();
            }
            fs.Dispose();
            br.Dispose();
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

        private readonly byte[] KeyMagic =                      // length 32
        {
            0xF2, 0x94, 0x31, 0xB3, 0xF2, 0x89, 0x28, 0xFE,
            0xF9, 0xA1, 0x6F, 0x06, 0xBC, 0xBC, 0x66, 0x5B,
            0xA6, 0xD7, 0x7E, 0x2F, 0xA9, 0x25, 0x78, 0xFB,
            0xE7, 0xAC, 0x6E, 0x19, 0xEE, 0x67, 0x34, 0x1B
        };
        private readonly string KeyMagicStr = "8hr48uky,8ugi8ewra4g8d5vbf5hb5s6";
        private readonly string KeyFileName = "pack_keyfile_kfueheish15538fa9or.key";
    }
}
