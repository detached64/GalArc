using GalArc.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using Utility;
using Utility.Extensions;

namespace ArcFormats.Valkyria
{
    public class DAT : ArchiveFormat
    {
        public override void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            try
            {
                uint indexSize = br.ReadUInt32();
                if (indexSize == 0 || indexSize >= fs.Length)
                {
                    throw new InvalidDataException("Invalid index size");
                }

                int fileCount = (int)indexSize / 0x10C;
                if (indexSize != (uint)fileCount * 0x10C || (uint)fileCount > 10000)
                {
                    throw new InvalidDataException("Invalid file count or index size");
                }

                uint indexOffset = 4;
                long baseOffset = indexOffset + indexSize;
                var entries = new List<Entry>();

                for (int i = 0; i < fileCount; i++)
                {
                    var entry = new Entry();
                    entry.Name = br.ReadCString();
                    fs.Position = indexOffset + 0x104;
                    uint offset = br.ReadUInt32();
                    entry.Offset = (uint)(baseOffset + offset);
                    entry.Size = br.ReadUInt32();
                    if (entry.Offset + entry.Size > fs.Length)
                    {
                        throw new InvalidDataException($"Invalid file placement: {entry.Name} at offset {entry.Offset} with size {entry.Size}");
                    }

                    entries.Add(entry);
                    indexOffset += 0x10C;
                }
                Logger.InitBar(fileCount);
                Directory.CreateDirectory(folderPath);

                foreach (var entry in entries)
                {
                    try
                    {
                        string safeName = Path.GetFileName(entry.Name);
                        string outputPath = Path.Combine(folderPath, safeName);

                        fs.Position = entry.Offset;
                        byte[] data = new byte[entry.Size];
                        int bytesRead = fs.Read(data, 0, (int)entry.Size);

                        if (bytesRead == entry.Size)
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                            File.WriteAllBytes(outputPath, data);
                            Logger.UpdateBar();
                            data = null;
                        }
                        else
                        {
                            throw new InvalidDataException($"Failed to read complete file: {entry.Name}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Error extracting {entry.Name}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidDataException($"Error processing DAT archive: {ex.Message}", ex);
            }
            fs.Dispose();
            br.Dispose();
        }

        public override void Pack(string folderPath, string filePath)
        {
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);
            FileInfo[] files = new DirectoryInfo(folderPath).GetFiles();
            uint indexSize = (uint)(files.Length * 0x10C);
            uint offset = 0;
            bw.Write(indexSize);

            Logger.InitBar(files.Length);

            foreach (var file in files)
            {
                bw.WritePaddedString(file.Name, 0x104);
                bw.Write(offset);
                bw.Write((uint)file.Length);
                offset += (uint)file.Length;
            }

            foreach (var file in files)
            {
                byte[] bytes = File.ReadAllBytes(file.FullName);
                bw.Write(bytes);
                bytes = null;
                Logger.UpdateBar();
            }
            fw.Dispose();
            bw.Dispose();
        }
    }
}
