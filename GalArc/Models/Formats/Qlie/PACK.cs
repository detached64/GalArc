using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using GalArc.I18n;
using GalArc.Infrastructure.Logging;
using GalArc.Infrastructure.Progress;
using GalArc.Models.Database.Commons;
using GalArc.Models.Formats.Commons;
using GalArc.Models.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace GalArc.Models.Formats.Qlie;

internal class PACK : ArcFormat, IUnpackConfigurable, IPackConfigurable
{
    public override string Name => "PACK";
    public override string Description => "Qlie Pack Archive";
    public override bool CanWrite => false;

    private QliePACKUnpackOptions _unpackOptions;
    public ArcOptions UnpackOptions => _unpackOptions ??= new QliePACKUnpackOptions();

    private QliePACKPackOptions _packOptions;
    public ArcOptions PackOptions => _packOptions ??= new QliePACKPackOptions();

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
    private readonly string[] KeyLocations = [".", "..", "..\\DLL", "DLL"];

    public override void Unpack(string input, string output)
    {
        using FileStream fs = File.OpenRead(input);
        using BinaryReader br = new(fs);

        #region read header
        QlieHeader qheader = new();
        fs.Position = fs.Length - 0x1C;
        qheader.Magic = Encoding.ASCII.GetString(br.ReadBytes(16)).TrimEnd('\0');
        qheader.FileCount = br.ReadInt32();
        qheader.IndexOffset = br.ReadInt64();
        if (!IsSaneCount(qheader.FileCount) || qheader.IndexOffset < 0 || qheader.IndexOffset >= fs.Length || !Regex.IsMatch(qheader.Magic, HeaderMagicPattern))
        {
            throw new InvalidDataException("Invalid header");
        }
        List<QlieEntry> entries = new(qheader.FileCount);
        #endregion

        #region get version, retrieve game key and init encryption
        int major = qheader.Magic[11] - '0';
        int minor = qheader.Magic[13] - '0';
        int version = (major * 10) + minor;
        Logger.Info($"File Pack Version: {major}.{minor}");
        byte[] game_key = _unpackOptions.Key;
        QlieEncryption qenc = QlieEncryption.Create(version, game_key);
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
                HashSize = br.ReadInt32(),
                Key = br.ReadBytes(0x400)
            };
            if (qkey.HashSize > fs.Length || qkey.HashSize < 0x44)
            {
                throw new InvalidDataException("Invalid key");
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
            if (_unpackOptions.SaveKey && qkey.Key != null)
            {
                File.WriteAllBytes(Path.Combine(output, "key.bin"), qkey.Key);
            }
        }
        #endregion

        #region read hash
        if (_unpackOptions.SaveHash)
        {
            byte[] rawHashData = null;
            if (version >= 20)
            {
                fs.Position = fs.Length - 0x440 - qkey.HashSize;
                rawHashData = br.ReadBytes(qkey.HashSize);
            }
            else
            {
                string hashFile = Path.ChangeExtension(input, "hash");
                if (File.Exists(hashFile))
                {
                    rawHashData = File.ReadAllBytes(hashFile);
                }
            }
            File.WriteAllBytes(Path.ChangeExtension(input, "hash"), rawHashData);
            if (rawHashData != null)
            {
                QlieHashReader hashReader = new(rawHashData);
                Logger.Info($"Hash Version: {hashReader.HashVersion / 10.0:0.0}");
                byte[] hashData = hashReader.GetHashData();
                if (hashData != null)
                {
                    File.WriteAllBytes(Path.Combine(output, "hash.bin"), hashData);
                }
            }
        }
    #endregion

    #region read entries
    TryAgainWithEnc20:
        try
        {
            fs.Position = qheader.IndexOffset;
            for (int i = 0; i < qheader.FileCount; i++)
            {
                QlieEntry entry = new();
                int length = br.ReadInt16();
                if (qenc.IsUnicode)
                {
                    length *= 2;
                }
                entry.RawName = br.ReadBytes(length);
                entry.Name = qenc.DecryptName(entry.RawName, length, (int)key);
                Logger.Debug($"Entry {i}: {entry.Name}");
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
        ProgressManager.SetMax(qheader.FileCount);
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
                //byte[] res_key = GetLocalKeyFromExe(input);
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
            ProgressManager.Progress();
        }
        #endregion
    }

    public override void Pack(string folderPath, string filePath)
    {
        const string magic_pattern = "FilePackVer{0}";
        int hash_ver = _packOptions.HashVersion switch
        {
            "1.2" => 12,
            "1.3" => 13,
            "1.4" => 14,
            _ => throw new InvalidVersionException(InvalidVersionType.NotSupported),
        };
        int pack_ver = _packOptions.Version switch
        {
            "1.0" => 10,
            "2.0" => 20,
            "3.0" => 30,
            "3.1" => 31,
            _ => throw new InvalidVersionException(InvalidVersionType.NotSupported),
        };
        QlieEncryption qenc = QlieEncryption.Create(_packOptions.Version);
        FileInfo[] files = new DirectoryInfo(folderPath).GetFiles("*", SearchOption.AllDirectories);
        using FileStream fw = File.Create(filePath);
        using BinaryWriter bw = new(fw);
        List<QlieEntry> entries = new(files.Length);
        long baseOffset = 0;
        foreach (FileInfo file in files)
        {
            byte[] data = File.ReadAllBytes(file.FullName);
            bw.Write(data);
            QlieEntry entry = new();
            entry.Name = Path.GetRelativePath(folderPath, file.FullName);
            entry.Hash = qenc.ComputeHash(data, data.Length);       // returns 0 for 1.0, 2.0
            entry.RawName = qenc.EncryptName(entry.Name, (int)entry.Hash);
            entry.Offset = baseOffset;
            entry.Size = (uint)data.Length;
            entry.UnpackedSize = entry.Size;
            baseOffset += entry.Size;
            entries.Add(entry);
        }
        //foreach (QlieEntry entry in entries)
        //{
        //    bw.Write((short)entry.RawName.Length);
        //    bw.Write(entry.RawName);
        //    bw.Write(entry.Offset);
        //    bw.Write(entry.Size);
        //    bw.Write(entry.UnpackedSize);
        //    bw.Write(0);                // entry.IsPacked
        //    bw.Write(0);                // entry.IsEncrypted
        //    bw.Write(entry.Hash);
        //}
        // Write hash
        QlieHashWriter hashWriter = new(folderPath, hash_ver, pack_ver, false);
        byte[] hashData = hashWriter.CreateHash();
        bw.Write(hashData);
        // Write key
        QlieKey key = new()
        {
            Magic = Encoding.ASCII.GetBytes(KeyMagic.PadRight(32, '\0')),
            HashSize = hashData.Length,
            Key = new byte[0x400]
        };
        QlieEncryption.Encrypt(key.Magic, 32, 0);
        bw.Write(key.Magic);
        bw.Write(key.HashSize);
        bw.Write(key.Key);
        // Write header
        bw.Write(Encoding.ASCII.GetBytes(string.Format(magic_pattern, _packOptions.Version).PadRight(16, '\0')));
        bw.Write(entries.Count);
        bw.Write(baseOffset);
        //using (FileStream fwHash = File.Create(Path.ChangeExtension(filePath, "hash")))
        //{
        //    using (BinaryWriter bwHash = new BinaryWriter(fwHash))
        //    {
        //        bwHash.Write((short)entries.Count);
        //        foreach (QlieEntry entry in entries)
        //        {
        //            bwHash.Write((short)entry.RawName.Length);
        //            bwHash.Write(entry.RawName);
        //            bwHash.Write(0);
        //            bwHash.Write(0);
        //        }
        //    }
        //}
    }

    //public override void Pack(string folderPath, string filePath)
    //{
    //    QlieHashWriter hashWriter = new(folderPath, 14, 31, false);
    //    File.WriteAllBytes(Path.ChangeExtension(filePath, "hash"), hashWriter.CreateHash14());
    //}

    private byte[] TryFindLocalKey(string input)
    {
        string folder = Path.GetDirectoryName(input);
        if (!string.IsNullOrEmpty(_unpackOptions.FKeyPath))
        {
            return File.Exists(_unpackOptions.FKeyPath)
                ? File.ReadAllBytes(_unpackOptions.FKeyPath)
                : throw new FileNotFoundException("Specified key.fkey not found.");
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
    private static byte[] GetLocalKeyFromExe(string archive_path)
    {
        string exe_path = Path.GetDirectoryName(Path.GetDirectoryName(archive_path));
        foreach (string file_path in Directory.GetFiles(exe_path, "*.exe", SearchOption.TopDirectoryOnly))
        {
            using ResourceReader reader = new(file_path);
            byte[] key = reader.ReadResource("RESKEY");
            if (key != null)
            {
                return key;
            }
        }
        return null;
    }
}

internal sealed class QlieHeader        // length: 0x1C
{
    public string Magic { get; set; }
    public int FileCount { get; set; }
    public long IndexOffset { get; set; }
}

internal sealed class QlieKey           // length: 0x20 + 0x4 + 0x400 = 0x424
{
    public byte[] Magic { get; set; }
    public int HashSize { get; set; }
    public byte[] Key { get; set; }    // 0x400, 0x100 key + 0x300 padding
}

internal sealed class QlieEntry : PackedEntry
{
    public byte[] RawName { get; set; }
    public new long Offset { get; set; }
    public uint Hash { get; set; }          // for check, not necessary
    public uint IsEncrypted { get; set; }
    public byte[] CommmonKey { get; set; }  // length: 0x40
    public uint Key { get; set; }
    public ushort NameLength { get; set; }
    public uint NameHash { get; set; }
    public long Index { get; set; }
}

internal partial class QliePACKUnpackOptions : ArcOptions
{
    public readonly QlieScheme Scheme;

    public QliePACKUnpackOptions()
    {
        if (Design.IsDesignMode)
            return;
        Scheme = DatabaseManager.LoadScheme(DatabaseSerializationContext.Default.QlieScheme);
        Names.Add(GuiStrings.DefaultEncryption);
        if (Scheme?.KnownSchemes != null)
        {
            foreach (KeyValuePair<string, byte[]> pair in Scheme.KnownSchemes)
            {
                Names.Add(pair.Key);
            }
        }
    }

    [ObservableProperty]
    private string fKeyPath;
    [ObservableProperty]
    private bool saveKey;
    [ObservableProperty]
    private bool saveHash;
    [ObservableProperty]
    private ObservableCollection<string> names = [];
    [ObservableProperty]
    private string selectedName = GuiStrings.DefaultEncryption;
    public byte[] Key => Scheme?.KnownSchemes?.GetValueOrDefault(SelectedName);
}

internal partial class QliePACKPackOptions : ArcOptions
{
    [ObservableProperty]
    private string version = "3.1";
    [ObservableProperty]
    private IReadOnlyList<string> versions = [/*"1.0", "2.0", "3.0", */"3.1"];
    [ObservableProperty]
    private string hashVersion = "1.4";
    [ObservableProperty]
    private IReadOnlyList<string> hashVersions = ["1.2", "1.3", "1.4"];
}
