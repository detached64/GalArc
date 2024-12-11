using System;
using System.Collections.Generic;
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
        public string Path { get; set; }
        /// <summary>
        /// File offset
        /// </summary>
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

        public static object[] Instances
        {
            get
            {
                if (instances == null)
                {
                    GetTypes();
                }
                return instances.ToArray();
            }
        }

        private static List<object> instances;

        public static void GetTypes()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type[] types = assembly.GetTypes();
            instances = new List<object>();
            foreach (Type type in types)
            {
                if (type.IsSubclassOf(typeof(ArchiveFormat)))
                {
                    instances.Add(Activator.CreateInstance(type));
                }
            }
        }
    }
}
