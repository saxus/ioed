using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
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
    /// <summary>Very large buffers can freeze or blank simple text controls; preview is capped.</summary>
    private const int InstructionXmlEditorDisplayMaxChars = 2_000_000;

    private string _referenceInstructionXml = string.Empty;
    private string _targetInstructionXml = string.Empty;
    private string _mergedInstructionXml = string.Empty;

    private readonly PartLibrary _partLibrary;
    private readonly ColorLibrary _colorLibrary;
    private readonly IPartImageProxyFactory _imageProxyFactory;
    private readonly ILoaderDialogPresenter _loaderDialog;
    private readonly IFilePickerService _filePicker;
    private readonly IDialogService _dialogs;
    private readonly IAppLifetime _appLifetime;
    private readonly ISettingsUiPresenter _settingsUi;

    public ICommand OpenFilesCommand { get; }
    public ICommand SaveFileCommand { get; }
    public ICommand SaveAsCommand { get; }
    public ICommand ExitCommand { get; }
    public ICommand OpenSettingsCommand { get; }

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

                if (value is null)
                {
                    ClearInstructionXmlViews();
                }
            }
        }
    }

    public string ReferenceInstructionXml => _referenceInstructionXml;

    public string TargetInstructionXml => _targetInstructionXml;

    public string MergedInstructionXml => _mergedInstructionXml;

    private void ClearInstructionXmlViews()
    {
        _referenceInstructionXml = string.Empty;
        _targetInstructionXml = string.Empty;
        _mergedInstructionXml = string.Empty;
        RaisePropertyChanged(nameof(ReferenceInstructionXml));
        RaisePropertyChanged(nameof(TargetInstructionXml));
        RaisePropertyChanged(nameof(MergedInstructionXml));
    }

    /// <summary>Passes merged XML explicitly so we can refresh the UI before <see cref="IoEdProject.MergedInstruction"/> is assigned (that assignment fires <see cref="INotifyPropertyChanged"/> early otherwise).</summary>
    private void ApplyInstructionXmlDocuments(IoEdProject project, XDocument mergedInstruction)
    {
        _referenceInstructionXml = TruncateInstructionXmlForEditor(project.Reference.Instruction.Document.ToString(), "reference");
        _targetInstructionXml = TruncateInstructionXmlForEditor(project.Target.Instruction.Document.ToString(), "target");
        _mergedInstructionXml = TruncateInstructionXmlForEditor(mergedInstruction.ToString(), "merged");
        RaisePropertyChanged(nameof(ReferenceInstructionXml));
        RaisePropertyChanged(nameof(TargetInstructionXml));
        RaisePropertyChanged(nameof(MergedInstructionXml));
    }

    private static string TruncateInstructionXmlForEditor(string xml, string role)
    {
        if (xml.Length <= InstructionXmlEditorDisplayMaxChars)
        {
            return xml;
        }

        var suffix =
            "\r\n\r\n<!-- IoEditor: instruction XML truncated for editor display (" + role +
            "); full length " + xml.Length +
            " characters; increase InstructionXmlEditorDisplayMaxChars if needed -->";
        var headLen = InstructionXmlEditorDisplayMaxChars - suffix.Length;
        if (headLen <= 0)
        {
            return suffix.TrimStart();
        }

        return xml[..headLen] + suffix;
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
            ? $"IoEditor - {Project!.Target.FileName}"
            : "IoEditor";

    public IEnumerable<InterimStepData> StepDictionaryRows
        => Project?.InterimData?.StepDictionary?.OrderBy(static kv => kv.Key).Select(static kv => kv.Value)
           ?? Enumerable.Empty<InterimStepData>();

    public MainViewModel(
        PartLibrary partLibrary,
        ColorLibrary colorLibrary,
        IPartImageProxyFactory imageProxyFactory,
        ILoaderDialogPresenter loaderDialog,
        IFilePickerService filePicker,
        IDialogService dialogs,
        IAppLifetime appLifetime,
        ISettingsUiPresenter settingsUi)
    {
        _partLibrary = partLibrary;
        _colorLibrary = colorLibrary;
        _imageProxyFactory = imageProxyFactory;
        _loaderDialog = loaderDialog;
        _filePicker = filePicker;
        _dialogs = dialogs;
        _appLifetime = appLifetime;
        _settingsUi = settingsUi;

        OpenFilesCommand = new DelegateCommand(OpenFilesCmd);
        SaveFileCommand = new DelegateCommand(SaveFileCmd);
        SaveAsCommand = new DelegateCommand(SaveAsCmd);
        ExitCommand = new DelegateCommand(ExitCmd);
        OpenSettingsCommand = new DelegateCommand(OpenSettingsCmd);
    }

    private static Window? GetMainWindow()
        => (Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow as Window;

    private void ExitCmd(object? _) => _appLifetime.Shutdown();

    private async void OpenSettingsCmd(object? _)
    {
        var owner = GetMainWindow();
        _ = await _settingsUi.ShowAsync(owner);
    }

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

            ApplyInstructionXmlDocuments(project, instruction);

            project.MergedInstruction = instruction;
            project.MergedImageResources = imageResources;

            project.InterimData.StepDictionary = StepDictionaryBuilder.FromInstructionDocument(project.Target.Instruction.Document);
            RaisePropertyChanged(nameof(StepDictionaryRows));

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
