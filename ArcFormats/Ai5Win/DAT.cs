using Log;
using System;
using System.IO;
using System.Linq;
using Utility;

namespace ArcFormats.Ai5Win
{
    public class DAT
    {
        private readonly static int NameLength = 0x14;
        public static void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);

            fs.Position = 4;
            uint key = br.ReadUInt32();
            fs.Position = 0;
            int fileCount = (int)(br.ReadInt32() ^ key);
            fs.Position = 0x23;
            ArcScheme scheme = new ArcScheme()
            {
                nameKey = br.ReadByte(),
                sizeKey = key,
                offsetKey = key
            };

            fs.Position = 8;
            LogUtility.InitBar(fileCount);
            Directory.CreateDirectory(folderPath);

            for (int i = 0; i < fileCount; i++)
            {
                uint size = br.ReadUInt32() ^ scheme.sizeKey;
                uint offset = br.ReadUInt32() ^ scheme.offsetKey;
                string name = ArcEncoding.Shift_JIS.GetString(Xor.xor(br.ReadBytes(NameLength), scheme.nameKey)).TrimEnd('\0');
                string path = Path.Combine(folderPath, name);
                long pos = fs.Position;
                fs.Position = offset;
                File.WriteAllBytes(path, br.ReadBytes((int)size));
                fs.Position = pos;
                LogUtility.UpdateBar();
            }

            fs.Dispose();
            br.Dispose();
        }
    }
}
