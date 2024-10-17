using Log;
using System;
using System.IO;
using Utility;
using Utility.Extensions;

namespace ArcFormats.NScripter
{
    public class NSA
    {
        public void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            ushort fileCount = BigEndian.Convert(br.ReadUInt16());
            uint baseOffset = BigEndian.Convert(br.ReadUInt32());
            LogUtility.InitBar(fileCount);
            Directory.CreateDirectory(folderPath);
            for (int i = 0; i < fileCount; i++)
            {
                string relativePath = br.ReadCString(ArcEncoding.Shift_JIS);
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
                string dir = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                File.WriteAllBytes(fullPath, br.ReadBytes((int)packedSize));
                fs.Position = pos;
                LogUtility.UpdateBar();
            }
            fs.Dispose();
            br.Dispose();
        }

        public void Pack(string folderPath, string filePath)
        {
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);
            DirectoryInfo d = new DirectoryInfo(folderPath);
            FileInfo[] files = d.GetFiles("*", SearchOption.AllDirectories);
            ushort fileCount = (ushort)files.Length;
            bw.Write(BigEndian.Convert(fileCount));
            bw.Write(0);
            LogUtility.InitBar(files.Length);
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
                LogUtility.UpdateBar();
            }
            fw.Position = 2;
            bw.Write(BigEndian.Convert(baseOffset));
            fw.Dispose();
            bw.Dispose();
        }
    }
}
