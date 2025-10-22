using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GalArc.Enums;
using GalArc.I18n;
using GalArc.Infrastructure.Logging;
using GalArc.Infrastructure.Progress;
using GalArc.Infrastructure.Settings;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GalArc.ViewModels;

internal partial class StatusViewModel : ViewModelBase, IDisposable
{
    [ObservableProperty]
    private ObservableCollection<LogEntry> logEntries = [];

    [ObservableProperty]
    private ObservableCollection<LogEntry> selectedLogEntries = [];

    [ObservableProperty]
    private int currentProgressValue;

    [ObservableProperty]
    private int currentProgressMax;

    [ObservableProperty]
    private int overallProgressValue;

    [ObservableProperty]
    private int overallProgressMax;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ExitCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    private bool isOperationRunning;

    public StatusViewModel()
    {
        if (Design.IsDesignMode)
            return;
        Logger.LogEvent += OnLogReceived;
        ProgressManager.ProgressEvent += OnProgressUpdated;
        ProgressManager.OverallProgressEvent += OnOverallProgressUpdated;
        StartCommand.Execute(null);
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts = null;
        Logger.LogEvent -= OnLogReceived;
        ProgressManager.ProgressEvent -= OnProgressUpdated;
        ProgressManager.OverallProgressEvent -= OnOverallProgressUpdated;
    }

    private void OnLogReceived(object sender, LogEventArgs e)
    {
        Dispatcher.UIThread.Post(() => LogEntries.Add(e.Entry));
    }

    private void OnProgressUpdated(object sender, ProgressEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            switch (e.Action)
            {
                case ProgressAction.Progress:
                    CurrentProgressValue++;
                    break;
                case ProgressAction.SetMax:
                    CurrentProgressMax = e.Max;
                    break;
                case ProgressAction.SetValue:
                    CurrentProgressValue = Math.Min(CurrentProgressMax, e.Value);
                    break;
                case ProgressAction.Finish:
                    CurrentProgressValue = CurrentProgressMax;
                    break;
            }
        });
    }

    private void OnOverallProgressUpdated(object sender, ProgressEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            switch (e.Action)
            {
                case ProgressAction.Progress:
                    OverallProgressValue++;
                    break;
                case ProgressAction.SetMax:
                    OverallProgressMax = e.Max;
                    break;
                case ProgressAction.SetValue:
                    OverallProgressValue = Math.Min(OverallProgressMax, e.Value);
                    break;
                case ProgressAction.Finish:
                    OverallProgressValue = OverallProgressMax;
                    break;
            }
        });
    }

    private CancellationTokenSource _cts = new();

    [RelayCommand]
    private async Task StartAsync()
    {
        _cts?.Dispose();
        _cts = new();
        IsOperationRunning = true;
        try
        {
            await Task.Run(Execute);
        }
        catch (OperationCanceledException)
        {
            Logger.Info(MsgStrings.OperationCancelled);
        }
        catch (Exception ex)
        {
            Logger.ErrorFormat(MsgStrings.ErrorOperation, ex.Message);
            Logger.Error(ex.ToString());
            if (ex.InnerException != null)
            {
                Logger.ErrorFormat(MsgStrings.InnerException, ex.InnerException.Message);
                Logger.Error(ex.InnerException.ToString());
            }
        }
        finally
        {
            IsOperationRunning = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanExit))]
    private void Exit(Window window)
    {
        if (CanExit)
        {
            window?.Close();
        }
    }

    [RelayCommand(CanExecute = nameof(IsOperationRunning))]
    private void Cancel()
    {
        _cts?.Cancel();
    }

    private void Execute()
    {
        #region Configure Settings
        Logger.Info(MsgStrings.ConfiguringSettings);
        string input = SettingsManager.Settings.InputPath;
        string output = SettingsManager.Settings.OutputPath;
        bool is_batch_mode = SettingsManager.Settings.Operation == OperationType.Unpack &&
            (input.Contains('*') || input.Contains('?'));
        string[] batch_inputs = [];
        if (!is_batch_mode)
        {
            switch (SettingsManager.Settings.Operation)
            {
                case OperationType.Unpack:
                    if (!File.Exists(input))
                        throw new FileNotFoundException(MsgStrings.ErrorFileNotFound, input);
                    break;
                case OperationType.Pack:
                    if (!Directory.Exists(input))
                        throw new DirectoryNotFoundException(MsgStrings.ErrorDirNotFound);
                    break;
            }
        }
        else
        {
            string dir = Path.GetDirectoryName(input);
            string pattern = Path.GetFileName(input);
            if (Directory.Exists(dir))
            {
                batch_inputs = Directory.GetFiles(dir, pattern);
            }
            if (batch_inputs.Length == 0)
            {
                throw new FileNotFoundException("No files match the given pattern.", input);
            }
        }
        #endregion

        #region Execute Main Operation
        Logger.InfoFormat(MsgStrings.StartingOperation, SettingsManager.Settings.Operation switch
        {
            OperationType.Unpack => MsgStrings.OperationUnpack,
            OperationType.Pack => MsgStrings.OperationPack,
            _ => throw new InvalidOperationException("Unknown operation type.")
        });
        switch (SettingsManager.Settings.Operation)
        {
            case OperationType.Unpack:
                if (is_batch_mode)
                {
                    ProgressManager.OverallSetMax(batch_inputs.Length);
                    foreach (string batch_input in batch_inputs)
                    {
                        _cts.Token.ThrowIfCancellationRequested();
                        ProgressManager.SetValue(0);
                        ProgressManager.SetMax(0);
                        string batch_output = Path.Combine(SettingsManager.Settings.OutputPath,
                            Path.GetFileNameWithoutExtension(batch_input));
                        Directory.CreateDirectory(batch_output);
                        Logger.InfoFormat(MsgStrings.ProcessingFile, batch_input);
                        try
                        {
                            SettingsManager.Settings.UnpackFormat.Unpack(batch_input, batch_output);
                        }
                        catch (Exception ex) when (SettingsManager.Settings.ContinueOnError)
                        {
                            Logger.ErrorFormat(MsgStrings.ErrorOperation, ex.Message);
                            Logger.Info(MsgStrings.ContinueOnError);
                            ProgressManager.OverallProgress();
                            continue;
                        }
                        ProgressManager.OverallProgress();
                        Logger.Info(MsgStrings.SuccessUnpack);
                    }
                }
                else
                {
                    _cts.Token.ThrowIfCancellationRequested();
                    Directory.CreateDirectory(output);
                    Logger.InfoFormat(MsgStrings.ProcessingFile, input);
                    ProgressManager.OverallSetMax(1);
                    SettingsManager.Settings.UnpackFormat.Unpack(input, output);
                    ProgressManager.OverallProgress();
                    Logger.Info(MsgStrings.SuccessUnpack);
                }
                break;
            case OperationType.Pack:
                _cts.Token.ThrowIfCancellationRequested();
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                Logger.InfoFormat(MsgStrings.ProcessingFolder, input);
                ProgressManager.OverallSetMax(1);
                SettingsManager.Settings.PackFormat.Pack(input, output);
                ProgressManager.OverallProgress();
                Logger.Info(MsgStrings.SuccessPack);
                break;
        }
        #endregion
    }

    [RelayCommand(CanExecute = nameof(CanCopyLog))]
    private async Task CopyLogAsync(Window window)
    {
        if (SelectedLogEntries == null || SelectedLogEntries.Count == 0)
        {
            return;
        }
        StringBuilder sb = new();
        foreach (LogEntry entry in SelectedLogEntries)
        {
            sb.AppendLine(entry.ToString());
        }
        try
        {
            await window.Clipboard.SetTextAsync(sb.ToString());
            Logger.Info("Selected log entries copied to clipboard.");
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to copy log entries to clipboard: {ex.Message}");
        }
    }

    private bool CanCopyLog => LogEntries?.Count > 0 && SelectedLogEntries?.Count > 0;

    private bool CanExit => !IsOperationRunning;
}
