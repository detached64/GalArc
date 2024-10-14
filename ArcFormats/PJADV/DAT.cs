using ArcFormats.Properties;
using Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Utility;

namespace ArcFormats.PJADV
{
    public class DAT
    {
        public static UserControl UnpackExtraOptions = new UnpackDATOptions();

        public static UserControl PackExtraOptions = new PackDATOptions();

        private class Entry
        {
            public string name;
            public string path;
            public uint offset;
            public uint size;
        }

        public void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);

            if (Encoding.ASCII.GetString(br.ReadBytes(11)) != "GAMEDAT PAC")
            {
                LogUtility.ErrorInvalidArchive();
            }
            int version = br.ReadByte();
            int nameLen;
            if (version == 'K')
            {
                nameLen = 16;
                LogUtility.ShowVersion("dat", 1);
            }
            else if (version == '2')
            {
                nameLen = 32;
                LogUtility.ShowVersion("dat", 2);
            }
            else
            {
                LogUtility.Error($"Version {version} not supported");
                return;
            }

            int count = br.ReadInt32();
            int baseOffset = 0x10 + count * (nameLen + 8);
            List<Entry> entries = new List<Entry>();
            LogUtility.InitBar(count);
            Directory.CreateDirectory(folderPath);

            for (int i = 0; i < count; i++)
            {
                Entry entry = new Entry();
                entry.name = ArcEncoding.Shift_JIS.GetString(br.ReadBytes(nameLen)).TrimEnd('\0');
                entry.path = Path.Combine(folderPath, entry.name);
                entries.Add(entry);
            }
            for (int i = 0; i < count; i++)
            {
                entries[i].offset = br.ReadUInt32() + (uint)baseOffset;
                entries[i].size = br.ReadUInt32();
            }
            foreach (Entry entry in entries)
            {
                fs.Position = entry.offset;
                byte[] buffer = br.ReadBytes((int)entry.size);
                if (UnpackDATOptions.toDecryptScripts && entry.name.Contains("textdata") && buffer.Take(5).ToArray().SequenceEqual(new byte[] { 0x95, 0x6b, 0x3c, 0x9d, 0x63 }))
                {
                    LogUtility.Debug(string.Format(Resources.logTryDecScr, entry.name));
                    DecryptScript(buffer);
                }
                File.WriteAllBytes(entry.path, buffer);
                LogUtility.UpdateBar();
            }
            fs.Dispose();
            br.Dispose();
        }

        public void Pack(string folderPath, string filePath)
        {
            FileStream fs = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(Encoding.ASCII.GetBytes("GAMEDAT PAC"));
            int nameLength = Global.Version == "1" ? 16 : 32;
            bw.Write(Global.Version == "1" ? (byte)'K' : (byte)'2');
            DirectoryInfo d = new DirectoryInfo(folderPath);
            FileInfo[] files = d.GetFiles();

            bw.Write(files.Length);
            List<Entry> entries = new List<Entry>();
            LogUtility.InitBar(files.Length);
            uint thisOffset = 0;

            foreach (FileInfo file in files)
            {
                Entry entry = new Entry();
                entry.name = file.Name;
                entry.path = file.FullName;
                entry.size = (uint)file.Length;
                entry.offset = thisOffset;
                thisOffset += entry.size;
                entries.Add(entry);
            }
            foreach (Entry entry in entries)
            {
                bw.Write(ArcEncoding.Shift_JIS.GetBytes(entry.name.PadRight(nameLength, '\0')));
            }
            foreach (Entry entry in entries)
            {
                bw.Write(entry.offset);
                bw.Write(entry.size);
            }
            foreach (Entry entry in entries)
            {
                byte[] buffer = File.ReadAllBytes(entry.path);
                if (PackDATOptions.toEncryptScripts && entry.name.Contains("textdata") && buffer.Take(5).ToArray().SequenceEqual(new byte[] { 0x95, 0x6b, 0x3c, 0x9d, 0x63 }))
                {
                    LogUtility.Debug(string.Format(Resources.logTryEncScr, entry.name));
                    DecryptScript(buffer);
                }
                bw.Write(buffer);
                LogUtility.UpdateBar();
            }
            fs.Dispose();
            bw.Dispose();
        }

        private static void DecryptScript(byte[] data)
        {
            byte key = 0xC5;
            for (int i = 0; i < data.Length; ++i)
            {
                data[i] ^= key;
                key += 0x5C;
            }
        }
    }

    public class PAK : DAT
    {
        public static new UserControl UnpackExtraOptions = DAT.UnpackExtraOptions;

        public static new UserControl PackExtraOptions = DAT.PackExtraOptions;
    }

}
