using ArcFormats.Properties;
using Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Utility;

namespace ArcFormats.Softpal
{
    public class PAC
    {
        public static UserControl UnpackExtraOptions = new UnpackPACOptions();

        public static UserControl PackExtraOptions = new PackPACOptions();

        private static byte[] magicV2 = { 0x50, 0x41, 0x43, 0x20 };     //"PAC "
        private struct Entry
        {
            public string fileName { get; set; }
            public uint fileSize { get; set; }
            public uint offset { get; set; }
        }

        public void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            bool isVer1 = !br.ReadBytes(4).SequenceEqual(magicV2);
            fs.Dispose();
            br.Dispose();
            if (isVer1)
            {
                LogUtility.ShowVersion("pac", 1);
                pacV1_unpack(filePath, folderPath);
            }
            else
            {
                LogUtility.ShowVersion("pac", 2);
                pacV2_unpack(filePath, folderPath);
            }
        }

        private static void pacV1_unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            int fileCount = br.ReadUInt16();
            fs.Position = 0x3fe;
            List<Entry> entries = new List<Entry>();
            LogUtility.InitBar(fileCount);
            Directory.CreateDirectory(folderPath);

            for (int i = 0; i < fileCount; i++)
            {
                Entry entry = new Entry();
                entry.fileName = ArcEncoding.Shift_JIS.GetString(br.ReadBytes(32)).TrimEnd('\0');
                entry.fileSize = br.ReadUInt32();
                entry.offset = br.ReadUInt32();
                entries.Add(entry);
            }
            for (int i = 0; i < fileCount; i++)
            {
                byte[] data = br.ReadBytes((int)entries[i].fileSize);
                if (UnpackPACOptions.toDecryptScripts && data.Length >= 16 && data[0] == 36)  //'$'
                {
                    try
                    {
                        LogUtility.Debug(string.Format(Resources.logTryDecScr, entries[i].fileName));
                        DecryptScript(data);
                    }
                    catch
                    {
                        LogUtility.Error(Resources.logErrorDecScrFailed, false);
                    }
                }
                File.WriteAllBytes(Path.Combine(folderPath, entries[i].fileName), data);
                LogUtility.UpdateBar();
            }
            fs.Dispose();
        }

        private static void pacV2_unpack(string filePath, string folderPath)
        {
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            if (Encoding.ASCII.GetString(br.ReadBytes(4)) != "PAC ")
            {
                LogUtility.Error_NotValidArchive();
            }
            br.ReadInt32();
            uint fileCount = br.ReadUInt32();
            LogUtility.InitBar((int)fileCount);
            fs.Position = 0x804;
            Directory.CreateDirectory(folderPath);
            for (int i = 0; i < fileCount; i++)
            {
                Entry entry = new Entry();
                entry.fileName = ArcEncoding.Shift_JIS.GetString(br.ReadBytes(32)).TrimEnd('\0');
                entry.fileSize = br.ReadUInt32();
                entry.offset = br.ReadUInt32();
                long pos = fs.Position;
                fs.Position = entry.offset;
                byte[] fileData = br.ReadBytes((int)entry.fileSize);
                if (UnpackPACOptions.toDecryptScripts && fileData.Length >= 16 && fileData[0] == 36)  //'$'
                {
                    try
                    {
                        LogUtility.Debug(string.Format(Resources.logTryDecScr, entry.fileName));
                        DecryptScript(fileData);
                    }
                    catch
                    {
                        LogUtility.Error(Resources.logErrorDecScrFailed, false);
                    }
                }

                File.WriteAllBytes(folderPath + "\\" + entry.fileName, fileData);
                fs.Position = pos;
                LogUtility.UpdateBar();
            }
            fs.Dispose();
            br.Dispose();
        }

        private static void DecryptScript(byte[] data)
        {
            int count = (data.Length - 16) / 4;
            int shift = 4;
            for (int i = 0; i < count; i++)
            {
                int index = 16 + i * 4;
                uint value = BitConverter.ToUInt32(data, index);

                byte rotatedByte = Binary.RotByteL((byte)value, shift++);
                value = (value & 0xFFFFFF00u) | rotatedByte;
                value ^= 0x084DF873u ^ 0xFF987DEEu;

                byte[] bytes = BitConverter.GetBytes(value);
                Buffer.BlockCopy(bytes, 0, data, index, 4);
            }
        }

        public void Pack(string folderPath, string filePath)
        {
            if (Global.Version == "1")
            {
                pacV1_pack(folderPath, filePath);
            }
            else
            {
                pacV2_pack(folderPath, filePath);
            }
        }

        private static void pacV1_pack(string folderPath, string filePath)
        {
            string[] files = Directory.GetFiles(folderPath);
            var characterCount = CountFirstCharacters(files);
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);
            int fileCount = files.Length;
            LogUtility.InitBar(fileCount);
            LogWindow.Instance.bar.Maximum = fileCount + 1;

            bw.Write((ushort)fileCount);
            ushort countToThis = 0;
            for (int i = 0; i < 255; i++)
            {
                characterCount.TryGetValue((char)i, out int count);
                ushort thisCount = (ushort)count;
                bw.Write(countToThis);
                countToThis += thisCount;
                bw.Write(thisCount);
            }

            uint offset = 0x3fe + (uint)(40 * fileCount);
            foreach (var str in files)
            {
                bw.Write(ArcEncoding.Shift_JIS.GetBytes(Path.GetFileName(str).PadRight(32, '\0')));
                uint size = (uint)new FileInfo(str).Length;
                bw.Write(size);
                bw.Write(offset);
                offset += size;
            }

            foreach (var str in files)
            {
                bw.Write(File.ReadAllBytes(str));
                LogUtility.UpdateBar();
            }

            bw.Write(Encoding.ASCII.GetBytes("EOF "));
            fw.Dispose();
            bw.Dispose();
        }

        private static void pacV2_pack(string folderPath, string filePath)
        {
            string[] files = Directory.GetFiles(folderPath);
            var characterCount = CountFirstCharacters(files);
            uint fileCount = (uint)files.Length;

            LogUtility.InitBar(fileCount);
            LogWindow.Instance.bar.Maximum = (int)fileCount + 1;

            FileStream fw = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite);
            BinaryWriter bw = new BinaryWriter(fw);
            //header
            bw.Write(Encoding.ASCII.GetBytes("PAC "));
            bw.Write(0);
            bw.Write(fileCount);
            //index
            int countToThis = 0;
            for (int i = 0; i < 255; i++)
            {
                characterCount.TryGetValue((char)i, out int count);
                bw.Write(countToThis);
                countToThis += count;
                bw.Write(count);
            }
            //entries
            uint currentOffset = 2052 + 40 * fileCount;
            foreach (string str in files)
            {
                bw.Write(ArcEncoding.Shift_JIS.GetBytes(Path.GetFileName(str).PadRight(32, '\0')));
                uint size = (uint)new FileInfo(str).Length;
                bw.Write(size);
                bw.Write(currentOffset);
                currentOffset += size;
            }
            //data
            foreach (string str in files)
            {
                byte[] fileData = File.ReadAllBytes(str);
                bw.Write(fileData);
                LogUtility.UpdateBar();
            }
            //end
            if (PackPACOptions.toCompute)
            {
                uint checksum = 0;
                fw.Position = 0;
                using (BufferedStream bufferedStream = new BufferedStream(fw, 65536))
                {
                    int b;
                    while ((b = bufferedStream.ReadByte()) != -1)
                    {
                        checksum += (uint)b;
                    }
                    bw.Write(checksum);
                    bw.Write(Encoding.ASCII.GetBytes("EOF "));
                }
            }
            else
            {
                bw.Write(0);
                bw.Write(Encoding.ASCII.GetBytes("EOF "));
            }
            LogUtility.UpdateBar();
            fw.Dispose();
            bw.Dispose();
        }

        private static Dictionary<char, int> CountFirstCharacters(string[] strings)
        {
            var countDict = new Dictionary<char, int>();

            foreach (var str in strings)
            {
                if (str.Length > 0)
                {
                    char firstChar = Path.GetFileName(str)[0];
                    if (countDict.ContainsKey(firstChar))
                    {
                        countDict[firstChar]++;
                    }
                    else
                    {
                        countDict[firstChar] = 1;
                    }
                }
            }
            return countDict;
        }
    }
}