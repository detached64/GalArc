using GalArc.Logs;
using GalArc.Settings;
using GalArc.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ArcFormats
{
    public abstract class ArcFormat
    {
        public virtual WidgetTemplate UnpackWidget => Empty.Instance;

        public virtual WidgetTemplate PackWidget => Empty.Instance;

        public virtual IEnumerable<ArcSetting> Settings { get; protected set; }

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

    public static class ArcResources
    {
        public static List<ArcFormat> Formats { get; }

        static ArcResources()
        {
            Formats = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t =>
                    t.IsSubclassOf(typeof(ArcFormat)) &&
                    !t.IsAbstract &&
                    t.GetConstructor(Type.EmptyTypes) != null)
                .Reverse()
                .Select(t => Activator.CreateInstance(t) as ArcFormat)
                .ToList();
        }
    }

    public class ArcSetting
    {
        public ArcSetting(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public virtual object Value
        {
            get => ResSettings.Default[Name];
            set
            {
                ResSettings.Default[Name] = value;
                ResSettings.Default.Save();
            }
        }

        public T Get<T>()
        {
            var value = Value;
            if (value == null || !(value is T))
            {
                return default;
            }
            return (T)value;
        }
    }

    public class EncodingSetting : ArcSetting
    {
        private readonly Encoding DefaultEncoding = Encoding.GetEncoding(932);

        public EncodingSetting(string name) : base(name) { }

        public override object Value
        {
            get
            {
                try
                {
                    return Encoding.GetEncoding((int)base.Value);
                }
                catch
                {
                    Logger.Info($"Invalid encoding value: {base.Value}");
                    return DefaultEncoding;
                }
            }
            set => base.Value = ((Encoding)value).CodePage;
        }
    }
}
