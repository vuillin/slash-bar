using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using SlashBar.Modules.Shortcuts;

namespace SlashBar.UI.Shell;

public sealed class SidePanelDockShelfItem {
    public required string Key { get; init; }
    public required string Label { get; init; }
    public ImageSource? Icon { get; init; }
    public System.Windows.Media.Brush Background { get; init; } = System.Windows.Media.Brushes.Gray;
    public required DockedSidePanelWindow Panel { get; init; }
}

/// <summary>
/// Bottom-left shelf of minimized side panels.
/// </summary>
public static class SidePanelDockShelf {

    private static readonly ObservableCollection<SidePanelDockShelfItem> Items = new();
    private static SidePanelDockShelfWindow? _window;

    public static bool Contains(DockedSidePanelWindow panel) =>
        Items.Any(i => ReferenceEquals(i.Panel, panel));

    public static bool Minimize(DockedSidePanelWindow panel) {
        if (Contains(panel))
            return false;

        if (!ShortcutCatalog.TryGetDockVisual(panel.DockShelfKey, out var icon, out var background, out var label))
            return false;

        Items.Add(new SidePanelDockShelfItem {
            Key = panel.DockShelfKey,
            Label = label,
            Icon = icon,
            Background = background,
            Panel = panel,
        });

        EnsureWindow().Refresh(Items);
        return true;
    }

    public static void Restore(DockedSidePanelWindow panel) {
        Remove(panel);
        panel.RestoreFromShelf();
    }

    public static void Remove(DockedSidePanelWindow panel) {
        for (var i = Items.Count - 1; i >= 0; i--) {
            if (!ReferenceEquals(Items[i].Panel, panel))
                continue;
            Items.RemoveAt(i);
        }

        if (_window == null)
            return;

        _window.Refresh(Items);
    }

    private static SidePanelDockShelfWindow EnsureWindow() {
        if (_window != null)
            return _window;

        _window = new SidePanelDockShelfWindow();
        return _window;
    }
}
