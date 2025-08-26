using GalArc.Infrastructure.Progress;
using GalArc.Models.Formats.Commons;
using GalArc.Models.Utils;
using System;
using System.IO;
using System.Linq;

namespace GalArc.Models.Formats.Palette;

internal class PAK : ArcFormat
{
    public override string Name => "PAK";
    public override string Description => "Palette PAK Format";
    public override bool CanWrite => true;

    private readonly byte[] Magic = Utility.HexStringToByteArray("055041434b32");

    public override void Unpack(string filePath, string folderPath)
    {
        using FileStream fs = File.OpenRead(filePath);
        using BinaryReader br = new(fs);
        if (!br.ReadBytes(6).SequenceEqual(Magic))
        {
            throw new InvalidArchiveException();
        }
        int count = br.ReadInt32();
        ProgressManager.SetMax(count);

        for (int i = 0; i < count; i++)
        {
            byte nameLen = br.ReadByte();
            byte[] buffer = br.ReadBytes(nameLen);
            for (int j = 0; j < nameLen; j++)
            {
                buffer[j] ^= 0xff;
            }
            string name = ArcEncoding.Shift_JIS.GetString(buffer);
            uint offset = br.ReadUInt32();
            int size = br.ReadInt32();
            long pos = fs.Position;
            fs.Position = offset;
            byte[] data = br.ReadBytes(size);
            File.WriteAllBytes(Path.Combine(folderPath, name), data);
            data = null;
            fs.Position = pos;
            ProgressManager.Progress();
        }
    }

    public override void Pack(string folderPath, string filePath)
    {
        using FileStream fw = File.Create(filePath);
        using BinaryWriter bw = new(fw);

        string[] files = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly);
        int nameLenSum = Utility.GetNameLengthSum(files, ArcEncoding.Shift_JIS);
        int fileCount = files.Length;
        int baseOffset = 10 + nameLenSum + (9 * fileCount);

        bw.Write(Magic);
        bw.Write(fileCount);
        ProgressManager.SetMax(fileCount);

        foreach (string file in files)
        {
            byte[] name = ArcEncoding.Shift_JIS.GetBytes(Path.GetFileName(file));
            int fileSize = (int)new FileInfo(file).Length;
            bw.Write((byte)name.Length);
            for (int i = 0; i < name.Length; i++)
            {
                bw.Write((byte)(name[i] ^ 0xff));
            }
            bw.Write(name);
            bw.Write(baseOffset);
            baseOffset += fileSize;
            bw.Write(fileSize);
            ProgressManager.Progress();
        }
        foreach (string file in files)
        {
            byte[] data = File.ReadAllBytes(file);
            bw.Write(data);
            data = null;
        }
    }
}
