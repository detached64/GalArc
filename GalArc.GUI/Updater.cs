using GalArc.Logs;
using GalArc.Strings;
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

        internal static Version CurrentVersion => Assembly.GetExecutingAssembly().GetName().Version;

        internal static string LatestVersion { get; private set; }

        public async Task DownloadVersionAsync()
        {
            Logger.ShowCheckingUpdate();
            int result = 0;
            using (var cts = new CancellationTokenSource())
            {
                cts.CancelAfter(10000);
                try
                {
                    LatestVersion = await DownloadContentAsync(cts.Token);
                    result = CompareVersion(LatestVersion, CurrentVersion.ToString());
                }
                catch (OperationCanceledException)
                {
                    Logger.Error(LogStrings.TimedOut, false);
                    return;
                }
                catch (Exception)
                {
                    Logger.ShowCheckError();
                    return;
                }
            }
            Logger.ShowCheckSuccess(result);
            Logger.ShowProgramVersion(CurrentVersion.ToString(), LatestVersion);
            if (result > 0)
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

        private int CompareVersion(string v1, string v2)
        {
            string[] parts1 = v1.Split('.');
            string[] parts2 = v2.Split('.');

            for (int i = 0; i < Math.Max(parts1.Length, parts2.Length); i++)
            {
                int num1 = int.Parse(parts1[i]);
                int num2 = int.Parse(parts2[i]);
                if (num1 > num2)
                {
                    return 1;
                }
                if (num1 < num2)
                {
                    return -1;
                }
            }
            return 0;
        }
    }
}
