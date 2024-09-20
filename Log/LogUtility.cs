// File: Log/LogUtility.cs
// Date: 2024/08/28
// Description: 显示日志的工具类，提供一系列显示日志的静态方法
//
// Copyright (C) 2024 detached64
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Log
{
    public enum LogLevel
    {
        Error,
        Info,
        Debug
    }

    public class LogUtility
    {
        public static event EventHandler<string> Process;
        public static event EventHandler<string> ErrorOccured;
        private static CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private static void SaveLog(string log)
        {
            System.IO.File.AppendAllText("log.txt", "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]" + log);
        }
        private static void WriteAndSaveLog(string log, int logLevel = 1)
        {
            LogLevel level = (LogLevel)logLevel;
            if (level == LogLevel.Debug && LogWindow.Instance.log_chkbxDebug.Checked || level == LogLevel.Info || level == LogLevel.Error)
            {
                LogWindow.Instance.log_txtLog.AppendText(log + Environment.NewLine);
            }
            if (LogWindow.Instance.log_chkbxSave.Checked)
            {
                SaveLog("[" + level.ToString() + "]" + log + Environment.NewLine);
            }
        }
        public static void NewInstance()
        {
            System.IO.File.AppendAllText("log.txt", Environment.NewLine + "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "][New Instance]" + Environment.NewLine);
        }
        /// <summary>
        /// Show error log and throw exception if <paramref name="toThrow"/> is true.
        /// </summary>
        /// <param name="log"></param>
        /// <param name="toThrow"></param>
        /// <exception cref="Exception"></exception>
        public static void Error(string log, bool toThrow = true)
        {
            if (!toThrow)
            {
                WriteAndSaveLog(log, 0);
                OnErrorOccur("Error occurs.");
            }
            else
            {
                throw new Exception(log);
            }
        }
        public static void Info(string log)
        {
            WriteAndSaveLog(log, 1);
        }
        public static void Debug(string log)
        {
            WriteAndSaveLog(log, 2);
        }
        public static void Error_NotValidArchive()
        {
            throw new Exception("Error:Not a valid archive.");
        }
        public static void Error_NeedAnotherFile(string ext1, string ext2)
        {
            ext1 = ext1?.ToUpper().Replace(".", string.Empty) ?? "(no extension)";
            ext2 = ext2?.ToUpper().Replace(".", string.Empty) ?? "(no extension)";
            throw new Exception("Error:" + ext1 + " and " + ext2 + " file with the same name should be in the same directory.");
        }
        public static void Error_NeedOriginalFile(string ext)
        {
            ext = ext?.ToUpper().Replace(".", string.Empty) ?? "(no extension)";
            throw new Exception("Error:Original " + ext + " file not found.");
        }

        public static void InitPack(string inputFolderPath, string outputFilePath)
        {
            Debug("Input folder path:\t" + inputFolderPath);
            Debug("Output file path:\t" + outputFilePath);
            Info("Packing……");
            LogWindow.Instance.bar.Value = 0;
        }
        public static void FinishPack()
        {
            Info("Pack finished.");
            LogWindow.Instance.bar.Value = LogWindow.Instance.bar.Maximum;
            OnProcess("Pack finished.");
        }
        public static void InitUnpack(string inputFilePath, string outputFolderPath)
        {
            Debug("Input file path:\t" + inputFilePath);
            Debug("Output folder path:\t" + outputFolderPath);
            Info("Unpacking……");
            LogWindow.Instance.bar.Value = 0;
        }
        public static void FinishUnpack()
        {
            Info("Unpack finished.");
            LogWindow.Instance.bar.Value = LogWindow.Instance.bar.Maximum;
            OnProcess("Unpack finished.");
        }

        internal static void OnProcess(string message)
        {
            Process?.Invoke(null, message);
        }
        internal static void OnErrorOccur(string message)
        {
            ErrorOccured?.Invoke(null, message);
        }
        internal static async Task OnShowAndDisappear(string message, int second = 5)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            try
            {
                OnProcess(message);
                await Task.Delay(second * 1000, token);
                if (!token.IsCancellationRequested)
                {
                    OnProcess(string.Empty);
                }
            }
            catch (TaskCanceledException)
            {
                //ignore
            }
        }

        public static void InitBar(int max)
        {
            LogWindow.Instance.bar.Maximum = max;
            Debug(max.ToString() + " files inside.");
        }
        public static void InitBar(uint max)
        {
            LogWindow.Instance.bar.Maximum = (int)max;
            Debug(max.ToString() + " files inside.");
        }
        public static void UpdateBar()
        {
            LogWindow.Instance.bar.PerformStep();
        }

        public static void ShowCheckingUpdate()
        {
            Info("Checking for update……");
            OnProcess("Checking for update……");
        }
        public static async void ShowCheckSuccess(bool existNewer)
        {
            if (existNewer)
            {
                Info("New update available!");
                await OnShowAndDisappear("New update available!");
            }
            else
            {
                Info("You are using the latest version of GalArc.");
                await OnShowAndDisappear("You are using the latest version of GalArc.");
            }
        }
        public static void ShowCheckError()
        {
            Error("Error occurs while checking for update.",false);
        }

        public static void ShowVersion(string extension, int version)
        {
            Info($"Valid {extension} v{version} archive detected.");
        }
    }
}
