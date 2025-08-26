using GalArc.Infrastructure.Progress;
using GalArc.Models.Formats.Commons;
using GalArc.Models.Utils;
using System.IO;
using System.Text;

namespace GalArc.Models.Formats.Ai5Win;

internal class VSD : ArcFormat
{
    public override string Name => "VSD";

    public override string Description => "Ai5Win/Ai6Win Video File";

    public override void Unpack(string filePath, string folderPath)
    {
        using FileStream fs = File.OpenRead(filePath);
        using BinaryReader br = new(fs);
        if (Encoding.ASCII.GetString(br.ReadBytes(4)) != "VSD1")
        {
            throw new InvalidArchiveException();
        }
        uint offset = br.ReadUInt32() + 8;
        uint size = (uint)new FileInfo(filePath).Length - offset;
        ProgressManager.SetMax(1);
        fs.Position = offset;
        using SubStream ss = new(fs, offset, size, true);
        using FileStream output = File.Create(Path.Combine(folderPath, Path.GetFileNameWithoutExtension(filePath) + ".mpg"));
        ss.CopyTo(output);
        ProgressManager.Progress();
    }
}
