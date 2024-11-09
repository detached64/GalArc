using GalArc.Logs;
using GalArc.Strings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;

namespace GalArc.Extensions.GARbroDB
{
    public class Deserializer
    {
        private static string LoadedContent { get; set; } = null;

        public static void LoadScheme()
        {
            if (!File.Exists(GARbroDBConfig.Path))
            {
                return;
            }
            LoadedContent = File.ReadAllText(GARbroDBConfig.Path);
        }

        /// <summary>
        /// Deserialize the specified scheme.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="jsonEngineName"></param>
        /// <param name="jsonNodeName"></param>
        /// <returns></returns>
        public static Scheme Deserialize(Type type, string jsonEngineName)
        {
            if (!ExtensionsConfig.IsEnabled || !GARbroDBConfig.IsEnabled)
            {
                return null;
            }
            if (string.IsNullOrEmpty(LoadedContent))
            {
                LoadScheme();
                if (string.IsNullOrEmpty(LoadedContent))
                {
                    return null;
                }
            }

            var result = new Scheme();

            try
            {
                JObject jsonObject = JObject.Parse(LoadedContent);
                JToken selectedToken = jsonObject.SelectToken($"['SchemeMap']['{jsonEngineName}']");
                string selectedJson = selectedToken.ToString();
                result = JsonConvert.DeserializeObject(selectedJson, type) as Scheme;
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
