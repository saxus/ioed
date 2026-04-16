using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using IoEditor.Models.Studio;

namespace IoEditor.Desktop.Views;

/// <summary>Visual for one <see cref="IndexedStep"/> inside a merged segment column.</summary>
internal sealed class IndexedStepDataTemplate : IDataTemplate
{
    private readonly IndexedStepItemDataTemplate _itemTemplate = new();

    public Control? Build(object? data)
    {
        if (data is not IndexedStep step)
        {
            return null;
        }

        var root = new StackPanel();

        var header = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Background = Brushes.Wheat,
            Margin = new Thickness(0, 0, 6, 6)
        };
        var runIndex = new TextBlock { Foreground = Brushes.Maroon, FontWeight = FontWeight.Bold };
        runIndex.Bind(TextBlock.TextProperty, new Binding(nameof(IndexedStep.Index)) { StringFormat = "#{0}" });
        var runModel = new TextBlock { Foreground = Brushes.Black, Margin = new Thickness(4, 0, 0, 0) };
        runModel.Bind(TextBlock.TextProperty, new Binding(nameof(IndexedStep.Model)));
        var runCount = new TextBlock { Foreground = Brushes.Black, FontWeight = FontWeight.Bold, Margin = new Thickness(4, 0, 0, 0) };
        runCount.Bind(TextBlock.TextProperty, new Binding("Items.Count"));
        header.Children.Add(runIndex);
        header.Children.Add(runModel);
        header.Children.Add(runCount);
        root.Children.Add(header);

        var items = new ItemsControl
        {
            ItemsSource = step.Items,
            ItemTemplate = _itemTemplate,
            ItemsPanel = new FuncTemplate<Panel?>(() => new WrapPanel { Orientation = Orientation.Horizontal })
        };
        root.Children.Add(items);

        return root;
    }

    public bool Match(object? data) => data is IndexedStep;
}
