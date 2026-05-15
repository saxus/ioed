using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;

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
