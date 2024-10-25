using ArcFormats.Properties;
using ArcFormats.Templates;
using Log;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Utility;
using Utility.Extensions;

namespace ArcFormats.Cmvs
{
    public class CPZ
    {
        public static UserControl PackExtraOptions = new VersionOnly("1");

        private static byte[] KeyV1 =
        {
            0x92, 0xCD, 0x97, 0x90, 0x8C, 0xD7, 0x8C, 0xD5, 0x8B, 0x4B, 0x93, 0xFA, 0x9A, 0xD7, 0x8C, 0xBF,
            0x8C, 0xC9, 0x8C, 0xEB, 0x8D, 0x69, 0x8D, 0x8B, 0x8C, 0xD2, 0x8C, 0xD6, 0x8B, 0x6D, 0x8C, 0xE3,
            0x8C, 0xFB, 0x8C, 0xD0, 0x8C, 0xC8, 0x8C, 0xF0, 0x8B, 0xFE, 0x8C, 0xAA, 0x8C, 0xF4, 0x8B, 0x4B,
            0x9C, 0x58, 0x8C, 0xD3, 0x96, 0xC8, 0x8C, 0xCB, 0x8C, 0xCE, 0x8C, 0xF3, 0x8C, 0xD6, 0x8B, 0x52,
        };
        private static void cpzV1_unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            fs.Position = 4;
            int fileCount = br.ReadInt32();
            LogUtility.InitBar(fileCount);
            long indexSize = br.ReadInt64();
            byte[] index = br.ReadBytes((int)indexSize);
            for (int i = 0; i < index.Length; i++)
            {
                index[i] = (byte)((index[i] ^ KeyV1[i & 0x3F]) - 0x6C);
            }

            MemoryStream ms = new MemoryStream(index);
            BinaryReader reader = new BinaryReader(ms);
            uint baseOffset = (uint)fs.Position;
            Directory.CreateDirectory(folderPath);

            for (int i = 0; i < fileCount; i++)
            {
                int entrySize = reader.ReadInt32();
                int size = reader.ReadInt32();
                long offset = reader.ReadInt64() + baseOffset;
                ms.Position += 8;
                string fileName = reader.ReadCString(ArcEncoding.Shift_JIS);
                fs.Position = offset;
                byte[] data = br.ReadBytes(size);
                for (int j = 0; j < data.Length; j++)
                {
                    data[j] = (byte)((data[j] ^ KeyV1[j & 0x3F]) - 0x6C);
                }
                string path = Path.Combine(folderPath, fileName);
                File.WriteAllBytes(path, data);
                ms.Position++;
                LogUtility.UpdateBar();
            }

            fs.Dispose();
            br.Dispose();
        }
        private static void cpzV1_pack(string folderPath, string filePath)
        {
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);
            string[] files = Directory.GetFiles(folderPath);
            bw.Write(Encoding.ASCII.GetBytes("CPZ1"));
            int fileCount = files.Length;
            bw.Write(fileCount);
            LogUtility.InitBar(fileCount);
            long indexSize = 26 * fileCount + Utility.Utils.GetNameLengthSum(files, ArcEncoding.Shift_JIS);
            bw.Write(indexSize * 2);

            long offset = 0;
            MemoryStream raw_index = new MemoryStream();
            BinaryWriter indexWriter = new BinaryWriter(raw_index);
            foreach (string file in files)
            {
                indexWriter.Write(26 + ArcEncoding.Shift_JIS.GetByteCount(Path.GetFileName(file)));
                uint fileSize = (uint)new FileInfo(file).Length;
                indexWriter.Write(fileSize);
                indexWriter.Write(offset);
                offset += fileSize;
                indexWriter.Write((long)0);
                indexWriter.Write(ArcEncoding.Shift_JIS.GetBytes(Path.GetFileName(file)));
                indexWriter.Write((short)0);
            }
            indexWriter.Write(new byte[indexSize]);

            byte[] index = raw_index.ToArray();
            for (int i = 0; i < index.Length; i++)
            {
                index[i] = (byte)((index[i] + 0x6C) ^ KeyV1[i & 0x3F]);
            }
            bw.Write(index);

            foreach (string file in files)
            {
                byte[] data = File.ReadAllBytes(file);
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = (byte)((data[i] + 0x6C) ^ KeyV1[i & 0x3F]);
                }
                bw.Write(data);
                LogUtility.UpdateBar();
            }
            fw.Dispose();
            bw.Dispose();
            indexWriter.Dispose();
        }

        private class HeaderV6
        {
            public uint Magic;         //"CPZ6"
            public uint DirCount;
            public uint DirIndexLength;
            public uint FileIndexLength;
            public uint[] IndexVerify;
            public uint[] Md5Data;
            public uint IndexKey;
            public uint IsEncrypt;     //1
            public uint IndexSeed;
            public uint HeaderCRC;
        }

        private static void ReadHeaderV6(BinaryReader br, HeaderV6 header)
        {
            header.Magic = br.ReadUInt32();
            header.DirCount = br.ReadUInt32() ^ 0xfe3a53da;
            header.DirIndexLength = br.ReadUInt32() ^ 0x37f298e8;
            header.FileIndexLength = br.ReadUInt32() ^ 0x7a6f3a2d;
            header.IndexVerify = new uint[4];
            header.IndexVerify[0] = br.ReadUInt32();
            header.IndexVerify[1] = br.ReadUInt32();
            header.IndexVerify[2] = br.ReadUInt32();
            header.IndexVerify[3] = br.ReadUInt32();
            header.Md5Data = new uint[4];
            header.Md5Data[0] = br.ReadUInt32();
            header.Md5Data[1] = br.ReadUInt32();
            header.Md5Data[2] = br.ReadUInt32();
            header.Md5Data[3] = br.ReadUInt32();
            header.Md5Data[0] ^= 0x43de7c1a;
            header.Md5Data[1] ^= 0xcc65f416;
            header.Md5Data[2] ^= 0xd016a93d;
            header.Md5Data[3] ^= 0x97a3ba9b;
            header.IndexKey = br.ReadUInt32() ^ 0xae7d39b7;
            header.IsEncrypt = br.ReadUInt32() ^ 0xfb73a956;
            header.IndexSeed = br.ReadUInt32() ^ 0x37acf832;
            header.HeaderCRC = br.ReadUInt32();

            CmvsMD5.ComputeHash(header.Md5Data);
            CMVSUtils.Swap(ref header.Md5Data[0], ref header.Md5Data[2]);
            CMVSUtils.Swap(ref header.Md5Data[2], ref header.Md5Data[3]);

            header.Md5Data[0] ^= 0x45a76c2f;
            header.Md5Data[1] -= 0x5ba17fcb;
            header.Md5Data[2] ^= 0x79abe8ad;
            header.Md5Data[3] -= 0x1c08561b;

            header.IndexSeed = Binary.RotR(header.IndexSeed, 5);
            header.IndexSeed *= 0x7da8f173;
            header.IndexSeed += 0x13712765;
        }
        private static void cpzV6_unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            HeaderV6 header = new HeaderV6();
            ReadHeaderV6(br, header);
        }

        public void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            if (Encoding.ASCII.GetString(br.ReadBytes(3)) != "CPZ")
            {
                LogUtility.ErrorInvalidArchive();
            }

            int version = br.ReadChar() - '0';
            fs.Dispose();
            br.Dispose();

            switch (version)
            {
                case 1:
                    cpzV1_unpack(filePath, folderPath);
                    break;
                case 6:
                    cpzV6_unpack(filePath, folderPath);
                    break;
                default:
                    LogUtility.Error(Resources.logErrorNotSupportedVersion);
                    break;
            }
        }

        public void Pack(string folderPath, string filePath)
        {
            switch (Config.Version)
            {
                case "1":
                    cpzV1_pack(folderPath, filePath);
                    break;
            }
        }


    }
}
