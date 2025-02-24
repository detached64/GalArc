using GalArc.Logs;
using System.IO;
using System.Text;

namespace ArcFormats.NekoPack
{
    public class DAT : ArchiveFormat
    {
        private string Magic = "NEKOPACK";

        public override void Unpack(string filePath, string folderPath)
        {
            int count;
            using (FileStream fs = File.OpenRead(filePath))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    if (!string.Equals(Encoding.ASCII.GetString(br.ReadBytes(8)), Magic))
                    {
                        Logger.ErrorInvalidArchive();
                    }
                    fs.Position = 16;
                    count = br.ReadInt32();
                }
            }

            if (IsSaneCount(count))
            {
                DatV1 v1 = new DatV1();
                v1.Unpack(filePath, folderPath);
            }
            else
            {

            }
        }
    }
}
