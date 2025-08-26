using GalArc.Infrastructure.Progress;
using GalArc.Models.Formats.Commons;
using GalArc.Models.Utils;
using System;
using System.IO;

namespace GalArc.Models.Formats.NScripter;

internal class NSA : ArcFormat
{
    public override string Name => "NSA";
    public override string Description => "NScripter NSA Archive";
    public override bool CanWrite => true;

    public override void Unpack(string filePath, string folderPath)
    {
        using FileStream fs = File.OpenRead(filePath);
        using BinaryReader br = new(fs);
        ushort fileCount = BigEndian.Convert(br.ReadUInt16());
        uint baseOffset = BigEndian.Convert(br.ReadUInt32());
        ProgressManager.SetMax(fileCount);
        for (int i = 0; i < fileCount; i++)
        {
            string relativePath = br.ReadCString();
            bool isCompressed = br.ReadByte() != 0;
            if (isCompressed)
            {
                throw new NotImplementedException();
            }
            uint offset = BigEndian.Convert(br.ReadUInt32()) + baseOffset;
            uint packedSize = BigEndian.Convert(br.ReadUInt32());
            uint unpackedSize = BigEndian.Convert(br.ReadUInt32());
            long pos = fs.Position;
            fs.Position = offset;
            string fullPath = Path.Combine(folderPath, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            byte[] buffer = br.ReadBytes((int)packedSize);
            File.WriteAllBytes(fullPath, buffer);
            buffer = null;
            fs.Position = pos;
            ProgressManager.Progress();
        }
    }

    public override void Pack(string folderPath, string filePath)
    {
        using FileStream fw = File.Create(filePath);
        using BinaryWriter bw = new(fw);
        DirectoryInfo d = new(folderPath);
        FileInfo[] files = d.GetFiles("*", SearchOption.AllDirectories);
        ushort fileCount = (ushort)files.Length;
        bw.Write(BigEndian.Convert(fileCount));
        bw.Write(0);
        ProgressManager.SetMax(files.Length);
        uint offset = 0;
        foreach (FileInfo file in files)
        {
            string relativePath = file.FullName[(folderPath.Length + 1)..];
            bw.Write(ArcEncoding.Shift_JIS.GetBytes(relativePath));
            bw.Write((byte)0);
            bw.Write((byte)0);
            bw.Write(BigEndian.Convert(offset));
            uint size = (uint)file.Length;
            bw.Write(BigEndian.Convert(size));
            bw.Write(BigEndian.Convert(size));
            offset += size;
        }
        uint baseOffset = (uint)fw.Position;
        foreach (FileInfo file in files)
        {
            byte[] data = File.ReadAllBytes(file.FullName);
            bw.Write(data);
            data = null;
            ProgressManager.Progress();
        }
        fw.Position = 2;
        bw.Write(BigEndian.Convert(baseOffset));
    }
}
