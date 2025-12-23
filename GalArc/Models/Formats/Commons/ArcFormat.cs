using System;

namespace GalArc.Models.Formats.Commons;

internal abstract class ArcFormat
{
    public abstract string Name { get; }
    public abstract string Description { get; }
    public virtual bool CanWrite { get; }
    public virtual bool CanFind { get; } = true;
    public virtual bool IsSingleFileArchive { get; }

    public abstract void Unpack(string input, string output);

    public virtual void Pack(string input, string output)
    {
        throw new NotImplementedException();
    }

    public static bool IsSaneCount(int count)
    {
        return count > 0 && count < 0x10000;
    }

    public override string ToString()
    {
        return $"{Name} - {Description}";
    }
}
