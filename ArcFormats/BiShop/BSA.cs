using GalArc.Controls;
using GalArc.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Utility;
using Utility.Extensions;

namespace ArcFormats.BiShop
{
    public class BSA : ArchiveFormat
    {
        public override OptionsTemplate PackExtraOptions => PackBSAOptions.Instance;

        private byte[] Magic = Utils.HexStringToByteArray("4253417263000000");

        private List<string> path = new List<string>();

        private int realCount = 0;

        private string rootDir = string.Empty;

        private int fileCount = 0;

        private class BsaEntry : Entry
        {
            internal uint NameOffset { get; set; }
            internal uint DataOffset { get; set; }
        }

        public override void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);

            if (!br.ReadBytes(8).SequenceEqual(Magic))
            {
                Logger.ErrorInvalidArchive();
            }

            ushort version = br.ReadUInt16();
            fs.Dispose();
            br.Dispose();
            if (version > 1)
            {
                Logger.ShowVersion("bsa", 2);
                UnpackV2(filePath, folderPath);
            }
            else if (version == 1)
            {
                Logger.ShowVersion("bsa", 1);
                UnpackV1(filePath, folderPath);
            }
            else
            {
                Logger.ErrorInvalidArchive();
            }
        }

        private void UnpackV1(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            br.ReadBytes(10);
            ushort fileCount = br.ReadUInt16();
            uint indexOffset = br.ReadUInt32();
            Directory.CreateDirectory(folderPath);
            Logger.InitBar(fileCount);

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
                Logger.UpdateBar();
            }
            fs.Dispose();
            br.Dispose();
        }

        private void UnpackV2(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            br.ReadBytes(10);
            ushort fileCount = br.ReadUInt16();
            uint indexOffset = br.ReadUInt32();
            uint nameOffset = indexOffset + (uint)fileCount * 12;
            Directory.CreateDirectory(folderPath);
            Logger.InitBar(fileCount);

            fs.Seek(indexOffset, SeekOrigin.Begin);
            path.Clear();
            realCount = 0;
            path.Add(folderPath);

            for (int i = 0; i < fileCount; i++)
            {
                BsaEntry entry = new BsaEntry();

                entry.NameOffset = br.ReadUInt32() + nameOffset;
                entry.DataOffset = br.ReadUInt32();
                entry.Size = br.ReadUInt32();

                long pos = fs.Position;
                fs.Position = entry.NameOffset;
                string name = br.ReadCString();
                if (name[0] == '>')
                {
                    path.Add(name.Substring(1));
                }
                else if (name[0] == '<')
                {
                    path.RemoveAt(path.Count - 1);
                }
                else
                {
                    fs.Position = entry.DataOffset;
                    string path = Path.Combine(Path.Combine(this.path.ToArray()), name);
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    File.WriteAllBytes(path, br.ReadBytes((int)entry.Size));
                    realCount++;
                }
                fs.Position = pos;
                Logger.UpdateBar();
            }
            Logger.Debug(realCount.ToString() + " among them are actually files.");
            fs.Dispose();
            br.Dispose();
        }

        public override void Pack(string folderPath, string filePath)
        {
            switch (PackExtraOptions.Version)
            {
                case "1":
                    PackV1(folderPath, filePath);
                    break;
                case "2":
                    PackV2(folderPath, filePath);
                    break;
            }
        }

        private void PackV1(string folderPath, string filePath)
        {
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);
            // header
            bw.Write(Magic);
            bw.Write((ushort)1);
            DirectoryInfo d = new DirectoryInfo(folderPath);
            FileInfo[] files = d.GetFiles();
            int fileCount = files.Length;
            Logger.InitBar(fileCount);
            Logger.SetBarMax(fileCount + 1);
            bw.Write((ushort)fileCount);
            bw.Write(0);
            // data
            MemoryStream ms = new MemoryStream();
            BinaryWriter bwIndex = new BinaryWriter(ms);
            foreach (FileInfo file in files)
            {
                bwIndex.WritePaddedString(file.Name, 32);
                bwIndex.Write((uint)fw.Position);
                bwIndex.Write((uint)file.Length);
                byte[] data = File.ReadAllBytes(file.FullName);
                bw.Write(data);
                data = null;
                Logger.UpdateBar();
            }
            // entry
            uint indexOffset = (uint)fw.Position;
            fw.Position = 12;
            bw.Write(indexOffset);
            fw.Position = indexOffset;
            Logger.UpdateBar();
            ms.WriteTo(fw);
            ms.Dispose();
            fw.Dispose();
            bw.Dispose();
            bwIndex.Dispose();
        }

        private void PackV2(string folderPath, string filePath)
        {
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);
            // init
            MemoryStream index = new MemoryStream();
            MemoryStream names = new MemoryStream();
            BinaryWriter bwIndex = new BinaryWriter(index);
            BinaryWriter bwNames = new BinaryWriter(names);
            rootDir = folderPath;
            this.fileCount = 0;
            // header
            bw.Write(Magic);
            bw.Write((ushort)3);
            int fileCount = Utils.GetFileCount(folderPath);
            Logger.InitBar(fileCount);
            bw.Write((ushort)fileCount);
            bw.Write(0);
            // others
            Write(bw, folderPath, bwIndex, bwNames);
            uint indexOffset = (uint)fw.Position;
            fw.Position = 10;
            bw.Write((ushort)this.fileCount);
            bw.Write(indexOffset);
            fw.Position = fw.Length;
            index.WriteTo(fw);
            names.WriteTo(fw);
            bw.Dispose();
            fw.Dispose();
            bwIndex.Dispose();
            bwNames.Dispose();
        }

        private void Write(BinaryWriter bw, string path, BinaryWriter bwIndex, BinaryWriter bwNames)
        {
            // enter folder
            if (path != rootDir)
            {
                bwIndex.Write((uint)bwNames.BaseStream.Position);
                bwIndex.Write((long)0);

                bwNames.Write(ArcEncoding.Shift_JIS.GetBytes(">" + Path.GetFileName(path)));
                bwNames.Write('\0');

                fileCount++;
            }

            foreach (string file in Directory.GetFiles(path))
            {
                bwIndex.Write((uint)bwNames.BaseStream.Position);
                bwIndex.Write((uint)bw.BaseStream.Position);
                bwIndex.Write((uint)new FileInfo(file).Length);

                bwNames.Write(ArcEncoding.Shift_JIS.GetBytes(Path.GetFileName(file)));
                bwNames.Write('\0');

                byte[] data = File.ReadAllBytes(file);
                bw.Write(data);
                data = null;

                fileCount++;
                Logger.UpdateBar();
            }

            foreach (string dir in Directory.GetDirectories(path))
            {
                Write(bw, dir, bwIndex, bwNames);
            }

            // leave folder
            if (path != rootDir)
            {
                bwIndex.Write((uint)bwNames.BaseStream.Position);
                bwIndex.Write((long)0);

                bwNames.Write(Encoding.ASCII.GetBytes("<\0"));

                fileCount++;
            }
        }
    }
}