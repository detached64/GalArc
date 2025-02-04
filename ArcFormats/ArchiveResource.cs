using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Utility;

namespace ArcFormats
{
    public abstract class ArchiveFormat
    {
        public abstract void Unpack(string filePath, string folderPath);

        public virtual void Pack(string folderPath, string filePath)
        {
            throw new NotImplementedException();
        }

        public virtual void DeserializeScheme(out string name, out int count)
        {
            name = null;
            count = -1;
        }

        public static bool IsSaneCount(int count)
        {
            return count > 0 && count < 0x10000;
        }
    }

    public class Entry
    {
        /// <summary>
        /// File name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// File path
        /// </summary>
        /// Absolute path
        public string Path { get; set; }
        /// <summary>
        /// File offset
        /// </summary>
        /// Offset from the beginning of the file
        public uint Offset { get; set; }
        /// <summary>
        /// File size
        /// </summary>
        /// Also used for packed size
        public uint Size { get; set; }
    }

    public class PackedEntry : Entry
    {
        public bool IsPacked { get; set; }
        public uint UnpackedSize { get; set; }
    }

    public static class ArcSettings
    {
        public static Encoding Encoding = ArcEncoding.Shift_JIS;

        public static string Version = null;

        public static List<ArchiveFormat> Formats =>
            Assembly.GetExecutingAssembly().GetTypes()
            .Where(t =>
                t.IsSubclassOf(typeof(ArchiveFormat)) &&
                !t.IsAbstract &&
                t.GetConstructor(Type.EmptyTypes) != null)
            .Select(t => Activator.CreateInstance(t) as ArchiveFormat)
            .OfType<ArchiveFormat>()
            .Reverse()
            .ToList();
    }
}
