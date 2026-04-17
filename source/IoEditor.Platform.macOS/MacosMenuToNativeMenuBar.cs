using System;
using Avalonia.Controls;
using Avalonia.Input;

namespace IoEditor.Platform;

/// <summary>Rebuilds an existing <see cref="NativeMenu"/> from the in-window <see cref="Menu"/> (single XAML source).</summary>
/// <remarks>
/// macOS requires the same <see cref="NativeMenu"/> instance after the first <see cref="NativeMenu.SetMenu"/> call;
/// replacing with a new instance throws "The menu being updated does not match."
/// </remarks>
internal static class MacosMenuToNativeMenuBar
{
    public static void RebuildBar(NativeMenu bar, Menu menu)
    {
        bar.Items.Clear();
        foreach (var o in menu.Items)
        {
            if (o is MenuItem mi)
            {
                bar.Items.Add(CloneMenuItem(mi));
            }
        }
    }

    private static NativeMenuItemBase CloneMenuItem(MenuItem source)
    {
        if (HasSubmenuItems(source))
        {
            var sub = new NativeMenu();
            foreach (var o in source.Items)
            {
                switch (o)
                {
                    case MenuItem child:
                        sub.Items.Add(CloneMenuItem(child));
                        break;
                    case Separator:
                        sub.Items.Add(new NativeMenuItemSeparator());
                        break;
                }
            }

            return new NativeMenuItem
            {
                Header = StripAccessKeyText(source.Header),
                Menu = sub,
                IsEnabled = source.IsEnabled,
            };
        }

        return new NativeMenuItem
        {
            Header = StripAccessKeyText(source.Header),
            Command = source.Command,
            CommandParameter = source.CommandParameter,
            Gesture = MapCtrlToMetaForMenuBar(source.HotKey ?? (source.InputGesture as KeyGesture)),
            IsEnabled = source.IsEnabled,
        };
    }

    private static bool HasSubmenuItems(MenuItem source)
    {
        foreach (var o in source.Items)
        {
            if (o is MenuItem or Separator)
            {
                return true;
            }
        }

        return false;
    }

    private static string StripAccessKeyText(object? header)
    {
        if (header is not string s)
        {
            return header?.ToString() ?? string.Empty;
        }

        Span<char> buffer = stackalloc char[s.Length];
        var w = 0;
        foreach (var ch in s)
        {
            if (ch != '_')
            {
                buffer[w++] = ch;
            }
        }

        return new string(buffer[..w]);
    }

    private static KeyGesture? MapCtrlToMetaForMenuBar(KeyGesture? gesture)
    {
        if (gesture is null)
        {
            return null;
        }

        var m = gesture.KeyModifiers;
        if ((m & KeyModifiers.Control) == 0)
        {
            return gesture;
        }

        m = (m & ~KeyModifiers.Control) | KeyModifiers.Meta;
        return new KeyGesture(gesture.Key, m);
    }
}
