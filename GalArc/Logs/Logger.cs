// File: Logs/Logger.cs
// Date: 2024/08/28
// Description: Logger class for logging and showing logs.
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

using GalArc.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GalArc.Logs
{
    public enum LogLevel
    {
        Error,
        Info,
        Debug
    }

    public static class Logger
    {
        public static event EventHandler<string> Process;
        public static event EventHandler<string> ErrorOccured;

        private const int CacheSize = 12;
        private const string LogPath = "log.txt";

        private static readonly List<string> _logCache = new List<string>();
        private static readonly object _lock = new object();
        private static readonly IProgress<int> progress = new Progress<int>(value => LogWindow.Instance.bar.Value = value);
        private static CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        private static int _barValue;
        private static int barValue
        {
            get => _barValue;
            set
            {
                _barValue = value;
                if (_barValue <= LogWindow.Instance.bar.Maximum)
                {
                    progress.Report(_barValue);
                }
            }
        }

        private static void Append(string msg)
        {
            lock (_lock)
            {
                _logCache.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]{msg}");
            }

            if (_logCache.Count > CacheSize)
            {
                Flush();
            }
        }

        private static void Append(string msg, int level)
        {
            lock (_lock)
            {
                _logCache.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}][{(LogLevel)level}]{msg}");
            }

            if (_logCache.Count > CacheSize)
            {
                Flush();
            }
        }

        public static void Flush(bool isClosing = false)
        {
            lock (_lock)
            {
                if (_logCache.Count == 0)
                {
                    return;
                }
                if (!BaseSettings.Default.ToSaveLog)
                {
                    _logCache.Clear();
                    return;
                }

                File.AppendAllLines(LogPath, _logCache, Encoding.UTF8);
                _logCache.Clear();
                if (isClosing)
                {
                    using (StreamWriter sw = File.AppendText(LogPath))
                    {
                        sw.WriteLine();
                    }
                }
            }
        }

        private static void WriteAndSaveLog(string msg, int logLevel = 1)
        {
            LogLevel level = (LogLevel)logLevel;
            if ((level == LogLevel.Debug && BaseSettings.Default.IsDebugMode) || level == LogLevel.Info || level == LogLevel.Error)
            {
                if (LogWindow.Instance.log_txtLog.InvokeRequired)
                {
                    LogWindow.Instance.log_txtLog.Invoke(new Action(() => LogWindow.Instance.log_txtLog.AppendText(msg + Environment.NewLine)));
                }
                else
                {
                    LogWindow.Instance.log_txtLog.AppendText(msg + Environment.NewLine);
                }
            }
            if (BaseSettings.Default.ToSaveLog)
            {
                Append(msg, logLevel);
            }
        }

        public static void NewInstance()
        {
            Append($"[{Resources.logNewInstance}]");
        }

        public static void Error(string log, bool toThrow = true)
        {
            if (!toThrow)
            {
                WriteAndSaveLog(log, 0);
                OnErrorOccur(string.Format(Resources.logErrorOccur, log));
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

        public static void ErrorNeedAnotherFile(string file)
        {
            throw new Exception(string.Format(Resources.logErrorSpecifiedFileNotFound, file));
        }

        public static void ErrorNeedOriginalFile(string file)
        {
            throw new Exception(string.Format(Resources.logErrorOriginalFileNotFound, file));
        }

        public static void InitUnpack(string input, string output)
        {
            barValue = 0;
            Debug(Resources.logInputFile + "\t" + input);
            Debug(Resources.logOutputFolder + "\t" + output);
            Info(Resources.logUnpacking);
        }

        public static void FinishUnpack()
        {
            barValue = LogWindow.Instance.bar.Maximum;
            InfoRevoke(Resources.logUnpackFinished);
        }

        public static void InitPack(string input, string output)
        {
            barValue = 0;
            Debug(Resources.logInputFolder + "\t" + input);
            Debug(Resources.logOutputFile + "\t" + output);
            Info(Resources.logPacking);
        }

        public static void FinishPack()
        {
            barValue = LogWindow.Instance.bar.Maximum;
            InfoRevoke(Resources.logPackFinished);
        }

        private static void OnProcess(string message)
        {
            Process?.Invoke(null, message);
        }

        private static void OnErrorOccur(string message)
        {
            ErrorOccured?.Invoke(null, message);
        }

        private static async Task OnShowAndDisappear(string message, int second = 5)
        {
            _cancellationToken.Cancel();
            _cancellationToken.Dispose();
            _cancellationToken = new CancellationTokenSource();
            var token = _cancellationToken.Token;

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
            InitBar(max, 1);
        }

        public static void InitBar(uint max)
        {
            InitBar(max, 1);
        }

        public static void InitBar(int max, int m)
        {
            barValue = 0;
            SetBarMax(max * m);
            Debug(string.Format(Resources.logFileCountInside, max));
        }

        public static void InitBar(uint max, int m)
        {
            InitBar((int)max, m);
        }

        public static void UpdateBar()
        {
            barValue++;
        }

        public static void SetBarMax(int max)
        {
            if (LogWindow.Instance.bar.IsHandleCreated)
            {
                LogWindow.Instance.bar.Invoke(new Action(() => LogWindow.Instance.bar.Maximum = max));
            }
        }

        public static void SetBarValue(int value)
        {
            barValue = value;
        }

        public static void ResetBar()
        {
            barValue = 0;
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

        public static void ShowProgramVersion(string cv, string lv)
        {
            Debug(string.Format(Resources.logVersions, cv, lv));
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
