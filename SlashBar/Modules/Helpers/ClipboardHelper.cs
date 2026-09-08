using System.Windows;
using SlashBar.Modules.Clipboard;

namespace SlashBar.Modules;


/// <summary>
/// Copies text to the clipboard.
/// </summary>
public static class ClipboardHelper {

    public static void SetText(string text) {
        ClipboardHistory.Watcher.IgnoreNext();
        System.Windows.Clipboard.SetText(text);
    }
}