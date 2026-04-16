using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;

namespace IoEditor.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// TabControl lazily materializes tab content; named fields can be null at <see cref="Opened"/>.
    /// Wire this handler to each XML <see cref="TextEditor"/> in XAML <c>Loaded</c>.
    /// </summary>
    public void XmlEditor_Loaded(object? sender, RoutedEventArgs e)
    {
        if (sender is not TextEditor editor)
        {
            return;
        }

        var xmlDef = HighlightingManager.Instance.GetDefinitionByExtension(".xml");
        if (xmlDef != null)
        {
            editor.SyntaxHighlighting = xmlDef;
        }
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
