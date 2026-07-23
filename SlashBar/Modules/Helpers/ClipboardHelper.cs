using System.Windows;

namespace SlashBar.Modules;


/// <summary>
/// Copie du texte dans le presse-papiers
/// </summary>
public static class ClipboardHelper {

    public static void SetText(string text) {
        System.Windows.Clipboard.SetText(text);
    }
}