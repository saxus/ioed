using Avalonia.Markup.Xaml;

namespace IoEditor.Desktop.Views;

public partial class LoaderWindow : Avalonia.Controls.Window
{
    public LoaderWindow()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
