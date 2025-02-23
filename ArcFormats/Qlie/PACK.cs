using GalArc.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ArcFormats.Qlie
{
    public class PACK : ArchiveFormat
    {
        private readonly string HeaderMagicPattern = "^FilePackVer[0-9]\\.[0-9]$";

        /// <summary>
        /// Magic of the key in pack archives. Used by 2.0 and above.
        /// </summary>
        private readonly string KeyMagic = "8hr48uky,8ugi8ewra4g8d5vbf5hb5s6";

        /// <summary>
        /// Key file name in pack archives. Used by 3.0 and above.
        /// </summary>
        private readonly string KeyFileName = "pack_keyfile_kfueheish15538fa9or.key";

        public override void Unpack(string input, string output)
        {
            FileStream fs = File.OpenRead(input);
            BinaryReader br = new BinaryReader(fs);

            #region read header
            QlieHeader qheader = new QlieHeader();
            fs.Position = fs.Length - 0x1C;
            qheader.Magic = Encoding.ASCII.GetString(br.ReadBytes(16)).TrimEnd('\0');
            qheader.FileCount = br.ReadInt32();
            qheader.IndexOffset = br.ReadInt64();
            if (!IsSaneCount(qheader.FileCount) || qheader.IndexOffset < 0 || qheader.IndexOffset >= fs.Length || !Regex.IsMatch(qheader.Magic, HeaderMagicPattern))
            {
                Logger.Error("Invalid header");
            }
            List<QlieEntry> entries = new List<QlieEntry>(qheader.FileCount);
            #endregion

            #region get version & init encryption
            QlieEncryption qenc;
            int major = int.Parse(qheader.Magic.Substring(11, 1));
            int minor = int.Parse(qheader.Magic.Substring(13, 1));
            int version = major * 10 + minor;
            Logger.Info($"File Pack Version: {major}.{minor}");
            switch (version)
            {
                case 10:
                    qenc = new Encryption10();
                    break;
                case 20:
                    qenc = new Encryption20();
                    break;
                case 30:
                    qenc = new Encryption30();
                    break;
                case 31:
                    qenc = new Encryption31();
                    break;
                default:
                    throw new ArgumentException("Unknown version", nameof(version));
            }
            #endregion

            #region read key
            uint key = 0;
            QlieKey qkey = null;
            if (version >= 20)
            {
                fs.Position = fs.Length - 0x440;
                qkey = new QlieKey()
                {
                    Magic = br.ReadBytes(32),
                    HashSize = br.ReadUInt32(),
                    Key = br.ReadBytes(0x400)
                };
                if (qkey.HashSize > fs.Length || qkey.HashSize < 0x44)
                {
                    Logger.Error("Invalid key");
                }
                if (version >= 30)
                {
                    key = qenc.ComputeHash(qkey.Key, 0x100) & 0xFFFFFFF;
                }
                // Optional: decrypt & check key
                QlieEncryption.Decrypt(qkey.Magic, 32, key);
                if (!string.Equals(Encoding.ASCII.GetString(qkey.Magic), KeyMagic))
                {
                    Logger.Info("Key magic failed to match. The archive may be corrupted.");
                }
                // Optional: save key
                if (qkey.Key != null)
                {
                    Directory.CreateDirectory(output);
                    File.WriteAllBytes(Path.Combine(output, "key.bin"), qkey.Key);
                }
            }
            #endregion

            #region read hash
            byte[] hashData = null;
            if (version >= 20)
            {
                fs.Position = fs.Length - 0x440 - qkey.HashSize;
                hashData = br.ReadBytes((int)qkey.HashSize);
            }
            else
            {
                string hashFile = Path.ChangeExtension(input, "hash");
                if (File.Exists(hashFile))
                {
                    hashData = File.ReadAllBytes(hashFile);
                }
            }
            QlieHashReader hashReader = new QlieHashReader(hashData);
            Logger.Info($"Hash Version: {hashReader.HashVersion / 10.0:0.0}");
            byte[] hash = hashReader.GetHashData();
            // Optional: save hash
            if (hash != null)
            {
                Directory.CreateDirectory(output);
                File.WriteAllBytes(Path.Combine(output, "hash.bin"), hash);
            }
            #endregion

            #region read entries
            TryAgainWithEnc20:
            try
            {
                fs.Position = qheader.IndexOffset;
                for (int i = 0; i < qheader.FileCount; i++)
                {
                    QlieEntry entry = new QlieEntry();
                    int length = br.ReadInt16();
                    if (qenc.IsUnicode)
                    {
                        length *= 2;
                    }
                    entry.Name = qenc.DecryptName(br.ReadBytes(length), length, (int)key);
                    entry.Path = Path.Combine(output, entry.Name);
                    entry.Offset = br.ReadInt64();
                    entry.Size = br.ReadUInt32();
                    entry.UnpackedSize = br.ReadUInt32();
                    entry.IsPacked = br.ReadInt32() != 0 && entry.Size != entry.UnpackedSize;
                    entry.IsEncrypted = br.ReadUInt32();
                    entry.Hash = br.ReadUInt32();
                    entry.Key = key;
                    entries.Add(entry);
                }
            }
            catch
            {
                if (qenc.Version == 10)
                {
                    entries.Clear();
                    qenc = new Encryption20();
                    goto TryAgainWithEnc20;
                }
                throw;
            }
            #endregion

            #region extract files
            Logger.InitBar(qheader.FileCount);
            bool need_common_key = version == 31;
            bool is_common_key_obtained = false;
            byte[] common_key = null;
            foreach (QlieEntry entry in entries)
            {
                if (need_common_key)
                {
                    entry.CommmonKey = common_key;
                }
                fs.Position = entry.Offset;
                byte[] data = br.ReadBytes((int)entry.Size);
                qenc.DecryptEntry(data, entry);
                if (entry.IsPacked)
                {
                    data = QlieEncryption.Decompress(data);
                }
                if (need_common_key && !is_common_key_obtained && string.Equals(entry.Name, KeyFileName))
                {
                    //Optional: get & check key; check hash
                    //byte[] res_key = GetResourceKey(input);
                    //if (res_key != null && !res_key.SequenceEqual(data))
                    //{
                    //    Logger.Info("Key mismatch");
                    //}
                    common_key = QlieEncryption.GetCommonKey(data);
                    is_common_key_obtained = true;
                }
                Directory.CreateDirectory(Path.GetDirectoryName(entry.Path));
                File.WriteAllBytes(entry.Path, data);
                data = null;
                Logger.UpdateBar();
            }
            #endregion

            fs.Dispose();
            br.Dispose();
        }
    }

    internal class QlieHeader        // length: 0x1C
    {
        public string Magic { get; set; }
        public int FileCount { get; set; }
        public long IndexOffset { get; set; }
    }

    internal class QlieKey           // length: 0x20 + 0x4 + 0x400 = 0x424
    {
        public byte[] Magic { get; set; }
        public uint HashSize { get; set; }
        public byte[] Key { get; set; }    // 0x400, 0x100 key + 0x300 padding
    }

    internal class QlieEntry : PackedEntry
    {
        public new long Offset { get; set; }
        public uint Hash { get; set; }          // for check, not necessary
        public uint IsEncrypted { get; set; }
        public byte[] CommmonKey { get; set; }  // length: 0x40, for 3.1
        public uint Key { get; set; }           // for 3.1
    }
}
