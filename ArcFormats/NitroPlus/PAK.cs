using Log;
using System;
using System.IO;
using System.Windows.Forms;
using Utility;
using Utility.Compression;

namespace ArcFormats.NitroPlus
{
    public class PAK
    {
        public static UserControl PackExtraOptions = new PackPAKOptions();

        private class Entry
        {
            public uint PathLen { get; set; }
            public string Path { get; set; }
            public uint Offset { get; set; }
            public uint UnpackedSize { get; set; }
            public uint Size { get; set; }
            public string FullPath { get; set; }
            public bool IsPacked { get; set; }
        }

        public void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            fs.Position = 4;
            int fileCount = br.ReadInt32();
            LogUtility.InitBar(fileCount);
            fs.Position += 4;
            int comSize = br.ReadInt32();
            fs.Position = 0x114;

            using (MemoryStream ms = new MemoryStream(Zlib.DecompressBytes(br.ReadBytes(comSize))))
            {
                using (BinaryReader brEntry = new BinaryReader(ms))
                {
                    int dataOffset = 276 + comSize;
                    fs.Position = dataOffset;

                    while (ms.Position != ms.Length)
                    {
                        Entry entry = new Entry();
                        entry.PathLen = brEntry.ReadUInt32();
                        entry.Path = ArcEncoding.Shift_JIS.GetString(brEntry.ReadBytes((int)entry.PathLen));
                        entry.FullPath = Path.Combine(folderPath, entry.Path);

                        entry.Offset = brEntry.ReadUInt32() + (uint)dataOffset;
                        entry.UnpackedSize = brEntry.ReadUInt32();
                        entry.Size = brEntry.ReadUInt32();
                        entry.IsPacked = brEntry.ReadUInt32() != 0;
                        uint size = brEntry.ReadUInt32();
                        if (entry.IsPacked)
                        {
                            entry.Size = size;
                        }

                        Utils.CreateParentDirectoryIfNotExists(entry.FullPath);

                        byte[] data = br.ReadBytes((int)entry.Size);
                        byte[] backup = new byte[data.Length];
                        Array.Copy(data, backup, data.Length);
                        try
                        {
                            byte[] result = Zlib.DecompressBytes(data);
                            File.WriteAllBytes(entry.FullPath, result);
                            result = null;
                        }
                        catch
                        {
                            File.WriteAllBytes(entry.FullPath, backup);
                        }
                        backup = null;
                        data = null;
                        LogUtility.UpdateBar();
                    }
                }
            }

            fs.Dispose();
            br.Dispose();
        }

        public void Pack(string folderPath, string filePath)
        {
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);
            string[] fullPaths = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
            string[] relativePaths = Utils.GetRelativePaths(fullPaths, folderPath);

            bw.Write(2);
            int fileCount = fullPaths.Length;
            bw.Write(fileCount);
            LogUtility.InitBar(fileCount);


            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (BinaryWriter bwIndex = new BinaryWriter(memoryStream))
                {
                    uint offset = 0;
                    foreach (string relativePath in relativePaths)
                    {
                        bwIndex.Write(ArcEncoding.Shift_JIS.GetByteCount(relativePath));
                        bwIndex.Write(ArcEncoding.Shift_JIS.GetBytes(relativePath));
                        bwIndex.Write(offset);
                        uint fileSize = (uint)new FileInfo(Path.Combine(folderPath, relativePath)).Length;
                        bwIndex.Write(fileSize);
                        bwIndex.Write(fileSize);
                        bwIndex.Write((long)0);
                        offset += fileSize;
                    }

                    byte[] uncomIndex = memoryStream.ToArray();

                    bw.Write(uncomIndex.Length);
                    byte[] comIndex = Zlib.CompressBytes(uncomIndex);
                    bw.Write(comIndex.Length);

                    if (!string.IsNullOrEmpty(PackPAKOptions.OriginalFilePath) && File.Exists(PackPAKOptions.OriginalFilePath))
                    {
                        FileStream fs = File.OpenRead(PackPAKOptions.OriginalFilePath);
                        BinaryReader br = new BinaryReader(fs);
                        fs.Position = 16;
                        byte[] reserve = br.ReadBytes(260);
                        fs.Dispose();
                        br.Dispose();

                        bw.Write(reserve);
                    }
                    else
                    {
                        bw.Write(new byte[260]);
                    }
                    bw.Write(comIndex);
                }
            }

            foreach (string fullPath in fullPaths)
            {
                byte[] data = File.ReadAllBytes(fullPath);
                bw.Write(data);
                data = null;
                LogUtility.UpdateBar();
            }

            fw.Dispose();
            bw.Dispose();
        }
    }
}