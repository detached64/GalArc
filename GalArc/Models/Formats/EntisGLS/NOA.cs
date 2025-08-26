using CommunityToolkit.Mvvm.ComponentModel;
using GalArc.Infrastructure.Progress;
using GalArc.Models.Formats.Commons;
using GalArc.Models.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GalArc.Models.Formats.EntisGLS;

internal class NOA : ArcFormat, IUnpackConfigurable, IPackConfigurable
{
    public override string Name => "NOA";
    public override string Description => "EntisGLS Archive";
    public override bool CanWrite => true;

    private EntisGLSNOAUnpackOptions _unpackOptions;
    public ArcOptions UnpackOptions => _unpackOptions ??= new EntisGLSNOAUnpackOptions();

    private EntisGLSNOAPackOptions _packOptions;
    public ArcOptions PackOptions => _packOptions ??= new EntisGLSNOAPackOptions();

    private readonly byte[] Magic1 = Utility.HexStringToByteArray("456e7469731a00000004000200000000");

    private readonly byte[] Magic2 = Utility.HexStringToByteArray("45524953412d417263686976652066696c65");

    private class Header
    {
        public byte[] Magic1 { get; set; }
        public byte[] Magic2 { get; set; }
        public uint NoaSizeWithout { get; set; }
    }

    private class EntryHeader
    {
        public string Magic { get; set; }
        public long IndexSize { get; set; }
        public int FileCount { get; set; }
    }

    private class NoaEntry
    {
        public ulong Size { get; set; }
        public uint Attribute { get; set; }
        public uint EncType { get; set; }
        public long Offset { get; set; }
        public TimeStamp TimeStamp { get; set; }
        public uint ExtraInfoLen { get; set; }
        public string ExtraInfo { get; set; }
        public uint NameLen { get; set; }
        public string Name { get; set; }

        public NoaEntry()
        {
            TimeStamp = new TimeStamp();
            Size = 0;
            Attribute = 0;
            EncType = 0;
            Offset = 0;
            ExtraInfoLen = 0;
            ExtraInfo = string.Empty;
            NameLen = 0;
            Name = string.Empty;
        }
    }

    private class TimeStamp
    {
        public byte Second { get; set; }
        public byte Minute { get; set; }
        public byte Hour { get; set; }
        public byte Week { get; set; }
        public byte Day { get; set; }
        public byte Month { get; set; }
        public ushort Year { get; set; }
    }

    public override void Unpack(string filePath, string folderPath)
    {
        using FileStream fs = File.OpenRead(filePath);
        using BinaryReader br = new(fs);
        Header header = new();
        header.Magic1 = br.ReadBytes(16);
        header.Magic2 = br.ReadBytes(18);

        if (!header.Magic1.SequenceEqual(Magic1) || !header.Magic2.SequenceEqual(Magic2))
        {
            throw new InvalidArchiveException();
        }

        br.ReadBytes(22);
        header.NoaSizeWithout = br.ReadUInt32();
        br.BaseStream.Position += 4;

        EntryHeader entry_header = new();
        entry_header.Magic = Encoding.ASCII.GetString(br.ReadBytes(8));
        if (entry_header.Magic != "DirEntry")
        {
            throw new InvalidArchiveException();
        }

        entry_header.IndexSize = br.ReadInt64();
        entry_header.FileCount = br.ReadInt32();
        ProgressManager.SetMax(entry_header.FileCount);
        //List<EntisGLS_noa_timeStamp> l = new List<EntisGLS_noa_timeStamp>();

        Encoding encoding = _unpackOptions.Encoding;
        long pos;
        for (int i = 0; i < entry_header.FileCount; i++)
        {
            NoaEntry entry = new();
            entry.Size = br.ReadUInt64();
            entry.Attribute = br.ReadUInt32();
            entry.EncType = br.ReadUInt32();
            entry.Offset = br.ReadInt64() + 64;

            if (entry.EncType != 0)
            {
                throw new NotImplementedException("Error:encrypted noa file not supported.");
            }
            br.ReadBytes(8);
            //entry.timestamp.second = br.ReadByte();
            //entry.timestamp.minute = br.ReadByte();
            //entry.timestamp.hour = br.ReadByte();
            //entry.timestamp.week = br.ReadByte();
            //entry.timestamp.day = br.ReadByte();
            //entry.timestamp.month = br.ReadByte();
            //entry.timestamp.year = br.ReadUInt16();
            //l.Add(entry.timestamp);
            //Save timestamp disabled
            entry.ExtraInfoLen = br.ReadUInt32();
            entry.ExtraInfo = Encoding.ASCII.GetString(br.ReadBytes((int)entry.ExtraInfoLen));
            entry.NameLen = br.ReadUInt32();
            entry.Name = encoding.GetString(br.ReadBytes((int)(entry.NameLen - 1)));
            br.ReadByte();
            pos = fs.Position;
            fs.Seek(entry.Offset, SeekOrigin.Begin);
            br.ReadBytes(16);
            byte[] buffer = br.ReadBytes((int)entry.Size);
            File.WriteAllBytes(Path.Combine(folderPath, entry.Name), buffer);
            buffer = null;
            fs.Position = pos;
            ProgressManager.Progress();
        }
        //string jsonStr = JsonSerializer.Serialize(l);
        //File.WriteAllText(folderPath + "\\" + "TimestampInfo.json", jsonStr);
    }

    public override void Pack(string folderPath, string filePath)
    {
        //string jsonPath = folderPath + "\\" + "TimestampInfo.json";
        using FileStream fw = File.Create(filePath);
        using BinaryWriter bw = new(fw);
        string[] files = Directory.GetFiles(folderPath);
        //string[] excludedNames = new string[] { "TimestampInfo.json" };
        //var filteredFiles = files.Where(file => !excludedNames.Contains(Path.GetFileName(file)));
        int fileCount = files.Length;
        //string jsonStr;
        //List<EntisGLS_noa_timeStamp> time = new();
        //if (File.Exists(jsonPath))
        //{
        //    fileCount = Util.GetFileCount_All(folderPath) - 1;
        //    jsonStr = File.ReadAllText(jsonPath);
        //}
        //else
        //{
        //    fileCount = Util.GetFileCount_All(folderPath);
        //    jsonStr = string.Empty;
        //}
        ProgressManager.SetMax(fileCount);
        //if (string.IsNullOrEmpty(jsonStr))
        //{
        //    for (int j = 0; j < fileCount; j++)
        //    {
        //        EntisGLS_noa_timeStamp ts = new();
        //        ts.second = 0x00;
        //        ts.minute = 0x00;
        //        ts.hour = 0x00;
        //        ts.week = 0x00;
        //        ts.day = 0x00;
        //        ts.month = 0x00;
        //        ts.year = 0;
        //        time.Add(ts);
        //    }
        //}
        //else
        //{
        //    time = JsonSerializer.Deserialize<List<EntisGLS_noa_timeStamp>>(jsonStr);
        //}

        Encoding encoding = _packOptions.Encoding;
        //header
        bw.Write(Magic1);
        bw.Write(Magic2);
        bw.Write(new byte[30]);//skip file size,temporarily
                               //entry header
        bw.Write(Encoding.ASCII.GetBytes("DirEntry"));
        //compute index size
        long indexSize = 4 + Utility.GetNameLengthSum(files, encoding) + fileCount + (40 * fileCount);
        bw.Write(indexSize);
        bw.Write(fileCount);

        long offset = 16 + indexSize;
        int i = 0;
        //entry
        foreach (string file in files)
        {
            bw.Write(new FileInfo(file).Length);
            bw.Write((long)0);//attribute&encType
            bw.Write(offset);
            offset += new FileInfo(file).Length + 16;//"filedata" + file size

            //bw.Write(time[i].second);
            //bw.Write(time[i].minute);
            //bw.Write(time[i].hour);
            //bw.Write(time[i].week);
            //bw.Write(time[i].day);
            //bw.Write(time[i].month);
            //bw.Write(time[i].year);
            bw.Write((long)0);//timestamp disabled
            bw.Write(0);
            bw.Write(encoding.GetBytes(Path.GetFileName(file)).Length + 1);
            bw.Write(encoding.GetBytes(Path.GetFileName(file)));
            bw.Write('\0');
            i++;
        }
        foreach (string file in files)
        {
            bw.Write(Encoding.ASCII.GetBytes("filedata"));
            bw.Write(new FileInfo(file).Length);
            byte[] data = File.ReadAllBytes(file);
            bw.Write(data);
            data = null;
            ProgressManager.Progress();
        }
        int size = (int)fw.Position;
        fw.Position = 56;//write size
        bw.Write(size - 64);
    }
}

internal partial class EntisGLSNOAUnpackOptions : ArcOptions
{
    [ObservableProperty]
    private IReadOnlyList<Encoding> encodings = ArcEncoding.SupportedEncodings;
    [ObservableProperty]
    private Encoding encoding = Encoding.UTF8;
}

internal partial class EntisGLSNOAPackOptions : ArcOptions
{
    [ObservableProperty]
    private IReadOnlyList<Encoding> encodings = ArcEncoding.SupportedEncodings;
    [ObservableProperty]
    private Encoding encoding = Encoding.UTF8;
}
