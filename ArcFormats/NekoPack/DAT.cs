using System.IO;
using System.Text;
using Utility.Exceptions;

namespace ArcFormats.NekoPack
{
    public class DAT : ArcFormat
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
                        throw new InvalidArchiveException();
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
