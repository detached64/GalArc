using GalArc.Logs;
using System;
using System.IO;
using Utility;

namespace ArcFormats.Ai5Win
{
    public class DAT : ARC
    {
        private readonly int NameLength = 0x14;

        public override void Unpack(string filePath, string folderPath)
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
                NameKey = br.ReadByte(),
                SizeKey = key,
                OffsetKey = key
            };

            fs.Position = 8;
            Logger.InitBar(fileCount);
            Directory.CreateDirectory(folderPath);

            for (int i = 0; i < fileCount; i++)
            {
                uint size = br.ReadUInt32() ^ scheme.SizeKey;
                uint offset = br.ReadUInt32() ^ scheme.OffsetKey;
                byte[] nameBuffer = br.ReadBytes(NameLength);
                for (int j = 0; j < NameLength; j++)
                {
                    nameBuffer[j] ^= scheme.NameKey;
                }
                string name = ArcEncoding.Shift_JIS.GetString(nameBuffer).TrimEnd('\0');
                string path = Path.Combine(folderPath, name);
                long pos = fs.Position;
                fs.Position = offset;
                byte[] data = br.ReadBytes((int)size);
                File.WriteAllBytes(path, data);
                data = null;
                fs.Position = pos;
                Logger.UpdateBar();
            }

            fs.Dispose();
            br.Dispose();
        }
    }
}
