using CommunityToolkit.Mvvm.ComponentModel;
using GalArc.I18n;
using GalArc.Infrastructure.Logging;
using GalArc.Infrastructure.Progress;
using GalArc.Models.Formats.Commons;
using GalArc.Models.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GalArc.Models.Formats.NeXAS;

internal class PAC : ArcFormat, IUnpackConfigurable, IPackConfigurable
{
    public override string Name => "PAC";
    public override string Description => "NeXAS Archive";
    public override bool CanWrite => true;

    private NeXASPACUnpackOptions _unpackOptions;
    public ArcOptions UnpackOptions => _unpackOptions ??= new NeXASPACUnpackOptions();

    private NeXASPACPackOptions _packOptions;
    public ArcOptions PackOptions => _packOptions ??= new NeXASPACPackOptions();

    internal enum Method
    {
        None,
        Lzss,
        Huffman,
        Zlib,
        Zlib1,
        Zlib2,
        Zstd = 7
    }

    public static Array MethodValues => Enum.GetValues<Method>();

    private const string Magic = "PAC";

    private readonly string[] NoCompressionExts = ["png", "fnt", "ogg"];

    private int FileCount;
    private string FolderPath;
    private BinaryReader Reader;

    public override void Unpack(string filePath, string folderPath)
    {
        using FileStream fs = File.OpenRead(filePath);
        Reader = new BinaryReader(fs);
        FolderPath = folderPath;
        if (Encoding.ASCII.GetString(Reader.ReadBytes(3)) != Magic)
        {
            throw new InvalidArchiveException();
        }
        fs.Position++;
        FileCount = Reader.ReadInt32();
        if (!IsSaneCount(FileCount))
        {
            UnpackLegacy();
            return;
        }

        int methodMagic = Reader.ReadInt32();
        Method method = (Method)methodMagic;
        Logger.Info(MsgStrings.CompressionMethod, method);
        List<PackedEntry> entries = TryReadIndex();
        ProgressManager.SetMax(FileCount);
        foreach (PackedEntry entry in entries)
        {
            fs.Seek(entry.Offset, SeekOrigin.Begin);
            byte[] fileData = Reader.ReadBytes((int)entry.Size);

            if (entry.UnpackedSize != entry.Size && method != Method.None && Enum.IsDefined(method)) // compressed
            {
                Logger.Debug(MsgStrings.TryDecompressWithMethod, entry.Name, method);
                try
                {
                    switch (method)
                    {
                        case Method.Huffman:
                            File.WriteAllBytes(entry.Path, HuffmanHelper.Decode(fileData, (int)entry.UnpackedSize));
                            break;
                        case Method.Lzss:
                            File.WriteAllBytes(entry.Path, LzssHelper.Decompress(fileData));
                            break;
                        case Method.Zlib:
                        case Method.Zlib1:
                        case Method.Zlib2:
                            File.WriteAllBytes(entry.Path, ZlibHelper.Decompress(fileData));
                            break;
                        case Method.Zstd:
                            File.WriteAllBytes(entry.Path, Zstd.Decompress(fileData));
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(MsgStrings.ErrorDecompressFailed);
                    Logger.Debug(ex.Message);
                }
            }
            else    // No compression or unknown method
            {
                File.WriteAllBytes(entry.Path, fileData);
            }
            fileData = null;
            ProgressManager.Progress();
        }
        Reader.Dispose();
    }

    private List<PackedEntry> TryReadIndex()
    {
        try
        {
            return ReadNewIndex();
        }
        catch
        {
            try
            {
                return ReadOldIndex();
            }
            catch
            {
                throw new InvalidArchiveException();
            }
        }
    }

    private List<PackedEntry> ReadOldIndex()
    {
        List<PackedEntry> entries = [];
        Reader.BaseStream.Position = 12;
        for (int i = 0; i < FileCount; i++)
        {
            PackedEntry entry = new();
            entry.Name = _unpackOptions.Encoding.GetString(Reader.ReadBytes(64)).TrimEnd('\0');
            entry.Path = Path.Combine(FolderPath, entry.Name);
            entry.Offset = Reader.ReadUInt32();
            entry.UnpackedSize = Reader.ReadUInt32();
            entry.Size = Reader.ReadUInt32();
            entries.Add(entry);
        }
        Logger.ShowVersion("pac", 1);
        return entries;
    }

    private List<PackedEntry> ReadNewIndex()
    {
        List<PackedEntry> entries = [];
        Reader.BaseStream.Seek(-4, SeekOrigin.End);
        int packedLen = Reader.ReadInt32();
        int unpackedLen = FileCount * 76;
        Reader.BaseStream.Seek(-4 - packedLen, SeekOrigin.End);
        byte[] packedIndex = Reader.ReadBytes(packedLen);
        for (int i = 0; i < packedLen; i++)
        {
            packedIndex[i] ^= 0xff;
        }
        byte[] index = HuffmanHelper.Decode(packedIndex, unpackedLen);
        using MemoryStream msIndex = new(index);
        using BinaryReader ReaderIndex = new(msIndex);
        for (int i = 0; i < FileCount; i++)
        {
            PackedEntry entry = new();
            entry.Name = _unpackOptions.Encoding.GetString(ReaderIndex.ReadBytes(64)).TrimEnd('\0');
            entry.Path = Path.Combine(FolderPath, entry.Name);
            entry.Offset = ReaderIndex.ReadUInt32();
            entry.UnpackedSize = ReaderIndex.ReadUInt32();
            entry.Size = ReaderIndex.ReadUInt32();
            entries.Add(entry);
        }
        Logger.ShowVersion("pac", 2);
        return entries;
    }

    private void UnpackLegacy()
    {
        Reader.BaseStream.Position = 3;
        uint baseOffset = Reader.ReadUInt32();
        if (baseOffset > Reader.BaseStream.Length)
        {
            throw new InvalidArchiveException();
        }
        List<Entry> entries = [];
        long pos = Reader.BaseStream.Position;
        while (Reader.BaseStream.Position < baseOffset)
        {
            Entry entry = new();
            List<byte> nameBytes = [];
            byte b;
            while ((b = Reader.ReadByte()) != 0)
            {
                nameBytes.Add((byte)~b);
            }
            entry.Name = ArcEncoding.Shift_JIS.GetString([.. nameBytes]);
            entry.Path = Path.Combine(FolderPath, entry.Name);
            entry.Offset = Reader.ReadUInt32() + baseOffset;
            entry.Size = Reader.ReadUInt32();
            Logger.Debug(entry.Name);
            entries.Add(entry);
        }
        ProgressManager.SetMax(entries.Count);
        foreach (Entry entry in entries)
        {
            Reader.BaseStream.Position = entry.Offset;
            byte[] data = Reader.ReadBytes((int)entry.Size);
            for (int i = 0; i < 3; i++)
            {
                data[i] = (byte)~data[i];   // not sure if this applies to all files, at least .grp files need this
            }
            File.WriteAllBytes(entry.Path, data);
            data = null;
            ProgressManager.Progress();
        }
    }

    public override void Pack(string folderPath, string filePath)
    {
        string[] files = Directory.GetFiles(folderPath);
        using FileStream fw = File.Create(filePath);
        using BinaryWriter bw = new(fw);
        bw.Write(Encoding.ASCII.GetBytes(Magic));
        bw.Write('\0');
        bw.Write((uint)files.Length);
        bw.Write((int)_packOptions.Method);
        int indexLength = 76 * files.Length;
        uint offset = (uint)fw.Position;
        if (_packOptions.Version == 1)
        {
            bw.BaseStream.Position += indexLength;  // Reserve space for index
        }
        ProgressManager.SetMax(files.Length);

        List<PackedEntry> entries = [];
        foreach (string file in files)
        {
            PackedEntry entry = new();
            entry.Name = Path.GetFileName(file);
            entry.Offset = offset;
            byte[] data = File.ReadAllBytes(file);
            entry.UnpackedSize = (uint)data.Length;
            if (!entry.Name.HasAnyOfExtensions(NoCompressionExts))
            {
                entry.UnpackedSize = (uint)data.Length;
                switch (_packOptions.Method)
                {
                    case Method.Lzss:
                        data = LzssHelper.Compress(data);
                        break;
                    case Method.Huffman:
                        data = HuffmanHelper.Encode(data);
                        break;
                    case Method.Zlib:
                    case Method.Zlib1:
                    case Method.Zlib2:
                        data = ZlibHelper.Compress(data);
                        break;
                    case Method.Zstd:
                        data = Zstd.Compress(data);
                        break;
                }
            }
            bw.Write(data);
            entry.Size = (uint)data.Length;
            offset += entry.Size;
            entries.Add(entry);
            data = null;
        }

        Encoding encoding = _packOptions.Encoding;
        using MemoryStream index = new();
        using (BinaryWriter indexWriter = new(index))
        {
            foreach (PackedEntry entry in entries)
            {
                indexWriter.WritePaddedString(entry.Name, 64, '\0', encoding);
                indexWriter.Write(entry.Offset);
                indexWriter.Write(entry.UnpackedSize);
                indexWriter.Write(entry.Size);
            }
        }

        byte[] raw = index.ToArray();
        if (_packOptions.Version == 2)
        {
            byte[] packed = HuffmanHelper.Encode(raw);
            for (int i = 0; i < packed.Length; i++)
            {
                packed[i] ^= 0xff;
            }
            bw.Write(packed);
            bw.Write(packed.Length);
        }
        else
        {
            bw.BaseStream.Position = 12;
            bw.Write(raw);
        }
        ProgressManager.Progress();
    }
}

internal partial class NeXASPACUnpackOptions : ArcOptions
{
    [ObservableProperty]
    private IReadOnlyList<Encoding> encodings = ArcEncoding.SupportedEncodings;
    [ObservableProperty]
    private Encoding encoding = Encoding.UTF8;
}

internal partial class NeXASPACPackOptions : ArcOptions
{
    [ObservableProperty]
    private IReadOnlyList<Encoding> encodings = ArcEncoding.SupportedEncodings;
    [ObservableProperty]
    private Encoding encoding = Encoding.UTF8;
    [ObservableProperty]
    private int version = 2;
    [ObservableProperty]
    private IReadOnlyList<int> versions = [1, 2];
    [ObservableProperty]
    private PAC.Method method = PAC.Method.None;
}
