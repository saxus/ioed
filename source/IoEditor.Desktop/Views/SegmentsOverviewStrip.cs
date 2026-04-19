using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using IoEditor.Desktop.Converters;
using IoEditor.Models.Comparison;
using IoEditor.Models.Merging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace IoEditor.Desktop.Views;

/// <summary>
/// IDE-style minimap beside the segments list: colored bands per merge segment status and a viewport thumb.
/// </summary>
internal sealed class SegmentsOverviewStrip : Control
{
    public static readonly StyledProperty<ListBox?> TargetListBoxProperty =
        AvaloniaProperty.Register<SegmentsOverviewStrip, ListBox?>(nameof(TargetListBox));

    public static readonly StyledProperty<ScrollViewer?> ScrollViewerProperty =
        AvaloniaProperty.Register<SegmentsOverviewStrip, ScrollViewer?>(nameof(ScrollViewer));

    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<SegmentsOverviewStrip, IEnumerable?>(nameof(ItemsSource));

    private ScrollViewer? _scrollViewer;
    private ListBox? _listBox;
    private bool _dragging;
    private IBrush? _trackBrush;
    private IBrush? _thumbFillBrush;
    private IBrush? _thumbBorderBrush;

    static SegmentsOverviewStrip()
    {
        ClipToBoundsProperty.OverrideDefaultValue(typeof(SegmentsOverviewStrip), true);
        FocusableProperty.OverrideDefaultValue(typeof(SegmentsOverviewStrip), false);
    }

    public SegmentsOverviewStrip()
    {
        Width = 12;
        MinWidth = 12;
    }

    public ListBox? TargetListBox
    {
        get => GetValue(TargetListBoxProperty);
        set => SetValue(TargetListBoxProperty, value);
    }

    public ScrollViewer? ScrollViewer
    {
        get => GetValue(ScrollViewerProperty);
        set => SetValue(ScrollViewerProperty, value);
    }

    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ResolveThemeBrushes();
        Subscribe();
        InvalidateVisual();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Unsubscribe();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ScrollViewerProperty)
        {
            UnsubscribeScrollViewer();
            _scrollViewer = change.GetNewValue<ScrollViewer?>();
            SubscribeScrollViewer();
            InvalidateVisual();
        }

        if (change.Property == TargetListBoxProperty)
        {
            HookListBox(change.GetNewValue<ListBox?>());
            InvalidateVisual();
        }

        if (change.Property == ItemsSourceProperty)
        {
            InvalidateVisual();
        }
    }

    private void ResolveThemeBrushes()
    {
        _trackBrush = TryBrush("IoEditorSegmentsOverviewTrackBackground", Color.FromRgb(0x1e, 0x1e, 0x22));
        _thumbFillBrush = TryBrush("IoEditorSegmentsOverviewThumbFill", Color.FromArgb(0xaa, 0xff, 0xff, 0xff));
        _thumbBorderBrush = TryBrush("IoEditorSegmentsOverviewThumbBorder", Colors.White);
    }

    private IBrush TryBrush(string key, Color fallback)
    {
        if (this.TryFindResource(key, out var r) && r is IBrush b)
        {
            return b;
        }

        return new SolidColorBrush(fallback);
    }

    private void Subscribe()
    {
        SubscribeScrollViewer();
        HookListBox(TargetListBox);
    }

    private void Unsubscribe()
    {
        UnsubscribeScrollViewer();
        HookListBox(null);
    }

    private void HookListBox(ListBox? lb)
    {
        if (_listBox == lb)
        {
            return;
        }

        if (_listBox is not null)
        {
            _listBox.LayoutUpdated -= OnListLayoutUpdated;
        }

        _listBox = lb;
        if (_listBox is not null)
        {
            _listBox.LayoutUpdated += OnListLayoutUpdated;
        }
    }

    private void SubscribeScrollViewer()
    {
        _scrollViewer ??= ScrollViewer;

        if (_scrollViewer is not null)
        {
            _scrollViewer.ScrollChanged += OnScrollChanged;
            _scrollViewer.LayoutUpdated += OnScrollLayoutUpdated;
        }
    }

    private void UnsubscribeScrollViewer()
    {
        if (_scrollViewer is null)
        {
            return;
        }

        _scrollViewer.ScrollChanged -= OnScrollChanged;
        _scrollViewer.LayoutUpdated -= OnScrollLayoutUpdated;
    }

    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        InvalidateVisual();
    }

    private void OnScrollLayoutUpdated(object? sender, EventArgs e)
    {
        InvalidateVisual();
    }

    private void OnListLayoutUpdated(object? sender, EventArgs e)
    {
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var h = Bounds.Height;
        var w = Bounds.Width;
        if (h <= 0 || w <= 0)
        {
            return;
        }

        var track = new Rect(0, 0, w, h);
        context.FillRectangle(_trackBrush ?? Brushes.Gray, track);

        var segments = GetSegments();
        var count = segments.Count;
        if (count == 0 || _listBox is null)
        {
            return;
        }

        var heights = MeasureSegmentHeights(count);
        var sum = heights.Sum();
        if (sum <= 0)
        {
            var eh = h / count;
            DrawMergedEqualityBands(context, segments, count, w, _ => eh);
        }
        else
        {
            DrawMergedEqualityBands(context, segments, count, w, i => h * heights[i] / sum);
        }

        DrawViewportThumb(context, w, h);
    }

    /// <summary>Merges consecutive segments with the same equality into one <see cref="DrawingContext.FillRectangle"/> call.</summary>
    private void DrawMergedEqualityBands(
        DrawingContext context,
        IReadOnlyList<MergedSegment> segments,
        int count,
        double w,
        Func<int, double> bandHeightAtIndex)
    {
        var acc = 0.0;
        var i = 0;
        while (i < count)
        {
            var equality = segments[i].Equality;
            var y0 = acc;
            var piece = bandHeightAtIndex(i);
            var runH = piece;
            acc += piece;
            i++;

            while (i < count && segments[i].Equality == equality)
            {
                piece = bandHeightAtIndex(i);
                runH += piece;
                acc += piece;
                i++;
            }

            DrawSegmentBand(context, equality, y0, runH, w);
        }
    }

    private void DrawSegmentBand(DrawingContext context, InstructionSegmentEquality equality, double y, double bandH, double w)
    {
        if (bandH <= 0)
        {
            return;
        }

        var brush = InstructionSegmentEqualityBrushes.For(equality);
        context.FillRectangle(brush, new Rect(0, y, w, bandH));
    }

    private void DrawViewportThumb(DrawingContext context, double w, double h)
    {
        var sv = _scrollViewer ?? ScrollViewer;
        if (sv is null)
        {
            return;
        }

        var extent = sv.Extent.Height;
        var viewport = sv.Viewport.Height;
        if (extent <= 0 || viewport <= 0)
        {
            return;
        }

        var offset = sv.Offset.Y;
        var scrollable = Math.Max(0, extent - viewport);
        var thumbH = extent > viewport ? Math.Max(4, h * viewport / extent) : h;
        var thumbY = scrollable > 0 ? offset / scrollable * (h - thumbH) : 0;
        thumbY = Math.Clamp(thumbY, 0, Math.Max(0, h - thumbH));

        var thumbRect = new Rect(0, thumbY, w, thumbH);
        context.FillRectangle(_thumbFillBrush ?? Brushes.White, thumbRect);
        context.DrawRectangle(new Pen(_thumbBorderBrush ?? Brushes.White, 2), thumbRect);
    }

    private List<MergedSegment> GetSegments()
    {
        if (ItemsSource is IEnumerable<MergedSegment> typed)
        {
            return typed.ToList();
        }

        var list = new List<MergedSegment>();
        if (ItemsSource is IEnumerable raw)
        {
            foreach (var item in raw)
            {
                if (item is MergedSegment m)
                {
                    list.Add(m);
                }
            }
        }

        return list;
    }

    private double[] MeasureSegmentHeights(int count)
    {
        var heights = new double[count];
        if (_listBox is null)
        {
            return heights;
        }

        for (var i = 0; i < count; i++)
        {
            var container = _listBox.ContainerFromIndex(i);
            if (container is Control c)
            {
                heights[i] = c.Bounds.Height;
            }
        }

        return heights;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        e.Pointer.Capture(this);
        _dragging = true;
        ScrollForLocalY(e.GetPosition(this).Y);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (!_dragging || e.Pointer.Captured != this)
        {
            return;
        }

        ScrollForLocalY(e.GetPosition(this).Y);
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.Pointer.Captured == this)
        {
            e.Pointer.Capture(null);
        }

        _dragging = false;
    }

    private void ScrollForLocalY(double localY)
    {
        var sv = _scrollViewer ?? ScrollViewer;
        if (sv is null)
        {
            return;
        }

        var h = Bounds.Height;
        if (h <= 0)
        {
            return;
        }

        var extent = sv.Extent.Height;
        var viewport = sv.Viewport.Height;
        var scrollable = Math.Max(0, extent - viewport);
        var ratio = Math.Clamp(localY / h, 0, 1);
        var newY = ratio * scrollable;

        sv.Offset = new Vector(sv.Offset.X, newY);
        InvalidateVisual();
    }
}
