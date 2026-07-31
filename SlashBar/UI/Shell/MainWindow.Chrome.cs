using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SlashBar;

public partial class MainWindow {

    private bool _ignoreDeactivate; // otherwise deactivate animation closes the bar
    private bool _isAnimating;
    private bool _isOpen;

    private void PositionAtBottom() {
        var work = SystemParameters.WorkArea;
        RootBorder.Width = Math.Min(520, work.Width * 0.38);

        if (ActualHeight <= 0 || ActualWidth <= 0)
            UpdateLayout();

        const double bottomMargin = 28;
        Left = work.Left + (work.Width - ActualWidth) / 2;
        Top = work.Bottom - Math.Max(ActualHeight, 48) - bottomMargin;
    }

    private void ToggleBar() {
        if (_isAnimating)
            return;

        if (_isOpen || IsVisible) {
            AnimateClose();
            return;
        }

        AnimateOpen();
    }

    private void AnimateOpen() {
        if (_isAnimating)
            return;

        _isAnimating = true;
        PositionAtBottom();
        ResetHistoryNavigation();
        SearchBox.Text = "";
        _completionIndex = 0;
        UpdateSuggestions();

        // Opaque window, invisible chrome → no layered-window flash
        BeginAnimation(OpacityProperty, null);
        Opacity = 1;
        ResetChromeVisuals();

        _ignoreDeactivate = true;
        Show();
        Activate();
        SearchBox.Focus();

        var easeOut = new QuadraticEase { EasingMode = EasingMode.EaseOut };

        var barFade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(170)) {
            EasingFunction = easeOut
        };
        var barSlide = new DoubleAnimation(16, 0, TimeSpan.FromMilliseconds(200)) {
            EasingFunction = easeOut
        };

        barFade.Completed += (_, _) => {
            _isOpen = true;
            _isAnimating = false;
            _ignoreDeactivate = false;
        };

        RootBorder.BeginAnimation(UIElement.OpacityProperty, barFade);
        BarSlideTransform.BeginAnimation(TranslateTransform.YProperty, barSlide);

        AnimateShortcutRailIn(LeftShortcutSlots, LeftShortcutSlide, easeOut);
        AnimateShortcutRailIn(RightShortcutSlots, RightShortcutSlide, easeOut);
    }

    private void AnimateClose() {
        if (_isAnimating || !IsVisible)
            return;

        _isAnimating = true;
        _ignoreDeactivate = true;

        var easeIn = new QuadraticEase { EasingMode = EasingMode.EaseIn };

        AnimateShortcutRailOut(LeftShortcutSlots, LeftShortcutSlide, easeIn);
        AnimateShortcutRailOut(RightShortcutSlots, RightShortcutSlide, easeIn);

        var barFade = new DoubleAnimation(RootBorder.Opacity, 0, TimeSpan.FromMilliseconds(140)) {
            EasingFunction = easeIn
        };
        var barSlide = new DoubleAnimation(BarSlideTransform.Y, 12, TimeSpan.FromMilliseconds(150)) {
            EasingFunction = easeIn
        };

        barFade.Completed += (_, _) => {
            BeginAnimation(OpacityProperty, null);
            Opacity = 0;
            ResetChromeVisuals();
            ResetSuggestionsInstant();

            Hide();
            _isOpen = false;
            _isAnimating = false;
            _ignoreDeactivate = false;
        };

        RootBorder.BeginAnimation(UIElement.OpacityProperty, barFade);
        BarSlideTransform.BeginAnimation(TranslateTransform.YProperty, barSlide);
    }

    private void ResetChromeVisuals() {
        RootBorder.BeginAnimation(UIElement.OpacityProperty, null);
        BarSlideTransform.BeginAnimation(TranslateTransform.YProperty, null);
        LeftShortcutSlots.BeginAnimation(UIElement.OpacityProperty, null);
        RightShortcutSlots.BeginAnimation(UIElement.OpacityProperty, null);
        LeftShortcutSlide.BeginAnimation(TranslateTransform.YProperty, null);
        RightShortcutSlide.BeginAnimation(TranslateTransform.YProperty, null);

        RootBorder.Opacity = 0;
        BarSlideTransform.Y = 16;
        LeftShortcutSlots.Opacity = 0;
        RightShortcutSlots.Opacity = 0;
        LeftShortcutSlide.Y = 8;
        RightShortcutSlide.Y = 8;
    }

    private static void AnimateShortcutRailIn(
        UIElement rail,
        TranslateTransform slide,
        IEasingFunction ease) {
        var fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(170)) {
            EasingFunction = ease
        };
        var slideIn = new DoubleAnimation(8, 0, TimeSpan.FromMilliseconds(200)) {
            EasingFunction = ease
        };

        rail.BeginAnimation(UIElement.OpacityProperty, fade);
        slide.BeginAnimation(TranslateTransform.YProperty, slideIn);
    }

    private static void AnimateShortcutRailOut(
        UIElement rail,
        TranslateTransform slide,
        IEasingFunction ease) {
        var fade = new DoubleAnimation(rail.Opacity, 0, TimeSpan.FromMilliseconds(140)) {
            EasingFunction = ease
        };
        var slideOut = new DoubleAnimation(slide.Y, 6, TimeSpan.FromMilliseconds(150)) {
            EasingFunction = ease
        };

        rail.BeginAnimation(UIElement.OpacityProperty, fade);
        slide.BeginAnimation(TranslateTransform.YProperty, slideOut);
    }

    private void Window_Deactivated(object? sender, EventArgs e) {
        if (_ignoreDeactivate || _isAnimating)
            return;

        // Popup is a separate HWND: don't close if the click stays on suggestions
        Dispatcher.BeginInvoke(() => {
            if (_ignoreDeactivate || _isAnimating || IsActive)
                return;

            if (SuggestionsPopup.IsOpen && SuggestionsPopup.IsMouseOver)
                return;

            AnimateClose();
        });
    }
}
