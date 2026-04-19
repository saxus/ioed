using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.VisualTree;
using IoEditor.Models.Merging;

namespace IoEditor.Desktop.Views;

internal static class SegmentsListScrollMetrics
{
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
}
