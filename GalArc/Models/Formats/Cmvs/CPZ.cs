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

    private CmvsCPZPackOptions _packOptions;
    public ArcOptions PackOptions => _packOptions ??= new CmvsCPZPackOptions();

    private readonly byte[] KeyV1 =
    [
        0x92, 0xCD, 0x97, 0x90, 0x8C, 0xD7, 0x8C, 0xD5, 0x8B, 0x4B, 0x93, 0xFA, 0x9A, 0xD7, 0x8C, 0xBF,
        0x8C, 0xC9, 0x8C, 0xEB, 0x8D, 0x69, 0x8D, 0x8B, 0x8C, 0xD2, 0x8C, 0xD6, 0x8B, 0x6D, 0x8C, 0xE3,
        0x8C, 0xFB, 0x8C, 0xD0, 0x8C, 0xC8, 0x8C, 0xF0, 0x8B, 0xFE, 0x8C, 0xAA, 0x8C, 0xF4, 0x8B, 0x4B,
        0x9C, 0x58, 0x8C, 0xD3, 0x96, 0xC8, 0x8C, 0xCB, 0x8C, 0xCE, 0x8C, 0xF3, 0x8C, 0xD6, 0x8B, 0x52,
    ];

    public override void Unpack(string filePath, string folderPath)
    {
        FileStream fs = File.OpenRead(filePath);
        BinaryReader br = new(fs);
        if (Encoding.ASCII.GetString(br.ReadBytes(3)) != "CPZ")
        {
            throw new InvalidArchiveException();
        }

        int version = br.ReadChar() - '0';
        fs.Dispose();
        br.Dispose();

        switch (version)
        {
            case 1:
                UnpackV1(filePath, folderPath);
                break;
            default:
                throw new InvalidVersionException(InvalidVersionType.NotSupported);
        }
    }

    public override void Pack(string folderPath, string filePath)
    {
        switch (_packOptions.Version)
        {
            case 1:
                PackV1(folderPath, filePath);
                break;
        }
    }

    private void UnpackV1(string filePath, string folderPath)
    {
        using FileStream fs = File.OpenRead(filePath);
        using BinaryReader br = new(fs);
        fs.Position = 4;
        int fileCount = br.ReadInt32();
        ProgressManager.SetMax(fileCount);
        long indexSize = br.ReadInt64();
        byte[] index = br.ReadBytes((int)indexSize);
        for (int i = 0; i < index.Length; i++)
        {
            index[i] = (byte)((index[i] ^ KeyV1[i & 0x3F]) - 0x6C);
        }

        MemoryStream ms = new(index);
        BinaryReader reader = new(ms);
        uint baseOffset = (uint)fs.Position;

        for (int i = 0; i < fileCount; i++)
        {
            int entrySize = reader.ReadInt32();
            int size = reader.ReadInt32();
            long offset = reader.ReadInt64() + baseOffset;
            ms.Position += 8;
            string fileName = reader.ReadCString();
            fs.Position = offset;
            byte[] data = br.ReadBytes(size);
            for (int j = 0; j < data.Length; j++)
            {
                data[j] = (byte)((data[j] ^ KeyV1[j & 0x3F]) - 0x6C);
            }
            string path = Path.Combine(folderPath, fileName);
            File.WriteAllBytes(path, data);
            ms.Position++;
            ProgressManager.Progress();
        }
    }

    private void PackV1(string folderPath, string filePath)
    {
        using FileStream fw = File.Create(filePath);
        using BinaryWriter bw = new(fw);
        string[] files = Directory.GetFiles(folderPath);
        bw.Write(Encoding.ASCII.GetBytes("CPZ1"));
        int fileCount = files.Length;
        bw.Write(fileCount);
        ProgressManager.SetMax(fileCount);
        long indexSize = (26 * fileCount) + Utility.GetNameLengthSum(files, ArcEncoding.Shift_JIS);
        bw.Write(indexSize * 2);

        long offset = 0;
        using MemoryStream raw_index = new();
        using BinaryWriter indexWriter = new(raw_index);
        foreach (string file in files)
        {
            indexWriter.Write(26 + ArcEncoding.Shift_JIS.GetByteCount(Path.GetFileName(file)));
            uint fileSize = (uint)new FileInfo(file).Length;
            indexWriter.Write(fileSize);
            indexWriter.Write(offset);
            offset += fileSize;
            indexWriter.Write((long)0);
            indexWriter.Write(ArcEncoding.Shift_JIS.GetBytes(Path.GetFileName(file)));
            indexWriter.Write((short)0);
        }
        indexWriter.Write(new byte[indexSize]);

        byte[] index = raw_index.ToArray();
        for (int i = 0; i < index.Length; i++)
        {
            index[i] = (byte)((index[i] + 0x6C) ^ KeyV1[i & 0x3F]);
        }
        bw.Write(index);

        foreach (string file in files)
        {
            byte[] data = File.ReadAllBytes(file);
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)((data[i] + 0x6C) ^ KeyV1[i & 0x3F]);
            }
            bw.Write(data);
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
