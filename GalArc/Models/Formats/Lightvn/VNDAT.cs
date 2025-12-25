using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using GalArc.I18n;
using GalArc.Infrastructure.Progress;
using GalArc.Models.Database.Commons;
using GalArc.Models.Formats.Commons;
using GalArc.Models.Utils;
using ICSharpCode.SharpZipLib.Zip;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace GalArc.Models.Formats.Lightvn;

internal class VNDAT : MCDAT, IUnpackConfigurable
{
    public override string Name => "VNDAT";
    public override string Description => "LightVN VNDAT Archive";

    private LightvnVNDATUnpackOptions _unpackOptions;
    public ArcOptions UnpackOptions => _unpackOptions ??= new LightvnVNDATUnpackOptions();

    public override void Unpack(string input, string output)
    {
        using ZipFile zip = new(input, StringCodec.FromEncoding(_unpackOptions.Encoding));
        List<ZipEntry> entries = [.. zip.Cast<ZipEntry>().Where(e => !e.IsDirectory)];
        if (entries.Any(e => e.IsCrypted))
        {
            throw new InvalidArchiveException("The archive is encrypted, but LightVN VNDAT format does not support decryption.");
        }
        ProgressManager.SetMax(entries.Count);
        foreach (ZipEntry entry in entries)
        {
            using Stream dataStream = zip.GetInputStream(entry);
            string path = Path.Combine(output, entry.Name);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            using FileStream outputStream = File.Create(path);
            dataStream.CopyTo(outputStream);
        }
        foreach (string file in Directory.GetFiles(output, "*", SearchOption.AllDirectories))
        {
            using FileStream fs = File.Open(file, FileMode.Open, FileAccess.ReadWrite);
            Decrypt(fs, _unpackOptions.Key);
            ProgressManager.Progress();
        }
    }
}

internal partial class LightvnVNDATUnpackOptions : ArcOptions
{
    internal static readonly byte[] DefaultKey = [0x64, 0x36, 0x63, 0x35, 0x66, 0x4B, 0x49, 0x33, 0x47, 0x67, 0x42, 0x57, 0x70, 0x5A, 0x46, 0x33, 0x54, 0x7A, 0x36, 0x69, 0x61, 0x33, 0x6B, 0x46, 0x30];

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
