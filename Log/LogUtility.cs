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

using Log.Properties;
using System;
using System.IO;
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
            File.AppendAllText("log.txt", "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]" + log);
        }
        private static void WriteAndSaveLog(string log, int logLevel = 1)
        {
            LogLevel level = (LogLevel)logLevel;
            if (level == LogLevel.Debug && Settings.Default.chkbxDebug || level == LogLevel.Info || level == LogLevel.Error)
            {
                LogWindow.Instance.log_txtLog.AppendText(log + Environment.NewLine);
            }
            if (Settings.Default.chkbxSave)
            {
                SaveLog("[" + level.ToString() + "]" + log + Environment.NewLine);
            }
        }
        public static void NewInstance()
        {
            File.AppendAllText("log.txt", Environment.NewLine + "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "][" + Resources.logNewInstance + "]" + Environment.NewLine);
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
                OnErrorOccur(Resources.logErrorOccur);
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
        public static void InfoRevoke(string log)
        {
            WriteAndSaveLog(log, 1);
            OnProcess(log);
        }
        public static void DebugRevoke(string log)
        {
            WriteAndSaveLog(log, 2);
            OnProcess(log);
        }
        public static void ErrorInvalidArchive()
        {
            throw new Exception(Resources.logErrorNotValidArc);
        }
        public static void ErrorNeedAnotherFile(string ext1, string ext2)
        {
            ext1 = ext1?.ToUpper().Replace(".", string.Empty) ?? Resources.logNoExtension;
            ext2 = ext2?.ToUpper().Replace(".", string.Empty) ?? Resources.logNoExtension;
            throw new Exception(string.Format(Resources.logErrorSameNameNotFound, ext1, ext2));
        }
        public static void ErrorNeedOriginalFile(string ext)
        {
            ext = ext?.ToUpper().Replace(".", string.Empty) ?? Resources.logNoExtension;
            throw new Exception(string.Format(Resources.logErrorOriginalFileNotFound, ext));
        }

        public static void InitPack(string inputFolderPath, string outputFilePath)
        {
            Debug(Resources.logInputFolder + "\t" + inputFolderPath);
            Debug(Resources.logOutputFile + "\t" + outputFilePath);
            Info(Resources.logPacking);
            LogWindow.Instance.bar.Value = 0;
        }
        public static void FinishPack()
        {
            LogWindow.Instance.bar.Value = LogWindow.Instance.bar.Maximum;
            InfoRevoke(Resources.logPackFinished);
        }
        public static void InitUnpack(string inputFilePath, string outputFolderPath)
        {
            Debug(Resources.logInputFile + "\t" + inputFilePath);
            Debug(Resources.logOutputFolder + "\t" + outputFolderPath);
            Info(Resources.logUnpacking);
            LogWindow.Instance.bar.Value = 0;
        }
        public static void FinishUnpack()
        {
            LogWindow.Instance.bar.Value = LogWindow.Instance.bar.Maximum;
            InfoRevoke(Resources.logUnpackFinished);
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
            Debug(string.Format(Resources.logFileCountInside, max));
        }
        public static void InitBar(uint max)
        {
            LogWindow.Instance.bar.Maximum = (int)max;
            Debug(string.Format(Resources.logFileCountInside, max));
        }
        public static void UpdateBar()
        {
            LogWindow.Instance.bar.PerformStep();
        }

        public static void ShowCheckingUpdate()
        {
            InfoRevoke(Resources.logUpdating);
        }
        public static async void ShowCheckSuccess(bool existNewer)
        {
            if (existNewer)
            {
                Info(Resources.logHasUpdate);
                await OnShowAndDisappear(Resources.logHasUpdate);
            }
            else
            {
                Info(Resources.logNoUpdate);
                await OnShowAndDisappear(Resources.logNoUpdate);
            }
        }
        public static void ShowCheckError()
        {
            Error(Resources.logUpdateError, false);
        }

        public static void ShowVersion(string extension, int version)
        {
            Info(string.Format(Resources.logValidArchiveDetected, extension, version));
        }

        public static void ShowVersion(string extension, string version)
        {
            Info(string.Format(Resources.logValidArchiveDetected, extension, version));
        }
    }
}
