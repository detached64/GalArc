using GalArc.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utility;
using Utility.Compression;
using Utility.Extensions;

namespace ArcFormats.GSPack
{
    internal class DAT
    {
        public class Entry
        {
            public string Name { get; set; }
            public uint Size { get; set; }
            public long Offset { get; set; }
        }

        public static readonly string[] ValidMagics = { "DataPack5", "GsPack5", "GsPack4" };
        public void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            {
                // Check magic (any of the valid ones)
                byte[] magicBytes = br.ReadBytes(9);  
                string magic = Encoding.ASCII.GetString(magicBytes).TrimEnd('\0');

                bool isValidMagic = false;
                foreach (string validMagic in ValidMagics)
                {
                    if (magic.StartsWith(validMagic))
                    {
                        isValidMagic = true;
                        break;
                    }
                }

                if (!isValidMagic)
                {
                    throw new InvalidDataException($"Not a Valid GSWIN Archive. Found magic: {magic}");
                }

                fs.Position = 0x30;
                int versionMinor = br.ReadUInt16();
                int versionMajor = br.ReadUInt16();
                uint indexSize = br.ReadUInt32();
                uint isEncrypted = br.ReadUInt32();
                int fileCount = br.ReadInt32();
                long dataOffset = br.ReadUInt32();
                int indexOffset = br.ReadInt32();
                int entrySize = versionMajor < 5 ? 0x48 : 0x68;
                int unpackedSize = fileCount * entrySize;

                // Sanity checks
                if (fileCount <= 0 || fileCount > 0xFFFFF || indexSize > 0xFFFFFF)
                {
                    throw new InvalidDataException("Invalid archive structure");
                }
                Logger.InitBar(fileCount);

                byte[] index;
                if (indexSize != 0)
                {
                    fs.Position = indexOffset;
                    byte[] packedIndex = br.ReadBytes((int)indexSize);

                    if ((isEncrypted & 1) != 0)
                    {
                        for (int i = 0; i < packedIndex.Length; i++)
                        {
                            packedIndex[i] ^= (byte)i;
                        }
                    }

                    index = Lzss.Decompress(packedIndex);
                }
                else
                {
                    fs.Position = indexOffset;
                    index = br.ReadBytes(unpackedSize);
                }

                int currentOffset = 0;
                var entries = new List<Entry>();
                for (int i = 0; i < fileCount; i++)
                {
                    string name = GetNullTerminatedString(index, currentOffset, 0x40);
                    if (!string.IsNullOrEmpty(name))
                    {
                        var entry = new Entry
                        {
                            Name = name,
                            Offset = dataOffset + BitConverter.ToUInt32(index, currentOffset + 0x40),
                            Size = BitConverter.ToUInt32(index, currentOffset + 0x44)
                        };

                        // Check if entry placement is valid
                        if (entry.Offset + entry.Size <= fs.Length)
                        {
                            entries.Add(entry);
                        }
                    }
                    currentOffset += entrySize;
                }

                // Extract files
                Directory.CreateDirectory(folderPath);
                foreach (Entry entry in entries)
                {
                    fs.Position = entry.Offset;
                    byte[] data = br.ReadBytes((int)entry.Size);

                    if ((isEncrypted & 2) != 0)
                    {
                        DecryptData(data, entry.Name);
                    }

                    string fileName = Path.Combine(folderPath, entry.Name);
                    Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                    File.WriteAllBytes(fileName, data);
                }
            }
            fs.Dispose();
            br.Dispose();

        }
        internal static void DecryptData(byte[] data, string key)
        {
            int numkey = 0;
            for (int i = 0; i < key.Length; ++i)
                numkey = numkey * 37 + (key[i] | 0x20);

            unsafe
            {
                fixed (byte* data8 = data)
                {
                    int* data32 = (int*)data8;
                    for (int count = data.Length / 4; count > 0; --count)
                        *data32++ ^= numkey;
                }
            }
        }

        internal static string GetNullTerminatedString(byte[] data, int offset, int maxLength)
        {
            int length = 0;
            while (length < maxLength && data[offset + length] != 0)
                length++;
            return ArcEncoding.Shift_JIS.GetString(data, offset, length);
        }

        public void Pack(string folderPath, string outputPath)
        {
            FileInfo[] files = new DirectoryInfo(folderPath).GetFiles();
            Logger.InitBar(files.Length);
        }
    }

}
