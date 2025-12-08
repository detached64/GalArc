using System;

namespace GalArc.Models.Formats.Commons;

internal abstract class ArcFormat
{
    public abstract string Name { get; }
    public abstract string Description { get; }
    public virtual bool CanWrite { get; }
    public virtual bool CanFind { get; } = true;

    public abstract void Unpack(string filePath, string folderPath);

    public virtual void Pack(string folderPath, string filePath)
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
