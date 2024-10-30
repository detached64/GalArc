using GalArc.Logs;
using System.IO;
using System.Linq;
using Utility;

namespace ArcFormats.Palette
{
    public class PAK
    {
        private static readonly byte[] Magic = Utils.HexStringToByteArray("055041434b32");

        public void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            if (!br.ReadBytes(6).SequenceEqual(Magic))
            {
                Logger.ErrorInvalidArchive();
            }
            int count = br.ReadInt32();
            Logger.InitBar(count);
            Directory.CreateDirectory(folderPath);

            for (int i = 0; i < count; i++)
            {
                byte nameLen = br.ReadByte();
                byte[] buffer = br.ReadBytes(nameLen);
                for (int j = 0; j < nameLen; j++)
                {
                    buffer[j] ^= 0xff;
                }
                string name = ArcEncoding.Shift_JIS.GetString(buffer);
                int offset = br.ReadInt32();
                int size = br.ReadInt32();
                long pos = fs.Position;
                fs.Position = offset;
                byte[] data = br.ReadBytes(size);
                File.WriteAllBytes(Path.Combine(folderPath, name), data);
                data = null;
                fs.Position = pos;
                Logger.UpdateBar();
            }
            fs.Dispose();
            br.Dispose();
        }

        public void Pack(string folderPath, string filePath)
        {
            FileStream fs = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fs);

            string[] files = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly);
            int nameLenSum = Utils.GetNameLengthSum(files, ArcEncoding.Shift_JIS);
            int fileCount = files.Length;
            int baseOffset = 10 + nameLenSum + 9 * fileCount;

            bw.Write(Magic);
            bw.Write(fileCount);
            Logger.InitBar(fileCount);

            foreach (string file in files)
            {
                byte[] name = ArcEncoding.Shift_JIS.GetBytes(Path.GetFileName(file));
                int fileSize = (int)new FileInfo(file).Length;
                bw.Write((byte)name.Length);
                for (int i = 0; i < name.Length; i++)
                {
                    bw.Write((byte)(name[i] ^ 0xff));
                }
                bw.Write(name);
                bw.Write(baseOffset);
                baseOffset += fileSize;
                bw.Write(fileSize);
                Logger.UpdateBar();
            }
            foreach (string file in files)
            {
                byte[] data = File.ReadAllBytes(file);
                bw.Write(data);
                data = null;
            }
            fs.Dispose();
            bw.Dispose();
        }
    }
}