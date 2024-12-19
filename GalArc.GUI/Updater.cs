using GalArc.GUI.Properties;
using GalArc.Logs;
using System;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace GalArc.GUI
{
    internal class Updater
    {
        private static readonly HttpClient httpClient = new HttpClient();

        private const string LatestVersionURL = "https://pastebin.com/raw/4pvccgbk";

        internal static readonly Version CurrentVersion = Assembly.GetExecutingAssembly().GetName().Version;

        internal static string LatestVersion;

        private bool isNewVerExist;

        public async Task DownloadFileAsync()
        {
            Logger.ShowCheckingUpdate();
            using (var cts = new CancellationTokenSource())
            {
                cts.CancelAfter(10000);
                try
                {
                    LatestVersion = await DownloadContentAsync(cts.Token);
                    CompareVersion(LatestVersion);
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
            Logger.ShowProgramVersion(CurrentVersion.ToString(), LatestVersion);
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

            string[] parts1 = CurrentVersion.ToString().Split('.');
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
