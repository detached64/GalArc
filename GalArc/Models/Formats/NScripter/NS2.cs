using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using GalArc.I18n;
using GalArc.Infrastructure.Progress;
using GalArc.Models.Database.Commons;
using GalArc.Models.Formats.Commons;
using GalArc.Models.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace GalArc.Models.Formats.NScripter;

internal class NS2 : ArcFormat, IUnpackConfigurable
{
    public override string Name => "NS2";
    public override string Description => "NScripter NS2 Archive";
    public override bool CanWrite => true;

    private NScripterNS2UnpackOptions _unpackOptions;
    public ArcOptions UnpackOptions => _unpackOptions ??= new NScripterNS2UnpackOptions();

    private class Ns2Entry : PackedEntry
    {
        public string RelativePath { get; set; }
    }

    public override void Unpack(string filePath, string folderPath)
    {
        List<Ns2Entry> entries = [];
        byte[] data = File.ReadAllBytes(filePath);
        uint dataOffset = BitConverter.ToUInt32(data, 0);
        if (dataOffset > data.Length)
        {
            if (string.IsNullOrEmpty(_unpackOptions.Key))
            {
                throw new InvalidArchiveException(MsgStrings.NeedDec);
            }
            Ns2Decryptor decryptor = new(data, Encoding.ASCII.GetBytes(_unpackOptions.Key));
            decryptor.Decrypt();
        }
        using MemoryStream ms = new(data);
        using BinaryReader br = new(ms);
        dataOffset = br.ReadUInt32();
        try
        {
            while (ms.Position < dataOffset - 1)
            {
                Ns2Entry entry = new();
                br.ReadByte();              //skip "
                entry.Path = Path.Combine(folderPath, br.ReadCString(0x22));
                entry.Size = br.ReadUInt32();
                entries.Add(entry);
            }
        }
        catch
        {
            throw new InvalidSchemeException();
        }
        br.ReadByte(); //skip 'e'

        ProgressManager.SetMax(entries.Count);

        foreach (Ns2Entry entry in entries)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(entry.Path));
            File.WriteAllBytes(entry.Path, br.ReadBytes((int)entry.Size));
            data = null;
            ProgressManager.Progress();
        }
    }

    public override void Pack(string folderPath, string filePath)
    {
        using FileStream fw = File.Create(filePath);
        using BinaryWriter bw = new(fw);

        string[] fullPaths = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
        string[] relativePaths = Utility.GetRelativePaths(fullPaths, folderPath);

        int fileCount = fullPaths.Length;
        ProgressManager.SetMax(fileCount);
        uint dataOffset = 4;

        Array.Sort(fullPaths, StringComparer.Ordinal);

        List<Ns2Entry> entries = [];

        for (int i = 0; i < fileCount; i++)
        {
            Ns2Entry entry = new();
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
            ProgressManager.Progress();
        }
    }
}

internal partial class NScripterNS2UnpackOptions : ArcOptions
{
    public readonly Ns2Scheme Scheme;
    public NScripterNS2UnpackOptions()
    {
        if (Design.IsDesignMode)
            return;
        Scheme = DatabaseManager.LoadScheme(DatabaseSerializationContext.Default.Ns2Scheme);
        Names.Add(GuiStrings.NoEncryption);
        if (Scheme?.KnownSchemes != null)
        {
            foreach (KeyValuePair<string, string> pair in Scheme.KnownSchemes)
            {
                Names.Add(pair.Key);
            }
        }
    }

    [ObservableProperty]
    private ObservableCollection<string> names = [];
    [ObservableProperty]
    private string selectedName = GuiStrings.NoEncryption;
    public string Key => Scheme?.KnownSchemes.GetValueOrDefault(SelectedName);
}

internal sealed class Ns2Decryptor
{
    private const int BlockSize = 32;
    private const int InitPos = 16;

    private readonly byte[] key1;
    private readonly byte[] key2;
    private readonly byte[] buffer = new byte[64];
    private readonly MD5 md5 = new();
    private readonly byte[] data;

    public Ns2Decryptor(byte[] data, byte[] key)
    {
        if (key.Length < 96)
        {
            throw new ArgumentException("Key length must be at least 96 bytes.");
        }
        key1 = [.. key.Take(48)];
        key2 = [.. key.Skip(48).Take(48)];
        this.data = data;
    }

    public void Decrypt()
    {
        for (int i = 0; i < data.Length; i += BlockSize)
        {
            byte[] state = new byte[16];
            byte[] temp1 = new byte[16];
            byte[] temp2 = new byte[16];

            Buffer.BlockCopy(data, InitPos + i, buffer, 0, 16);
            Buffer.BlockCopy(key1, 0, buffer, 16, 48);
            md5.Initialize();
            md5.Update(buffer, 0, 64);
            Buffer.BlockCopy(md5.State, 0, state, 0, 16);
            for (int j = 0; j < 16; j++)
            {
                buffer[j] = (byte)(data[i + j] ^ state[j]);
                temp1[j] = buffer[j];
            }
            Buffer.BlockCopy(key2, 0, buffer, 16, 48);
            md5.Initialize();
            md5.Update(buffer, 0, 64);
            Buffer.BlockCopy(md5.State, 0, state, 0, 16);
            for (int j = 0; j < 16; j++)
            {
                buffer[j] = (byte)(data[i + j + 16] ^ state[j]);
                temp2[j] = buffer[j];
            }
            Buffer.BlockCopy(temp2, 0, data, i, 16);
            Buffer.BlockCopy(key1, 0, buffer, 16, 48);
            md5.Initialize();
            md5.Update(buffer, 0, 64);
            Buffer.BlockCopy(md5.State, 0, state, 0, 16);
            for (int j = 0; j < 16; j++)
            {
                data[i + j + 16] = (byte)(temp1[j] ^ state[j]);
            }
        }
    }
}
