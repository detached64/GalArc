using Log;
using System.IO;
using System.Text;

namespace ArcFormats.Ai5Win
{
    public class VSD
    {
        public void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            if (Encoding.ASCII.GetString(br.ReadBytes(4)) != "VSD1")
            {
                LogUtility.ErrorInvalidArchive();
            }
            Directory.CreateDirectory(folderPath);
            LogUtility.InitBar(1);
            uint offset = br.ReadUInt32() + 8;
            uint size = (uint)new FileInfo(filePath).Length - offset;
            fs.Seek(offset, SeekOrigin.Begin);
            byte[] buffer = br.ReadBytes((int)size);
            File.WriteAllBytes(Path.Combine(folderPath, Path.GetFileNameWithoutExtension(filePath) + ".mpg"), buffer);
            buffer = null;
            LogUtility.UpdateBar();
            fs.Dispose();
            br.Dispose();
        }
    }
}