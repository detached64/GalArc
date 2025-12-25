using CommunityToolkit.Mvvm.ComponentModel;
using GalArc.I18n;
using GalArc.Infrastructure.Logging;
using GalArc.Infrastructure.Progress;
using GalArc.Models.Formats.Commons;
using GalArc.Models.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GalArc.Models.Formats.Softpal;

internal class PAC : ArcFormat, IUnpackConfigurable, IPackConfigurable
{
    public override string Name => "PAC";
    public override string Description => "Softpal/Amusecraft Archive";
    public override bool CanWrite => true;

    private SoftpalPACUnpackOptions _unpackOptions;
    public ArcOptions UnpackOptions => _unpackOptions ??= new SoftpalPACUnpackOptions();

    private SoftpalPACPackOptions _packOptions;
    public ArcOptions PackOptions => _packOptions ??= new SoftpalPACPackOptions();

    private readonly byte[] Magic = Utility.HexStringToByteArray("50414320");

    public override void Unpack(string filePath, string folderPath)
    {
        FileStream fs = File.OpenRead(filePath);
        BinaryReader br = new(fs);
        bool isVer1 = !br.ReadBytes(4).SequenceEqual(Magic);
        fs.Dispose();
        br.Dispose();
        if (isVer1)
        {
            Logger.ShowVersion("pac", 1);
            UnpackV1(filePath, folderPath);
        }
        else
        {
            Logger.ShowVersion("pac", 2);
            UnpackV2(filePath, folderPath);
        }
    }

    private void UnpackV1(string filePath, string folderPath)
    {
        using FileStream fs = File.OpenRead(filePath);
        using BinaryReader br = new(fs);
        int fileCount = br.ReadUInt16();
        fs.Position = 0x3fe;
        List<Entry> entries = [];
        ProgressManager.SetMax(fileCount);

        for (int i = 0; i < fileCount; i++)
        {
            Entry entry = new();
            entry.Name = ArcEncoding.Shift_JIS.GetString(br.ReadBytes(32)).TrimEnd('\0');
            entry.Size = br.ReadUInt32();
            entry.Offset = br.ReadUInt32();
            entries.Add(entry);
        }
        foreach (Entry entry in entries)
        {
            byte[] data = br.ReadBytes((int)entry.Size);
            if (_unpackOptions.DecryptScripts && data.Length >= 16 && data[0] == 36)  //'$'
            {
                try
                {
                    Logger.Debug(MsgStrings.Decrypting, entry.Name);
                    DecryptScript(data);
                }
                catch
                {
                    Logger.Error(MsgStrings.ErrorDecScrFailed);
                }
            }
            File.WriteAllBytes(Path.Combine(folderPath, entry.Name), data);
            data = null;
            ProgressManager.Progress();
        }
    }

    private void UnpackV2(string filePath, string folderPath)
    {
        using FileStream fs = File.OpenRead(filePath);
        using BinaryReader br = new(fs);
        if (!br.ReadBytes(4).SequenceEqual(Magic))
        {
            throw new InvalidArchiveException();
        }
        br.ReadInt32();
        int fileCount = br.ReadInt32();
        ProgressManager.SetMax(fileCount);
        fs.Position = 0x804;
        for (int i = 0; i < fileCount; i++)
        {
            Entry entry = new();
            entry.Name = ArcEncoding.Shift_JIS.GetString(br.ReadBytes(32)).TrimEnd('\0');
            entry.Size = br.ReadUInt32();
            entry.Offset = br.ReadUInt32();
            long pos = fs.Position;
            fs.Position = entry.Offset;
            byte[] data = br.ReadBytes((int)entry.Size);
            if (_unpackOptions.DecryptScripts && data.Length >= 16 && data[0] == 36)  //'$'
            {
                try
                {
                    Logger.Debug(MsgStrings.Decrypting, entry.Name);
                    DecryptScript(data);
                }
                catch
                {
                    Logger.Error(MsgStrings.ErrorDecScrFailed);
                }
            }

            File.WriteAllBytes(Path.Combine(folderPath, entry.Name), data);
            data = null;
            fs.Position = pos;
            ProgressManager.Progress();
        }
    }

    private static void DecryptScript(byte[] data)
    {
        int count = (data.Length - 16) / 4;
        int shift = 4;
        for (int i = 0; i < count; i++)
        {
            int index = 16 + (i * 4);
            uint value = BitConverter.ToUInt32(data, index);

            byte rotatedByte = Binary.RotByteL((byte)value, shift++);
            value = (value & 0xFFFFFF00u) | rotatedByte;
            value ^= 0x084DF873u ^ 0xFF987DEEu;

            byte[] bytes = BitConverter.GetBytes(value);
            Buffer.BlockCopy(bytes, 0, data, index, 4);
        }
    }

    private static void EncryptScript(byte[] data)
    {
        int count = (data.Length - 16) / 4;
        int shift = 4;
        for (int i = 0; i < count; i++)
        {
            int index = 16 + (i * 4);
            uint value = BitConverter.ToUInt32(data, index);
            value ^= 0x084DF873u ^ 0xFF987DEEu;

            byte rotatedByte = Binary.RotByteR((byte)value, shift++);
            value = (value & 0xFFFFFF00u) | rotatedByte;

            byte[] bytes = BitConverter.GetBytes(value);
            Buffer.BlockCopy(bytes, 0, data, index, 4);
        }
    }

    public override void Pack(string folderPath, string filePath)
    {
        if (_packOptions.Version == 1)
        {
            PackV1(folderPath, filePath);
        }
        else
        {
            PackV2(folderPath, filePath);
        }
    }

    private void PackV1(string folderPath, string filePath)
    {
        string[] files = Directory.GetFiles(folderPath);
        Dictionary<char, int> characterCount = CountFirstCharacters(files);
        using FileStream fw = File.Create(filePath);
        using BinaryWriter bw = new(fw);
        int fileCount = files.Length;
        ProgressManager.SetMax(fileCount);

        bw.Write((ushort)fileCount);
        ushort countToThis = 0;
        for (int i = 0; i < 255; i++)
        {
            characterCount.TryGetValue((char)i, out int count);
            ushort thisCount = (ushort)count;
            bw.Write(countToThis);
            countToThis += thisCount;
            bw.Write(thisCount);
        }

        uint offset = 0x3fe + (uint)(40 * fileCount);
        foreach (string str in files)
        {
            bw.WritePaddedString(Path.GetFileName(str), 32);
            uint size = (uint)new FileInfo(str).Length;
            bw.Write(size);
            bw.Write(offset);
            offset += size;
        }

        foreach (string str in files)
        {
            byte[] buffer = File.ReadAllBytes(str);
            if (_packOptions.EncryptScripts && buffer.Length >= 16 && buffer[0] == 36)  //'$'
            {
                try
                {
                    Logger.Debug(MsgStrings.Encrypting, Path.GetFileName(str));
                    EncryptScript(buffer);
                }
                catch
                {
                    Logger.Error(MsgStrings.ErrorEncScrFailed);
                }
            }
            bw.Write(buffer);
            buffer = null;
            ProgressManager.Progress();
        }

        bw.Write(Encoding.ASCII.GetBytes("EOF "));
    }

    private void PackV2(string folderPath, string filePath)
    {
        string[] files = Directory.GetFiles(folderPath);
        Dictionary<char, int> characterCount = CountFirstCharacters(files);
        int fileCount = files.Length;

        ProgressManager.SetMax(fileCount);

        using FileStream fw = File.Create(filePath);
        using BinaryWriter bw = new(fw);
        //header
        bw.Write(Magic);
        bw.Write(0);
        bw.Write(fileCount);
        //index
        int countToThis = 0;
        for (int i = 0; i < 255; i++)
        {
            characterCount.TryGetValue((char)i, out int count);
            bw.Write(countToThis);
            countToThis += count;
            bw.Write(count);
        }
        //entries
        uint currentOffset = (uint)(2052 + (40 * fileCount));
        foreach (string str in files)
        {
            bw.WritePaddedString(Path.GetFileName(str), 32);
            uint size = (uint)new FileInfo(str).Length;
            bw.Write(size);
            bw.Write(currentOffset);
            currentOffset += size;
        }
        //data
        foreach (string str in files)
        {
            byte[] buffer = File.ReadAllBytes(str);
            if (_packOptions.EncryptScripts && buffer.Length >= 16 && buffer[0] == 36)  //'$'
            {
                try
                {
                    Logger.Debug(MsgStrings.Encrypting, Path.GetFileName(str));
                    EncryptScript(buffer);
                }
                catch
                {
                    Logger.Error(MsgStrings.ErrorEncScrFailed);
                }
            }
            bw.Write(buffer);
            buffer = null;
            ProgressManager.Progress();
        }
        //end
        if (_packOptions.ComputeChecksum)
        {
            uint checksum = 0;
            fw.Position = 0;
            using BufferedStream bufferedStream = new(fw, 65536);
            int b;
            while ((b = bufferedStream.ReadByte()) != -1)
            {
                checksum += (uint)b;
            }
            bw.Write(checksum);
            bw.Write(Encoding.ASCII.GetBytes("EOF "));
        }
        else
        {
            bw.Write(0);
            bw.Write(Encoding.ASCII.GetBytes("EOF "));
        }
        ProgressManager.Progress();
    }

    private static Dictionary<char, int> CountFirstCharacters(string[] strings)
    {
        Dictionary<char, int> countDict = [];

        foreach (string str in strings)
        {
            if (str.Length > 0)
            {
                char firstChar = Path.GetFileName(str)[0];
                countDict[firstChar] = countDict.TryGetValue(firstChar, out int value) ? ++value : 1;
            }
        }
        return countDict;
    }
}

internal partial class SoftpalPACUnpackOptions : ArcOptions
{
    [ObservableProperty]
    private bool decryptScripts = true;
}

internal partial class SoftpalPACPackOptions : ArcOptions
{
    [ObservableProperty]
    private bool encryptScripts = true;
    [ObservableProperty]
    private bool computeChecksum = true;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanComputeChecksum))]
    private int version = 2;
    [ObservableProperty]
    private IReadOnlyList<int> versions = [1, 2];
    public bool CanComputeChecksum => Version == 2;
}
