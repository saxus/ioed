using Avalonia.Markup.Xaml;

namespace IoEditor.Desktop.Views;

public partial class SettingsWindow : Avalonia.Controls.Window
{
    public SettingsWindow()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
