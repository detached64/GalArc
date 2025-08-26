using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using GalArc.I18n;
using GalArc.Infrastructure.Progress;
using GalArc.Models.Database.Commons;
using GalArc.Models.Formats.Commons;
using GalArc.Models.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace GalArc.Models.Formats.AnimeGameSystem;

internal class DAT : ArcFormat, IUnpackConfigurable
{
    public override string Name => "DAT";
    public override string Description => "Anime Game System Archive";
    public override bool CanWrite => true;

    private const string Magic = "pack";

    private AgsDATUnpackOptions _unpackOptions;
    public ArcOptions UnpackOptions => _unpackOptions ??= new AgsDATUnpackOptions();

    public override void Unpack(string filePath, string folderPath)
    {
        AgsScheme.AgsKey key = null;
        bool isXored = _unpackOptions.SelectedFileMap != null &&
            _unpackOptions.Scheme.EncryptedArchives.Any(s => StringComparer.OrdinalIgnoreCase
                .Equals(s, Path.GetFileName(filePath))) &&
            _unpackOptions.SelectedFileMap.TryGetValue(Path.GetFileName(filePath), out key);
        using FileStream fs = File.OpenRead(filePath);
        using BinaryReader br = new(fs);
        fs.Position = 4;
        int fileCount = br.ReadUInt16();
        if (!IsSaneCount(fileCount))
        {
            throw new InvalidDataException(nameof(fileCount));
        }
        uint indexOffset = 6;
        uint indexSize = (uint)fileCount * 0x18;
        List<Entry> entries = new(fileCount);
        for (int i = 0; i < fileCount; i++)
        {
            Entry entry = new();
            entry.Name = br.ReadCString();
            fs.Position = indexOffset + 0x10;
            entry.Offset = br.ReadUInt32();
            fs.Position = indexOffset + 0x14;
            entry.Size = br.ReadUInt32();
            indexOffset += 0x18;
            entries.Add(entry);
        }

        ProgressManager.SetMax(fileCount);
        foreach (Entry entry in entries)
        {
            fs.Position = entry.Offset;
            byte[] data = br.ReadBytes((int)entry.Size);
            string fileName = Path.Combine(folderPath, entry.Name);
            if (isXored)
            {
                Decrypt(data, (byte)key.Initial, (byte)key.Increment);
            }
            File.WriteAllBytes(fileName, data);
            ProgressManager.Progress();
            data = null;
        }
    }

    private static void Decrypt(byte[] data, byte initial, byte increment)
    {
        for (int i = 0; i < data.Length; i++)
        {
            data[i] ^= initial;
            initial += increment;
        }
    }

    public override void Pack(string folderPath, string filePath)
    {
        using FileStream fw = File.Create(filePath);
        using BinaryWriter bw = new(fw);

        FileInfo[] files = new DirectoryInfo(folderPath).GetFiles();
        ProgressManager.SetMax(files.Length);

        bw.Write(Magic);
        bw.Write((ushort)files.Length);

        uint indexOffset = 6;
        uint dataOffset = indexOffset + (uint)(files.Length * 0x18); // index size is fileCount * 0x18

        foreach (FileInfo file in files)
        {
            fw.Position = indexOffset;
            byte[] nameBytes = Utility.GetPaddedBytes(file.Name, 0x10);
            bw.Write(nameBytes);
            bw.Write(0);
            bw.Write((uint)file.Length);
            indexOffset += 0x18;
        }

        indexOffset = 6;
        foreach (FileInfo file in files)
        {
            fw.Position = indexOffset + 0x10;
            bw.Write(dataOffset);
            fw.Position = dataOffset;
            byte[] fileData = File.ReadAllBytes(file.FullName);
            bw.Write(fileData);
            dataOffset += (uint)fileData.Length;
            indexOffset += 0x18;
            ProgressManager.Progress();
            fileData = null;
        }
    }
}

internal partial class AgsDATUnpackOptions : ArcOptions
{
    public readonly AgsScheme Scheme;

    public AgsDATUnpackOptions()
    {
        if (Design.IsDesignMode)
            return;
        Scheme = DatabaseManager.LoadScheme(DatabaseSerializationContext.Default.AgsScheme) ?? new AgsScheme();
        Names.Add(GuiStrings.NoEncryption);
        if (Scheme?.KnownSchemes != null)
        {
            foreach (string name in Scheme.KnownSchemes.Keys)
            {
                Names.Add(name);
            }
        }
    }

    [ObservableProperty]
    private ObservableCollection<string> names = [];
    [ObservableProperty]
    private string selectedName = GuiStrings.NoEncryption;
    public Dictionary<string, AgsScheme.AgsKey> SelectedFileMap => Scheme?.KnownSchemes != null ? Scheme.KnownSchemes.GetValueOrDefault(SelectedName) : null;
}
