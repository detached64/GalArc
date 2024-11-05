using GalArc.Logs;
using GalArc.Strings;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GalArc.DataBase
{
    public class Deserializer
    {
        /// <summary>
        /// Loaded json strings. Dictionary&lt;engineName, jsonString&gt;.
        /// </summary>
        public static Dictionary<string, string> LoadedJsons = new Dictionary<string, string>();

        private static List<Type> _Schemes;

        /// <summary>
        /// Contains all schemes in the assembly.
        /// </summary>
        internal static List<Type> Schemes
        {
            get
            {
                if (_Schemes == null)
                {
                    _Schemes = new List<Type>();
                    foreach (Type type in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetCustomAttributes(typeof(DatabaseSchemeAttribute), false).Any()).ToList())
                    {
                        _Schemes.Add(type);
                    }
                }
                return _Schemes;
            }
        }

        /// <summary>
        /// Load string from a json file.
        /// </summary>
        /// <param name="type"></param>
        private static void LoadScheme(Type type)
        {
            if (!DataBaseConfig.IsEnabled)
            {
                return;
            }
            string name = type.Name.Remove(type.Name.Length - 6);    // remove "Scheme"
            string path = Path.Combine(DataBaseConfig.Path, name + ".json");
            if (!File.Exists(path))
            {
                return;
            }
            string json = File.ReadAllText(path, Encoding.UTF8);
            if (!string.IsNullOrEmpty(json))
            {
                LoadedJsons[name] = File.ReadAllText(path, Encoding.UTF8);
            }
        }

        /// <summary>
        /// Load all strings from json files.
        /// </summary>
        private static void LoadSchemes()
        {
            if (!DataBaseConfig.IsEnabled)
            {
                return;
            }
            foreach (Type type in Schemes)
            {
                LoadScheme(type);
            }
        }

        /// <summary>
        /// Deserialize json string to a dictionary.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="jsonEngineName"></param>
        /// <param name="jsonNodeName"></param>
        /// <returns></returns>
        public static Dictionary<string, Scheme> Deserialize(Type type, string jsonEngineName, string jsonNodeName)
        {
            if (!DataBaseConfig.IsEnabled)
            {
                return null;
            }
            if (!LoadedJsons.TryGetValue(jsonEngineName, out var jsonToDeserialize) || string.IsNullOrEmpty(jsonToDeserialize))
            {
                return null;
            }

            var result = new Dictionary<string, Scheme>();

            try
            {
                JObject jsonObject = JObject.Parse(jsonToDeserialize);
                var selectedToken = jsonObject.SelectToken($"['{jsonNodeName}']");

                foreach (var token in selectedToken.Children<JProperty>())
                {
                    string schemeName = token.Name;
                    var schemeData = token.Value.ToObject(type);
                    result[schemeName] = (Scheme)schemeData;
                }
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to deserialize {jsonEngineName} scheme: {ex.Message}", false);
                return null;
            }
        }

        /// <summary>
        /// Read json string and deserialize it to Dictionary&lt;gameName, Scheme&gt;.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        public static Dictionary<string, Scheme> ReadScheme(Type type, string nodeName)
        {
            if (!DataBaseConfig.IsEnabled)
            {
                return null;
            }
            LoadScheme(type);
            return Deserialize(type, type.Name.Remove(type.Name.Length - 6), nodeName);
        }

        /// <summary>
        /// Get specific json file information.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static string GetInfo(Type type)
        {
            string name = type.Name.Remove(type.Name.Length - 6);    // remove "Scheme"
            string[] jsonNodeNames = (string[])type.GetField("JsonNodeName").GetValue(null);
            string path = Path.Combine(DataBaseConfig.Path, name + ".json");

            try
            {
                StringBuilder result = new StringBuilder();

                var json = LoadedJsons[name];
                JObject jsonObject = JObject.Parse(json);
                // engine name
                result.AppendLine(string.Format(SchemeInfos.InfoEngineName, name));

                // version
                int version = (int)jsonObject["Version"];
                result.AppendLine(string.Format(SchemeInfos.InfoVersion, version));

                // contents
                result.AppendLine(SchemeInfos.InfoContents);

                // scheme count
                foreach (var jsonNodeName in jsonNodeNames)
                {
                    JObject thisNode = (JObject)jsonObject[$"{jsonNodeName}"];
                    int count = thisNode.Count;
                    result.AppendLine(string.Format(SchemeInfos.InfoItems, $"{jsonNodeName}", count));
                }

                // file size
                FileInfo fileInfo = new FileInfo(path);
                result.AppendLine(string.Format(SchemeInfos.InfoSize, fileInfo.Length));

                // last modified time
                DateTime lastModified = File.GetLastWriteTime(path);
                result.AppendLine(string.Format(SchemeInfos.InfoLastModified, lastModified));

                // hash
                result.AppendLine(string.Format(SchemeInfos.InfoHash, json.GetHashCode()));

                jsonObject = null;
                return result.ToString();
            }
            catch (Exception)
            {
                return SchemeInfos.InfoFileNotFound;
            }
        }

        /// <summary>
        /// Get all json files information.
        /// </summary>
        /// <returns></returns>
        public static string GetInfos()
        {
            if (!DataBaseConfig.IsEnabled)
            {
                return SchemeInfos.InfoDisabled;
            }
            LoadSchemes();
            StringBuilder result = new StringBuilder();
            foreach (Type type in Schemes)
            {
                result.AppendLine(GetInfo(type));
                result.AppendLine();
            }
            return result.ToString();
        }
    }
}
