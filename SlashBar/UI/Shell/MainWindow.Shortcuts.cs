using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using SlashBar.Modules.Shortcuts;

namespace SlashBar;

public partial class MainWindow {

    private void InitShortcuts() {
        LeftShortcutSlots.ItemsSource = ShortcutCatalog.CreateLeftRail();
        RightShortcutSlots.ItemsSource = ShortcutCatalog.CreateRightRail();
    }

    private void ShortcutSlot_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) {
        if (!TryGetShortcutLift(sender, out var lift))
            return;

        var anim = new DoubleAnimation(0, -7, TimeSpan.FromMilliseconds(150)) {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        lift.BeginAnimation(TranslateTransform.YProperty, anim);
    }

    private void ShortcutSlot_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) {
        if (!TryGetShortcutLift(sender, out var lift))
            return;

        var anim = new DoubleAnimation(lift.Y, 0, TimeSpan.FromMilliseconds(150)) {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };
        lift.BeginAnimation(TranslateTransform.YProperty, anim);
    }

    private void ShortcutSlot_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
        if (sender is not FrameworkElement { DataContext: ShortcutSlot { Prefix: { Length: > 0 } prefix } })
            return;

        e.Handled = true;
        ExecuteShortcut(prefix);
    }

    private static bool TryGetShortcutLift(object sender, out TranslateTransform lift) {
        lift = null!;
        if (sender is not FrameworkElement { DataContext: ShortcutSlot { Prefix: not null } } fe)
            return false;

        if (fe.RenderTransform is not TransformGroup { Children.Count: >= 2 } group)
            return false;

        if (group.Children[1] is not TranslateTransform t)
            return false;

        lift = t;
        return true;
    }
}
