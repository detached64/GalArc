using GalArc.Controls;
using GalArc.Database;
using GalArc.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Utility;

namespace ArcFormats.Qlie
{
    public class PACK : ArchiveFormat
    {
        public override OptionsTemplate UnpackExtraOptions => UnpackPACKOptions.Instance;

        private readonly string HeaderMagicPattern = "^FilePackVer[0-9]\\.[0-9]$";

        /// <summary>
        /// Magic of the key in pack archives. Used by 2.0 and above.
        /// </summary>
        private readonly string KeyMagic = "8hr48uky,8ugi8ewra4g8d5vbf5hb5s6";

        /// <summary>
        /// Key file name in pack archives. Used by 3.0 and above.
        /// </summary>
        private readonly string KeyFileName = "pack_keyfile_kfueheish15538fa9or.key";

        /// <summary>
        /// Possible locations of key.fkey.
        /// </summary>
        private static readonly string[] KeyLocations = { ".", "..", "..\\DLL", "DLL" };

        internal static QlieScheme Scheme;
        internal static string SelectedKey;
        internal static string FKeyPath;
        internal static bool SaveKey = false;
        internal static bool SaveHash = false;

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

            #region get version, retrieve arc_key and init encryption
            int major = int.Parse(qheader.Magic.Substring(11, 1));
            int minor = int.Parse(qheader.Magic.Substring(13, 1));
            int version = major * 10 + minor;
            Logger.Info($"File Pack Version: {major}.{minor}");
            byte[] local_key_data = !string.IsNullOrWhiteSpace(SelectedKey) ? Convert.FromBase64String(SelectedKey) : null;
            QlieEncryption qenc = QlieEncryption.CreateEncryption(version, local_key_data);
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
                if (SaveKey && qkey.Key != null)
                {
                    Directory.CreateDirectory(output);
                    File.WriteAllBytes(Path.Combine(output, "key.bin"), qkey.Key);
                }
            }
            #endregion

            #region read hash
            byte[] rawHashData = null;
            if (version >= 20)
            {
                fs.Position = fs.Length - 0x440 - qkey.HashSize;
                rawHashData = br.ReadBytes((int)qkey.HashSize);
            }
            else
            {
                string hashFile = Path.ChangeExtension(input, "hash");
                if (File.Exists(hashFile))
                {
                    rawHashData = File.ReadAllBytes(hashFile);
                }
            }
            QlieHashReader hashReader = new QlieHashReader(rawHashData);
            Logger.Info($"Hash Version: {hashReader.HashVersion / 10.0:0.0}");
            byte[] hashData = hashReader.GetHashData();
            // Optional: save hash
            if (SaveHash && hashData != null)
            {
                Directory.CreateDirectory(output);
                File.WriteAllBytes(Path.Combine(output, "hash.bin"), hashData);
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
                    entry.RawName = br.ReadBytes(length);
                    entry.Name = qenc.DecryptName(entry.RawName, length, (int)key);
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
            bool need_common_key = version >= 30;
            bool is_common_key_obtained = false;
            byte[] common_key = null;
            if (need_common_key)
            {
                common_key = TryFindLocalKey(input);
                if (common_key != null)
                {
                    is_common_key_obtained = true;
                }
            }
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
                if (need_common_key && !is_common_key_obtained && string.Equals(entry.Name, KeyFileName, StringComparison.OrdinalIgnoreCase))
                {
                    //Optional: get & check key; check hash
                    //byte[] res_key = GetResourceKey(input);
                    //if (res_key != null && !res_key.SequenceEqual(data))
                    //{
                    //    Logger.Info("Key mismatch");
                    //}
                    if (version == 31)
                    {
                        common_key = QlieEncryption.GetCommonKey(data);
                        is_common_key_obtained = true;
                    }
                    else
                    {
                        throw new FileNotFoundException("key.fkey path must be specified.");
                    }
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

        /// <summary>
        /// Try to find key.fkey in possible locations.
        /// </summary>
        private byte[] TryFindLocalKey(string input)
        {
            string folder = Path.GetDirectoryName(input);
            if (!string.IsNullOrEmpty(FKeyPath))
            {
                if (File.Exists(FKeyPath))
                {
                    return File.ReadAllBytes(FKeyPath);
                }
                else
                {
                    throw new FileNotFoundException("Specified file not found.");
                }
            }
            foreach (string p in KeyLocations)
            {
                string path = Path.GetFullPath(Path.Combine(folder, p, "key.fkey"));
                if (File.Exists(path))
                {
                    return File.ReadAllBytes(path);
                }
            }
            return null;
        }

        /// <summary>
        /// Get key from the game executable.
        /// </summary>
        public static byte[] GetLocalKeyFromExe(string archive_path)
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

        public override void DeserializeScheme(out string name, out int count)
        {
            Scheme = Deserializer.ReadScheme<QlieScheme>();
            name = "Qlie";
            count = Scheme?.KnownKeys?.Count ?? 0;
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
        public byte[] RawName { get; set; }
        public new long Offset { get; set; }
        public uint Hash { get; set; }          // for check, not necessary
        public uint IsEncrypted { get; set; }
        public byte[] CommmonKey { get; set; }  // length: 0x40, for 3.1
        public uint Key { get; set; }           // for 3.1
    }
}
