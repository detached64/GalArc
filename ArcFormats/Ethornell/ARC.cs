using GalArc.Logs;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utility.Extensions;

namespace ArcFormats.Ethornell
{
    public class ARC : ArchiveFormat
    {
        private const string Magic20 = "BURIKO ARC20";
        private const string MagicDSCFormat100 = "DSC FORMAT 1.00\0";

        public override void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            if (Encoding.ASCII.GetString(br.ReadBytes(12)) != Magic20)
            {
                Logger.ErrorInvalidArchive();
            }
            int count = br.ReadInt32();
            List<Entry> entries = new List<Entry>(count);
            uint baseOffset = (uint)(16 + 0x80 * count);
            for (int i = 0; i < count; i++)
            {
                Entry entry = new Entry();
                entry.Name = Encoding.ASCII.GetString(br.ReadBytes(0x60)).TrimEnd('\0');
                entry.Offset = br.ReadUInt32() + baseOffset;
                entry.Size = br.ReadUInt32();
                br.BaseStream.Position += 0x18;
                entries.Add(entry);
            }
            Logger.InitBar(count);
            Directory.CreateDirectory(folderPath);
            foreach (Entry entry in entries)
            {
                br.BaseStream.Position = entry.Offset;
                byte[] data = br.ReadBytes((int)entry.Size);
                File.WriteAllBytes(Path.Combine(folderPath, entry.Name), Decrypt(data));
                Logger.UpdateBar();
            }
            fs.Dispose();
            br.Dispose();
        }

        public override void Pack(string folderPath, string filePath)
        {
            FileInfo[] files = new DirectoryInfo(folderPath).GetFiles();
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);
            bw.Write(Encoding.ASCII.GetBytes(Magic20));
            bw.Write(files.Length);
            uint baseOffset = 0;
            foreach (FileInfo file in files)
            {
                bw.WritePaddedString(file.Name, 0x60);
                bw.Write(baseOffset);
                uint size = (uint)file.Length;
                bw.Write(size);
                baseOffset += size;
                bw.Write(new byte[0x18]);
            }
            foreach (FileInfo file in files)
            {
                byte[] data = File.ReadAllBytes(file.FullName);
                bw.Write(data);
            }
            fw.Dispose();
            bw.Dispose();
        }

        private byte[] Decrypt(byte[] data)
        {
            switch (Encoding.ASCII.GetString(data, 0, 16))
            {
                case MagicDSCFormat100:
                    DscDecoder dscDecoder = new DscDecoder(data);
                    return dscDecoder.Decode();
                default:
                    return data;
            }
        }
    }
}
