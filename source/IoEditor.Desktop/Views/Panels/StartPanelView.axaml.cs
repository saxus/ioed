using Avalonia.Markup.Xaml;

namespace IoEditor.Desktop.Views.Panels;

public partial class StartPanelView : Avalonia.Controls.UserControl
{
    public StartPanelView()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
