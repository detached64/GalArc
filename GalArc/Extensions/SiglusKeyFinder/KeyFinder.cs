using GalArc.Properties;
using System;
using System.Diagnostics;
using System.IO;

namespace GalArc.Extensions.SiglusKeyFinder
{
    public static class KeyFinder
    {
        public static bool IsValidExe()
        {
            return File.Exists(SiglusKeyFinderConfig.Path);
        }

        public static string FindKey(string siglusEnginePath)
        {
            if (!BaseSettings.Default.IsSiglusKeyFinderEnabled || !IsValidExe())
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
