using GalArc.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Utility;
using Utility.Exceptions;
using Utility.Extensions;

namespace ArcFormats.Qlie
{
    internal interface IQlieHash
    {
        string Magic { get; set; }
        byte[] HashData { get; set; }
    }

    internal class QlieHash12 : IQlieHash      // The hash bytes lie in a separate file "datax.hash".
    {
        public string Magic { get; set; }
        public uint Const { get; set; }         // 0x200
        public int FileCount { get; set; }
        public uint IndexSize { get; set; }     // 2 * FileCount
        public uint HashDataSize { get; set; }
        public byte[] HashData { get; set; }    // length: QlieKey.HashSize - 32
    }

    internal class QlieHash13 : QlieHash12
    {
        public new uint Const { get; set; }     // 0x100
    }

    internal class QlieHash14 : IQlieHash
    {
        public string Magic { get; set; }
        public uint Const { get; set; }         // 0x100
        public int FileCount { get; set; }
        public uint IndexSize { get; set; }     // 4 * FileCount
        public uint HashDataSize { get; set; }
        public bool IsCompressed { get; set; }
        public byte[] Unknown { get; set; }     // length: 0x20
        public byte[] HashData { get; set; }    // length: QlieKey.HashSize - 0x44
    }

    internal sealed class QlieHashReader
    {
        private readonly string HashMagicPattern = "^HashVer[0-9]\\.[0-9]$";

        private byte[] Hash;
        public int HashVersion { get; }

        public QlieHashReader(byte[] hash)
        {
            Hash = hash;
            string magic = Encoding.ASCII.GetString(Hash, 0, 16).TrimEnd('\0');
            if (!Regex.IsMatch(magic, HashMagicPattern))
            {
                throw new InvalidDataException("Invalid hash magic.");
            }
            int major = int.Parse(magic.Substring(7, 1));
            int minor = int.Parse(magic.Substring(9, 1));
            HashVersion = major * 10 + minor;
        }

        public byte[] GetHashData()
        {
            IQlieHash hash = GetHash();
            if (hash != null)
            {
                byte[] result = hash.HashData;
                if (result != null)
                {
                    return QlieEncryption.Decompress(result);
                }
            }
            Logger.Error("Failed to get hash data.");
            return null;
        }

        public IQlieHash GetHash()
        {
            switch (HashVersion)
            {
                case 12:
                    return GetHash12();
                case 13:
                    return GetHash13();
                case 14:
                    return GetHash14();
                default:
                    throw new InvalidVersionException(InvalidVersionType.Unknown);
            }
        }

        private IQlieHash GetHash12()
        {
            QlieHash12 qhash = new QlieHash12()
            {
                Const = BitConverter.ToUInt32(Hash, 16),
                FileCount = BitConverter.ToInt32(Hash, 20),
                IndexSize = BitConverter.ToUInt32(Hash, 24),
                HashDataSize = BitConverter.ToUInt32(Hash, 28),
                HashData = new byte[Hash.Length - 32]
            };
            Buffer.BlockCopy(Hash, 32, qhash.HashData, 0, qhash.HashData.Length);
            if (qhash.Const != 0x200 || !IsSaneCount(qhash.FileCount) || qhash.HashDataSize != qhash.HashData.Length)
            {
                throw new ArgumentException("Invalid hash ver 1.2.");
            }
            QlieEncryption.Decrypt(qhash.HashData, qhash.HashData.Length, 0x428);
            return qhash;
        }

        private IQlieHash GetHash13()
        {
            QlieHash13 qhash = new QlieHash13()
            {
                Const = BitConverter.ToUInt32(Hash, 16),
                FileCount = BitConverter.ToInt32(Hash, 20),
                IndexSize = BitConverter.ToUInt32(Hash, 24),
                HashDataSize = BitConverter.ToUInt32(Hash, 28),
                HashData = new byte[Hash.Length - 32]
            };
            Buffer.BlockCopy(Hash, 32, qhash.HashData, 0, qhash.HashData.Length);
            if (qhash.Const != 0x100 || !IsSaneCount(qhash.FileCount) || qhash.HashDataSize != qhash.HashData.Length)
            {
                throw new ArgumentException("Invalid hash ver 1.3.");
            }
            QlieEncryption.Decrypt(qhash.HashData, qhash.HashData.Length, 0x428);
            return qhash;
        }

        private IQlieHash GetHash14()
        {
            QlieHash14 qhash = new QlieHash14()
            {
                Const = BitConverter.ToUInt32(Hash, 16),
                FileCount = BitConverter.ToInt32(Hash, 20),
                IndexSize = BitConverter.ToUInt32(Hash, 24),
                HashDataSize = BitConverter.ToUInt32(Hash, 28),
                IsCompressed = BitConverter.ToUInt32(Hash, 32) != 0,
                Unknown = new byte[32],
                HashData = new byte[Hash.Length - 68]
            };
            Buffer.BlockCopy(Hash, 36, qhash.Unknown, 0, qhash.Unknown.Length);
            Buffer.BlockCopy(Hash, 68, qhash.HashData, 0, qhash.HashData.Length);
            if (qhash.Const != 0x100 || !IsSaneCount(qhash.FileCount) || qhash.IndexSize != qhash.FileCount * 4 && qhash.IndexSize != qhash.FileCount * 2 || qhash.HashDataSize != qhash.HashData.Length)
            {
                throw new ArgumentException("Invalid hash ver 1.4.");
            }
            QlieEncryption.Decrypt(qhash.HashData, qhash.HashData.Length, 0x428);
            return qhash;
        }

        private bool IsSaneCount(int count)
        {
            return count > 0 && count < 0x10000;
        }
    }

    internal sealed class QlieHashWriter
    {
        private readonly string KeyFileName = "pack_keyfile_kfueheish15538fa9or.key";
        private bool IsUnicode => PackVersion >= 31;
        private readonly string FolderPath;
        private readonly int PackVersion;
        private List<string> Files = new List<string>();
        private LinkedList<QlieEntry>[] Entries = new LinkedList<QlieEntry>[256];

        public int HashVersion { get; }

        public QlieHashWriter(string path, int hash_version, int pack_version)
        {
            if (hash_version < 10 || hash_version > 14)
            {
                throw new ArgumentException("Invalid hash version.");
            }
            FolderPath = path;
            HashVersion = hash_version;
            PackVersion = pack_version;
        }

        private void CollectList()
        {
            Files = Directory.GetFiles(FolderPath, "*", SearchOption.AllDirectories).ToList();
            // Place the key file at the top of the list
            string keyFile = Files.FirstOrDefault(f => Path.GetFileName(f).Equals(KeyFileName, StringComparison.OrdinalIgnoreCase));
            if (keyFile != null)
            {
                Files.Remove(keyFile);
                Files.Insert(0, keyFile);
            }
            // Build linked lists
            int index = 0;
            foreach (var file in Files)
            {
                string name = Utils.GetRelativePath(file, FolderPath);
                uint hash = ComputeNameHash(name, out ushort nameLen);
                int position = GetPosition(hash);
                (Entries[position] ?? (Entries[position] = new LinkedList<QlieEntry>())).AddLast(new QlieEntry
                {
                    Name = name,
                    NameHash = hash,
                    NameLength = nameLen,
                    Index = index++
                });
            }
        }

        private byte[] GetHashData()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    for (int i = 0; i < Entries.Length; i++)
                    {
                        if (Entries[i] == null)
                        {
                            bw.Write(0);
                            continue;
                        }
                        bw.Write(Entries[i].Count);
                        foreach (QlieEntry entry in Entries[i])
                        {
                            bw.Write(entry.NameLength);
                            bw.Write(Encoding.Unicode.GetBytes(entry.Name));
                            bw.Write((long)entry.Index * 4);
                            bw.Write(entry.NameHash);
                        }
                    }
                    for (int i = 0; i < Files.Count; i++)
                    {
                        bw.Write(i);
                    }
                }
                return ms.ToArray();
            }
        }

        public byte[] GetHash()
        {
            CollectList();
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.WritePaddedString($"HashVer{HashVersion / 10}.{HashVersion % 10}", 16);
                    bw.Write(HashVersion > 12 ? 0x100 : 0x200);
                    bw.Write(Files.Count);
                    bw.Write(IsUnicode ? 4 * Files.Count : 2 * Files.Count);
                    byte[] hashData = GetHashData();
                    QlieEncryption.Encrypt(hashData, hashData.Length, 0x428);
                    bw.Write(hashData.Length);
                    if (HashVersion == 14)
                    {
                        bw.Write(0);
                        bw.Write(new byte[0x20]);
                    }
                    bw.Write(hashData);
                }
                return ms.ToArray();
            }
        }

        private uint ComputeNameHash(string name, out ushort nameLen)
        {
            uint hash = 0;
            byte[] bytes = Encoding.Unicode.GetBytes(name);
            int index = 1;
            for (int i = 0; i < bytes.Length; i += 2)
            {
                int cur_char = bytes[i] | (bytes[i + 1] << 8);
                int shift = index & 7;
                hash = (uint)(((cur_char << shift) + hash) & 0x3FFFFFFF);
                index++;
            }
            nameLen = (ushort)(bytes.Length / 2);
            return hash;
        }

        private int GetPosition(uint hash)
        {
            return (int)(((ushort)hash + (hash >> 8) + (hash >> 16)) % 256);
        }
    }
}
