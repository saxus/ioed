using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using IoEditor.Desktop.Converters;
using IoEditor.Models.Comparison;
using IoEditor.Models.Model;
using IoEditor.Models.Studio;

namespace IoEditor.Desktop.Views;

/// <summary>Template for items inside an <see cref="IndexedStep"/> (part / custom / submodel).</summary>
internal sealed class IndexedStepItemDataTemplate : IDataTemplate
{
    private static readonly ByteArrayToImageConverter ByteToImage = new();

    public Control? Build(object? data)
    {
        return data switch
        {
            IndexedStepPart p => BuildPart(p),
            IndexedStepCustomPart c => BuildCustom(c),
            IndexedStepSubmodel s => BuildSubmodel(s),
            _ => new TextBlock { Text = $"? {data?.GetType().Name}" }
        };
    }

    public bool Match(object? data) => data is IndexedStepItem;

    private static Border BuildPart(IndexedStepPart item)
    {
        var border = new Border
        {
            Margin = new Thickness(0, 0, 12, 0),
            BorderBrush = Brushes.Silver,
            BorderThickness = new Thickness(1),
            Height = 100,
            Width = 80,
            DataContext = item
        };
        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        var img = new Image { Height = 64, Width = 64 };
        img.Bind(Image.SourceProperty, new Binding(nameof(IndexedStepPart.Image)) { Converter = ByteToImage });
        Grid.SetRow(img, 0);
        grid.Children.Add(img);

        var tb1 = new TextBlock();
        tb1.Bind(TextBlock.TextProperty, new Binding(nameof(IndexedStepPart.Color)));
        Grid.SetRow(tb1, 1);
        grid.Children.Add(tb1);

        var tb2 = new TextBlock { Background = Brushes.WhiteSmoke };
        tb2.Bind(TextBlock.TextProperty, new Binding("Part"));
        Grid.SetRow(tb2, 2);
        grid.Children.Add(tb2);

        border.Child = grid;
        return border;
    }

    private static Border BuildCustom(IndexedStepCustomPart item)
    {
        var border = new Border
        {
            Margin = new Thickness(0, 0, 12, 0),
            BorderBrush = Brushes.Silver,
            BorderThickness = new Thickness(1),
            Height = 100,
            Width = 80,
            DataContext = item
        };
        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        var tb = new TextBlock { Background = Brushes.Yellow, FontWeight = FontWeight.Bold };
        tb.Bind(TextBlock.TextProperty, new Binding("Part.PartName"));
        Grid.SetRow(tb, 1);
        grid.Children.Add(tb);
        border.Child = grid;
        return border;
    }

    private static Border BuildSubmodel(IndexedStepSubmodel item)
    {
        var border = new Border
        {
            Margin = new Thickness(0, 0, 12, 0),
            BorderBrush = Brushes.Silver,
            BorderThickness = new Thickness(1),
            Height = 100,
            Width = 80,
            DataContext = item
        };
        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        var tb = new TextBlock { Background = Brushes.Wheat, FontWeight = FontWeight.Bold };
        tb.Bind(TextBlock.TextProperty, new Binding(nameof(IndexedStepSubmodel.ModelName)));
        Grid.SetRow(tb, 1);
        grid.Children.Add(tb);
        border.Child = grid;
        return border;
    }
}
