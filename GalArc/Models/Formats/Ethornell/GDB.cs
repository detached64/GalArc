using GalArc.Infrastructure.Progress;
using GalArc.Models.Formats.Commons;
using GalArc.Models.Utils;
using System.IO;
using System.Text;

namespace GalArc.Models.Formats.Ethornell;

internal class GDB : ArcFormat
{
    public override string Name => "GDB";
    public override string Description => "BGI.gdb Archive";

    private const string MagicSDCFormat100 = "SDC FORMAT 1.00\0";

    public override void Unpack(string filePath, string folderPath)
    {
        ProgressManager.SetMax(1);
        byte[] data = File.ReadAllBytes(filePath);
        if (Encoding.ASCII.GetString(data, 0, 16) != MagicSDCFormat100)
        {
            throw new InvalidArchiveException();
        }
        SdcDecoder decoder = new(data);
        byte[] output = decoder.Decode();
        File.WriteAllBytes(Path.Combine(folderPath, Path.GetFileName(filePath) + ".dec"), output);
        ProgressManager.Progress();
    }
}
