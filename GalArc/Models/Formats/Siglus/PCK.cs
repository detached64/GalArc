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

namespace GalArc.Models.Formats.Siglus;

internal class PCK : ArcFormat, IUnpackConfigurable
{
    public override string Name => "PCK";
    public override string Description => "Siglus Engine Scene.pck Archive";

    private SiglusPCKUnpackOptions _unpackOptions;
    public virtual ArcOptions UnpackOptions => _unpackOptions ??= new SiglusPCKUnpackOptions();

    private class ScenePckHeader
    {
        public int FileCount { get; set; }
        public uint NameTableOffset { get; set; }
        public uint NameOffset { get; set; }
        public uint DataTableOffset { get; set; }
        public uint DataOffset { get; set; }
        public bool UseExtraKey { get; set; }
    }

    protected class ScenePckEntry
    {
        public uint NameOffset { get; set; }
        public int NameLength { get; set; }
        public string Name { get; set; }
        public uint DataOffset { get; set; }
        public uint DataLength { get; set; }
        public byte[] Data { get; set; }
        public uint PackedLength { get; set; }
        public uint UnpackedLength { get; set; }
    }

    public override void Unpack(string filePath, string folderPath)
    {
        using FileStream fs = File.OpenRead(filePath);
        using BinaryReader br = new(fs);
        fs.Position = 52;
        ScenePckHeader header = new();
        header.NameTableOffset = br.ReadUInt32();
        header.FileCount = br.ReadInt32();
        header.NameOffset = br.ReadUInt32();
        fs.Position += 4;
        header.DataTableOffset = br.ReadUInt32();
        fs.Position += 4;
        header.DataOffset = br.ReadUInt32();
        fs.Position += 4;
        header.UseExtraKey = br.ReadByte() != 0;

        ProgressManager.SetMax(header.FileCount);
        List<ScenePckEntry> entries = new(header.FileCount);
        fs.Position = header.NameTableOffset;
        for (int i = 0; i < header.FileCount; i++)
        {
            ScenePckEntry entry = new();
            entry.NameOffset = br.ReadUInt32();
            entry.NameLength = br.ReadInt32();
            entries.Add(entry);
        }
        fs.Position = header.NameOffset;
        foreach (ScenePckEntry entry in entries)
        {
            entry.Name = Encoding.Unicode.GetString(br.ReadBytes(entry.NameLength * 2)) + ".ss";
        }
        fs.Position = header.DataTableOffset;
        foreach (ScenePckEntry entry in entries)
        {
            entry.DataOffset = br.ReadUInt32();
            entry.DataLength = br.ReadUInt32();
        }
        fs.Position = header.DataOffset;
        foreach (ScenePckEntry entry in entries)
        {
            fs.Position = header.DataOffset + entry.DataOffset;
            entry.Data = br.ReadBytes((int)entry.DataLength);
        }

        byte[] key = header.UseExtraKey ? (_unpackOptions.TryEachKey ? TryAllSchemes(entries[0], 0, _unpackOptions) : _unpackOptions.Key) : null;
        foreach (ScenePckEntry entry in entries)
        {
            SiglusUtils.DecryptWithKey(entry.Data, key);
            SiglusUtils.Decrypt(entry.Data, 0);

            entry.PackedLength = BitConverter.ToUInt32(entry.Data, 0);
            if (entry.PackedLength != entry.Data.Length)
            {
                throw new InvalidSchemeException();
            }
            entry.UnpackedLength = BitConverter.ToUInt32(entry.Data, 4);
            byte[] input = new byte[entry.PackedLength - 8];
            Buffer.BlockCopy(entry.Data, 8, input, 0, input.Length);
            try
            {
                entry.Data = SiglusUtils.Decompress(input, entry.UnpackedLength);
            }
            catch
            {
                throw new InvalidSchemeException();
            }
        }

        foreach (ScenePckEntry entry in entries)
        {
            string entryPath = Path.Combine(folderPath, entry.Name);
            File.WriteAllBytes(entryPath, entry.Data);
            ProgressManager.Progress();
            entry.Data = null;
        }
        entries.Clear();
    }

    protected static bool IsRightKey(byte[] data, byte[] key, int type)
    {
        byte[] backup = new byte[data.Length];
        Buffer.BlockCopy(data, 0, backup, 0, data.Length);

        SiglusUtils.DecryptWithKey(backup, key);
        SiglusUtils.Decrypt(backup, type);
        if (BitConverter.ToUInt32(backup, 0) != backup.Length)
        {
            return false;
        }
        try
        {
            byte[] bytes = new byte[backup.Length - 8];
            Buffer.BlockCopy(backup, 8, bytes, 0, bytes.Length);
            SiglusUtils.Decompress(bytes, BitConverter.ToUInt32(backup, 4));
        }
        catch
        {
            return false;
        }
        return true;
    }

    protected static byte[] TryAllSchemes(ScenePckEntry entry, int type, SiglusPCKUnpackOptions options)
    {
        foreach (KeyValuePair<string, byte[]> scheme in options.Scheme.KnownSchemes)
        {
            byte[] key = scheme.Value;
            if (key.Length != 16)
            {
                continue;
            }
            if (IsRightKey(entry.Data, key, type))
            {
                Logger.InfoFormat(MsgStrings.KeyFound, BitConverter.ToString(key));
                Logger.InfoFormat(MsgStrings.MatchedGame, scheme.Key);
                return key;
            }
        }
        Logger.Info(MsgStrings.KeyNotFound);
        return null;
    }
}

internal partial class SiglusPCKUnpackOptions : ArcOptions
{
    public readonly SiglusScheme Scheme;
    public SiglusPCKUnpackOptions()
    {
        if (Design.IsDesignMode)
            return;
        Scheme = DatabaseManager.LoadScheme(DatabaseSerializationContext.Default.SiglusScheme);
        Names.Add(GuiStrings.TryEveryEnc);
        if (Scheme?.KnownSchemes != null)
        {
            foreach (KeyValuePair<string, byte[]> item in Scheme.KnownSchemes)
            {
                Names.Add(item.Key);
            }
        }
    }

    [ObservableProperty]
    private ObservableCollection<string> names = [];
    [ObservableProperty]
    private string selectedName = GuiStrings.TryEveryEnc;
    public byte[] Key => Scheme.KnownSchemes.GetValueOrDefault(SelectedName);
    public bool TryEachKey => SelectedName == GuiStrings.TryEveryEnc;
}
