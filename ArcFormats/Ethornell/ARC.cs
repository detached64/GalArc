using GalArc.Controls;
using GalArc.Logs;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utility;
using Utility.Exceptions;
using Utility.Extensions;

namespace ArcFormats.Ethornell
{
    public class ARC : ArcFormat
    {
        public override OptionsTemplate PackExtraOptions => PackARCOptions.Instance;

        private const string Magic = "PackFile    ";
        private const string Magic20 = "BURIKO ARC20";
        private const string MagicDSCFormat100 = "DSC FORMAT 1.00\0";

        public override void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            int version = 0;
            switch (Encoding.ASCII.GetString(br.ReadBytes(12)))
            {
                case Magic:
                    version = 1;
                    break;
                case Magic20:
                    version = 2;
                    break;
                default:
                    throw new InvalidArchiveException();
            }
            Logger.ShowVersion("arc", version);
            int count = br.ReadInt32();
            List<Entry> entries = new List<Entry>(count);
            uint baseOffset = (uint)(16 + (version == 1 ? 0x20 : 0x80) * count);
            for (int i = 0; i < count; i++)
            {
                Entry entry = new Entry();
                entry.Name = ArcEncoding.Shift_JIS.GetString(br.ReadBytes(version == 1 ? 0x10 : 0x60)).TrimEnd('\0');
                entry.Offset = br.ReadUInt32() + baseOffset;
                entry.Size = br.ReadUInt32();
                br.BaseStream.Position += version == 1 ? 8 : 0x18;
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
                data = null;
            }
            fs.Dispose();
            br.Dispose();
        }

        public override void Pack(string folderPath, string filePath)
        {
            FileInfo[] files = new DirectoryInfo(folderPath).GetFiles();
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);
            string magic = PackExtraOptions.Version == "1" ? Magic : Magic20;
            bw.Write(Encoding.ASCII.GetBytes(magic));
            bw.Write(files.Length);
            uint baseOffset = 0;
            foreach (FileInfo file in files)
            {
                bw.WritePaddedString(file.Name, PackExtraOptions.Version == "1" ? 0x10 : 0x60);
                bw.Write(baseOffset);
                uint size = (uint)file.Length;
                bw.Write(size);
                baseOffset += size;
                bw.Write(new byte[PackExtraOptions.Version == "1" ? 8 : 0x18]);
            }
            foreach (FileInfo file in files)
            {
                byte[] data = File.ReadAllBytes(file.FullName);
                bw.Write(data);
                data = null;
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
