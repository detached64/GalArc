using GalArc.Infrastructure.Logging;
using GalArc.Models.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GalArc.Models.Formats.Qlie;

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

    private readonly byte[] Hash;
    public int HashVersion { get; }

    public QlieHashReader(byte[] hash)
    {
        Hash = hash;
        string magic = Encoding.ASCII.GetString(Hash, 0, 16).TrimEnd('\0');
        if (!Regex.IsMatch(magic, HashMagicPattern))
        {
            throw new InvalidDataException("Invalid hash magic.");
        }
        int major = magic[7] - '0';
        int minor = magic[9] - '0';
        HashVersion = (major * 10) + minor;
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
        return HashVersion switch
        {
            12 => GetHash12(),
            13 => GetHash13(),
            14 => GetHash14(),
            _ => throw new InvalidVersionException(InvalidVersionType.Unknown),
        };
    }

    private QlieHash12 GetHash12()
    {
        QlieHash12 qhash = new()
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

    private QlieHash13 GetHash13()
    {
        QlieHash13 qhash = new()
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

    private QlieHash14 GetHash14()
    {
        QlieHash14 qhash = new()
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
        if (qhash.Const != 0x100 || !IsSaneCount(qhash.FileCount) || (qhash.IndexSize != qhash.FileCount * 4 && qhash.IndexSize != qhash.FileCount * 2) || qhash.HashDataSize != qhash.HashData.Length)
        {
            throw new ArgumentException("Invalid hash ver 1.4.");
        }
        QlieEncryption.Decrypt(qhash.HashData, qhash.HashData.Length, 0x428);
        return qhash;
    }

    private static bool IsSaneCount(int count)
    {
        return count > 0 && count < 0x10000;
    }
}

internal sealed class QlieHashWriter(string path, int hash_version, int pack_version, bool as_separate)
{
    private readonly string KeyFileName = "pack_keyfile_kfueheish15538fa9or.key";
    private bool IsUnicode => PackVersion >= 31;

    private readonly int PackVersion = pack_version;
    private List<string> Paths = [];
    private readonly LinkedList<QlieEntry>[] HashBuckets = new LinkedList<QlieEntry>[256];

    public int HashVersion { get; } = hash_version;

    public byte[] CreateHash()
    {
        return HashVersion switch
        {
            14 => CreateHash14(),
            _ => throw new InvalidVersionException(InvalidVersionType.NotSupported, HashVersion),
        };
    }

    private byte[] CreateHash14()
    {
        CollectList14();
        using MemoryStream ms = new();
        using (BinaryWriter bw = new(ms))
        {
            bw.WritePaddedString($"HashVer{HashVersion / 10}.{HashVersion % 10}", 16);
            bw.Write(0x100);
            bw.Write(Paths.Count);
            bw.Write(IsUnicode ? 4 * Paths.Count : 2 * Paths.Count);
            byte[] hashData = CreateHashData14();
            //File.WriteAllBytes(Path.Combine(FolderPath, "datax.hash"), hashData);
            QlieEncryption.Encrypt(hashData, hashData.Length, 0x428);
            bw.Write(hashData.Length);
            bw.Write(0);
            bw.Write(new byte[0x20]);
            bw.Write(hashData);
        }
        return ms.ToArray();
    }

    private byte[] CreateHashData12()
    {
        using MemoryStream ms = new();
        using BinaryWriter bw = new(ms);
        for (int i = 0; i < Paths.Count; i += 256)
        {
            int current = Math.Min(256, Paths.Count - i);
            bw.Write((ushort)current);
            for (int j = i; j < current + i; j++)
            {
                string name = Paths[j];
                uint hash = ComputeNameHash14(name, out ushort nameLen);
                int position = GetPosition(hash);
                bw.Write((ushort)position);
                bw.Write((ushort)hash);
                bw.Write((ushort)(IsUnicode ? 4 * i : 2 * i));
                bw.Write(nameLen);
                bw.Write(Encoding.Unicode.GetBytes(name));
            }
        }
        return ms.ToArray();
    }

    private byte[] CreateHashData14()
    {
        using MemoryStream ms = new();
        using (BinaryWriter bw = new(ms))
        {
            for (int i = 0; i < HashBuckets.Length; i++)
            {
                if (HashBuckets[i] == null)
                {
                    bw.Write(0);
                    continue;
                }
                bw.Write(HashBuckets[i].Count);
                foreach (QlieEntry entry in HashBuckets[i])
                {
                    bw.Write(entry.NameLength);
                    bw.Write(Encoding.Unicode.GetBytes(entry.Name));
                    bw.Write(entry.Index * 4);
                    bw.Write(entry.NameHash);
                }
            }
            for (int i = 0; i < Paths.Count; i++)
            {
                bw.Write(i);
            }
        }
        return ms.ToArray();
    }

    private void CollectList14()
    {
        Paths = [.. Utility.GetRelativePaths(Directory.GetFiles(path, "*", SearchOption.AllDirectories), path)];
        string keyFile = Paths.FirstOrDefault(f => f.Equals(KeyFileName, StringComparison.OrdinalIgnoreCase));
        if (keyFile != null)
        {
            Paths.Remove(keyFile);
            Paths.Sort();
            Paths.Insert(0, keyFile);
        }
        else
        {
            throw new FileNotFoundException("Key file is missing.", KeyFileName);
        }
        // Build linked lists
        int index = 0;
        foreach (string path in Paths)
        {
            uint hash = ComputeNameHash14(path, out ushort nameLen);
            int position = GetPosition(hash);
            (HashBuckets[position] ??= new LinkedList<QlieEntry>()).AddLast(new QlieEntry
            {
                Name = path,
                NameHash = hash,
                NameLength = nameLen,
                Index = index++
            });
        }
    }

    private static uint ComputeNameHash13(string name, out ushort nameLen)
    {
        uint hash = 0;
        byte[] bytes = ArcEncoding.Shift_JIS.GetBytes(name);
        int index = 1;
        for (int i = 0; i < bytes.Length; i++)
        {
            hash += (uint)(bytes[index - 1] << (index & 7));
        }
        nameLen = (ushort)bytes.Length;
        return hash;
    }

    private static uint ComputeNameHash14(string name, out ushort nameLen)
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

    private static int GetPosition(uint hash)
    {
        return (int)(((ushort)hash + (hash >> 8) + (hash >> 16)) % 256);
    }
}
