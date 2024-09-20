using Log;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Utility;

namespace ArcFormats.Cmvs
{
    public class CPZ
    {
        static byte[] Key_cpzV1 =
        {
            0x92, 0xCD, 0x97, 0x90, 0x8C, 0xD7, 0x8C, 0xD5, 0x8B, 0x4B, 0x93, 0xFA, 0x9A, 0xD7, 0x8C, 0xBF,
            0x8C, 0xC9, 0x8C, 0xEB, 0x8D, 0x69, 0x8D, 0x8B, 0x8C, 0xD2, 0x8C, 0xD6, 0x8B, 0x6D, 0x8C, 0xE3,
            0x8C, 0xFB, 0x8C, 0xD0, 0x8C, 0xC8, 0x8C, 0xF0, 0x8B, 0xFE, 0x8C, 0xAA, 0x8C, 0xF4, 0x8B, 0x4B,
            0x9C, 0x58, 0x8C, 0xD3, 0x96, 0xC8, 0x8C, 0xCB, 0x8C, 0xCE, 0x8C, 0xF3, 0x8C, 0xD6, 0x8B, 0x52,
        };

        public static void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            if (Encoding.ASCII.GetString(br.ReadBytes(3)) != "CPZ")
            {
                LogUtility.Error_NotValidArchive();
            }

            int version = br.ReadChar() - '0';
            fs.Dispose();
            br.Dispose();

            switch (version)
            {
                case 1:
                    cpzV1_unpack(filePath, folderPath);
                    break;
            }
        }

        public static void Pack(string folderPath, string filePath)
        {
            switch (Global.Version)
            {
                case "1":
                    cpzV1_pack(folderPath, filePath);
                    break;
            }
        }

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
                index[i] = (byte)((index[i] ^ Key_cpzV1[i & 0x3F]) - 0x6C);
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
                string fileName = Utilities.ReadCString(reader, ArcEncoding.Shift_JIS);
                fs.Position = offset;
                byte[] data = br.ReadBytes(size);
                for (int j = 0; j < data.Length; j++)
                {
                    data[j] = (byte)((data[j] ^ Key_cpzV1[j & 0x3F]) - 0x6C);
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
            long indexSize = 26 * fileCount + Utilities.GetNameLenSum(files, ArcEncoding.Shift_JIS);
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
                index[i] = (byte)((index[i] + 0x6C) ^ Key_cpzV1[i & 0x3F]);
            }
            bw.Write(index);

            foreach (string file in files)
            {
                byte[] data = File.ReadAllBytes(file);
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = (byte)((data[i] + 0x6C) ^ Key_cpzV1[i & 0x3F]);
                }
                bw.Write(data);
                LogUtility.UpdateBar();
            }
            fw.Dispose();
            bw.Dispose();
            indexWriter.Dispose();
        }

        private static void cpzV6_unpack(string filePath, string folderPath)
        {

        }
    }
}
