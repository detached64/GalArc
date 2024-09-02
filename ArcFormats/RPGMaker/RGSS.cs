using Log;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace ArcFormats.RPGMaker
{
    public class RGSS
    {
        public static void Unpack(string filePath, string folderPath, Encoding encoding)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            if (br.ReadUInt32() != 0x53534752) // 'RGSS'
            {
                LogUtility.Error_NotValidArchive();
            }
            fs.Position = 7;
            int version = br.ReadByte();
            fs.Dispose();
            br.Dispose();
            if (version == 1)
            {
                LogUtility.Info("Valid " + Path.GetExtension(filePath).Replace(".", string.Empty) + " v1 archive detected.");
                rgssV1_unpack(filePath, folderPath);
            }
            else if (version == 3)
            {
                LogUtility.Info("Valid " + Path.GetExtension(filePath).Replace(".", string.Empty) + " v3 archive detected.");
                rgssV3_unpack(filePath, folderPath);
            }
            else
            {
                LogUtility.Error("Error:version not recognized.");
            }
        }

        private static void rgssV1_unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            Directory.CreateDirectory(folderPath);
            KeyGen keygen = new KeyGen(0xDEADCAFE);
            fs.Position = 8;
            long fileCount = 0;
            while (fs.Position < fs.Length)
            {
                uint nameLen = br.ReadUInt32() ^ keygen.Compute();
                byte[] nameBytes = br.ReadBytes((int)nameLen);
                string name = DecryptName(nameBytes, keygen);
                string fullPath = Path.Combine(folderPath, name);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                uint size = br.ReadUInt32() ^ keygen.Compute();
                File.WriteAllBytes(fullPath, DecryptData(br.ReadBytes((int)size), new KeyGen(keygen.GetCurrent())));
                fileCount++;
            }
            LogUtility.Debug(fileCount + " files inside.");
            fs.Dispose();
            br.Dispose();
        }

        private static void rgssV3_unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            Directory.CreateDirectory(folderPath);
            fs.Position = 8;
            uint key = br.ReadUInt32() * 9 + 3;
            long fileCount = 0;
            bool isFirst = true;
            long maxIndex = 13;
            while (fs.Position < maxIndex)
            {
                uint dataOffset = br.ReadUInt32() ^ key;
                if (isFirst)
                {
                    maxIndex = dataOffset - 16;
                    isFirst = false;
                }
                uint fileSize = br.ReadUInt32() ^ key;
                uint thisKey = br.ReadUInt32() ^ key;
                uint nameLen = br.ReadUInt32() ^ key;
                string fullPath = Path.Combine(folderPath, DecryptName(br.ReadBytes((int)nameLen), key));
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                long pos = fs.Position;
                fs.Position = dataOffset;
                File.WriteAllBytes(fullPath, DecryptData(br.ReadBytes((int)fileSize), new KeyGen(thisKey)));
                fs.Position = pos;
                //LogUtility.Debug(pos.ToString());
                fileCount++;
            }
            LogUtility.Debug(fileCount + " files inside.");
            fs.Dispose();
            br.Dispose();
        }


        private static string DecryptName(byte[] data, KeyGen keygen)
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] ^= (byte)keygen.Compute();
            }
            return Encoding.UTF8.GetString(data);
        }

        private static string DecryptName(byte[] data, uint key)
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] ^= (byte)(key >> (i << 3));
            }
            return Encoding.UTF8.GetString(data);
        }

        private static byte[] DecryptData(byte[] data, KeyGen keygen)
        {
            uint key = keygen.Compute();
            for (int i = 0; i < data.Length;)
            {
                data[i] ^= (byte)(key >> (i << 3));
                i++;
                if ((i & 3) == 0)
                {
                    key = keygen.Compute();
                }
            }
            return data;
        }


        internal class KeyGen
        {
            uint m_seed;
            public KeyGen(uint seed)
            {
                m_seed = seed;
            }
            public uint Compute()
            {
                uint result = m_seed;
                m_seed = m_seed * 7 + 3;
                return result;
            }
            public uint GetCurrent()
            {
                return m_seed;
            }
        }
    }
}
