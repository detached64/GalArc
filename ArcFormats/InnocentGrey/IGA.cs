using GalArc.Controls;
using GalArc.Logs;
using GalArc.Strings;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utility.Exceptions;

namespace ArcFormats.InnocentGrey
{
    public class IGA : ArcFormat
    {
        public override WidgetTemplate UnpackWidget => UnpackIGAWidget.Instance;
        public override WidgetTemplate PackWidget => PackIGAWidget.Instance;

        private const string Magic = "IGA0";

        private class InnoIgaEntry : Entry
        {
            public uint NameOffset { get; set; }
            public uint DataOffset { get; set; }
            public uint NameLen { get; set; }
        }

        public override void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            List<InnoIgaEntry> entries = new List<InnoIgaEntry>();
            List<InnoIgaEntry> entriesUpdate = new List<InnoIgaEntry>();
            if (Encoding.ASCII.GetString(br.ReadBytes(4)) != Magic)
            {
                throw new InvalidArchiveException();
            }

            fs.Position = 16;
            uint indexSize = VarInt.UnpackUint(br);

            long endPos = fs.Position + indexSize;
            while (fs.Position < endPos)
            {
                var entry = new InnoIgaEntry();
                entry.NameOffset = VarInt.UnpackUint(br);
                entry.DataOffset = VarInt.UnpackUint(br);
                entry.Size = VarInt.UnpackUint(br);
                entries.Add(entry);
            }

            Logger.InitBar(entries.Count);
            uint nameIndexSize = VarInt.UnpackUint(br);
            long endName = fs.Position + nameIndexSize;

            Directory.CreateDirectory(folderPath);

            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                uint thisNameLen;
                if (i + 1 < entries.Count)
                {
                    thisNameLen = entries[i + 1].NameOffset - entries[i].NameOffset;
                }
                else
                {
                    thisNameLen = nameIndexSize - entries[i].NameOffset;
                }

                entry.Name = VarInt.UnpackString(br, thisNameLen);
                entry.DataOffset += (uint)endName;
                entriesUpdate.Add(entry);
            }

            foreach (var entry in entriesUpdate)
            {
                fs.Position = entry.DataOffset;
                byte[] buffer = new byte[entry.Size];
                br.Read(buffer, 0, (int)entry.Size);
                int key = UnpackIGAWidget.DecryptScripts && Path.GetExtension(entry.Name) == ".s" ? 0xFF : 0;
                if (key != 0)
                {
                    Logger.Debug(string.Format(LogStrings.TryDecScr, entry.Name));
                }
                for (uint j = 0; j < entry.Size; j++)
                {
                    buffer[j] ^= (byte)((j + 2) ^ key);
                }
                File.WriteAllBytes(Path.Combine(folderPath, entry.Name), buffer);
                buffer = null;
                Logger.UpdateBar();
            }
            fs.Dispose();
            br.Dispose();
        }

        public override void Pack(string folderPath, string filePath)
        {
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);
            bw.Write(Encoding.ASCII.GetBytes(Magic));
            bw.Write(0);    // don't know accurate value , set to 0
            bw.Write(2);
            bw.Write(2);
            List<InnoIgaEntry> l = new List<InnoIgaEntry>();

            DirectoryInfo dir = new DirectoryInfo(folderPath);
            FileInfo[] files = dir.GetFiles();
            uint nameOffset = 0;
            uint dataOffset = 0;
            foreach (FileInfo file in files)
            {
                InnoIgaEntry entry = new InnoIgaEntry();
                entry.Name = file.Name;
                entry.NameLen = (uint)entry.Name.Length;
                entry.NameOffset = nameOffset;
                entry.DataOffset = dataOffset;
                entry.Size = (uint)file.Length;
                l.Add(entry);
                nameOffset += entry.NameLen;
                dataOffset += entry.Size;
            }
            int fileCount = files.Length;
            Logger.InitBar(fileCount);

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
                                bwEntry.Write(VarInt.PackUint(i.NameOffset));
                                bwEntry.Write(VarInt.PackUint(i.DataOffset));
                                bwEntry.Write(VarInt.PackUint(i.Size));
                                bwFileName.Write(VarInt.PackString(i.Name));
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
                byte[] buffer = File.ReadAllBytes(Path.Combine(folderPath, entry.Name));
                int key = PackIGAWidget.EncryptScripts && Path.GetExtension(entry.Name) == ".s" ? 0xFF : 0;
                if (key != 0)
                {
                    Logger.Debug(string.Format(LogStrings.TryEncScr, entry.Name));
                }
                for (uint j = 0; j < entry.Size; j++)
                {
                    buffer[j] ^= (byte)((j + 2) ^ key);
                }
                bw.Write(buffer);
                buffer = null;
                Logger.UpdateBar();
            }
            fw.Dispose();
            bw.Dispose();
        }
    }

    internal static class VarInt
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