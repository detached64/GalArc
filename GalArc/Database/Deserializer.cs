using GalArc.Settings;
using GalArc.Strings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GalArc.Database
{
    public static class Deserializer
    {
        private static List<Type> schemes;

        private static List<Type> Schemes => schemes ?? (schemes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(ArcScheme).IsAssignableFrom(t))
            .OrderBy(t => t.Name)
            .ToList());

        public static int SchemeCount => Schemes.Count;

        public static int LoadedSchemeCount { get; set; }

        private static string ReadScheme(string name)
        {
            string path = Path.Combine(DatabaseConfig.Path, name + ".json");
            if (!File.Exists(path))
            {
                return null;
            }
            return File.ReadAllText(path, Encoding.UTF8);
        }

        private static string ReadScheme<T>() where T : ArcScheme
        {
            return ReadScheme(typeof(T).Name.Remove(typeof(T).Name.Length - 6));
        }

        public static T LoadScheme<T>() where T : ArcScheme
        {
            if (!BaseSettings.Default.IsDatabaseEnabled)
            {
                return default;
            }
            string json = ReadScheme<T>();
            if (string.IsNullOrEmpty(json))
            {
                throw new InvalidDataException($"Failed to read {typeof(T).Name} scheme.");
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to deserialize scheme: {ex.Message}");
            }
        }

        private static string GetInfo(Type type, out bool isValid)
        {
            string name = type.Name.Remove(type.Name.Length - 6);    // remove "Scheme"
            string path = Path.Combine(DatabaseConfig.Path, name + ".json");

            StringBuilder result = new StringBuilder();
            // engine name
            result.AppendFormat(GUIStrings.InfoEngineName, name).AppendLine();

            try
            {
                string json = ReadScheme(name);
                JObject jsonObject = JObject.Parse(json);

                // contents
                result.AppendLine(GUIStrings.InfoContents);
                foreach (var property in jsonObject.Properties())
                {
                    int count = 0;
                    switch (property.Value.Type)
                    {
                        case JTokenType.Array:
                            count = property.Value.Count();
                            break;
                        case JTokenType.Object:
                            count = property.Value.Children().Count();
                            break;
                    }
                    result.Append("  ").AppendFormat(GUIStrings.InfoItems, property.Name, count).AppendLine();
                }

                // file size
                FileInfo fileInfo = new FileInfo(path);
                result.AppendFormat(GUIStrings.InfoSize, fileInfo.Length).AppendLine();

                // last modified time
                DateTime lastModified = File.GetLastWriteTime(path);
                result.AppendFormat(GUIStrings.InfoLastModified, lastModified).AppendLine();

                jsonObject = null;
                isValid = true;
                return result.ToString();
            }
            catch
            {
                StringBuilder error = new StringBuilder();
                error.AppendFormat(GUIStrings.InfoEngineName, name).AppendLine();
                error.AppendLine(GUIStrings.InfoFailedToReadInfos);
                isValid = false;
                return error.ToString();
            }
        }

        public static string GetInfos()
        {
            if (!BaseSettings.Default.IsDatabaseEnabled)
            {
                return GUIStrings.InfoDisabled;
            }
            StringBuilder result = new StringBuilder();
            int count = 0;
            StringBuilder info = new StringBuilder();
            foreach (Type type in Schemes)
            {
                info.AppendLine(GetInfo(type, out bool flag));
                if (flag)
                {
                    count++;
                }
            }
            result.AppendFormat(LogStrings.SchemeCount, SchemeCount).AppendLine();
            result.AppendFormat(LogStrings.SchemeLoadedCount, LoadedSchemeCount).AppendLine();
            result.AppendFormat(LogStrings.ValidDatabaseCount, count).AppendLine().AppendLine();
            result.AppendLine(info.ToString());
            return result.ToString();
        }
    }
}
