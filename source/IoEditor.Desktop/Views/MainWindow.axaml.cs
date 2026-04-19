using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using IoEditor.Platform;

namespace IoEditor.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow()
        : this(NullMainWindowMenuIntegration.Instance)
    {
    }

    public MainWindow(IMainWindowMenuIntegration mainWindowMenuIntegration)
    {
        InitializeComponent();
        if (this.FindControl<Menu>("MainMenu") is { } menu)
        {
            mainWindowMenuIntegration.AttachMainMenu(this, menu);
        }
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
