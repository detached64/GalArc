using Log;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;
using Utility;
using Utility.Compression;

namespace ArcFormats.NitroPlus
{
    public class PAK
    {
        public static UserControl PackExtraOptions = new PackPAKOptions();

        private struct Entry
        {
            public uint pathLen { get; set; }
            public string path { get; set; }
            public uint offset { get; set; }
            public uint unpackedSize { get; set; }
            public uint size { get; set; }
            public string fullPath { get; set; }
            public bool isPacked { get; set; }
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
                        entry.pathLen = brEntry.ReadUInt32();
                        entry.path = ArcEncoding.Shift_JIS.GetString(brEntry.ReadBytes((int)entry.pathLen));
                        entry.fullPath = Path.Combine(folderPath, entry.path);

                        entry.offset = brEntry.ReadUInt32() + (uint)dataOffset;
                        entry.unpackedSize = brEntry.ReadUInt32();
                        entry.size = brEntry.ReadUInt32();
                        entry.isPacked = brEntry.ReadUInt32() != 0;
                        uint size = brEntry.ReadUInt32();
                        if (entry.isPacked)
                        {
                            entry.size = size;
                        }

                        string dir = Path.GetDirectoryName(entry.fullPath);
                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }

                        byte[] data = br.ReadBytes((int)entry.size);
                        try
                        {
                            byte[] result = Zlib.DecompressBytes(data);
                            File.WriteAllBytes(entry.fullPath, result);
                        }
                        catch
                        {
                            File.WriteAllBytes(entry.fullPath, data);
                        }
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
            bw.Write(2);
            int fileCount = Utilities.GetFileCount_All(folderPath);
            bw.Write(fileCount);
            LogUtility.InitBar(fileCount);

            string[] filePaths = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);

            int pathLenSum = 0;
            foreach (string fullPath in filePaths)
            {
                pathLenSum += ArcEncoding.Shift_JIS.GetByteCount(fullPath.Substring(folderPath.Length + 1));
            }

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (BinaryWriter bwIndex = new BinaryWriter(memoryStream))
                {
                    uint offset = 0;
                    foreach (string fullPath in filePaths)
                    {
                        string relativePath = fullPath.Substring(folderPath.Length + 1);
                        bwIndex.Write(ArcEncoding.Shift_JIS.GetByteCount(relativePath));
                        bwIndex.Write(ArcEncoding.Shift_JIS.GetBytes(relativePath));
                        bwIndex.Write(offset);
                        uint fileSize = (uint)new FileInfo(fullPath).Length;
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

            foreach (string fullPath in filePaths)
            {
                bw.Write(File.ReadAllBytes(fullPath));
                LogUtility.UpdateBar();
            }

            fw.Dispose();
            bw.Dispose();
        }
    }
}