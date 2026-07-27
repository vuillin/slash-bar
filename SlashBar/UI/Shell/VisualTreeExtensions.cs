using System.Windows;
using System.Windows.Media;

namespace SlashBar.UI.Shell;

public static class VisualTreeExtensions {

    public static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject {
        while (current != null) {
            if (current is T match)
                return match;
            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }
}
