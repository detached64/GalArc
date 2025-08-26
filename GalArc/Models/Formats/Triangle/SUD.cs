using GalArc.Infrastructure.Progress;
using GalArc.Models.Formats.Commons;
using System.IO;

namespace GalArc.Models.Formats.Triangle;

internal class SUD : ArcFormat
{
    public override string Name => "SUD";
    public override string Description => "Triangle SUD Audio Archive";
    public override bool CanWrite => true;

    public override void Unpack(string filePath, string folderPath)
    {
        using FileStream fs = File.OpenRead(filePath);
        using BinaryReader br = new(fs);
        for (int index = 1; fs.Position < fs.Length; index++)
        {
            uint size = br.ReadUInt32();
            byte[] data = br.ReadBytes((int)size);
            File.WriteAllBytes(Path.Combine(folderPath, index.ToString("D6") + ".ogg"), data);
            data = null;
        }
    }

    public override void Pack(string folderPath, string filePath)
    {
        using FileStream fw = File.Create(filePath);
        using BinaryWriter bw = new(fw);
        DirectoryInfo d = new(folderPath);
        FileInfo[] files = d.GetFiles("*.ogg");
        ProgressManager.SetMax(files.Length);
        foreach (FileInfo file in files)
        {
            bw.Write((uint)file.Length);
            byte[] data = File.ReadAllBytes(file.FullName);
            bw.Write(data);
            data = null;
            ProgressManager.Progress();
        }
    }
}
