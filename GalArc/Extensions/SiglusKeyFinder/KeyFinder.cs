using System;
using System.Diagnostics;
using System.IO;

namespace GalArc.Extensions.SiglusKeyFinder
{
    public static class KeyFinder
    {
        public static bool IsValidExe()
        {
            if (string.IsNullOrEmpty(SiglusKeyFinderConfig.Path))
            {
                return false;
            }
            if (!File.Exists(SiglusKeyFinderConfig.Path))
            {
                return false;
            }
            return true;
        }

        public static string FindKey(string siglusEnginePath)
        {
            if (!SiglusKeyFinderConfig.IsEnabled)
            {
                return null;
            }
            if (!IsValidExe())
            {
                return null;
            }

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = SiglusKeyFinderConfig.Path,
                Arguments = siglusEnginePath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new Process())
            {
                process.StartInfo = startInfo;

                try
                {
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd().Trim();
                    return output;
                }
                catch
                {
                    return null;
                }
                finally
                {
                    if (!process.HasExited)
                    {
                        process.Kill();
                    }
                }
            }
        }
    }
}
