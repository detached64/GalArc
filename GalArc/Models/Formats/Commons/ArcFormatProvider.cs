using System;
using System.Collections.Generic;
using System.Linq;

namespace GalArc.Models.Formats.Commons;

internal static partial class ArcFormatProvider
{
    private static readonly Lazy<List<ArcFormat>> _formats = new(InitFormats);
    private static readonly Lazy<List<ArcFormat>> _writeableFormats = new(InitWriteableFormats);

    private static List<ArcFormat> InitFormats() => [.. LoadFormatsGenerated().OrderBy(f => $"{f.Name} - {f.Description}")];
    private static List<ArcFormat> InitWriteableFormats() => [.. GetFormats().Where(f => f.CanWrite)];
    public static List<ArcFormat> GetFormats() => _formats.Value;
    public static List<ArcFormat> GetWriteableFormats() => _writeableFormats.Value;
}
