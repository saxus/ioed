using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using IoEditor.Platform;

namespace IoEditor.Desktop.Views;

public partial class MainWindow : Window
{
    private ListBox? _segmentsListBox;
    private ScrollViewer? _segmentsScrollViewer;
    private EventHandler<ScrollChangedEventArgs>? _segmentsScrollChangedHandler;
    private EventHandler? _segmentsLayoutHandler;

    public MainWindow()
        : this(NullMainWindowMenuIntegration.Instance)
    {
    }

    public MainWindow(IMainWindowMenuIntegration mainWindowMenuIntegration)
    {
        InitializeComponent();
        if (this.FindControl<Menu>("MainMenu") is { } menu)
        {
            mainWindowMenuIntegration.AttachMainMenu(this, menu);
        }

        Loaded += OnLoadedWireSegmentsOverview;
        Closed += OnMainWindowClosed;
    }

    private void OnMainWindowClosed(object? sender, EventArgs e)
    {
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
        AttachSegmentsStatusTracking();

        UpdateTopVisibleSegmentStatus();
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
            return;
        }

        statusText.Text = SegmentsListScrollMetrics.GetTopVisibleSegmentStatusText(_segmentsListBox, _segmentsScrollViewer)
                          ?? string.Empty;
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
