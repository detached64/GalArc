using ArcFormats.Templates;
using Log;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Utility;

namespace ArcFormats.BiShop
{
    public class BSA
    {
        public static UserControl PackExtraOptions = new VersionOnly("1/2");

        private static byte[] magic = Utilities.HexStringToByteArray("4253417263000000");

        private static List<string> m_path = new List<string> { };

        private static int RealCount = 0;

        private static string RootDir = string.Empty;

        private static int FileCount = 0;

        private struct Entry
        {
            internal int nameOffset { get; set; }
            internal uint dataOffset { get; set; }
            internal uint dataSize { get; set; }
        }

        public void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);

            if (!br.ReadBytes(8).SequenceEqual(magic))
            {
                LogUtility.ErrorInvalidArchive();
            }

            ushort version = br.ReadUInt16();
            fs.Dispose();
            br.Dispose();
            if (version > 1)
            {
                LogUtility.ShowVersion("bsa", 2);
                bsaV2_unpack(filePath, folderPath);
            }
            else if (version == 1)
            {
                LogUtility.ShowVersion("bsa", 1);
                bsaV1_unpack(filePath, folderPath);
            }
            else
            {
                LogUtility.ErrorInvalidArchive();
            }
        }

        private static void bsaV1_unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            br.ReadBytes(10);
            ushort fileCount = br.ReadUInt16();
            uint indexOffset = br.ReadUInt32();
            Directory.CreateDirectory(folderPath);
            LogUtility.InitBar(fileCount);

            fs.Seek(indexOffset, SeekOrigin.Begin);
            for (int i = 0; i < fileCount; i++)
            {
                string path = Path.Combine(folderPath, ArcEncoding.Shift_JIS.GetString(br.ReadBytes(32)).TrimEnd('\0'));
                uint dataOffset = br.ReadUInt32();
                uint dataSize = br.ReadUInt32();
                long pos = fs.Position;
                fs.Position = dataOffset;
                File.WriteAllBytes(path, br.ReadBytes((int)dataSize));
                fs.Position = pos;
                LogUtility.UpdateBar();
            }
            fs.Dispose();
            br.Dispose();
        }

        private static void bsaV2_unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            br.ReadBytes(10);
            ushort fileCount = br.ReadUInt16();
            uint indexOffset = br.ReadUInt32();
            uint nameOffset = indexOffset + (uint)fileCount * 12;
            Directory.CreateDirectory(folderPath);
            LogUtility.InitBar(fileCount);

            fs.Seek(indexOffset, SeekOrigin.Begin);
            m_path.Clear();
            RealCount = 0;
            m_path.Add(folderPath);

            for (int i = 0; i < fileCount; i++)
            {
                Entry entry = new Entry();

                entry.nameOffset = br.ReadInt32() + (int)nameOffset;
                entry.dataOffset = br.ReadUInt32();
                entry.dataSize = br.ReadUInt32();

                long pos = fs.Position;
                fs.Position = entry.nameOffset;
                string name = Utilities.ReadCString(br, ArcEncoding.Shift_JIS);
                if (name[0] == '>')
                {
                    m_path.Add(name.Substring(1));
                }
                else if (name[0] == '<')
                {
                    m_path.RemoveAt(m_path.Count - 1);
                }
                else
                {
                    fs.Position = entry.dataOffset;
                    string path = Path.Combine(Path.Combine(m_path.ToArray()), name);
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    File.WriteAllBytes(path, br.ReadBytes((int)entry.dataSize));
                    RealCount++;
                }
                fs.Position = pos;
                LogUtility.UpdateBar();
            }
            LogUtility.Debug(RealCount.ToString() + " among them are actually files.");
            fs.Dispose();
            br.Dispose();
        }

        public void Pack(string folderPath, string filePath)
        {
            if (Global.Version == "1")
            {
                bsaV1_pack(folderPath, filePath);
            }
            else if (Global.Version == "2")
            {
                bsaV2_pack(folderPath, filePath);
            }
        }

        private static void bsaV1_pack(string folderPath, string filePath)
        {
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);
            // header
            bw.Write(magic);
            bw.Write((ushort)1);
            int fileCount = Utilities.GetFileCount_TopOnly(folderPath);
            LogUtility.InitBar(fileCount);
            LogWindow.Instance.bar.Maximum = fileCount + 1;
            bw.Write((ushort)fileCount);
            bw.Write(0);
            // data
            string[] files = Directory.GetFiles(folderPath, "*", SearchOption.TopDirectoryOnly);
            MemoryStream ms = new MemoryStream();
            BinaryWriter bwIndex = new BinaryWriter(ms);
            for (int i = 0; i < fileCount; i++)
            {
                bwIndex.Write(ArcEncoding.Shift_JIS.GetBytes(Path.GetFileName(files[i]).PadRight(32, '\0')));
                bwIndex.Write((uint)fw.Position);
                bwIndex.Write((uint)new FileInfo(files[i]).Length);
                bw.Write(File.ReadAllBytes(files[i]));
                LogUtility.UpdateBar();
            }
            // entry
            uint indexOffset = (uint)fw.Position;
            fw.Position = 12;
            bw.Write(indexOffset);
            fw.Position = indexOffset;
            LogUtility.UpdateBar();
            ms.WriteTo(fw);
            ms.Dispose();
            fw.Dispose();
            bw.Dispose();
            bwIndex.Dispose();
        }

        private static void bsaV2_pack(string folderPath, string filePath)
        {
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);
            // init
            MemoryStream index = new MemoryStream();
            MemoryStream names = new MemoryStream();
            BinaryWriter bwIndex = new BinaryWriter(index);
            BinaryWriter bwNames = new BinaryWriter(names);
            RootDir = folderPath;
            FileCount = 0;
            // header
            bw.Write(magic);
            bw.Write((ushort)3);
            int fileCount = Utilities.GetFileCount_All(folderPath);
            LogUtility.InitBar(fileCount);
            bw.Write((ushort)fileCount);
            bw.Write(0);
            // others
            Write(bw, folderPath, bwIndex, bwNames);
            uint indexOffset = (uint)fw.Position;
            fw.Position = 10;
            bw.Write((ushort)FileCount);
            bw.Write(indexOffset);
            fw.Position = fw.Length;
            index.WriteTo(fw);
            names.WriteTo(fw);
            bw.Dispose();
            fw.Dispose();
            bwIndex.Dispose();
            bwNames.Dispose();
        }

        private static void Write(BinaryWriter bw, string path, BinaryWriter bwIndex, BinaryWriter bwNames)
        {
            // enter folder
            if (path != RootDir)
            {
                bwIndex.Write((uint)bwNames.BaseStream.Position);
                bwIndex.Write((long)0);

                bwNames.Write(ArcEncoding.Shift_JIS.GetBytes(">" + Path.GetFileName(path)));
                bwNames.Write('\0');

                FileCount++;
            }

            foreach (string file in Directory.GetFiles(path))
            {
                bwIndex.Write((uint)bwNames.BaseStream.Position);
                bwIndex.Write((uint)bw.BaseStream.Position);
                bwIndex.Write((uint)new FileInfo(file).Length);

                bwNames.Write(ArcEncoding.Shift_JIS.GetBytes(Path.GetFileName(file)));
                bwNames.Write('\0');

                bw.Write(File.ReadAllBytes(file));

                FileCount++;
                LogUtility.UpdateBar();
            }

            foreach (string dir in Directory.GetDirectories(path))
            {
                Write(bw, dir, bwIndex, bwNames);
            }

            // leave folder
            if (path != RootDir)
            {
                bwIndex.Write((uint)bwNames.BaseStream.Position);
                bwIndex.Write((long)0);

                bwNames.Write(Encoding.ASCII.GetBytes("<\0"));

                FileCount++;
            }
        }
    }
}