using GalArc.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using Utility;
using Utility.Extensions;

namespace ArcFormats.Valkyria
{
    internal class DAT
    {
        public class Entry
        {
            public string Name { get; set; }
            public long Offset { get; set; }
            public uint Size { get; set; }
        }

        public void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs); 
            {
                try
                {
                    // Read index size and validate
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

                    // Initialize offset calculation
                    uint indexOffset = 4; // Starting after the index size
                    long baseOffset = indexOffset + indexSize;
                    var entries = new List<Entry>();

                    // Read file entries
                    for (int i = 0; i < fileCount; i++)
                    {
                        var entry = new Entry();

                        // Read filename (0x104 bytes)
                        entry.Name = br.ReadCString(ArcEncoding.Shift_JIS);
                        fs.Position = indexOffset + 0x104;

                        // Read offset and size
                        uint offset = br.ReadUInt32();
                        entry.Offset = baseOffset + offset;
                        entry.Size = br.ReadUInt32();

                        // Validate entry
                        if (entry.Offset + entry.Size > fs.Length)
                        {
                            throw new InvalidDataException(
                                $"Invalid file placement: {entry.Name} at offset {entry.Offset} with size {entry.Size}");
                        }

                        entries.Add(entry);
                        indexOffset += 0x10C; // Move to next entry
                    }

                    // Extract files
                    Logger.InitBar(fileCount);
                    Directory.CreateDirectory(folderPath);

                    foreach (var entry in entries)
                    {
                        try
                        {
                            // Sanitize filename and create full path
                            string safeName = Path.GetFileName(entry.Name);
                            string outputPath = Path.Combine(folderPath, safeName);

                            // Read and write file data
                            fs.Position = entry.Offset;
                            byte[] data = new byte[entry.Size];
                            int bytesRead = fs.Read(data, 0, (int)entry.Size);

                            if (bytesRead == entry.Size)
                            {
                                // Ensure directory exists for file
                                string directoryPath = Path.GetDirectoryName(outputPath);
                                if (!string.IsNullOrEmpty(directoryPath))
                                {
                                    Directory.CreateDirectory(directoryPath);
                                }

                                File.WriteAllBytes(outputPath, data);
                                Logger.UpdateBar();
                            }
                            else
                            {
                                throw new InvalidDataException($"Failed to read complete file: {entry.Name}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.UpdateBar($"Error extracting {entry.Name}: {ex.Message}");
                            // Continue with next file
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException($"Error processing DAT archive: {ex.Message}", ex);
                }
              
            }
            fs.Dispose();
            br.Dispose();
        }


        public void Pack(string folderPath, string filePath)
        {
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);
            FileInfo[] files  = new DirectoryInfo(folderPath).GetFiles();
            // Calculate index size
            uint indexSize = (uint)(files.Length * 0x10C);

            // Write placeholder for index size
            bw.Write(indexSize);

            // Calculate base offset for file data
            long baseOffset = 4 + indexSize; // 4 bytes for index size + index entries
            long currentDataOffset = baseOffset;

            // Create list to store entries for later writing
            var entries = new List<(string name, byte[] data, uint offset)>();

            Logger.InitBar(files.Length);

            // First pass: Process all files and collect data
            foreach (var file in files)
            {
                byte[] fileData = File.ReadAllBytes(file.FullName);

                // Store entry information
                entries.Add((file.Name, fileData, (uint)(currentDataOffset - baseOffset)));

                // Update offset for next file
                currentDataOffset += fileData.Length;

                Logger.UpdateBar();
            }

            // Second pass: Write index entries
            foreach (var entry in entries)
            {
                // Write filename (padded to 0x104 bytes)
                byte[] nameBytes = ArcEncoding.Shift_JIS.GetBytes(entry.name);
                if (nameBytes.Length > 0x104)
                {
                    // Truncate if necessary to match format
                    bw.Write(nameBytes, 0, 0x104);
                }
                else
                {
                    bw.Write(nameBytes);
                    bw.Write(new byte[0x104 - nameBytes.Length]); // Padding
                }

                // Write offset and sizes
                bw.Write(entry.offset);                    // Offset from base
                bw.Write((uint)entry.data.Length);        // Size
            }

            // Third pass: Write file data
            foreach (var entry in entries)
            {
                bw.Write(entry.data);
            }
            fw.Dispose();
            bw.Dispose();
        }
    }
}
