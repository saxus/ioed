using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using IoEditor.Desktop.ViewModels.Panels;
using IoEditor.Models.Merging;

namespace IoEditor.Desktop.Views.Panels;

public partial class ProjectPanelView : Avalonia.Controls.UserControl
{
    private ListBox? _segmentsListBox;
    private ScrollViewer? _segmentsScrollViewer;
    private EventHandler<ScrollChangedEventArgs>? _segmentsScrollChangedHandler;
    private EventHandler? _segmentsLayoutHandler;

    public ProjectPanelView()
    {
        InitializeComponent();
        Loaded += OnLoadedWireSegmentsOverview;
        Unloaded += OnUnloaded;
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        if (_segmentsListBox is not null)
        {
            _segmentsListBox.SelectionChanged -= OnSegmentsListBoxSelectionChanged;
        }

        DetachSegmentsStatusTracking();
    }

    private void OnLoadedWireSegmentsOverview(object? sender, RoutedEventArgs e)
    {
        Loaded -= OnLoadedWireSegmentsOverview;

        if (this.FindControl<ListBox>("SegmentsListBox") is not { } listBox ||
            this.FindControl<SegmentsOverviewStrip>("SegmentsOverviewStrip") is not { } strip)
        {
            return;
        }

        var scrollViewer = listBox.GetVisualDescendants().OfType<ScrollViewer>()
            .FirstOrDefault(sv => !sv.GetVisualAncestors().OfType<ListBoxItem>().Any());

        strip.TargetListBox = listBox;
        strip.ScrollViewer = scrollViewer;

        _segmentsListBox = listBox;
        _segmentsScrollViewer = scrollViewer;
        listBox.SelectionChanged += OnSegmentsListBoxSelectionChanged;
        AttachSegmentsStatusTracking();

        UpdateTopVisibleSegmentStatus();
        UpdateDifferenceNavigationButtons();
    }

    private void AttachSegmentsStatusTracking()
    {
        DetachSegmentsStatusTracking();

        if (_segmentsScrollViewer is null || _segmentsListBox is null)
        {
            return;
        }

        _segmentsScrollChangedHandler = (_, _) => UpdateTopVisibleSegmentStatus();
        _segmentsLayoutHandler = (_, _) => UpdateTopVisibleSegmentStatus();
        _segmentsScrollViewer.ScrollChanged += _segmentsScrollChangedHandler;
        _segmentsScrollViewer.LayoutUpdated += _segmentsLayoutHandler;
        _segmentsListBox.LayoutUpdated += _segmentsLayoutHandler;
    }

    private void DetachSegmentsStatusTracking()
    {
        if (_segmentsScrollViewer is not null)
        {
            if (_segmentsScrollChangedHandler is not null)
            {
                _segmentsScrollViewer.ScrollChanged -= _segmentsScrollChangedHandler;
            }

            if (_segmentsLayoutHandler is not null)
            {
                _segmentsScrollViewer.LayoutUpdated -= _segmentsLayoutHandler;
            }
        }

        if (_segmentsListBox is not null && _segmentsLayoutHandler is not null)
        {
            _segmentsListBox.LayoutUpdated -= _segmentsLayoutHandler;
        }

        _segmentsScrollChangedHandler = null;
        _segmentsLayoutHandler = null;
    }

    private void UpdateTopVisibleSegmentStatus()
    {
        if (this.FindControl<TextBlock>("StatusSegmentText") is not { } statusText)
        {
            return;
        }

        if (_segmentsListBox is null || _segmentsScrollViewer is null || _segmentsListBox.Items.Count == 0)
        {
            statusText.Text = string.Empty;
            UpdateDifferenceNavigationButtons();
            return;
        }

        statusText.Text = SegmentsListScrollMetrics.GetTopVisibleSegmentStatusText(_segmentsListBox, _segmentsScrollViewer)
                          ?? string.Empty;
        UpdateDifferenceNavigationButtons();
    }

    private void OnSegmentsListBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        UpdateDifferenceNavigationButtons();
    }

    private void UpdateDifferenceNavigationButtons()
    {
        if (this.FindControl<Button>("SegmentsPrevDiffButton") is not { } prev ||
            this.FindControl<Button>("SegmentsNextDiffButton") is not { } next)
        {
            return;
        }

        if (_segmentsListBox is null || _segmentsScrollViewer is null || _segmentsListBox.Items.Count == 0)
        {
            prev.IsEnabled = false;
            next.IsEnabled = false;
            return;
        }

        var anchor = SegmentsListScrollMetrics.GetNavigationAnchorIndex(_segmentsListBox, _segmentsScrollViewer);
        prev.IsEnabled = SegmentsListScrollMetrics.FindPreviousDifferenceListIndex(_segmentsListBox, anchor) is not null;
        next.IsEnabled = SegmentsListScrollMetrics.FindNextDifferenceListIndex(_segmentsListBox, anchor) is not null;
    }

    private void OnSegmentsPrevDifferenceClick(object? sender, RoutedEventArgs e)
    {
        NavigateAdjacentDifference(goNext: false);
    }

    private void OnSegmentsNextDifferenceClick(object? sender, RoutedEventArgs e)
    {
        NavigateAdjacentDifference(goNext: true);
    }

    private void OnViewKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.G)
        {
            return;
        }

        var mods = e.KeyModifiers;
        var jumpChord = (mods & KeyModifiers.Control) != 0 || (mods & KeyModifiers.Meta) != 0;
        if (!jumpChord)
        {
            return;
        }

        if (DataContext is ProjectPanelViewModel vm && !vm.IsSegmentsViewActive)
        {
            return;
        }

        if (this.FindControl<TextBox>("JumpToTextBox") is not { } tb)
        {
            return;
        }

        tb.Focus();
        tb.SelectAll();
        e.Handled = true;
    }

    private void OnJumpToKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }

        var useSource = (e.KeyModifiers & KeyModifiers.Shift) == 0;
        ExecuteJumpTo(useSource);
        e.Handled = true;
    }

    private void OnJumpToSourceClick(object? sender, RoutedEventArgs e) => ExecuteJumpTo(useSource: true);

    private void OnJumpToTargetClick(object? sender, RoutedEventArgs e) => ExecuteJumpTo(useSource: false);

    private void ExecuteJumpTo(bool useSource)
    {
        if (_segmentsListBox is null || _segmentsScrollViewer is null ||
            this.FindControl<TextBox>("JumpToTextBox") is not { } jumpBox)
        {
            return;
        }

        var text = jumpBox.Text?.Trim() ?? string.Empty;
        if (!int.TryParse(text, System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture, out var pageNumber))
        {
            return;
        }

        var count = _segmentsListBox.Items.Count;
        for (var i = 0; i < count; i++)
        {
            if (_segmentsListBox.Items[i] is not MergedSegment seg)
            {
                continue;
            }

            var segment = useSource ? seg.ReferenceSegment : seg.TargetSegment;
            var steps = segment?.Steps;
            if (steps is null || steps.Count == 0)
            {
                continue;
            }

            if (!steps.Any(s => s.PageNumber == pageNumber))
            {
                continue;
            }

            seg.IsExpanded = true;
            SegmentsListScrollMetrics.TryScrollSegmentIndexToTop(_segmentsListBox, _segmentsScrollViewer, i);
            _segmentsListBox.SelectedIndex = i;
            UpdateDifferenceNavigationButtons();
            UpdateTopVisibleSegmentStatus();
            return;
        }
    }

    private void NavigateAdjacentDifference(bool goNext)
    {
        if (_segmentsListBox is null || _segmentsScrollViewer is null)
        {
            return;
        }

        var anchor = SegmentsListScrollMetrics.GetNavigationAnchorIndex(_segmentsListBox, _segmentsScrollViewer);
        var target = goNext
            ? SegmentsListScrollMetrics.FindNextDifferenceListIndex(_segmentsListBox, anchor)
            : SegmentsListScrollMetrics.FindPreviousDifferenceListIndex(_segmentsListBox, anchor);

        if (target is not int idx)
        {
            return;
        }

        SegmentsListScrollMetrics.TryScrollSegmentIndexToTop(_segmentsListBox, _segmentsScrollViewer, idx);
        _segmentsListBox.SelectedIndex = idx;
        UpdateDifferenceNavigationButtons();
        UpdateTopVisibleSegmentStatus();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
