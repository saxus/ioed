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

        var header = new Grid
        {
            Background = Brushes.Wheat,
            Margin = new Thickness(0, 0, 6, 6)
        };
        header.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        header.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        header.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));

        var leftPanel = new StackPanel { Orientation = Orientation.Horizontal };
        var runIndex = new TextBlock { Foreground = Brushes.Maroon, FontWeight = FontWeight.Bold };
        runIndex.Bind(TextBlock.TextProperty, new Binding(nameof(IndexedStep.Index)) { StringFormat = "#{0}" });
        var runModel = new TextBlock { Foreground = Brushes.Black, Margin = new Thickness(4, 0, 0, 0) };
        runModel.Bind(TextBlock.TextProperty, new Binding(nameof(IndexedStep.Model)));
        var runCount = new TextBlock { Foreground = Brushes.Black, FontWeight = FontWeight.Bold, Margin = new Thickness(4, 0, 0, 0) };
        runCount.Bind(TextBlock.TextProperty, new Binding("Items.Count"));
        leftPanel.Children.Add(runIndex);
        leftPanel.Children.Add(runModel);
        leftPanel.Children.Add(runCount);
        Grid.SetColumn(leftPanel, 0);
        header.Children.Add(leftPanel);

        var rightPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 0, 4, 0)
        };
        Grid.SetColumn(rightPanel, 2);

        if (step.PageNumber.HasValue)
        {
            var runLocation = new TextBlock
            {
                Text = $"Page {step.PageNumber} Col {step.ColumnNumber}",
                Foreground = Brushes.DimGray
            };
            rightPanel.Children.Add(runLocation);
        }

        if (step.IsCallout)
        {
            var calloutText = step.CalloutParentStepIndex.HasValue
                ? $"[Callout #{step.CalloutParentStepIndex}]"
                : "[Callout]";
            var runCallout = new TextBlock
            {
                Text = calloutText,
                Foreground = Brushes.SteelBlue,
                Margin = new Thickness(6, 0, 0, 0)
            };
            rightPanel.Children.Add(runCallout);
        }

        header.Children.Add(rightPanel);
        root.Children.Add(header);

        var items = new ItemsControl
        {
            ItemsSource = step.Items,
            ItemTemplate = _itemTemplate,
            ItemsPanel = new FuncTemplate<Panel?>(() => new StackPanel { Orientation = Orientation.Vertical })
        };
        root.Children.Add(items);

        return root;
    }

    public bool Match(object? data) => data is IndexedStep;
}
