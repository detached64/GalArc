using Log;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ArcFormats.InnocentGrey
{
    public class IGA
    {
        private class Header
        {
            public byte[] magic { get; set; }   //IGA0
            public uint unknown1 { get; set; }  //checksum?
            public uint unknown2 { get; set; }  //2
            public uint unknown3 { get; set; }  //2
        }

        private class Entry
        {
            public uint nameOffset { get; set; }
            public uint dataOffset { get; set; }
            public uint fileSize { get; set; }
            public string fileName { get; set; }
            public uint nameLen { get; set; }
        }

        public void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            List<Entry> entries = new List<Entry>();
            List<Entry> entriesUpdate = new List<Entry>();
            if (Encoding.ASCII.GetString(br.ReadBytes(4)) != "IGA0")
            {
                LogUtility.ErrorInvalidArchive();
            }

            fs.Position = 16;
            uint indexSize = VarInt.UnpackUint(br);

            long endPos = fs.Position + indexSize;
            while (fs.Position < endPos)
            {
                var entry = new Entry();
                entry.nameOffset = VarInt.UnpackUint(br);
                entry.dataOffset = VarInt.UnpackUint(br);
                entry.fileSize = VarInt.UnpackUint(br);
                entries.Add(entry);
            }

            LogUtility.InitBar(entries.Count);
            uint nameIndexSize = VarInt.UnpackUint(br);
            long endName = fs.Position + nameIndexSize;

            Directory.CreateDirectory(folderPath);

            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                uint thisNameLen;
                if (i + 1 < entries.Count)
                {
                    thisNameLen = entries[i + 1].nameOffset - entries[i].nameOffset;
                }
                else
                {
                    thisNameLen = nameIndexSize - entries[i].nameOffset;
                }

                entry.fileName = VarInt.UnpackString(br, thisNameLen);
                entry.dataOffset += (uint)endName;
                entriesUpdate.Add(entry);
            }

            foreach (var entry in entriesUpdate)
            {
                fs.Position = entry.dataOffset;
                byte[] buffer = new byte[entry.fileSize];
                br.Read(buffer, 0, (int)entry.fileSize);
                int key = Path.GetExtension(entry.fileName) == ".s" ? 0xFF : 0; //decrypt script file
                for (uint j = 0; j < entry.fileSize; j++)
                {
                    buffer[j] ^= (byte)((j + 2) ^ key);
                }
                File.WriteAllBytes(Path.Combine(folderPath, entry.fileName), buffer);
                LogUtility.UpdateBar();
            }
            fs.Dispose();
            br.Dispose();
        }

        public void Pack(string folderPath, string filePath)
        {
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);
            bw.Write(Encoding.ASCII.GetBytes("IGA0"));
            bw.Write(0);    // don't know accurate value , set to 0
            bw.Write(2);
            bw.Write(2);
            List<Entry> l = new List<Entry>();

            DirectoryInfo dir = new DirectoryInfo(folderPath);
            FileInfo[] files = dir.GetFiles();
            uint nameOffset = 0;
            uint dataOffset = 0;
            foreach (FileInfo file in files)
            {
                Entry entry = new Entry();
                entry.fileName = file.Name;
                entry.nameLen = (uint)entry.fileName.Length;
                entry.nameOffset = nameOffset;
                entry.dataOffset = dataOffset;
                entry.fileSize = (uint)file.Length;
                l.Add(entry);
                nameOffset += entry.nameLen;
                dataOffset += entry.fileSize;
            }
            int fileCount = files.Length;
            LogUtility.InitBar(fileCount);

            using (MemoryStream msEntry = new MemoryStream())
            {
                using (BinaryWriter bwEntry = new BinaryWriter(msEntry))
                {
                    using (MemoryStream msFileName = new MemoryStream())
                    {
                        using (BinaryWriter bwFileName = new BinaryWriter(msFileName))
                        {
                            foreach (var i in l)
                            {
                                bwEntry.Write(VarInt.PackUint(i.nameOffset));
                                bwEntry.Write(VarInt.PackUint(i.dataOffset));
                                bwEntry.Write(VarInt.PackUint(i.fileSize));
                                bwFileName.Write(VarInt.PackString(i.fileName));
                            }
                            bw.Write(VarInt.PackUint((uint)msEntry.Length));
                            msEntry.WriteTo(fw);

                            bw.Write(VarInt.PackUint((uint)msFileName.Length));
                            msFileName.WriteTo(fw);
                        }
                    }
                }
            }

            foreach (var entry in l)
            {
                byte[] buffer = File.ReadAllBytes(folderPath + "\\" + entry.fileName);
                int key = Path.GetExtension(entry.fileName) == ".s" ? 0xFF : 0;
                for (uint j = 0; j < entry.fileSize; j++)
                {
                    buffer[j] ^= (byte)((j + 2) ^ key);
                }
                bw.Write(buffer);
                LogUtility.UpdateBar();
            }
            fw.Dispose();
            bw.Dispose();
        }
    }

    internal class VarInt
    {
        public static uint UnpackUint(BinaryReader br)
        {
            uint value = 0;
            while ((value & 1) == 0)
            {
                value = value << 7 | br.ReadByte();
            }
            return value >> 1;
        }

        public static string UnpackString(BinaryReader br, uint length)
        {
            var bytes = new byte[length];
            for (uint i = 0; i < length; ++i)
            {
                bytes[i] = (byte)UnpackUint(br);
            }
            return Encoding.GetEncoding(932).GetString(bytes);
        }

        public static byte[] PackUint(uint a)
        {
            List<byte> result = new List<byte>();
            uint v = a;

            if (v == 0)
            {
                result.Add(0x01);
                return result.ToArray();
            }

            v = (v << 1) + 1;
            byte curByte = (byte)(v & 0xFF);
            while ((v & 0xFFFFFFFFFFFFFFFE) != 0)
            {
                result.Add(curByte);
                v >>= 7;
                curByte = (byte)(v & 0xFE);
            }

            result.Reverse();
            return result.ToArray();
        }

        public static byte[] PackString(string s)
        {
            byte[] bytes = Encoding.GetEncoding(932).GetBytes(s);
            List<byte> rst = new List<byte>();
            foreach (byte b in bytes)
            {
                rst.AddRange(PackUint(b));
            }
            return rst.ToArray();
        }

    }
}