using GalArc.GUI.Properties;
using GalArc.Logs;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GalArc.GUI
{
    internal class Updater
    {
        private static readonly HttpClient httpClient = new HttpClient();

        private static readonly string LatestVersionURL = "https://pastebin.com/raw/4pvccgbk";

        internal static readonly string currentVersion = Common.Version.CurrentVer;

        internal static string latestVersion;

        internal static bool isNewVerExist = false;

        public async Task DownloadFileAsync()
        {
            Logger.ShowCheckingUpdate();
            using (var cts = new CancellationTokenSource())
            {
                cts.CancelAfter(10000);     // 10s
                try
                {
                    latestVersion = await DownloadContentAsync(cts.Token);
                    CompareVersion(latestVersion);
                }
                catch (OperationCanceledException)
                {
                    Logger.Error(Resources.logTimedOut, false);
                    return;
                }
                catch (Exception)
                {
                    Logger.ShowCheckError();
                    return;
                }
            }
            Logger.ShowCheckSuccess(isNewVerExist);
            Logger.ShowProgramVersion(currentVersion, latestVersion);
            if (isNewVerExist)
            {
                UpdateBox box = new UpdateBox();
                box.ShowDialog();
            }
        }

        private async Task<string> DownloadContentAsync(CancellationToken cancellationToken)
        {
            var response = await httpClient.GetAsync(LatestVersionURL, cancellationToken);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        private void CompareVersion(string latestVersion)
        {
            isNewVerExist = false;

            string[] parts1 = currentVersion.Split('.');
            string[] parts2 = latestVersion.Split('.');

            for (int i = 0; i < Math.Max(parts1.Length, parts2.Length); i++)
            {
                int num1 = (i < parts1.Length) ? int.Parse(parts1[i]) : 0;
                int num2 = (i < parts2.Length) ? int.Parse(parts2[i]) : 0;
                if (num1 > num2)
                {
                    break;
                }
                if (num1 < num2)
                {
                    isNewVerExist = true;
                    break;
                }
            }
        }
    }
}
