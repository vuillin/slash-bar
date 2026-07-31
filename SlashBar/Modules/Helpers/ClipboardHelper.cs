using System.Windows;

namespace SlashBar.Modules;


/// <summary>
/// Copies text to the clipboard.
/// </summary>
public static class ClipboardHelper {

    public static void SetText(string text) {
        System.Windows.Clipboard.SetText(text);
    }
}