using Log;
using System.IO;

namespace ArcFormats.Triangle
{
    public class SUD
    {
        public void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            Directory.CreateDirectory(folderPath);
            int index = 1;
            while (fs.Position < fs.Length)
            {
                uint size = br.ReadUInt32();
                byte[] data = br.ReadBytes((int)size);
                File.WriteAllBytes(folderPath + "\\" + index.ToString("D6") + ".ogg", data);
                index++;
            }
            fs.Dispose();
            br.Dispose();
        }

        public void Pack(string folderPath, string filePath)
        {
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);
            string[] files = Directory.GetFiles(folderPath, ".ogg");
            LogUtility.InitBar(files.Length);
            foreach (string file in files)
            {
                bw.Write((uint)new FileInfo(file).Length);
                bw.Write(File.ReadAllBytes(file));
                LogUtility.UpdateBar();
            }
            fw.Dispose();
            bw.Dispose();
        }
    }
}