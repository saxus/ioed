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
    private static readonly RgbHexToBrushConverter RgbToBrush = new();

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

    private static Control BuildPart(IndexedStepPart item)
    {
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 4),
            DataContext = item
        };

        var imgBorder = new Border
        {
            Background = Brushes.White,
            BorderBrush = Brushes.LightGray,
            BorderThickness = new Thickness(1),
            Width = 44,
            Height = 44,
            Margin = new Thickness(0, 0, 8, 0),
            VerticalAlignment = VerticalAlignment.Top
        };
        var img = new Image { Width = 40, Height = 40, Stretch = Stretch.Uniform };
        img.Bind(Image.SourceProperty, new Binding(nameof(IndexedStepPart.Image)) { Converter = ByteToImage });
        imgBorder.Child = img;
        row.Children.Add(imgBorder);

        var info = new StackPanel { VerticalAlignment = VerticalAlignment.Center };

        var nameText = new TextBlock { FontWeight = FontWeight.Bold, TextWrapping = TextWrapping.Wrap, MaxWidth = 180 };
        nameText.Bind(TextBlock.TextProperty, new Binding("Part.Description"));
        info.Children.Add(nameText);

        var numText = new TextBlock { Foreground = Brushes.Gray };
        numText.Bind(TextBlock.TextProperty, new Binding("Part.BLItemNo"));
        info.Children.Add(numText);

        var colorRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 0) };
        var colorSquare = new Border
        {
            Width = 10,
            Height = 10,
            Margin = new Thickness(0, 1, 4, 0),
            VerticalAlignment = VerticalAlignment.Center,
            BorderBrush = Brushes.DarkGray,
            BorderThickness = new Thickness(1)
        };
        colorSquare.Bind(Border.BackgroundProperty, new Binding("Color.RGBValue") { Converter = RgbToBrush });
        colorRow.Children.Add(colorSquare);

        var colorText = new TextBlock { Foreground = Brushes.DimGray };
        colorText.Bind(TextBlock.TextProperty, new Binding("Color.StudioColorName"));
        colorRow.Children.Add(colorText);

        info.Children.Add(colorRow);
        row.Children.Add(info);

        return row;
    }

    private static Control BuildCustom(IndexedStepCustomPart item)
    {
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 4),
            DataContext = item
        };

        var imgBorder = new Border
        {
            Background = Brushes.White,
            BorderBrush = Brushes.LightGray,
            BorderThickness = new Thickness(1),
            Width = 44,
            Height = 44,
            Margin = new Thickness(0, 0, 8, 0),
            VerticalAlignment = VerticalAlignment.Top
        };
        imgBorder.Child = new TextBlock
        {
            Text = "?",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.Gray
        };
        row.Children.Add(imgBorder);

        var info = new StackPanel { VerticalAlignment = VerticalAlignment.Center };

        var nameText = new TextBlock { FontWeight = FontWeight.Bold, TextWrapping = TextWrapping.Wrap, MaxWidth = 180 };
        nameText.Bind(TextBlock.TextProperty, new Binding("Part.Description"));
        info.Children.Add(nameText);

        var colorRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 0) };
        var colorSquare = new Border
        {
            Width = 10,
            Height = 10,
            Margin = new Thickness(0, 1, 4, 0),
            VerticalAlignment = VerticalAlignment.Center,
            BorderBrush = Brushes.DarkGray,
            BorderThickness = new Thickness(1)
        };
        colorSquare.Bind(Border.BackgroundProperty, new Binding("Color.RGBValue") { Converter = RgbToBrush });
        colorRow.Children.Add(colorSquare);

        var colorText = new TextBlock { Foreground = Brushes.DimGray };
        colorText.Bind(TextBlock.TextProperty, new Binding("Color.StudioColorName"));
        colorRow.Children.Add(colorText);

        info.Children.Add(colorRow);
        row.Children.Add(info);

        return row;
    }

    private static Control BuildSubmodel(IndexedStepSubmodel item)
    {
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 4),
            DataContext = item
        };

        var imgBorder = new Border
        {
            Background = Brushes.White,
            BorderBrush = Brushes.LightGray,
            BorderThickness = new Thickness(1),
            Width = 44,
            Height = 44,
            Margin = new Thickness(0, 0, 8, 0),
            VerticalAlignment = VerticalAlignment.Top
        };
        imgBorder.Child = new TextBlock
        {
            Text = "S",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.SlateBlue
        };
        row.Children.Add(imgBorder);

        var nameText = new TextBlock
        {
            FontWeight = FontWeight.Bold,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 180,
            VerticalAlignment = VerticalAlignment.Center
        };
        nameText.Bind(TextBlock.TextProperty, new Binding(nameof(IndexedStepSubmodel.ModelName)));
        row.Children.Add(nameText);

        return row;
    }
}
