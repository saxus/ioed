using Avalonia.Controls;
using Avalonia.Layout;

namespace IoEditor.Platform;

internal static class SimpleMessageBox
{
    public static async Task ShowAsync(Window? owner, string message, string title)
    {
        var panel = new StackPanel { Margin = new Avalonia.Thickness(16), Spacing = 16 };
        panel.Children.Add(new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap });
        var ok = new Button { Content = "OK", MinWidth = 80 };
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        buttons.Children.Add(ok);
        panel.Children.Add(buttons);

        var dlg = new Window
        {
            Title = title,
            Width = 440,
            SizeToContent = SizeToContent.Height,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            Content = panel
        };
        ok.Click += (_, _) => dlg.Close();
        await ShowWindowAsync(owner, dlg);
    }

    public static async Task<bool> ConfirmAsync(Window? owner, string message, string title)
    {
        var holder = new ResultHolder();
        var panel = new StackPanel { Margin = new Avalonia.Thickness(16), Spacing = 16 };
        panel.Children.Add(new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap });
        var yes = new Button { Content = "Yes", MinWidth = 80 };
        var no = new Button { Content = "No", MinWidth = 80 };
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, HorizontalAlignment = HorizontalAlignment.Right };
        buttons.Children.Add(yes);
        buttons.Children.Add(no);
        panel.Children.Add(buttons);

        var dlg = new Window
        {
            Title = title,
            Width = 440,
            SizeToContent = SizeToContent.Height,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            Content = panel
        };
        yes.Click += (_, _) => { holder.Accepted = true; dlg.Close(); };
        no.Click += (_, _) => { holder.Accepted = false; dlg.Close(); };
        await ShowWindowAsync(owner, dlg);
        return holder.Accepted;
    }

    private sealed class ResultHolder
    {
        public bool Accepted { get; set; }
    }

    private static async Task ShowWindowAsync(Window? owner, Window dlg)
    {
        if (owner != null)
        {
            await dlg.ShowDialog(owner);
        }
        else
        {
            var tcs = new TaskCompletionSource();
            dlg.Closed += (_, _) => tcs.TrySetResult();
            dlg.Show();
            await tcs.Task;
        }
    }
}
