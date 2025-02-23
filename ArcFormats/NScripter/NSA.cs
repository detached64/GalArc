using GalArc.Logs;
using System;
using System.IO;
using Utility;
using Utility.Extensions;

namespace ArcFormats.NScripter
{
    public class NSA : ArchiveFormat
    {
        public override void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            ushort fileCount = BigEndian.Convert(br.ReadUInt16());
            uint baseOffset = BigEndian.Convert(br.ReadUInt32());
            Logger.InitBar(fileCount);
            Directory.CreateDirectory(folderPath);
            for (int i = 0; i < fileCount; i++)
            {
                string relativePath = br.ReadCString();
                bool isCompressed = br.ReadByte() != 0;
                if (isCompressed)
                {
                    throw new NotImplementedException();
                }
                uint offset = BigEndian.Convert(br.ReadUInt32()) + baseOffset;
                uint packedSize = BigEndian.Convert(br.ReadUInt32());
                uint unpackedSize = BigEndian.Convert(br.ReadUInt32());
                long pos = fs.Position;
                fs.Position = offset;
                string fullPath = Path.Combine(folderPath, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                byte[] buffer = br.ReadBytes((int)packedSize);
                File.WriteAllBytes(fullPath, buffer);
                buffer = null;
                fs.Position = pos;
                Logger.UpdateBar();
            }
            fs.Dispose();
            br.Dispose();
        }

        public override void Pack(string folderPath, string filePath)
        {
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);
            DirectoryInfo d = new DirectoryInfo(folderPath);
            FileInfo[] files = d.GetFiles("*", SearchOption.AllDirectories);
            ushort fileCount = (ushort)files.Length;
            bw.Write(BigEndian.Convert(fileCount));
            bw.Write(0);
            Logger.InitBar(files.Length);
            uint offset = 0;
            foreach (FileInfo file in files)
            {
                string relativePath = file.FullName.Substring(folderPath.Length + 1);
                bw.Write(ArcEncoding.Shift_JIS.GetBytes(relativePath));
                bw.Write((byte)0);
                bw.Write((byte)0);
                bw.Write(BigEndian.Convert(offset));
                uint size = (uint)file.Length;
                bw.Write(BigEndian.Convert(size));
                bw.Write(BigEndian.Convert(size));
                offset += size;
            }
            uint baseOffset = (uint)fw.Position;
            foreach (FileInfo file in files)
            {
                byte[] data = File.ReadAllBytes(file.FullName);
                bw.Write(data);
                data = null;
                Logger.UpdateBar();
            }
            fw.Position = 2;
            bw.Write(BigEndian.Convert(baseOffset));
            fw.Dispose();
            bw.Dispose();
        }
    }
}
