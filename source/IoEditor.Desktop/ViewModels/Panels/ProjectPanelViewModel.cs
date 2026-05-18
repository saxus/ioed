using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Xml.Linq;
using Avalonia.Controls;
using IoEditor.Desktop.Utils;
using IoEditor.Model;
using IoEditor.Models.Comparison;
using IoEditor.Models.Configuration;
using IoEditor.Models.ImageCache;
using IoEditor.Models.Instructions;
using IoEditor.Models.Merging;
using IoEditor.Models.Model;
using IoEditor.Models.Studio;
using IoEditor.Platform;
using Microsoft.Extensions.Options;

namespace IoEditor.Desktop.ViewModels.Panels;

/// <summary>Panel that displays a loaded reference/target project pair with diff navigation.</summary>
internal sealed class ProjectPanelViewModel : EditorPanelViewModelBase
{
    /// <summary>Very large buffers can freeze or blank simple text controls; preview is capped.</summary>
    private const int InstructionXmlEditorDisplayMaxChars = 2_000_000;

    private readonly PartLibrary _partLibrary;
    private readonly ColorLibrary _colorLibrary;
    private readonly IPartImageProxyFactory _imageProxyFactory;
    private readonly IFilePickerService _filePicker;
    private readonly IDialogService _dialogs;
    private readonly IOptionsMonitor<StudioOptions> _studioOptions;
    private readonly Func<Window?> _getMainWindow;

    private string _referenceInstructionXml = string.Empty;
    private string _targetInstructionXml = string.Empty;
    private string _mergedInstructionXml = string.Empty;

    private MainNavSection _selectedSection = MainNavSection.Segments;

    public ICommand SaveCommand { get; }
    public ICommand SaveAsCommand { get; }
    public ICommand CloseCommand { get; }
    public ICommand SelectNavSectionCommand { get; }

    /// <summary>Raised when the user clicks Close; the shell removes this panel.</summary>
    public event Action<ProjectPanelViewModel>? CloseRequested;

    private IoEdProject? _project;

    public IoEdProject? Project
    {
        get => _project;
        private set
        {
            if (_project == value)
            {
                return;
            }

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
            RaisePropertyChanged(nameof(Title));
            RaisePropertyChanged(nameof(StepDictionaryRows));

            if (value is null)
            {
                ClearInstructionXmlViews();
            }
        }
    }

    public override string Title
        => !string.IsNullOrEmpty(_project?.Target?.FileName)
            ? Path.GetFileName(_project!.Target.FileName)
            : "Project";

    public MainNavSection SelectedSection
    {
        get => _selectedSection;
        set
        {
            var clamped = ClampSection(value);
            if (_selectedSection == clamped)
            {
                return;
            }

            _selectedSection = clamped;
            RaisePropertyChanged(nameof(SelectedSection));
            RaiseNavSectionVisualProperties();
        }
    }

    public bool IsSegmentsViewActive => SelectedSection == MainNavSection.Segments;
    public bool IsReferenceXmlViewActive => SelectedSection == MainNavSection.ReferenceXml;
    public bool IsTargetXmlViewActive => SelectedSection == MainNavSection.TargetXml;
    public bool IsGeneratedXmlViewActive => SelectedSection == MainNavSection.GeneratedXml;
    public bool IsStepDictionaryViewActive => SelectedSection == MainNavSection.StepDictionary;

    public bool ShowXmlDebugTabs => _studioOptions.CurrentValue.ShowXmlDebugTabs;

    public string ReferenceInstructionXml => _referenceInstructionXml;
    public string TargetInstructionXml => _targetInstructionXml;
    public string MergedInstructionXml => _mergedInstructionXml;

    /// <summary>Stable binding source for the Segments list (avoids null path when no project).</summary>
    public IEnumerable<MergedSegment> MergeSegments
        => _project?.MergeModel?.Segments ?? Enumerable.Empty<MergedSegment>();

    public IEnumerable<InterimStepData> StepDictionaryRows
        => _project?.InterimData?.StepDictionary?.OrderBy(static kv => kv.Key).Select(static kv => kv.Value)
           ?? Enumerable.Empty<InterimStepData>();

    public ProjectPanelViewModel(
        PartLibrary partLibrary,
        ColorLibrary colorLibrary,
        IPartImageProxyFactory imageProxyFactory,
        IFilePickerService filePicker,
        IDialogService dialogs,
        IOptionsMonitor<StudioOptions> studioOptions,
        Func<Window?> getMainWindow)
    {
        _partLibrary = partLibrary;
        _colorLibrary = colorLibrary;
        _imageProxyFactory = imageProxyFactory;
        _filePicker = filePicker;
        _dialogs = dialogs;
        _studioOptions = studioOptions;
        _getMainWindow = getMainWindow;

        _ = _studioOptions.OnChange(_ =>
        {
            RaisePropertyChanged(nameof(ShowXmlDebugTabs));
            var clamped = ClampSection(_selectedSection);
            if (clamped != _selectedSection)
            {
                _selectedSection = clamped;
                RaisePropertyChanged(nameof(SelectedSection));
            }

            RaiseNavSectionVisualProperties();
        });

        SaveCommand = new DelegateCommand(SaveCmd);
        SaveAsCommand = new DelegateCommand(SaveAsCmd);
        CloseCommand = new DelegateCommand(_ => CloseRequested?.Invoke(this));
        SelectNavSectionCommand = new DelegateCommand(SelectNavSectionCmd);
    }

    /// <summary>Loads a project from the given file paths. Should be called once after construction.</summary>
    public void LoadProject(string reference, string target)
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
            var comparisonResult = stepComparer.Compare(project.Reference, project.Target);
            project.ComparisonResult = comparisonResult;

            var mergeBuilder = new MergeModelBuilder();
            var mergeResult = mergeBuilder.Build(comparisonResult, project.Reference.Instruction);
            project.MergeModel = mergeResult;

            Console.WriteLine("Merging instructions");
            (var instruction, var imageResources) = InstructionMerger.Merge(project);

            ApplyInstructionXmlDocuments(project, instruction);

            project.MergedInstruction = instruction;
            project.MergedImageResources = imageResources;

            var refStepDict = StepDictionaryBuilder.FromInstructionDocument(project.Reference.Instruction.Document);
            var tgtStepDict = StepDictionaryBuilder.FromInstructionDocument(project.Target.Instruction.Document);

            EnrichSteps(comparisonResult.IndexedReferenceSteps, refStepDict);
            EnrichSteps(comparisonResult.IndexedTargetSteps, tgtStepDict);

            project.InterimData.StepDictionary = tgtStepDict;
            RaisePropertyChanged(nameof(StepDictionaryRows));

            Console.WriteLine("Done loading project");
        }
        finally
        {
            Console.WriteLine($"Done. Elapsed: {sw.Elapsed}");
        }
    }

    private static void EnrichSteps(List<IndexedStep> steps, Dictionary<int, InterimStepData> dict)
    {
        foreach (var step in steps)
        {
            if (dict.TryGetValue(step.Index, out var data))
            {
                step.PageNumber = data.PageNumber;
                step.ColumnNumber = data.ColumnNumber;
                step.IsCallout = data.IsCallout;
                step.CalloutParentStepIndex = data.CalloutParentStepIndex;
            }
        }
    }

    private void SelectNavSectionCmd(object? parameter)
    {
        if (parameter is MainNavSection m)
        {
            SelectedSection = m;
            return;
        }

        if (parameter is string s && Enum.TryParse<MainNavSection>(s, ignoreCase: true, out var parsed))
        {
            SelectedSection = parsed;
        }
    }

    private void SaveCmd(object? _)
    {
        if (_project is null)
        {
            return;
        }

        try
        {
            StudioFileSaver.Save(_project.TargetFileName, _project);
            _ = _dialogs.ShowInfoAsync("Saved.");
        }
        catch (Exception ex)
        {
            _ = _dialogs.ShowErrorAsync($"Error saving file: {ex.Message}");
        }
    }

    private async void SaveAsCmd(object? _)
    {
        var owner = _getMainWindow();
        if (owner is null || _project is null)
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
            StudioFileSaver.Save(filePath, _project);
            await _dialogs.ShowInfoAsync("Done.");
        }
        catch (Exception ex)
        {
            await _dialogs.ShowErrorAsync($"Error saving file: {ex.Message}");
        }
    }

    private MainNavSection ClampSection(MainNavSection section)
    {
        if (!ShowXmlDebugTabs && section is MainNavSection.ReferenceXml
                             or MainNavSection.TargetXml
                             or MainNavSection.GeneratedXml
                             or MainNavSection.StepDictionary)
        {
            return MainNavSection.Segments;
        }

        return section;
    }

    private void RaiseNavSectionVisualProperties()
    {
        RaisePropertyChanged(nameof(IsSegmentsViewActive));
        RaisePropertyChanged(nameof(IsReferenceXmlViewActive));
        RaisePropertyChanged(nameof(IsTargetXmlViewActive));
        RaisePropertyChanged(nameof(IsGeneratedXmlViewActive));
        RaisePropertyChanged(nameof(IsStepDictionaryViewActive));
    }

    private void OnProjectPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IoEdProject.MergeModel))
        {
            RaisePropertyChanged(nameof(MergeSegments));
        }
    }

    /// <summary>
    /// Passes merged XML explicitly so the UI refreshes before
    /// <see cref="IoEdProject.MergedInstruction"/> is assigned.
    /// </summary>
    private void ApplyInstructionXmlDocuments(IoEdProject project, XDocument mergedInstruction)
    {
        _referenceInstructionXml = TruncateInstructionXmlForEditor(project.Reference.Instruction.Document.ToString(), "reference");
        _targetInstructionXml = TruncateInstructionXmlForEditor(project.Target.Instruction.Document.ToString(), "target");
        _mergedInstructionXml = TruncateInstructionXmlForEditor(mergedInstruction.ToString(), "merged");
        RaisePropertyChanged(nameof(ReferenceInstructionXml));
        RaisePropertyChanged(nameof(TargetInstructionXml));
        RaisePropertyChanged(nameof(MergedInstructionXml));
    }

    private void ClearInstructionXmlViews()
    {
        _referenceInstructionXml = string.Empty;
        _targetInstructionXml = string.Empty;
        _mergedInstructionXml = string.Empty;
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
}
