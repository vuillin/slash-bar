using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SlashBar;

public partial class ClipPanelWindow {

    private void PositionLeft() {
        var screen = System.Windows.Forms.Screen.PrimaryScreen
            ?? System.Windows.Forms.Screen.AllScreens[0];
        var area = screen.WorkingArea;

        var height = area.Height * 0.5;
        Left = area.Left;
        Top = area.Top + (area.Height - height) / 2;
        Height = height;
        Width = LeftMargin + PanelWidth + TabWidth;
    }

    private void FadeDockTab(bool show) {
        DockButton.IsHitTestVisible = show;

        var anim = new DoubleAnimation(
            DockButton.Opacity,
            show ? 1 : 0,
            TimeSpan.FromMilliseconds(200)) {
            EasingFunction = new QuadraticEase {
                EasingMode = show ? EasingMode.EaseOut : EasingMode.EaseIn
            }
        };

        DockButton.BeginAnimation(OpacityProperty, anim);
    }

    private void AnimateOpen() {
        if (_isAnimating)
            return;

        _isAnimating = true;
        _isDetached = false;
        ResetButton.Visibility = Visibility.Collapsed;
        DockButton.BeginAnimation(OpacityProperty, null);
        DockButton.Opacity = 1;
        DockButton.IsHitTestVisible = true;

        PositionLeft();
        SetChevronCollapsed(false);
        SubscribeHistory();
        RefreshHistory();

        SlideTransform.X = CollapsedX;
        Show();
        Activate();

        AnimateSlide(CollapsedX, 0, 260, EasingMode.EaseOut, () => {
            _isCollapsed = false;
            _isAnimating = false;
        });
    }

    private void AnimateClose() {
        if (_isAnimating || !IsVisible)
            return;

        _isAnimating = true;

        // détaché : disparaît sur place
        if (_isDetached) {
            var fade = new DoubleAnimation(Opacity, 0, TimeSpan.FromMilliseconds(160));
            fade.Completed += (_, _) => {
                BeginAnimation(OpacityProperty, null);
                Opacity = 1;
                Hide();
                _isDetached = false;
                _isCollapsed = false;
                _isAnimating = false;
                ResetButton.Visibility = Visibility.Collapsed;
                DockButton.BeginAnimation(OpacityProperty, null);
                DockButton.Opacity = 1;
                DockButton.IsHitTestVisible = true;
                SetChevronCollapsed(false);
            };
            BeginAnimation(OpacityProperty, fade);
            return;
        }

        var from = SlideTransform.X;
        var to = CollapsedX - TabWidth;

        AnimateSlide(from, to, 200, EasingMode.EaseIn, () => {
            SlideTransform.BeginAnimation(TranslateTransform.XProperty, null);
            SlideTransform.X = CollapsedX;
            Hide();
            _isCollapsed = false;
            _isAnimating = false;
            SetChevronCollapsed(false);
        });
    }

    private void AnimateCollapse() {
        if (_isAnimating || _isCollapsed || _isDetached || !IsVisible)
            return;

        _isAnimating = true;

        AnimateSlide(SlideTransform.X, CollapsedX, 220, EasingMode.EaseInOut, () => {
            _isCollapsed = true;
            _isAnimating = false;
            SetChevronCollapsed(true);
        });
    }

    private void AnimateExpand() {
        if (_isAnimating || !_isCollapsed)
            return;

        _isAnimating = true;
        Activate();

        AnimateSlide(SlideTransform.X, 0, 220, EasingMode.EaseInOut, () => {
            _isCollapsed = false;
            _isAnimating = false;
            SetChevronCollapsed(false);
        });
    }

    private void AnimateSlide(double from, double to, int ms, EasingMode mode, Action onDone) {
        var anim = new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(ms)) {
            EasingFunction = new QuadraticEase { EasingMode = mode }
        };
        anim.Completed += (_, _) => onDone();
        SlideTransform.BeginAnimation(TranslateTransform.XProperty, anim);
    }

    private void SetChevronCollapsed(bool collapsed) {
        Chevron.Data = Geometry.Parse(collapsed
            ? "M 1,0 L 6,6 L 1,12"
            : "M 6,0 L 1,6 L 6,12");
    }
}
