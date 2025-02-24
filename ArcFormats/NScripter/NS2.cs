using ArcFormats.Properties;
using GalArc.Controls;
using GalArc.Database;
using GalArc.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utility;
using Utility.Extensions;

namespace ArcFormats.NScripter
{
    public class NS2 : ArchiveFormat
    {
        public override OptionsTemplate UnpackExtraOptions => UnpackNS2Options.Instance;

        internal static Ns2Scheme Scheme;

        internal static string Key;

        private class Ns2Entry : PackedEntry
        {
            public string RelativePath { get; set; }
        }

        public override void Unpack(string filePath, string folderPath)
        {
            List<Ns2Entry> entries = new List<Ns2Entry>();
            byte[] data = File.ReadAllBytes(filePath);
            uint dataOffset = BitConverter.ToUInt32(data, 0);
            if (dataOffset > data.Length)
            {
                if (string.IsNullOrEmpty(Key))
                {
                    Logger.Error(Resources.msgNeedDec);
                    return;
                }
                Ns2Decryptor decryptor = new Ns2Decryptor(data, Encoding.ASCII.GetBytes(Key));
                decryptor.Decrypt();
            }
            MemoryStream ms = new MemoryStream(data);
            BinaryReader br = new BinaryReader(ms);
            dataOffset = br.ReadUInt32();
            try
            {
                while (ms.Position < dataOffset - 1)
                {
                    Ns2Entry entry = new Ns2Entry();
                    br.ReadByte();              //skip "
                    entry.Path = Path.Combine(folderPath, br.ReadCString(0x22));
                    entry.Size = br.ReadUInt32();
                    entries.Add(entry);
                }
            }
            catch (Exception)
            {
                Logger.Error(Resources.msgWrongScheme);
            }
            br.ReadByte(); //skip 'e'

            Directory.CreateDirectory(folderPath);
            Logger.InitBar(entries.Count);

            foreach (Ns2Entry entry in entries)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(entry.Path));
                File.WriteAllBytes(entry.Path, br.ReadBytes((int)entry.Size));
                data = null;
                Logger.UpdateBar();
            }
            ms.Dispose();
            br.Dispose();
        }

        public override void Pack(string folderPath, string filePath)
        {
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);

            string[] fullPaths = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
            string[] relativePaths = Utils.GetRelativePaths(fullPaths, folderPath);

            int fileCount = fullPaths.Length;
            Logger.InitBar(fileCount);
            uint dataOffset = 4;

            Array.Sort(fullPaths, StringComparer.Ordinal);

            List<Ns2Entry> entries = new List<Ns2Entry>();

            for (int i = 0; i < fileCount; i++)
            {
                Ns2Entry entry = new Ns2Entry();
                entry.RelativePath = relativePaths[i];
                entry.Path = Path.Combine(folderPath, relativePaths[i]);
                entry.Size = (uint)new FileInfo(entry.Path).Length;
                dataOffset += (uint)(ArcEncoding.Shift_JIS.GetBytes(entry.RelativePath).Length + 2);
                dataOffset += 4;
                entries.Add(entry);
            }
            dataOffset++;//'e'
            bw.Write(dataOffset);
            foreach (Ns2Entry entry in entries)
            {
                bw.Write('\"');
                bw.Write(ArcEncoding.Shift_JIS.GetBytes(entry.RelativePath));
                bw.Write('\"');
                bw.Write(entry.Size);
            }
            bw.Write('e');

            foreach (Ns2Entry entry in entries)
            {
                byte[] buffer = File.ReadAllBytes(entry.Path);
                bw.Write(buffer);
                buffer = null;
                Logger.UpdateBar();
            }
            fw.Dispose();
            bw.Dispose();
        }

        public override void DeserializeScheme(out string name, out int count)
        {
            Scheme = Deserializer.ReadScheme<Ns2Scheme>();
            name = "Ns2";
            count = Scheme?.KnownKeys?.Count ?? 0;
        }
    }
}