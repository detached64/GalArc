using GalArc.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ArcFormats.Qlie
{
    public partial class PACK : ArchiveFormat
    {
        private List<QlieEntry> entries;

        public override void Unpack(string input, string output)
        {
            FileStream fs = File.OpenRead(input);
            BinaryReader br = new BinaryReader(fs);
            // Init & check
            fs.Position = fs.Length - 0x1C;
            QlieHeader qheader = new QlieHeader
            {
                Magic = Encoding.ASCII.GetString(br.ReadBytes(16)).TrimEnd('\0'),
                FileCount = br.ReadInt32(),
                IndexOffset = br.ReadInt64()
            };
            if (!string.Equals(qheader.Magic, HeaderMagic) || !IsSaneCount(qheader.FileCount) || qheader.IndexOffset < 0 || qheader.IndexOffset >= fs.Length)
            {
                Logger.Error("Invalid header");
            }
            entries = new List<QlieEntry>(qheader.FileCount);

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
            if (!string.Equals(qhash.Magic, HashMagic) || qhash.C != 0x100 || qhash.FileCount != qheader.FileCount || qhash.IndexSize != 4 * qheader.FileCount)
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

            fs.Position = qheader.IndexOffset;
            for (int i = 0; i < qheader.FileCount; i++)
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
                if (!is_common_key_obtained && string.Equals(entry.Name, KeyFileName))
                {
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

        private readonly string HeaderMagic = "FilePackVer3.1"; // length 16, padded with nulls
        private readonly string HashMagic = "HashVer1.4";       // length 16, padded with nulls
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
