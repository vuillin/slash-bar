using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SlashBar;

public partial class ClipPanelWindow {

    private void EnterDetachedMode() {
        _isDetached = true;
        ResetButton.Visibility = Visibility.Visible;
        FadeDockTab(show: false);
    }

    private void ResetToDock() {
        if (_isAnimating)
            return;

        _isAnimating = true;
        _isDetached = false;
        _isCollapsed = false;
        SetChevronCollapsed(false);

        var screen = System.Windows.Forms.Screen.PrimaryScreen
            ?? System.Windows.Forms.Screen.AllScreens[0];
        var area = screen.WorkingArea;
        var height = area.Height * 0.5;
        var targetLeft = (double)area.Left;
        var targetTop = area.Top + (area.Height - height) / 2;

        // stop toute anim de slide en cours
        SlideTransform.BeginAnimation(TranslateTransform.XProperty, null);
        SlideTransform.X = 0;

        var ease = new QuadraticEase { EasingMode = EasingMode.EaseInOut };
        var animL = new DoubleAnimation(Left, targetLeft, TimeSpan.FromMilliseconds(280)) { EasingFunction = ease };
        var animT = new DoubleAnimation(Top, targetTop, TimeSpan.FromMilliseconds(280)) { EasingFunction = ease };
        var animH = new DoubleAnimation(Height, height, TimeSpan.FromMilliseconds(280)) { EasingFunction = ease };

        animL.Completed += (_, _) => {
            Left = targetLeft;
            Top = targetTop;
            Height = height;
            Width = LeftMargin + PanelWidth + TabWidth;
            BeginAnimation(LeftProperty, null);
            BeginAnimation(TopProperty, null);
            BeginAnimation(HeightProperty, null);

            ResetButton.Visibility = Visibility.Collapsed;
            FadeDockTab(show: true);
            _isAnimating = false;
        };

        BeginAnimation(LeftProperty, animL);
        BeginAnimation(TopProperty, animT);
        BeginAnimation(HeightProperty, animH);
    }
}
