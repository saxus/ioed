using Avalonia.Markup.Xaml;

namespace IoEditor.Desktop.Views.Panels;

public partial class SettingsPanelView : Avalonia.Controls.UserControl
{
    public SettingsPanelView()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
