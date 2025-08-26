using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GalArc.Enums;
using GalArc.I18n;
using GalArc.Infrastructure.Settings;
using GalArc.Models.Formats.Commons;
using GalArc.Services;
using GalArc.Views;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GalArc.ViewModels;

internal partial class MainViewModel : ViewModelBase
{
    private readonly IShowDialogService _showDialogService;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartCommand))]
    private string inputPath;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartCommand))]
    private string outputPath;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartCommand))]
    [NotifyPropertyChangedFor(nameof(InputWatermark))]
    [NotifyPropertyChangedFor(nameof(OutputWatermark))]
    [NotifyPropertyChangedFor(nameof(FormatOptions))]
    private OperationType selectedOperation;

    [ObservableProperty]
    private List<ArcFormat> unpackFormats;

    [ObservableProperty]
    private List<ArcFormat> packFormats;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartCommand))]
    [NotifyPropertyChangedFor(nameof(FormatOptions))]
    private ArcFormat selectedUnpackFormat;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartCommand))]
    [NotifyPropertyChangedFor(nameof(FormatOptions))]
    private ArcFormat selectedPackFormat;

    [ObservableProperty]
    private bool overwriteExistingFiles;

    [ObservableProperty]
    private bool continueOnError;

    public string InputWatermark => SelectedOperation == OperationType.Unpack ? GuiStrings.WaterInputFile : GuiStrings.WaterInputFolder;

    public string OutputWatermark => SelectedOperation == OperationType.Unpack ? GuiStrings.WaterOutputFolder : GuiStrings.WaterOutputFile;

    private ArcOptions UnpackFormatOptions => (SelectedUnpackFormat as IUnpackConfigurable)?.UnpackOptions;

    private ArcOptions PackFormatOptions => (SelectedPackFormat as IPackConfigurable)?.PackOptions;

    public ArcOptions FormatOptions => SelectedOperation switch
    {
        OperationType.Unpack => UnpackFormatOptions,
        OperationType.Pack => PackFormatOptions,
        _ => null,
    };

    public MainViewModel(IShowDialogService showDialogService)
    {
        _showDialogService = showDialogService;
        InputPath = SettingsManager.Settings.InputPath;
        OutputPath = SettingsManager.Settings.OutputPath;
        UnpackFormats = ArcFormatProvider.GetFormats();
        PackFormats = ArcFormatProvider.GetWriteableFormats();
        SelectedOperation = SettingsManager.Settings.Operation;
        SelectedUnpackFormat = UnpackFormats.Count > 0 ? UnpackFormats[SettingsManager.Settings.UnpackFormatIndex] : null;
        SelectedPackFormat = PackFormats.Count > 0 ? PackFormats[SettingsManager.Settings.PackFormatIndex] : null;
        ContinueOnError = SettingsManager.Settings.ContinueOnError;
    }

    public MainViewModel() : this(null)
    {
    }

    [RelayCommand]
    private async Task BrowseInput()
    {
        switch (SelectedOperation)
        {
            case OperationType.Unpack:
                IReadOnlyList<IStorageFile> resultFile = await App.Top.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
                {
                    AllowMultiple = false,
                });
                if (resultFile?.Count > 0)
                {
                    InputPath = resultFile[0].Path.LocalPath;
                }
                break;
            case OperationType.Pack:
                IReadOnlyList<IStorageFolder> resultFolder = await App.Top.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
                {
                    AllowMultiple = false,
                });
                if (resultFolder?.Count > 0)
                {
                    InputPath = resultFolder[0].Path.LocalPath;
                }
                break;
        }
    }

    [RelayCommand]
    private async Task BrowseOutput()
    {
        switch (SelectedOperation)
        {
            case OperationType.Unpack:
                IReadOnlyList<IStorageFolder> resultFolder = await App.Top.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
                {
                    AllowMultiple = false,
                });
                if (resultFolder?.Count >= 1)
                {
                    OutputPath = resultFolder[0].Path.LocalPath;
                }
                break;
            case OperationType.Pack:
                IStorageFile resultFile = await App.Top.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions());
                if (resultFile != null)
                {
                    OutputPath = resultFile.Path.LocalPath;
                }
                break;
        }
    }

    [RelayCommand(CanExecute = nameof(CanStart))]
    private async Task Start(Window window)
    {
        await _showDialogService.ShowDialogAsync<StatusView, StatusViewModel>(window);
    }

    [RelayCommand]
    private static void Exit(Window window)
    {
        window?.Close();
    }

    [RelayCommand]
    private void DropInput(IEnumerable<IStorageItem> fileItems)
    {
        foreach (IStorageItem fileItem in fileItems)
        {
            switch (SelectedOperation)
            {
                case OperationType.Unpack:
                    if (fileItem is IStorageFile)
                    {
                        InputPath = fileItem.Path.LocalPath;
                        break;
                    }
                    break;
                case OperationType.Pack:
                    if (fileItem is IStorageFolder)
                    {
                        InputPath = fileItem.Path.LocalPath;
                        break;
                    }
                    break;
            }
        }
    }

    [RelayCommand]
    private void DropOutput(IEnumerable<IStorageItem> fileItems)
    {
        foreach (IStorageItem fileItem in fileItems)
        {
            switch (SelectedOperation)
            {
                case OperationType.Unpack:
                    if (fileItem is IStorageFolder)
                    {
                        OutputPath = fileItem.Path.LocalPath;
                        break;
                    }
                    break;
                case OperationType.Pack:
                    if (fileItem is IStorageFile)
                    {
                        OutputPath = fileItem.Path.LocalPath;
                        break;
                    }
                    break;
            }
        }
    }

    [RelayCommand]
    private async Task OpenSettings(Window window)
    {
        await _showDialogService.ShowDialogAsync<SettingsView, SettingsViewModel>(window);
    }

    [RelayCommand]
    private async Task OpenAbout(Window window)
    {
        await _showDialogService.ShowDialogAsync<AboutView, AboutViewModel>(window);
    }

    [RelayCommand]
    private async Task CheckUpdates(Window window)
    {
        await _showDialogService.ShowDialogAsync<UpdateView, UpdateViewModel>(window);
    }

    private void SyncPath()
    {
        switch (SelectedOperation)
        {
            case OperationType.Unpack:
                if (InputPath.Contains('*') || InputPath.Contains('?'))
                {
                    OutputPath = Path.GetDirectoryName(InputPath);
                }
                else
                {
                    if (File.Exists(InputPath))
                    {
                        OutputPath = Path.Combine(Path.GetDirectoryName(InputPath), Path.GetFileNameWithoutExtension(InputPath));
                    }
                }
                break;
            case OperationType.Pack:
                if (Directory.Exists(InputPath))
                {
                    string path = InputPath.TrimEnd(Path.DirectorySeparatorChar);
                    OutputPath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileName(path) +
                        (SelectedPackFormat != null ? "." + SelectedPackFormat.Name.ToLower() : string.Empty));
                }
                break;
        }
    }

    private bool CanStart => !string.IsNullOrWhiteSpace(InputPath) &&
        !string.IsNullOrWhiteSpace(OutputPath) &&
        ((SelectedOperation is OperationType.Unpack && SelectedUnpackFormat != null) || (SelectedOperation is OperationType.Pack && SelectedPackFormat != null));

    partial void OnInputPathChanged(string value)
    {
        SettingsManager.Settings.InputPath = value;
        SyncPath();
    }

    partial void OnOutputPathChanged(string value)
    {
        SettingsManager.Settings.OutputPath = value;
    }

    partial void OnSelectedOperationChanged(OperationType value)
    {
        SettingsManager.Settings.Operation = value;
        SyncPath();
    }

    partial void OnSelectedUnpackFormatChanged(ArcFormat value)
    {
        SettingsManager.Settings.UnpackFormatIndex = UnpackFormats.IndexOf(value);
        SettingsManager.Settings.UnpackFormat = value;
        SyncPath();
    }

    partial void OnSelectedPackFormatChanged(ArcFormat value)
    {
        SettingsManager.Settings.PackFormatIndex = PackFormats.IndexOf(value);
        SettingsManager.Settings.PackFormat = value;
        SyncPath();
    }

    partial void OnContinueOnErrorChanged(bool value)
    {
        SettingsManager.Settings.ContinueOnError = value;
    }
}
