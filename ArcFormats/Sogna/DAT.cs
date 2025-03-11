using GalArc.Logs;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utility;
using Utility.Exceptions;
using Utility.Extensions;

namespace ArcFormats.Sogna
{
    public class DAT : ArchiveFormat
    {
        private readonly string Magic = "SGS.DAT 1.00";

        public override void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);

            if (Encoding.ASCII.GetString(br.ReadBytes(12)) != Magic)
            {
                throw new InvalidArchiveException();
            }

            uint fileCount = br.ReadUInt32();
            uint indexOffset = 0x10;
            var entries = new List<PackedEntry>((int)fileCount);
            for (int i = 0; i < fileCount; ++i)
            {
                var entry = new PackedEntry();
                br.BaseStream.Position = indexOffset;
                entry.Name = ArcEncoding.Shift_JIS.GetString(br.ReadBytes(0x10)).TrimEnd('\0');
                br.BaseStream.Position = indexOffset + 0x13;
                entry.IsPacked = br.ReadByte() != 0;
                br.BaseStream.Position = indexOffset + 0x14;
                entry.Size = br.ReadUInt32();
                br.BaseStream.Position = indexOffset + 0x18;
                entry.UnpackedSize = br.ReadUInt32();
                br.BaseStream.Position = indexOffset + 0x1C;
                entry.Offset = br.ReadUInt32();
                entries.Add(entry);
                indexOffset += 0x20;
            }

            Logger.InitBar(fileCount);
            Directory.CreateDirectory(folderPath);
            foreach (PackedEntry entry in entries)
            {
                br.BaseStream.Position = entry.Offset;
                string fileName = Path.Combine(folderPath, entry.Name);
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                byte[] data = br.ReadBytes((int)entry.Size);
                if (entry.IsPacked)
                {
                    br.BaseStream.Position = entry.Offset;
                    byte[] unpacked = new byte[entry.UnpackedSize];
                    LzUnpack(data, unpacked);
                    File.WriteAllBytes(fileName, unpacked);
                }
                else
                {
                    File.WriteAllBytes(fileName, data);
                }
                Logger.UpdateBar();
            }
            fs.Dispose();
            br.Dispose();
        }

        public override void Pack(string folderPath, string filePath)
        {
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);

            FileInfo[] files = new DirectoryInfo(folderPath).GetFiles("*", SearchOption.AllDirectories);
            Logger.InitBar(files.Length);

            bw.Write(Encoding.ASCII.GetBytes(Magic));
            bw.BaseStream.Position = 12;
            bw.Write(files.Length);

            long indexOffset = 0x10;
            long dataOffset = 0x10 + (files.Length * 0x20);

            foreach (FileInfo file in files)
            {
                string relativePath = file.FullName.Substring(folderPath.Length + 1);

                bw.BaseStream.Position = indexOffset;
                bw.WritePaddedString(relativePath, 0x10);
                bw.BaseStream.Position = indexOffset + 0x13;
                bw.Write((byte)0); // Not packed
                bw.BaseStream.Position = indexOffset + 0x14;
                bw.Write((uint)file.Length);
                bw.BaseStream.Position = indexOffset + 0x18;
                bw.Write((uint)file.Length);
                bw.BaseStream.Position = indexOffset + 0x1C;
                bw.Write((uint)dataOffset);
                indexOffset += 0x20;

                bw.BaseStream.Position = dataOffset;
                byte[] fileData = File.ReadAllBytes(file.FullName);
                bw.Write(fileData);
                dataOffset += file.Length;
                Logger.UpdateBar();
            }
            fw.Dispose();
            bw.Dispose();
        }

        private void LzUnpack(byte[] input, byte[] output)
        {
            using (var ms = new MemoryStream(input))
            {
                using (var reader = new BinaryReader(ms))
                {
                    int dst = 0;
                    int bits = 0;
                    byte mask = 0;
                    while (dst < output.Length)
                    {
                        mask >>= 1;
                        if (mask == 0)
                        {
                            bits = reader.ReadByte();
                            if (-1 == bits)
                            {
                                break;
                            }
                            mask = 0x80;
                        }
                        if ((mask & bits) != 0)
                        {
                            int offset = reader.ReadUInt16();
                            int count = (offset >> 12) + 1;
                            offset &= 0xFFF;
                            Binary.CopyOverlapped(output, dst - offset, dst, count);
                            dst += count;
                        }
                        else
                        {
                            output[dst++] = reader.ReadByte();
                        }
                    }
                }
            }
        }
    }
}
