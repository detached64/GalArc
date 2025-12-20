using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using GalArc.I18n;
using GalArc.Infrastructure.Progress;
using GalArc.Models.Database.Commons;
using GalArc.Models.Formats.Commons;
using GalArc.Models.Utils;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace GalArc.Models.Formats.Lightvn;

internal class VNDAT : ArcFormat, IUnpackConfigurable
{
    public override string Name => "VNDAT";
    public override string Description => "LightVN VNDAT Archive";

    private LightvnVNDATUnpackOptions _unpackOptions;
    public ArcOptions UnpackOptions => _unpackOptions ??= new LightvnVNDATUnpackOptions();

    public override void Unpack(string filePath, string folderPath)
    {
        using ZipFile zip = new(filePath, StringCodec.FromEncoding(_unpackOptions.Encoding));
        List<ZipEntry> entries = [.. zip.Cast<ZipEntry>().Where(e => !e.IsDirectory)];
        if (entries.Any(e => e.IsCrypted))
        {
            throw new InvalidArchiveException("The archive is encrypted, but LightVN VNDAT format does not support decryption.");
        }
        ProgressManager.SetMax(entries.Count);
        foreach (ZipEntry entry in entries)
        {
            using Stream input = zip.GetInputStream(entry);
            string path = Path.Combine(folderPath, entry.Name);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            using FileStream output = File.Create(path);
            input.CopyTo(output);
        }
        string[] files = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
        byte[] xorKey = _unpackOptions.Key;
        byte[] reversedXorKey = [.. xorKey.Reverse()];
        foreach (string file in files)
        {
            using FileStream fs = File.Open(file, FileMode.Open, FileAccess.ReadWrite);
            if (fs.Length < 100)
            {
                byte[] data = new byte[fs.Length];
                fs.ReadExactly(data);
                for (long i = 0; i < data.Length; i++)
                {
                    data[i] ^= reversedXorKey[i % reversedXorKey.Length];
                }
                fs.Seek(0, SeekOrigin.Begin);
                fs.Write(data, 0, data.Length);
            }
            else
            {
                byte[] header = new byte[100];
                fs.ReadExactly(header);
                for (int i = 0; i < header.Length; i++)
                {
                    header[i] ^= xorKey[i % xorKey.Length];
                }
                fs.Seek(0, SeekOrigin.Begin);
                fs.Write(header, 0, header.Length);

                fs.Seek(-99, SeekOrigin.End);
                byte[] footer = new byte[99];
                fs.ReadExactly(footer);
                for (int i = 0; i < footer.Length; i++)
                {
                    footer[i] ^= reversedXorKey[i % reversedXorKey.Length];
                }
                fs.Seek(-99, SeekOrigin.End);
                fs.Write(footer, 0, footer.Length);
            }
            ProgressManager.Progress();
        }
    }
}

internal partial class LightvnVNDATUnpackOptions : ArcOptions
{
    private static readonly byte[] DefaultKey = [0x64, 0x36, 0x63, 0x35, 0x66, 0x4B, 0x49, 0x33, 0x47, 0x67, 0x42, 0x57, 0x70, 0x5A, 0x46, 0x33, 0x54, 0x7A, 0x36, 0x69, 0x61, 0x33, 0x6B, 0x46, 0x30];

    public readonly LightvnScheme Scheme;

    public LightvnVNDATUnpackOptions()
    {
        if (Design.IsDesignMode)
            return;
        Scheme = DatabaseManager.LoadScheme(DatabaseSerializationContext.Default.LightvnScheme);
        Names.Add(GuiStrings.DefaultEncryption);
        if (Scheme?.KnownSchemes != null)
        {
            foreach (KeyValuePair<string, byte[]> pair in Scheme.KnownSchemes)
            {
                Names.Add(pair.Key);
            }
        }
    }

    [ObservableProperty]
    private IReadOnlyList<Encoding> encodings = ArcEncoding.SupportedEncodings;
    [ObservableProperty]
    private Encoding encoding = Encoding.UTF8;
    [ObservableProperty]
    private ObservableCollection<string> names = [];
    [ObservableProperty]
    private string selectedName = GuiStrings.DefaultEncryption;
    public byte[] Key => Scheme?.KnownSchemes?.GetValueOrDefault(SelectedName) ?? DefaultKey;
}
