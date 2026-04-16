using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using IoEditor.Desktop.Services;
using IoEditor.Platform;
using IoEditor.Desktop.Utils;
using IoEditor.Model;
using System.Collections.Generic;
using IoEditor.Models.Comparison;
using IoEditor.Models.Instructions;
using IoEditor.Models.ImageCache;
using IoEditor.Models.Merging;
using IoEditor.Models.Model;
using IoEditor.Models.Studio;
using Microsoft.Extensions.Options;

namespace IoEditor.Desktop.ViewModels;

internal sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly PartLibrary _partLibrary;
    private readonly ColorLibrary _colorLibrary;
    private readonly IPartImageProxyFactory _imageProxyFactory;
    private readonly ILoaderDialogPresenter _loaderDialog;
    private readonly IFilePickerService _filePicker;
    private readonly IDialogService _dialogs;
    private readonly IAppLifetime _appLifetime;

    public ICommand OpenFilesCommand { get; }
    public ICommand SaveFileCommand { get; }
    public ICommand SaveAsCommand { get; }
    public ICommand ExitCommand { get; }
    public ICommand RefreshMergeCommand { get; }

    private IoEdProject? _project;
    public IoEdProject? Project
    {
        get => _project;
        set
        {
            if (_project != value)
            {
                if (_project is not null)
                {
                    _project.PropertyChanged -= OnProjectPropertyChanged;
                }

                _project = value;

                if (_project is not null)
                {
                    _project.PropertyChanged += OnProjectPropertyChanged;
                }

                RaisePropertyChanged(nameof(Project));
                RaisePropertyChanged(nameof(MergeSegments));
                RaisePropertyChanged(nameof(WindowTitle));
                RaisePropertyChanged(nameof(StepDictionaryRows));
            }
        }
    }

    private void OnProjectPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IoEdProject.MergeModel))
        {
            RaisePropertyChanged(nameof(MergeSegments));
        }
    }

    /// <summary>Stable binding source for the Segments list (avoids null path when no project).</summary>
    public IEnumerable<MergedSegment> MergeSegments
        => Project?.MergeModel?.Segments ?? Enumerable.Empty<MergedSegment>();

    public string WindowTitle
        => !string.IsNullOrEmpty(Project?.Target?.FileName)
            ? $"IO Editor - {Project!.Target.FileName}"
            : "IO Editor";

    public IEnumerable<InterimStepData> StepDictionaryRows
        => Project?.InterimData?.StepDictionary?.Values ?? Enumerable.Empty<InterimStepData>();

    public MainViewModel(
        PartLibrary partLibrary,
        ColorLibrary colorLibrary,
        IPartImageProxyFactory imageProxyFactory,
        ILoaderDialogPresenter loaderDialog,
        IFilePickerService filePicker,
        IDialogService dialogs,
        IAppLifetime appLifetime)
    {
        _partLibrary = partLibrary;
        _colorLibrary = colorLibrary;
        _imageProxyFactory = imageProxyFactory;
        _loaderDialog = loaderDialog;
        _filePicker = filePicker;
        _dialogs = dialogs;
        _appLifetime = appLifetime;

        OpenFilesCommand = new DelegateCommand(OpenFilesCmd);
        SaveFileCommand = new DelegateCommand(SaveFileCmd);
        SaveAsCommand = new DelegateCommand(SaveAsCmd);
        ExitCommand = new DelegateCommand(ExitCmd);
        RefreshMergeCommand = new DelegateCommand(RefreshMergeCmd);
    }

    private static Window? GetMainWindow()
        => (Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow as Window;

    private void ExitCmd(object? _) => _appLifetime.Shutdown();

    private void SaveFileCmd(object? _)
    {
        _ = _dialogs.ShowErrorAsync("Save is not implemented yet.");
    }

    private async void SaveAsCmd(object? _)
    {
        var owner = GetMainWindow();
        if (owner is null || Project is null)
        {
            return;
        }

        var filePath = await _filePicker.PickSaveIoFileAsync(owner);
        if (string.IsNullOrEmpty(filePath))
        {
            return;
        }

        if (File.Exists(filePath))
        {
            if (!await _dialogs.ConfirmAsync("File already exists. Do you want to overwrite?", "Confirm Overwrite"))
            {
                return;
            }
        }

        try
        {
            StudioFileSaver.Save(filePath, Project);
            await _dialogs.ShowInfoAsync("Done.");
        }
        catch (Exception ex)
        {
            await _dialogs.ShowErrorAsync($"Error saving file: {ex.Message}");
        }
    }

    private async void OpenFilesCmd(object? _)
    {
        var owner = GetMainWindow();
        if (owner is null)
        {
            return;
        }

        var pick = await _loaderDialog.ShowAsync(owner);
        if (pick is null)
        {
            return;
        }

        try
        {
            OpenFiles(pick.Value.ReferencePath, pick.Value.TargetPath);
        }
        catch (Exception ex)
        {
            await _dialogs.ShowErrorAsync($"Error opening files: {ex.Message}");
        }
    }

    private async void RefreshMergeCmd(object? _)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var project = Project;
            if (project is null)
            {
                Console.WriteLine("No project is loaded");
                return;
            }

            (var instruction, var imageResources) = InstructionMerger.Merge(project);
            project.MergedInstruction = instruction;
            project.MergedImageResources = imageResources;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            await _dialogs.ShowErrorAsync($"Merge failed: {ex.Message}");
        }
        finally
        {
            Console.WriteLine($"Done. Elapsed: {sw.Elapsed}");
        }
    }

    public void OpenFiles(string reference, string target)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            Console.WriteLine("==== Loading project ====");
            Project = null;

            var project = IoEdProjectLoader.Load(reference, target);
            Project = project;

            Console.WriteLine("Compare reference and target files");
            var stepBuilder = new IndexedStepsBuilder(_partLibrary, _colorLibrary, _imageProxyFactory);
            var stepComparer = new StepComparer(stepBuilder);
            var comparisonResult = stepComparer.Compare(Project!.Reference, Project.Target);
            Project.ComparisonResult = comparisonResult;

            var mergeBuilder = new MergeModelBuilder();
            var mergeResult = mergeBuilder.Build(comparisonResult, Project.Reference.Instruction);
            project.MergeModel = mergeResult;

            Console.WriteLine("Merging instructions");
            (var instruction, var imageResources) = InstructionMerger.Merge(project);
            project.MergedInstruction = instruction;
            project.MergedImageResources = imageResources;

            Console.WriteLine("Done loading project");
        }
        finally
        {
            Console.WriteLine($"Done. Elapsed: {sw.Elapsed}");
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void RaisePropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
