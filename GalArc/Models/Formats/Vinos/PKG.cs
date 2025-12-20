using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using GalArc.Infrastructure.Progress;
using GalArc.Models.Database.Commons;
using GalArc.Models.Formats.Commons;
using GalArc.Models.Utils;
using ICSharpCode.SharpZipLib.Zip;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GalArc.Models.Formats.Vinos;

internal class PKG : ArcFormat, IUnpackConfigurable
{
    public override string Name => "PKG";
    public override string Description => "Vinos Windows Player PKWARE Archive";

    private VinosPKGUnpackOptions _unpackOptions;
    public ArcOptions UnpackOptions => _unpackOptions ??= new VinosPKGUnpackOptions();

    public override void Unpack(string filePath, string folderPath)
    {
        using ZipFile file = new(filePath, StringCodec.FromEncoding(_unpackOptions.Encoding));
        List<ZipEntry> entries = [.. file.Cast<ZipEntry>().Where(e => !e.IsDirectory)];
        bool is_encrypted = entries.Any(e => e.IsCrypted);
        if (is_encrypted)
        {
            file.Password = _unpackOptions.SelectedKey != null && _unpackOptions.SelectedKey.TryGetValue(Path.GetFileName(filePath), out string pwd)
                ? pwd
                : throw new InvalidDataException("The archive is encrypted, but no password is provided.");
        }
        ProgressManager.SetMax(entries.Count);
        foreach (ZipEntry entry in entries)
        {
            string name = entry.Name;
            string dir = Path.GetDirectoryName(name);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(Path.Combine(folderPath, dir));
            }
            string path = Path.Combine(folderPath, name);
            using (FileStream fs = File.Create(path))
            {
                file.GetInputStream(entry).CopyTo(fs);
            }
            ProgressManager.Progress();
        }
    }
}

internal partial class VinosPKGUnpackOptions : ArcOptions
{
    public readonly VinosScheme Scheme;

    public VinosPKGUnpackOptions()
    {
        if (Design.IsDesignMode)
            return;
        Scheme = DatabaseManager.LoadScheme(DatabaseSerializationContext.Default.VinosScheme);
        if (Scheme?.KnownSchemes != null)
        {
            KnownSchemes = Scheme.KnownSchemes;
            SelectedKey = KnownSchemes.Values.FirstOrDefault();
        }
    }

    [ObservableProperty]
    private IReadOnlyList<Encoding> encodings = ArcEncoding.SupportedEncodings;
    [ObservableProperty]
    private Encoding encoding = Encoding.UTF8;
    [ObservableProperty]
    private IReadOnlyDictionary<string, Dictionary<string, string>> knownSchemes = new Dictionary<string, Dictionary<string, string>>();
    [ObservableProperty]
    private Dictionary<string, string> selectedKey;
}
