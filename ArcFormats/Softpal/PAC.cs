using ArcFormats.Properties;
using GalArc.Controls;
using GalArc.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Utility;
using Utility.Extensions;

namespace ArcFormats.Softpal
{
    public class PAC : ArchiveFormat
    {
        private static readonly Lazy<OptionsTemplate> _lazyUnpackOptions = new Lazy<OptionsTemplate>(() => new UnpackPACOptions());
        private static readonly Lazy<OptionsTemplate> _lazyPackOptions = new Lazy<OptionsTemplate>(() => new PackPACOptions());
        public static OptionsTemplate UnpackExtraOptions => _lazyUnpackOptions.Value;
        public static OptionsTemplate PackExtraOptions => _lazyPackOptions.Value;

        private readonly byte[] Magic = Utils.HexStringToByteArray("50414320");

        public override void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            bool isVer1 = !br.ReadBytes(4).SequenceEqual(Magic);
            fs.Dispose();
            br.Dispose();
            if (isVer1)
            {
                Logger.ShowVersion("pac", 1);
                UnpackV1(filePath, folderPath);
            }
            else
            {
                Logger.ShowVersion("pac", 2);
                UnpackV2(filePath, folderPath);
            }
        }

        private void UnpackV1(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            int fileCount = br.ReadUInt16();
            fs.Position = 0x3fe;
            List<Entry> entries = new List<Entry>();
            Logger.InitBar(fileCount);
            Directory.CreateDirectory(folderPath);

            for (int i = 0; i < fileCount; i++)
            {
                Entry entry = new Entry();
                entry.Name = ArcEncoding.Shift_JIS.GetString(br.ReadBytes(32)).TrimEnd('\0');
                entry.Size = br.ReadUInt32();
                entry.Offset = br.ReadUInt32();
                entries.Add(entry);
            }
            foreach (Entry entry in entries)
            {
                byte[] data = br.ReadBytes((int)entry.Size);
                if (UnpackPACOptions.toDecryptScripts && data.Length >= 16 && data[0] == 36)  //'$'
                {
                    try
                    {
                        Logger.Debug(string.Format(Resources.logTryDecScr, entry.Name));
                        DecryptScript(data);
                    }
                    catch
                    {
                        Logger.Error(Resources.logErrorDecScrFailed, false);
                    }
                }
                File.WriteAllBytes(Path.Combine(folderPath, entry.Name), data);
                data = null;
                Logger.UpdateBar();
            }
            fs.Dispose();
        }

        private void UnpackV2(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            if (!br.ReadBytes(4).SequenceEqual(Magic))
            {
                Logger.ErrorInvalidArchive();
            }
            br.ReadInt32();
            uint fileCount = br.ReadUInt32();
            Logger.InitBar(fileCount);
            fs.Position = 0x804;
            Directory.CreateDirectory(folderPath);
            for (int i = 0; i < fileCount; i++)
            {
                Entry entry = new Entry();
                entry.Name = ArcEncoding.Shift_JIS.GetString(br.ReadBytes(32)).TrimEnd('\0');
                entry.Size = br.ReadUInt32();
                entry.Offset = br.ReadUInt32();
                long pos = fs.Position;
                fs.Position = entry.Offset;
                byte[] fileData = br.ReadBytes((int)entry.Size);
                if (UnpackPACOptions.toDecryptScripts && fileData.Length >= 16 && fileData[0] == 36)  //'$'
                {
                    try
                    {
                        Logger.Debug(string.Format(Resources.logTryDecScr, entry.Name));
                        DecryptScript(fileData);
                    }
                    catch
                    {
                        Logger.Error(Resources.logErrorDecScrFailed, false);
                    }
                }

                File.WriteAllBytes(folderPath + "\\" + entry.Name, fileData);
                fileData = null;
                fs.Position = pos;
                Logger.UpdateBar();
            }
            fs.Dispose();
            br.Dispose();
        }

        private void DecryptScript(byte[] data)
        {
            int count = (data.Length - 16) / 4;
            int shift = 4;
            for (int i = 0; i < count; i++)
            {
                int index = 16 + i * 4;
                uint value = BitConverter.ToUInt32(data, index);

                byte rotatedByte = Binary.RotByteL((byte)value, shift++);
                value = (value & 0xFFFFFF00u) | rotatedByte;            // 高24位不变，低8位左旋
                value ^= 0x084DF873u ^ 0xFF987DEEu;

                byte[] bytes = BitConverter.GetBytes(value);
                Buffer.BlockCopy(bytes, 0, data, index, 4);
            }
        }

        private void EncryptScript(byte[] data)
        {
            int count = (data.Length - 16) / 4;
            int shift = 4;
            for (int i = 0; i < count; i++)
            {
                int index = 16 + i * 4;
                uint value = BitConverter.ToUInt32(data, index);
                value ^= 0x084DF873u ^ 0xFF987DEEu;

                byte rotatedByte = Binary.RotByteR((byte)value, shift++);
                value = (value & 0xFFFFFF00u) | rotatedByte;            // 高24位不变，低8位右旋

                byte[] bytes = BitConverter.GetBytes(value);
                Buffer.BlockCopy(bytes, 0, data, index, 4);
            }
        }

        public override void Pack(string folderPath, string filePath)
        {
            if (PackExtraOptions.Version == "1")
            {
                PackV1(folderPath, filePath);
            }
            else
            {
                PackV2(folderPath, filePath);
            }
        }

        private void PackV1(string folderPath, string filePath)
        {
            string[] files = Directory.GetFiles(folderPath);
            var characterCount = CountFirstCharacters(files);
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);
            int fileCount = files.Length;
            Logger.InitBar(fileCount);
            Logger.SetBarMax(fileCount + 1);

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
                bw.WritePaddedString(Path.GetFileName(str), 32);
                uint size = (uint)new FileInfo(str).Length;
                bw.Write(size);
                bw.Write(offset);
                offset += size;
            }

            foreach (var str in files)
            {
                byte[] buffer = File.ReadAllBytes(str);
                if (PackPACOptions.toEncryptScripts && buffer.Length >= 16 && buffer[0] == 36)  //'$'
                {
                    try
                    {
                        Logger.Debug(string.Format(Resources.logTryEncScr, Path.GetFileName(str)));
                        EncryptScript(buffer);
                    }
                    catch
                    {
                        Logger.Error(Resources.logErrorEncScrFailed, false);
                    }
                }
                bw.Write(buffer);
                buffer = null;
                Logger.UpdateBar();
            }

            bw.Write(Encoding.ASCII.GetBytes("EOF "));
            fw.Dispose();
            bw.Dispose();
        }

        private void PackV2(string folderPath, string filePath)
        {
            string[] files = Directory.GetFiles(folderPath);
            var characterCount = CountFirstCharacters(files);
            uint fileCount = (uint)files.Length;

            Logger.InitBar(fileCount);
            Logger.SetBarMax((int)fileCount + 1);

            FileStream fw = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite);
            BinaryWriter bw = new BinaryWriter(fw);
            //header
            bw.Write(Magic);
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
                bw.WritePaddedString(Path.GetFileName(str), 32);
                uint size = (uint)new FileInfo(str).Length;
                bw.Write(size);
                bw.Write(currentOffset);
                currentOffset += size;
            }
            //data
            foreach (string str in files)
            {
                byte[] buffer = File.ReadAllBytes(str);
                if (PackPACOptions.toEncryptScripts && buffer.Length >= 16 && buffer[0] == 36)  //'$'
                {
                    try
                    {
                        Logger.Debug(string.Format(Resources.logTryEncScr, Path.GetFileName(str)));
                        EncryptScript(buffer);
                    }
                    catch
                    {
                        Logger.Error(Resources.logErrorEncScrFailed, false);
                    }
                }
                bw.Write(buffer);
                buffer = null;
                Logger.UpdateBar();
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
            Logger.UpdateBar();
            fw.Dispose();
            bw.Dispose();
        }

        private Dictionary<char, int> CountFirstCharacters(string[] strings)
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