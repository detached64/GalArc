using CommunityToolkit.Mvvm.ComponentModel;
using GalArc.I18n;
using GalArc.Infrastructure.Logging;
using GalArc.Infrastructure.Progress;
using GalArc.Models.Formats.Commons;
using GalArc.Models.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace GalArc.Models.Formats.Yuris;

internal class YPF : ArcFormat, IUnpackConfigurable
{
    public override string Name => "YPF";
    public override string Description => "Yuris Archive";

    private YurisYPFUnpackOptions _unpackOptions;
    public ArcOptions UnpackOptions => _unpackOptions ??= new YurisYPFUnpackOptions();

    private class YpfEntry : PackedEntry
    {
        public int NameLen { get; set; }
        public byte[] Data { get; set; }
    }

    private class YpfScheme
    {
        public byte Key { get; set; }
        public byte[] Table { get; set; }
        public int ExtraLen { get; set; }
        public byte[] ScriptKeyBytes { get; set; }
        public uint ScriptKey { get; set; }

        public YpfScheme()
        {
            Key = 0;
            ScriptKeyBytes = [];
            Table = Tables[0];
            ExtraLen = ExtraLens[0];
        }
    }

    private readonly List<YpfEntry> entries = [];
    private readonly YpfScheme scheme = new();

    private bool IsFirstGuessYpf = true;
    private bool IsFirstGuessYst = true;
    private string FolderPath;

    private static readonly byte[] Table1 = [0x09, 0x0B, 0x0D, 0x13, 0x15, 0x1B, 0x20, 0x23, 0x26, 0x29, 0x2C, 0x2F, 0x2E, 0x32];
    private static readonly byte[] Table2 = [0x0C, 0x10, 0x11, 0x19, 0x1C, 0x1E, 0x09, 0x0B, 0x0D, 0x13, 0x15, 0x1B, 0x20, 0x23, 0x26, 0x29, 0x2C, 0x2F, 0x2E, 0x32];
    private static readonly byte[] Table3 = [0x03, 0x48, 0x06, 0x35, 0x0C, 0x10, 0x11, 0x19, 0x1C, 0x1E, 0x09, 0x0B, 0x0D, 0x13, 0x15, 0x1B, 0x20, 0x23, 0x26, 0x29, 0x2C, 0x2F, 0x2E, 0x32];
    private static readonly byte[] Table0x1F4 = [0x3, 0xA, 0x6, 0x35, 0xC, 0x10, 0x11, 0x18, 0x1C, 0x1E, 0x9, 0xB, 0xD, 0x13, 0x15, 0x1B, 0x20, 0x23, 0x26, 0x29, 0x2C, 0x2F, 0x14, 0x2E];
    private static readonly List<byte[]> Tables = [Table3, Table2, Table1];
    private static readonly string[] TableNames = ["Table3", "Table2", "Table1"];
    private static readonly List<int> ExtraLens = [8, 4, 0];

    public override void Unpack(string filePath, string folderPath)
    {
        FileStream fs = File.OpenRead(filePath);
        BinaryReader br = new(fs);
        if (br.ReadUInt32() != 0x00465059)
        {
            throw new InvalidArchiveException();
        }

        FolderPath = folderPath;

        uint version = br.ReadUInt32();
        int fileCount = br.ReadInt32();
        uint indexSize = br.ReadUInt32();
        fs.Position += 16;
        ProgressManager.SetMax(fileCount);

        TryReadIndex(br, version, fileCount);

        foreach (YpfEntry entry in entries)
        {
            fs.Position = entry.Offset;
            entry.Data = br.ReadBytes((int)entry.Size);
            if (entry.IsPacked)
            {
                try
                {
                    entry.Data = ZlibHelper.Decompress(entry.Data);
                }
                catch (InvalidDataException)
                {
                    entry.Data = Snappy.Decompress(entry.Data);     // Game: まじかりっく⇔スカイハイ
                }
            }
            Directory.CreateDirectory(Path.GetDirectoryName(entry.Path));
            if (_unpackOptions.DecryptScripts && Path.GetExtension(entry.Path) == ".ybn"
                && entry.Data.Length >= 4 && BitConverter.ToUInt32(entry.Data, 0) == 0x42545359)
            {
                Logger.Debug(MsgStrings.Decrypting, entry.Name);
                entry.Data = TryDecryptScript(entry.Data);
            }
            File.WriteAllBytes(entry.Path, entry.Data);
            entry.Data = null;
            ProgressManager.Progress();
        }
    }

    private void TryReadIndex(BinaryReader br, uint version, int fileCount)
    {
        foreach (byte[] table in Tables)
        {
            foreach (int length in ExtraLens)
            {
                scheme.Table = table;
                scheme.ExtraLen = length;
                Logger.Debug($"Try {TableNames[Tables.IndexOf(table)]} , Extra Length = {length}……");
                try
                {
                    Reset();
                    ReadIndex(br, fileCount);
                    return;
                }
                catch
                { }
            }
        }
        if (version == 0x1F4)
        {
            scheme.Table = Table0x1F4;
            foreach (int length in ExtraLens)
            {
                scheme.ExtraLen = length;
                Logger.Debug($"Try special table , Extra Length = {length}……");
                try
                {
                    Reset();
                    ReadIndex(br, fileCount);
                    return;
                }
                catch { }
            }
        }
        throw new InvalidOperationException("Failed to read index.");
    }

    private void ReadIndex(BinaryReader br, int fileCount)
    {
        br.BaseStream.Position = 32;

        for (int i = 0; i < fileCount; i++)
        {
            br.BaseStream.Position += 4;
            YpfEntry entry = new();
            entry.NameLen = DecryptNameLength((byte)(br.ReadByte() ^ 0xff));
            byte[] name = br.ReadBytes(entry.NameLen);
            DecryptName(name);
            entry.Name = ArcEncoding.Shift_JIS.GetString(name);
            if (entry.Name.ContainsInvalidChars())
            {
                throw new Exception();
            }
            entry.Path = Path.Combine(FolderPath, entry.Name);
            br.BaseStream.Position++;
            entry.IsPacked = br.ReadByte() != 0;
            entry.UnpackedSize = br.ReadUInt32();
            entry.Size = br.ReadUInt32();
            entry.Offset = br.ReadUInt32();
            br.BaseStream.Position += scheme.ExtraLen;
            entries.Add(entry);
        }
    }

    private int DecryptNameLength(byte len)
    {
        int pos = Array.IndexOf(scheme.Table, len);
        return pos == -1 ? len : (pos & 1) != 0 ? scheme.Table[pos - 1] : scheme.Table[pos + 1];
    }

    private void DecryptName(byte[] name)
    {
        if (IsFirstGuessYpf)
        {
            scheme.Key = (byte)(name[^4] ^ '.');
            IsFirstGuessYpf = false;
        }
        for (int i = 0; i < name.Length; i++)
        {
            name[i] ^= scheme.Key;
        }
    }

    private byte[] TryDecryptScript(byte[] script)
    {
        byte[] result;
        try
        {
            FindXorKey(script, false);
            result = DecryptNewScript(script, BitConverter.ToUInt32(script, 12), BitConverter.ToUInt32(script, 16), BitConverter.ToUInt32(script, 20), BitConverter.ToUInt32(script, 24));
        }
        catch
        {
            try
            {
                FindXorKey(script, true);
                result = DecryptOldScript(script, BitConverter.ToUInt32(script, 8), BitConverter.ToUInt32(script, 12));
            }
            catch
            {
                IsFirstGuessYst = true;
                Logger.Error(MsgStrings.ErrorDecScrFailed);
                return script;
            }
        }
        IsFirstGuessYst = false;
        return result;
    }

    private void FindXorKey(byte[] script, bool isOld)
    {
        if (isOld)
        {
            if (script.Length < 44 || !IsFirstGuessYst)
            {
                return;
            }
            Buffer.BlockCopy(script, 44, scheme.ScriptKeyBytes, 0, 4);
        }
        else
        {
            if (!IsFirstGuessYst)
            {
                return;
            }
            MemoryStream ms = new(script);
            BinaryReader br = new(ms);
            ms.Position = 12;
            uint a = br.ReadUInt32();

            ms.Position = a + 40;
            scheme.ScriptKey = br.ReadUInt32();
            scheme.ScriptKeyBytes = BitConverter.GetBytes(scheme.ScriptKey);
        }
    }

    private byte[] DecryptNewScript(byte[] script, uint len1, uint len2, uint len3, uint len4)
    {
        byte[] result = new byte[script.Length];
        Buffer.BlockCopy(script, 0, result, 0, script.Length);
        uint pos = 32;
        for (uint i = 0; i < len1; i++)
        {
            result[i + pos] ^= scheme.ScriptKeyBytes[i & 3];
        }
        pos += len1;
        for (uint i = 0; i < len2; i++)
        {
            result[i + pos] ^= scheme.ScriptKeyBytes[i & 3];
        }
        pos += len2;
        for (uint i = 0; i < len3; i++)
        {
            result[i + pos] ^= scheme.ScriptKeyBytes[i & 3];
        }
        pos += len3;
        for (uint i = 0; i < len4; i++)
        {
            result[i + pos] ^= scheme.ScriptKeyBytes[i & 3];
        }
        return result;
    }

    private byte[] DecryptOldScript(byte[] script, uint len1, uint len2)
    {
        byte[] result = new byte[script.Length];
        Buffer.BlockCopy(script, 0, result, 0, script.Length);
        uint pos = 32;
        for (uint i = 0; i < len1; i++)
        {
            result[i + pos] ^= scheme.ScriptKeyBytes[i & 3];
        }
        pos += len1;
        for (uint i = 0; i < len2; i++)
        {
            result[i + pos] ^= scheme.ScriptKeyBytes[i & 3];
        }
        return result;
    }

    private void Reset()
    {
        entries.Clear();
        IsFirstGuessYpf = true;
        IsFirstGuessYst = true;
        scheme.Key = 0;
    }
}

internal partial class YurisYPFUnpackOptions : ArcOptions
{
    [ObservableProperty]
    private bool decryptScripts = true;
}
