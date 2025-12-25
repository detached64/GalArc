using GalArc.Infrastructure.Progress;
using GalArc.Models.Formats.Commons;
using System.IO;
using System.Linq;

namespace GalArc.Models.Formats.Lightvn;

internal class MCDAT : ArcFormat
{
    public override string Name => "MCDAT";
    public override string Description => "LightVN MCDAT Archive";
    public override bool IsSingleFileArchive => true;

    public override void Unpack(string input, string output)
    {
        ProgressManager.SetMax(1);
        File.Copy(input, output, true);
        using FileStream fs = File.Open(output, FileMode.Open, FileAccess.ReadWrite);
        Decrypt(fs, LightvnVNDATUnpackOptions.DefaultKey);
        ProgressManager.Progress();
    }

    protected static void Decrypt(FileStream fs, byte[] key)
    {
        byte[] reversedKey = [.. key.Reverse()];
        if (fs.Length < 100)
        {
            byte[] data = new byte[fs.Length];
            fs.ReadExactly(data);
            for (int i = 0; i < data.Length; i++)
            {
                data[i] ^= reversedKey[i % reversedKey.Length];
            }
            fs.Seek(0, SeekOrigin.Begin);
            fs.Write(data, 0, data.Length);
        }
        else
        {
            byte[] header = new byte[100];
            fs.ReadExactly(header);
            for (int i = 0; i < header.Length; i++)
            {
                header[i] ^= key[i % key.Length];
            }
            fs.Seek(0, SeekOrigin.Begin);
            fs.Write(header, 0, header.Length);
            fs.Seek(-99, SeekOrigin.End);
            byte[] footer = new byte[99];
            fs.ReadExactly(footer);
            for (int i = 0; i < footer.Length; i++)
            {
                footer[i] ^= reversedKey[i % reversedKey.Length];
            }
            fs.Seek(-99, SeekOrigin.End);
            fs.Write(footer, 0, footer.Length);
        }
    }
}
