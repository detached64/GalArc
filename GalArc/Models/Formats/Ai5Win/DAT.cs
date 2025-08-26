using GalArc.Infrastructure.Progress;
using GalArc.Models.Utils;
using System.IO;

namespace GalArc.Models.Formats.Ai5Win;

internal class DAT : ARC
{
    public override string Name => "DAT";
    public override string Description => "Ai5Win Archive";

    private readonly int NameLength = 0x14;

    public override void Unpack(string filePath, string folderPath)
    {
        using FileStream fs = File.OpenRead(filePath);
        using BinaryReader br = new(fs);

        fs.Position = 4;
        uint key = br.ReadUInt32();
        fs.Position = 0;
        int fileCount = (int)(br.ReadInt32() ^ key);
        fs.Position = 0x23;
        ArcScheme scheme = new();
        scheme.NameKey = br.ReadByte();
        scheme.SizeKey = key;
        scheme.OffsetKey = key;

        fs.Position = 8;
        ProgressManager.SetMax(fileCount);

        for (int i = 0; i < fileCount; i++)
        {
            uint size = br.ReadUInt32() ^ scheme.SizeKey;
            uint offset = br.ReadUInt32() ^ scheme.OffsetKey;
            byte[] nameBuffer = br.ReadBytes(NameLength);
            for (int j = 0; j < NameLength; j++)
            {
                nameBuffer[j] ^= scheme.NameKey;
            }
            string name = ArcEncoding.Shift_JIS.GetString(nameBuffer).TrimEnd('\0');
            string path = Path.Combine(folderPath, name);
            long pos = fs.Position;
            fs.Position = offset;
            byte[] data = br.ReadBytes((int)size);
            File.WriteAllBytes(path, data);
            data = null;
            fs.Position = pos;
            ProgressManager.Progress();
        }
    }
}
