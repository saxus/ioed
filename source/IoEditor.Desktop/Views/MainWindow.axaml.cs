using Avalonia.Markup.Xaml;
using IoEditor.Platform;

namespace IoEditor.Desktop.Views;

public partial class MainWindow : Avalonia.Controls.Window
{
    public MainWindow()
        : this(NullMainWindowMenuIntegration.Instance)
    {
    }

    public MainWindow(IMainWindowMenuIntegration mainWindowMenuIntegration)
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
