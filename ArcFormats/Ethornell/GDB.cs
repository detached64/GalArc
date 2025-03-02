using GalArc.Logs;
using System.IO;
using System.Text;

namespace ArcFormats.Ethornell
{
    public class GDB : ArchiveFormat
    {
        private const string MagicSDCFormat100 = "SDC FORMAT 1.00\0";

        public override void Unpack(string filePath, string folderPath)
        {
            byte[] data = File.ReadAllBytes(filePath);
            if (Encoding.ASCII.GetString(data, 0, 16) != MagicSDCFormat100)
            {
                Logger.ErrorInvalidArchive();
            }
            SdcDecoder decoder = new SdcDecoder(data);
            byte[] output = decoder.Decode();
            Directory.CreateDirectory(folderPath);
            File.WriteAllBytes(Path.Combine(folderPath, Path.GetFileName(filePath) + ".dec"), output);
        }
    }
}
