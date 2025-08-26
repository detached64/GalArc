
namespace GalArc.Models.Formats.Commons;

public class Entry
{
    public string Name { get; set; }
    public string Path { get; set; }
    public uint Offset { get; set; }
    public uint Size { get; set; }
}

public class PackedEntry : Entry
{
    public bool IsPacked { get; set; }

    public uint UnpackedSize { get; set; }
}
