using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media;
using IoEditor.Models.Merging;
using IoEditor.Models.Studio;

namespace IoEditor.Desktop.Views;

/// <summary>Renders a <see cref="StepPair"/> as a two-column row for side-by-side step comparison.</summary>
internal sealed class StepPairDataTemplate : IDataTemplate
{
    private static readonly IndexedStepDataTemplate StepTemplate = new();

    public Control? Build(object? data)
    {
        if (data is not StepPair pair)
            return null;

        var outer = new Border
        {
            BorderBrush = Brushes.Silver,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Margin = new Thickness(0, 0, 0, 0)
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Pixel)));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        var leftCell = BuildStepCell(pair.Reference);
        Grid.SetColumn(leftCell, 0);
        grid.Children.Add(leftCell);

        var separator = new Border
        {
            Background = Brushes.Silver,
            Width = 1,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        Grid.SetColumn(separator, 1);
        grid.Children.Add(separator);

        var rightCell = BuildStepCell(pair.Target);
        Grid.SetColumn(rightCell, 2);
        grid.Children.Add(rightCell);

        outer.Child = grid;
        return outer;
    }

    public bool Match(object? data) => data is StepPair;

    private static Control BuildStepCell(IndexedStep? step)
    {
        if (step is null)
        {
            return new Border
            {
                Background = Brushes.Transparent,
                MinHeight = 20
            };
        }

        var built = StepTemplate.Build(step);
        if (built is null)
            return new Border { MinHeight = 20 };

        built.DataContext = step;
        built.Margin = new Thickness(4, 4, 4, 4);
        return built;
    }
}
