using GalArc.Controls;
using GalArc.Logs;
using System;
using System.IO;
using Utility;
using Utility.Compression;

namespace ArcFormats.NitroPlus
{
    public class PAK : ArchiveFormat
    {
        private static readonly Lazy<OptionsTemplate> _lazyPackOptions = new Lazy<OptionsTemplate>(() => new PackPAKOptions());
        public static OptionsTemplate PackExtraOptions => _lazyPackOptions.Value;

        private class NitroPakEntry : PackedEntry
        {
            public uint PathLen { get; set; }
            public string RelativePath { get; set; }
        }

        public override void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            fs.Position = 4;
            int fileCount = br.ReadInt32();
            Logger.InitBar(fileCount);
            fs.Position += 4;
            int comSize = br.ReadInt32();
            fs.Position = 0x114;

            using (MemoryStream ms = new MemoryStream(ZlibHelper.Decompress(br.ReadBytes(comSize))))
            {
                using (BinaryReader brEntry = new BinaryReader(ms))
                {
                    int dataOffset = 276 + comSize;
                    fs.Position = dataOffset;

                    while (ms.Position != ms.Length)
                    {
                        NitroPakEntry entry = new NitroPakEntry();
                        entry.PathLen = brEntry.ReadUInt32();
                        entry.RelativePath = ArcEncoding.Shift_JIS.GetString(brEntry.ReadBytes((int)entry.PathLen));
                        entry.Path = Path.Combine(folderPath, entry.RelativePath);

                        entry.Offset = brEntry.ReadUInt32() + (uint)dataOffset;
                        entry.UnpackedSize = brEntry.ReadUInt32();
                        entry.Size = brEntry.ReadUInt32();
                        entry.IsPacked = brEntry.ReadUInt32() != 0;
                        uint size = brEntry.ReadUInt32();
                        if (entry.IsPacked)
                        {
                            entry.Size = size;
                        }

                        Directory.CreateDirectory(Path.GetDirectoryName(entry.Path));

                        byte[] data = br.ReadBytes((int)entry.Size);
                        byte[] backup = new byte[data.Length];
                        Array.Copy(data, backup, data.Length);
                        try
                        {
                            byte[] result = ZlibHelper.Decompress(data);
                            File.WriteAllBytes(entry.Path, result);
                            result = null;
                        }
                        catch
                        {
                            File.WriteAllBytes(entry.Path, backup);
                        }
                        backup = null;
                        data = null;
                        Logger.UpdateBar();
                    }
                }
            }
            fs.Dispose();
            br.Dispose();
        }

        public override void Pack(string folderPath, string filePath)
        {
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);
            string[] fullPaths = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
            string[] relativePaths = Utils.GetRelativePaths(fullPaths, folderPath);

            bw.Write(2);
            int fileCount = fullPaths.Length;
            bw.Write(fileCount);
            Logger.InitBar(fileCount);

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
                    byte[] comIndex = ZlibHelper.Compress(uncomIndex);
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
                Logger.UpdateBar();
            }

            fw.Dispose();
            bw.Dispose();
        }
    }
}