using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.VisualTree;
using IoEditor.Models.Comparison;
using IoEditor.Models.Merging;

namespace IoEditor.Desktop.Views;

internal static class SegmentsListScrollMetrics
{
    /// <summary>Same vertical scroll clamp as <see cref="SegmentsOverviewStrip"/> / list thumb (logical offset).</summary>
    public static void SetVerticalScrollOffset(ScrollViewer scrollViewer, double offsetY)
    {
        var extent = scrollViewer.Extent.Height;
        var viewport = scrollViewer.Viewport.Height;
        var scrollable = Math.Max(0, extent - viewport);
        scrollViewer.Offset = new Vector(scrollViewer.Offset.X, Math.Clamp(offsetY, 0, scrollable));
    }

    /// <summary>List index of the first segment row whose bottom extends below the viewport top (scroll offset).</summary>
    public static int GetTopVisibleSegmentListIndex(ListBox listBox, ScrollViewer scrollViewer)
    {
        var count = listBox.Items.Count;
        if (count == 0)
        {
            return -1;
        }

        var presenter = listBox.GetVisualDescendants().OfType<ItemsPresenter>().FirstOrDefault();
        var offsetY = scrollViewer.Offset.Y;

        if (presenter is not null)
        {
            for (var i = 0; i < count; i++)
            {
                if (listBox.ContainerFromIndex(i) is not Visual v)
                {
                    continue;
                }

                var pt = v.TranslatePoint(default, presenter);
                var yTop = pt?.Y ?? 0;
                var h = v.Bounds.Height;
                if (yTop + h > offsetY + 0.5)
                {
                    return i;
                }
            }

            return count - 1;
        }

        return FallbackByStackedHeight(listBox, scrollViewer, count);
    }

    private static int FallbackByStackedHeight(ListBox listBox, ScrollViewer scrollViewer, int count)
    {
        var offsetY = scrollViewer.Offset.Y;
        double y = 0;

        for (var i = 0; i < count; i++)
        {
            if (listBox.ContainerFromIndex(i) is Control c)
            {
                var h = c.Bounds.Height;
                if (y + h > offsetY + 0.5)
                {
                    return i;
                }

                y += h;
            }
        }

        return count - 1;
    }

    /// <summary>Status text for the top-visible segment, or null if none.</summary>
    public static string? GetTopVisibleSegmentStatusText(ListBox listBox, ScrollViewer scrollViewer)
    {
        var ix = GetTopVisibleSegmentListIndex(listBox, scrollViewer);
        if (ix < 0 || ix >= listBox.Items.Count)
        {
            return null;
        }

        return listBox.Items[ix] is MergedSegment seg ? $"Segment {seg.SegmentIndex}" : null;
    }

    /// <summary>Anchor for difference navigation: explicit selection, otherwise top-visible row.</summary>
    public static int GetNavigationAnchorIndex(ListBox listBox, ScrollViewer scrollViewer)
    {
        if (listBox.SelectedIndex >= 0)
        {
            return listBox.SelectedIndex;
        }

        return GetTopVisibleSegmentListIndex(listBox, scrollViewer);
    }

    private static bool IsDifferenceSegment(MergedSegment segment)
        => segment.Equality != InstructionSegmentEquality.Equivalent;

    public static int? FindPreviousDifferenceListIndex(ListBox listBox, int anchorIndex)
    {
        for (var i = anchorIndex - 1; i >= 0; i--)
        {
            if (listBox.Items[i] is MergedSegment m && IsDifferenceSegment(m))
            {
                return i;
            }
        }

        return null;
    }

    public static int? FindNextDifferenceListIndex(ListBox listBox, int anchorIndex)
    {
        var count = listBox.Items.Count;
        for (var i = anchorIndex + 1; i < count; i++)
        {
            if (listBox.Items[i] is MergedSegment m && IsDifferenceSegment(m))
            {
                return i;
            }
        }

        return null;
    }

    /// <summary>Scrolls so the segment row aligns to the top of the viewport (same coordinate model as visibility metrics).</summary>
    public static bool TryScrollSegmentIndexToTop(ListBox listBox, ScrollViewer scrollViewer, int index)
    {
        var count = listBox.Items.Count;
        if ((uint)index >= (uint)count)
        {
            return false;
        }

        var presenter = listBox.GetVisualDescendants().OfType<ItemsPresenter>().FirstOrDefault();
        double yTop;
        if (presenter is not null && listBox.ContainerFromIndex(index) is Visual v)
        {
            yTop = v.TranslatePoint(default, presenter)?.Y ?? 0;
        }
        else
        {
            yTop = 0;
            for (var i = 0; i < index; i++)
            {
                if (listBox.ContainerFromIndex(i) is Control c)
                {
                    yTop += c.Bounds.Height;
                }
            }
        }

        SetVerticalScrollOffset(scrollViewer, yTop);
        return true;
    }
}
