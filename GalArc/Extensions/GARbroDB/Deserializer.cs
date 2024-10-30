using GalArc.Logs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GalArc.Extensions.GARbroDB
{
    public class Deserializer
    {

        public static string GARbroDBContent { get; internal set; } = null;

        public static void LoadScheme()
        {
            if (GARbroDBConfig.IsGARbroDBEnabled)
            {
                if (!File.Exists(GARbroDBConfig.GARbroDBPath))
                {
                    return;
                }
                GARbroDBContent = File.ReadAllText(GARbroDBConfig.GARbroDBPath);
            }
        }

        public static Dictionary<string, Dictionary<string, Scheme>> Deserialize(object instance)
        {
            if (!ExtensionsConfig.IsEnabled || !GARbroDBConfig.IsGARbroDBEnabled)
            {
                return null;
            }
            var result = new Dictionary<string, Dictionary<string, Scheme>>();

            Type scheme = instance.GetType();
            string jsonEngineName = scheme.GetField("JsonEngineName").GetValue(null).ToString();
            string jsonNodeName = scheme.GetField("JsonNodeName").GetValue(null).ToString();
            result.Add(jsonNodeName, new Dictionary<string, Scheme>());

            if (string.IsNullOrEmpty(GARbroDBContent))
            {
                return null;
            }
            try
            {
                JObject jsonObject = JObject.Parse(GARbroDBContent);
                var selectedToken = jsonObject.SelectToken($"['SchemeMap']['{jsonEngineName}']['{jsonNodeName}']");

                foreach (var token in selectedToken.Children<JProperty>())
                {
                    string schemeName = token.Name;
                    var schemeData = token.Value.ToObject(scheme);
                    result[jsonNodeName][schemeName] = (Scheme)schemeData;
                }
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to deserialize {jsonEngineName} scheme: {ex.Message}", false);
                return null;
            }
        }

        public static string TryReadJson(string path)
        {
            StringBuilder result = new StringBuilder();
            if (!File.Exists(path))
            {
                return GARbroDB.DBFileNotFound;
            }
            try
            {
                GARbroDBContent = File.ReadAllText(path);
                JObject jsonObject = JObject.Parse(GARbroDBContent);

                // version
                int version = (int)jsonObject["Version"];
                result.AppendLine(string.Format(GARbroDB.DBVersion, version));

                // contents
                result.AppendLine(GARbroDB.DBContents);

                // scheme count
                JObject schemeMap = (JObject)jsonObject["SchemeMap"];
                int SchemeMapCount = schemeMap.Count;
                result.AppendLine(string.Format(GARbroDB.DBItems, "SchemeMap", SchemeMapCount));
                JObject gameMap = (JObject)jsonObject["GameMap"];
                int GameMapCount = gameMap.Count;
                result.AppendLine(string.Format(GARbroDB.DBItems, "GameMap", GameMapCount));

                // file size
                FileInfo fileInfo = new FileInfo(path);
                result.AppendLine(string.Format(GARbroDB.DBSize, fileInfo.Length));

                // last modified time
                DateTime lastModified = File.GetLastWriteTime(path);
                result.AppendLine(string.Format(GARbroDB.DBLastModified, lastModified));

                // hash
                result.AppendLine(string.Format(GARbroDB.DBHash, GARbroDBContent.GetHashCode()));
                return result.ToString();
            }
            catch
            {
                return GARbroDB.DBFailedToReadInfos;
            }
        }
    }
}
