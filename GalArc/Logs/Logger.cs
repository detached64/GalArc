// File: Logs/Logger.cs
// Date: 2024/08/28
// Description: Logger class for logging, change status and progress bar.
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

using GalArc.Settings;
using GalArc.Strings;
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

    public enum ProgressAction
    {
        Progress,   // Increment
        SetMax,     // Set maximum value
        SetVal,     // Set value
        Finish      // Finish
    }

    public class ProgressEventArgs : EventArgs
    {
        public int Value;
        public int Max;
        public ProgressAction Action;
    }

    public class Logger
    {
        public static Logger Instance { get; } = new Logger();

        private Logger() { }

        public event EventHandler<string> LogMessageEvent;
        public event EventHandler<string> StatusMessageEvent;
        public event EventHandler<ProgressEventArgs> ProgressEvent;

        #region Event invokers
        private void OnLogMessageEvent(string e) => LogMessageEvent?.Invoke(this, e);
        private void OnStatusMessageEvent(string e) => StatusMessageEvent?.Invoke(this, e);
        private void OnProgressEvent(ProgressEventArgs e) => ProgressEvent?.Invoke(this, e);
        #endregion

        private const int CacheSize = 25;
        private static string DefaultPath => System.IO.Path.Combine(Environment.CurrentDirectory, "log.txt");

        public static string Path
        {
            get
            {
                if (string.IsNullOrEmpty(BaseSettings.Default.LogPath))
                {
                    BaseSettings.Default.LogPath = DefaultPath;
                    BaseSettings.Default.Save();
                    return DefaultPath;
                }
                return BaseSettings.Default.LogPath;
            }
            set
            {
                BaseSettings.Default.LogPath = value;
                BaseSettings.Default.Save();
            }
        }

        private static readonly List<string> _logCache = new List<string>();
        private static readonly object _lock = new object();
        private static CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        #region Basic internal methods for logging and status
        private void Append(string msg)
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

        private void Append(string msg, int level)
        {
            lock (_lock)
            {
                _logCache.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}][{(LogLevel)level}]\t{msg}");
            }

            if (_logCache.Count > CacheSize)
            {
                Flush();
            }
        }

        private void Flush()
        {
            lock (_lock)
            {
                if (_logCache.Count == 0)
                {
                    return;
                }
                if (!BaseSettings.Default.SaveLog)
                {
                    _logCache.Clear();
                    return;
                }

                File.AppendAllLines(Path, _logCache, Encoding.UTF8);
                _logCache.Clear();
            }
        }

        private void AppendAndSaveLog(string msg, int logLevel = 1)
        {
            LogLevel level = (LogLevel)logLevel;
            if (!(level == LogLevel.Debug && !BaseSettings.Default.IsDebugMode))
            {
                OnLogMessageEvent(msg);
            }
            Append(msg, logLevel);
        }

        private void DebugInternal(string log) => AppendAndSaveLog(log, 2);

        private void DebugInvokeInternal(string log)
        {
            AppendAndSaveLog(log, 2);
            OnStatusMessageEvent(log);
        }

        private void InfoInternal(string log) => AppendAndSaveLog(log, 1);

        private void InfoInvokeInternal(string log)
        {
            AppendAndSaveLog(log, 1);
            OnStatusMessageEvent(log);
        }

        private void ErrorInternal(string log, bool toThrow = true)
        {
            if (!toThrow)
            {
                string msg = string.Format(LogStrings.ErrorOccur, log);
                AppendAndSaveLog(msg, 0);
                OnStatusMessageEvent(msg);
            }
            else
            {
                throw new Exception(log);
            }
        }

        private void StatusInvokeInternal(string log) => Instance.StatusMessageEvent?.Invoke(null, log);
        #endregion

        #region Basic internal methods for progress bar
        private void BarReset()
        {
            OnProgressEvent(new ProgressEventArgs
            {
                Value = 0,
                Action = ProgressAction.SetVal
            });
        }

        private void BarProgress()
        {
            OnProgressEvent(new ProgressEventArgs
            {
                Action = ProgressAction.Progress,
            });
        }

        private void BarFinish()
        {
            OnProgressEvent(new ProgressEventArgs
            {
                Action = ProgressAction.Finish
            });
        }

        private void BarSetVal(int v)
        {
            OnProgressEvent(new ProgressEventArgs
            {
                Value = v,
                Action = ProgressAction.SetVal
            });
        }

        private void BarSetMax(int m)
        {
            OnProgressEvent(new ProgressEventArgs
            {
                Max = m,
                Action = ProgressAction.SetMax
            });
        }
        #endregion

        #region Public wrapped methods
        public static void Debug(string log) => Instance.DebugInternal(log);
        public static void DebugInvoke(string log) => Instance.DebugInvokeInternal(log);
        public static void Info(string log) => Instance.InfoInternal(log);
        public static void InfoInvoke(string log) => Instance.InfoInvokeInternal(log);
        public static void Error(string log, bool toThrow = true) => Instance.ErrorInternal(log, toThrow);
        public static void Status(string log) => Instance.StatusInvokeInternal(log);
        public static void ErrorInvalidArchive() => throw new Exception(LogStrings.ErrorNotValidArc);
        public static void ErrorNeedAnotherFile(string file) => throw new Exception(string.Format(LogStrings.ErrorSpecifiedFileNotFound, file));
        public static void ErrorNeedOriginalFile(string file) => throw new Exception(string.Format(LogStrings.ErrorOriginalFileNotFound, file));

        public static void FlushLog()
        {
            Instance.Flush();
            using (StreamWriter sw = File.AppendText(Path))
            {
                sw.WriteLine();
            }
        }

        public static void InitUnpack(string input, string output)
        {
            Instance.BarReset();
            Instance.DebugInternal(LogStrings.InputFile + "\t" + input);
            Instance.DebugInternal(LogStrings.OutputFolder + "\t" + output);
            Instance.InfoInternal(LogStrings.Unpacking);
        }

        public static void FinishUnpack()
        {
            Instance.BarFinish();
            Instance.InfoInvokeInternal(LogStrings.UnpackFinished);
        }

        public static void InitPack(string input, string output)
        {
            Instance.BarReset();
            Instance.DebugInternal(LogStrings.InputFolder + "\t" + input);
            Instance.DebugInternal(LogStrings.OutputFile + "\t" + output);
            Instance.InfoInternal(LogStrings.Packing);
        }

        public static void FinishPack()
        {
            Instance.BarFinish();
            Instance.InfoInvokeInternal(LogStrings.PackFinished);
        }

        public static void NewInstance()
        {
            Instance.Append($"[{LogStrings.NewInstance}]");
        }

        private static async Task ShowAndDisappear(string message, int time)
        {
            _cancellationToken.Cancel();
            _cancellationToken.Dispose();
            _cancellationToken = new CancellationTokenSource();
            var token = _cancellationToken.Token;
            Instance.StatusMessageEvent?.Invoke(null, message);
            if (time > 0)
            {
                try
                {
                    await Task.Delay(time, token);
                    if (!token.IsCancellationRequested)
                    {
                        Instance.StatusMessageEvent?.Invoke(null, string.Empty);
                    }
                }
                catch { }
            }
        }

        public static void InitBar(int max) => InitBar(max, 1);

        public static void InitBar(uint max) => InitBar(max, 1);

        public static void InitBar(uint max, int m) => InitBar((int)max, m);

        public static void InitBar(int max, int m)
        {
            Instance.BarReset();
            Instance.BarSetMax(max * m);
            Instance.DebugInternal(string.Format(LogStrings.FileCountInside, max));
        }

        public static void UpdateBar() => Instance.BarProgress();

        public static void SetBarMax(int max) => Instance.BarSetMax(max);

        public static void SetBarValue(int value) => Instance.BarSetVal(value);

        public static void ResetBar() => Instance.BarReset();

        public static void ShowCheckingUpdate() => Instance.InfoInvokeInternal(LogStrings.Updating);

        public static async void ShowCheckSuccess(int result)
        {
            switch (result)
            {
                case 0:
                    Instance.InfoInternal(LogStrings.NoUpdate);
                    await ShowAndDisappear(LogStrings.NoUpdate, 5000);
                    break;
                case 1:
                    Instance.InfoInternal(LogStrings.HasUpdate);
                    await ShowAndDisappear(LogStrings.HasUpdate, 5000);
                    break;
                case -1:
                    Instance.InfoInternal(LogStrings.PreReleaseVer);
                    await ShowAndDisappear(LogStrings.PreReleaseVer, 5000);
                    break;
            }
        }

        public static void ShowCheckError() => Instance.ErrorInternal(LogStrings.UpdateError, false);

        public static void ShowProgramVersion(string cv, string lv) => Instance.DebugInternal(string.Format(LogStrings.Versions, cv, lv));

        public static void ShowVersion(string extension, int version) => Instance.InfoInternal(string.Format(LogStrings.ValidArchiveDetected, extension, version));

        public static void ShowVersion(string extension, string version) => Instance.InfoInternal(string.Format(LogStrings.ValidArchiveDetected, extension, version));

        public static void ImportDatabaseScheme(string name, int count) => Instance.DebugInternal(string.Format(LogStrings.ImportDatabaseScheme, name, count));
        #endregion
    }
}
