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
using System.IO;
using System.Text.RegularExpressions;

namespace GalArc.Models.Formats.Seraph;

internal class DAT : ArcFormat, IUnpackConfigurable
{
    public override string Name => "DAT";
    public override string Description => "Seraph DAT Archive (ArchPac.dat, ScnPac.dat, Voice*.dat)";

    private SeraphDATUnpackOptions _unpackOptions;
    public ArcOptions UnpackOptions => _unpackOptions ??= new SeraphDATUnpackOptions();

    private class SeraphGroup
    {
        public uint Offset { get; set; }
        public int FileCount { get; set; }
        public List<Entry> Entries { get; set; }
    }

    private readonly string ArchPacName = "ArchPac.dat";

    private readonly string ScnPacName = "ScnPac.dat";

    private readonly string VoicePacName = @"^Voice(?:\d|pac)\.dat$";

    private List<SeraphGroup> Groups { get; } = new List<SeraphGroup>(0x40);

    private HashSet<long> Indices { get; } = [];


    public override void Unpack(string filePath, string folderPath)
    {
        string name = Path.GetFileName(filePath);
        if (name.Equals(ArchPacName, StringComparison.OrdinalIgnoreCase))
        {
            using FileStream fs = File.OpenRead(filePath);
            using BinaryReader br = new(fs);
            Indices.Clear();
            bool isGiven = false;
            if (_unpackOptions.UseBrutalForce)
            {
                Logger.Debug(MsgStrings.BrutalForcing);
                AddIndex();
                FilterPossibleIndexArchPac(br);
            }
            else
            {
                isGiven = true;
                Indices.Add(Convert.ToUInt32(_unpackOptions.IndexOffsetString, 16));
            }
            TryReadIndexArchPac(br, isGiven);
            ExtractArchPac(br, folderPath);
        }
        else if (name.Equals(ScnPacName, StringComparison.OrdinalIgnoreCase))
        {
            using FileStream fs = File.OpenRead(filePath);
            using BinaryReader br = new(fs);
            Logger.Info($"{MsgStrings.IndexOffset}: 00000000");
            UnpackScnPac(br, folderPath);
        }
        else if (Regex.IsMatch(name, VoicePacName, RegexOptions.IgnoreCase))
        {
            using FileStream fs = File.OpenRead(filePath);
            using BinaryReader br = new(fs);
            UnpackVoicePac(br, folderPath);
        }
        else
        {
            throw new InvalidArchiveException();
        }
    }

    private void FilterPossibleIndexArchPac(BinaryReader br)
    {
        int thisByte, lastByte = 0;
        long maxOffset = br.BaseStream.Length;
        for (uint i = 0; i < maxOffset; i++)
        {
            if (br.BaseStream.Position >= maxOffset)
            {
                break;
            }

            thisByte = br.ReadByte();
            if (thisByte == 0 && lastByte != 0)
            {
                Indices.Add(br.BaseStream.Position - 2);
            }
            lastByte = thisByte;
        }
    }

    private void TryReadIndexArchPac(BinaryReader br, bool isGiven)
    {
        foreach (long i in Indices)
        {
            Groups.Clear();
            if (ReadIndexArchPac(br, i))
            {
                return;
            }
        }
        if (isGiven)
        {
            throw new InvalidDataException("Failed to read index at the specified offset.");
        }
        else
        {
            throw new InvalidOperationException("Brutal forcing failed. Try specifying the index offset manually.");
        }
    }

    private bool ReadIndexArchPac(BinaryReader br, long indexOffset)
    {
        if (indexOffset >= br.BaseStream.Length)
        {
            return false;
        }
        br.BaseStream.Seek(indexOffset, SeekOrigin.Begin);
        int groupCount = br.ReadInt32();
        int fileCount = br.ReadInt32();
        if (groupCount <= 0 || groupCount > 0x40 || fileCount <= 0)
        {
            return false;
        }
        for (int i = 0; i < groupCount; i++)
        {
            SeraphGroup group = new();
            group.Offset = br.ReadUInt32();
            group.FileCount = br.ReadInt32();
            if (group.FileCount <= 0 || group.FileCount > fileCount || group.Offset > br.BaseStream.Length)
            {
                return false;
            }
            group.Entries = new List<Entry>(group.FileCount);
            Groups.Add(group);
        }
        uint baseOffset = 0;
        for (int i = groupCount - 1; i >= 0; i--)
        {
            uint thisOffset = br.ReadUInt32();
            if (thisOffset != 0)
            {
                return false;
            }
            thisOffset += baseOffset;
            for (int j = 0; j < Groups[i].FileCount; j++)
            {
                Entry entry = new();
                entry.Offset = thisOffset;
                thisOffset = br.ReadUInt32() + baseOffset;
                entry.Size = thisOffset - entry.Offset;
                entry.Name = $"{i}-{j:D5}.cts";
                Groups[i].Entries.Add(entry);
            }
            baseOffset = Groups[i].Entries[Groups[i].FileCount - 1].Offset + Groups[i].Entries[Groups[i].FileCount - 1].Size;
        }
        Logger.InfoFormat($"{MsgStrings.IndexOffset}: {indexOffset:X8}");
        ProgressManager.SetMax(fileCount);
        return true;
    }

    private void ExtractArchPac(BinaryReader br, string folderPath)
    {
        foreach (SeraphGroup group in Groups)
        {
            foreach (Entry entry in group.Entries)
            {
                br.BaseStream.Position = entry.Offset;
                byte[] buffer = br.ReadBytes((int)entry.Size);
                if (buffer.Length < 4)
                {
                    File.WriteAllBytes(Path.Combine(folderPath, entry.Name), buffer);
                }
                else
                {
                    uint sig = BitConverter.ToUInt32(buffer, 0);
                    if (sig == 1 && buffer[4] == 0x78)
                    {
                        byte[] raw = new byte[buffer.Length - 6];
                        Buffer.BlockCopy(buffer, 6, raw, 0, raw.Length);
                        raw = ZlibHelper.Decompress(raw);
                        File.WriteAllBytes(Path.Combine(folderPath, entry.Name), raw);
                        raw = null;
                    }
                    else if ((sig & 0xffff) == 0x9c78)
                    {
                        byte[] raw = new byte[buffer.Length - 2];
                        Buffer.BlockCopy(buffer, 2, raw, 0, raw.Length);
                        raw = ZlibHelper.Decompress(raw);
                        File.WriteAllBytes(Path.Combine(folderPath, entry.Name), raw);
                        raw = null;
                    }
                    else
                    {
                        File.WriteAllBytes(Path.Combine(folderPath, entry.Name), buffer);
                    }
                }
                buffer = null;
                ProgressManager.Progress();
            }
        }
    }

    private static void UnpackScnPac(BinaryReader br, string folderPath)
    {
        SeraphGroup group = new();
        group.FileCount = br.ReadInt32();
        if (group.FileCount <= 0)
        {
            throw new Exception("Failed to read index.");
        }
        group.Entries = new List<Entry>(group.FileCount);
        uint thisOffset = br.ReadUInt32();

        for (int i = 0; i < group.FileCount; i++)
        {
            Entry entry = new();
            entry.Offset = thisOffset;
            thisOffset = br.ReadUInt32();
            entry.Size = thisOffset - entry.Offset;
            entry.Name = $"{i:D5}";
            group.Entries.Add(entry);
        }
        ProgressManager.SetMax(group.FileCount);

        foreach (Entry entry in group.Entries)
        {
            br.BaseStream.Position = entry.Offset;
            byte[] buffer = br.ReadBytes((int)entry.Size);
            if (buffer.Length < 4)
            {
                File.WriteAllBytes(Path.Combine(folderPath, entry.Name), buffer);
                buffer = null;
                continue;
            }
            uint sig = BitConverter.ToUInt32(buffer, 0);
            if (sig == 1 && buffer[4] == 0x78)
            {
                byte[] raw = new byte[buffer.Length - 6];
                Buffer.BlockCopy(buffer, 6, raw, 0, raw.Length);
                buffer = ZlibHelper.Decompress(raw);
            }

            try
            {
                byte[] lz = LzDecompress(buffer);
                File.WriteAllBytes(Path.Combine(folderPath, entry.Name), lz);
            }
            catch
            {
                File.WriteAllBytes(Path.Combine(folderPath, entry.Name), buffer);
            }
            finally
            {
                buffer = null;
            }
            ProgressManager.Progress();
        }
    }

    private static void UnpackVoicePac(BinaryReader br, string folderPath)
    {
        int fileCount = br.ReadUInt16();
        uint dataOffset = 2 + (4 * (uint)fileCount);
        uint nextOffset = br.ReadUInt32();
        if (nextOffset < dataOffset || nextOffset > br.BaseStream.Length)
        {
            UnpackVoicePacV2(br, fileCount, folderPath);
        }
        else
        {
            UnpackVoicePacV1(br, fileCount, folderPath);
        }
    }

    private static void UnpackVoicePacV2(BinaryReader br, int fileCount, string folderPath)
    {
        br.BaseStream.Position = 2;
        List<Entry> entries = new(fileCount);
        for (int i = 0; i < fileCount; i++)
        {
            br.BaseStream.Position += 4;
            Entry entry = new();
            entry.Offset = br.ReadUInt32();
            entry.Size = br.ReadUInt32();
            entry.Name = $"{i:D5}.wav";
            entries.Add(entry);
        }
        ProgressManager.SetMax(fileCount);

        foreach (Entry entry in entries)
        {
            br.BaseStream.Position = entry.Offset;
            byte[] buffer = br.ReadBytes((int)entry.Size);
            File.WriteAllBytes(Path.Combine(folderPath, entry.Name), buffer);
            buffer = null;
            ProgressManager.Progress();
        }
    }

    private static void UnpackVoicePacV1(BinaryReader br, int fileCount, string folderPath)
    {
        br.BaseStream.Position = 2;
        List<Entry> entries = new(fileCount);
        uint thisOffset = br.ReadUInt32();
        for (int i = 0; i < fileCount; i++)
        {
            Entry entry = new();
            entry.Offset = thisOffset;
            thisOffset = br.ReadUInt32();
            entry.Size = thisOffset - entry.Offset;
            entry.Name = $"{i:D5}.wav";
            entries.Add(entry);
        }
        ProgressManager.SetMax(fileCount);

        foreach (Entry entry in entries)
        {
            br.BaseStream.Position = entry.Offset;
            byte[] buffer = br.ReadBytes((int)entry.Size);
            File.WriteAllBytes(Path.Combine(folderPath, entry.Name), buffer);
            buffer = null;
            ProgressManager.Progress();
        }
    }

    private void AddIndex()
    {
        if (_unpackOptions.Scheme?.KnownOffsets != null)
        {
            Indices.UnionWith(_unpackOptions.Scheme.KnownOffsets);
        }
    }

    private static byte[] LzDecompress(byte[] data)
    {
        uint unpacked_size = BitConverter.ToUInt32(data, 0);
        byte[] output = new byte[unpacked_size];
        int src = 4;
        int dst = 0;
        while (dst < unpacked_size)
        {
            if (src >= data.Length)
            {
                throw new InvalidDataException("Unexpected end of input");
            }
            byte ctl = data[src++];
            if ((ctl & 0x80) != 0)
            {
                ushort param = (ushort)(ctl << 8 | data[src++]);
                int offset = ((param >> 5) & 0x3FF) + 1;
                int count = (param & 0x1F) + 1;
                if (dst < offset || dst - offset + count > output.Length)
                {
                    throw new InvalidDataException("Invalid backreference");
                }
                Binary.CopyOverlapped(output, dst - offset, dst, count);
                dst += count;
            }
            else
            {
                int count = (ctl & 0x7F) + 1;
                if (src + count > data.Length)
                {
                    throw new InvalidDataException("Unexpected end of input");
                }
                Buffer.BlockCopy(data, src, output, dst, count);
                dst += count;
                src += count;
            }
        }
        return dst == unpacked_size ? output : throw new InvalidDataException("Output size mismatch");
    }
}

internal partial class SeraphDATUnpackOptions : ArcOptions
{
    public readonly SeraphScheme Scheme;
    public SeraphDATUnpackOptions()
    {
        if (Design.IsDesignMode)
            return;
        Scheme = DatabaseManager.LoadScheme(DatabaseSerializationContext.Default.SeraphScheme);
    }

    [ObservableProperty]
    private bool useBrutalForce = true;
    [ObservableProperty]
    private string indexOffsetString = "00000000";
}
