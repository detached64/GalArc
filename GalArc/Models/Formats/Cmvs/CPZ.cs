using CommunityToolkit.Mvvm.ComponentModel;
using GalArc.Infrastructure.Progress;
using GalArc.Models.Formats.Commons;
using GalArc.Models.Utils;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GalArc.Models.Formats.Cmvs;

internal class CPZ : ArcFormat, IPackConfigurable
{
    public override string Name => "CPZ";
    public override string Description => "Purple Software CMVS Archive";
    public override bool CanWrite => true;

    private const string Magic = "CPZ";

    private CmvsCPZPackOptions _packOptions;
    public ArcOptions PackOptions => _packOptions ??= new CmvsCPZPackOptions();

    public override void Unpack(string input, string output)
    {
        using FileStream fs = File.OpenRead(input);
        using BinaryReader br = new(fs);
        if (Encoding.ASCII.GetString(br.ReadBytes(3)) != Magic)
        {
            throw new InvalidArchiveException();
        }
        int version = br.ReadChar() - '0';
        CmvsEncryption enc = CmvsEncryption.Create(version);
        int fileCount = br.ReadInt32();
        ProgressManager.SetMax(fileCount);
        long indexSize = br.ReadInt64();
        byte[] index = enc.Decrypt(br.ReadBytes((int)indexSize));
        using MemoryStream indexStrem = new(index);
        using BinaryReader indexReader = new(indexStrem);
        uint baseOffset = (uint)fs.Position;
        for (int i = 0; i < fileCount; i++)
        {
            int entrySize = indexReader.ReadInt32();
            int size = indexReader.ReadInt32();
            long offset = indexReader.ReadInt64() + baseOffset;
            indexStrem.Position += 8;
            fs.Position = offset;
            string path = Path.Combine(output, ArcEncoding.Shift_JIS.GetString(indexReader.ReadBytes(entrySize - 24)).TrimEnd('\0'));
            File.WriteAllBytes(path, enc.Decrypt(br.ReadBytes(size)));
            ProgressManager.Progress();
        }
    }

    public override void Pack(string input, string output)
    {
        using FileStream fw = File.Create(output);
        using BinaryWriter bw = new(fw);
        CmvsEncryption enc = CmvsEncryption.Create(_packOptions.Version);
        bw.Write(Encoding.ASCII.GetBytes(Magic));
        bw.Write((byte)('0' + _packOptions.Version));
        FileInfo[] files = new DirectoryInfo(input).GetFiles();
        int fileCount = files.Length;
        ProgressManager.SetMax(fileCount);
        bw.Write(fileCount);
        using MemoryStream indexStream = new();
        using BinaryWriter indexWriter = new(indexStream);
        long offset = 0;
        foreach (FileInfo file in files)
        {
            int nameLength = ArcEncoding.Shift_JIS.GetByteCount(file.Name);
            int entrySize = 24 + nameLength + (4 - (nameLength % 4));
            indexWriter.Write(entrySize);
            indexWriter.Write((int)file.Length);
            indexWriter.Write(offset);
            offset += file.Length;
            indexWriter.Write((long)0);
            indexWriter.WritePaddedString(file.Name, nameLength + (4 - (nameLength % 4)));
        }
        long indexSize = (uint)indexStream.Length;
        bw.Write(indexSize);
        bw.Write(enc.Encrypt(indexStream.ToArray()));
        foreach (FileInfo file in files)
        {
            bw.Write(enc.Encrypt(File.ReadAllBytes(file.FullName)));
            ProgressManager.Progress();
        }
    }
}

internal partial class CmvsCPZPackOptions : ArcOptions
{
    [ObservableProperty]
    private int version = 1;
    [ObservableProperty]
    private IReadOnlyList<int> versions = [1];
}
