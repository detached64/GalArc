using GalArc.Logs;
using GalArc.Strings;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GalArc.Extensions.GARbroDB
{
    public class Deserializer
    {
        private static string LoadedContent { get; set; } = null;

        public static void LoadScheme()
        {
            if (GARbroDBConfig.IsEnabled)
            {
                if (!File.Exists(GARbroDBConfig.Path))
                {
                    return;
                }
                LoadedContent = File.ReadAllText(GARbroDBConfig.Path);
            }
        }

        /// <summary>
        /// Deserialize the scheme from the loaded content to Dictionary&lt;gameName, Scheme&gt;.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="jsonEngineName"></param>
        /// <param name="jsonNodeName"></param>
        /// <returns></returns>
        public static Dictionary<string, Scheme> Deserialize(Type type, string jsonEngineName, string jsonNodeName)
        {
            if (!ExtensionsConfig.IsEnabled || !GARbroDBConfig.IsEnabled)
            {
                return null;
            }
            var result = new Dictionary<string, Scheme>();

            if (string.IsNullOrEmpty(LoadedContent))
            {
                return null;
            }
            try
            {
                JObject jsonObject = JObject.Parse(LoadedContent);
                var selectedToken = jsonObject.SelectToken($"['SchemeMap']['{jsonEngineName}']['{jsonNodeName}']");

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
        /// Get the GARbroDB information.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetJsonInfo(string path)
        {
            if (!File.Exists(path))
            {
                return SchemeInfos.InfoFileNotFound;
            }
            try
            {
                StringBuilder result = new StringBuilder();

                LoadedContent = File.ReadAllText(path);
                JObject jsonObject = JObject.Parse(LoadedContent);

                // version
                int version = (int)jsonObject["Version"];
                result.AppendLine(string.Format(SchemeInfos.InfoVersion, version));

                // contents
                result.AppendLine(SchemeInfos.InfoContents);

                // scheme count
                JObject schemeMap = (JObject)jsonObject["SchemeMap"];
                int SchemeMapCount = schemeMap.Count;
                result.AppendLine(string.Format(SchemeInfos.InfoItems, "SchemeMap", SchemeMapCount));
                JObject gameMap = (JObject)jsonObject["GameMap"];
                int GameMapCount = gameMap.Count;
                result.AppendLine(string.Format(SchemeInfos.InfoItems, "GameMap", GameMapCount));

                // file size
                FileInfo fileInfo = new FileInfo(path);
                result.AppendLine(string.Format(SchemeInfos.InfoSize, fileInfo.Length));

                // last modified time
                DateTime lastModified = File.GetLastWriteTime(path);
                result.AppendLine(string.Format(SchemeInfos.InfoLastModified, lastModified));

                // hash
                result.AppendLine(string.Format(SchemeInfos.InfoHash, LoadedContent.GetHashCode()));

                jsonObject = null;
                return result.ToString();
            }
            catch
            {
                return SchemeInfos.InfoFileNotFound;
            }
        }
    }
}
