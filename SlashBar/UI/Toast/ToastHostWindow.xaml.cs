using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using SlashBar.Modules.Awake;
using SlashBar.Modules.Native;

namespace SlashBar;

public partial class ToastHostWindow : Window {

    private const int DefaultEphemeralMs = 1600;
    private const int MarginPx = 20;

    private bool _stickyVisible;
    private int _ephemeralId;
    private bool _anchorValid;
    private int _anchorRightPx;
    private int _anchorTopPx;


    public ToastHostWindow() {
        InitializeComponent();
    }


    public void ShowSticky(string title, string? detail) {
        StickyText.Text = title;

        if (string.IsNullOrWhiteSpace(detail)) {
            StickyDetail.Text = "";
            StickyDetail.Visibility = Visibility.Collapsed;
        }
        else {
            StickyDetail.Text = detail;
            StickyDetail.Visibility = Visibility.Visible;
        }

        StickyRoot.Margin = new Thickness(0, 0, 0, EphemeralRoot.Visibility == Visibility.Visible ? 8 : 0);

        if (_stickyVisible)
            return; 

        _stickyVisible = true;
        StickyRoot.Visibility = Visibility.Visible;
        AnimateIn(StickyRoot, StickySlide, keepShowing: () => _stickyVisible);
    }


    public void HideSticky() {
        if (!_stickyVisible)
            return;

        _stickyVisible = false;
        AnimateOut(StickyRoot, StickySlide, onHidden: () => {
            StickyRoot.Visibility = Visibility.Collapsed;
            StickyRoot.Margin = new Thickness(0);
            if (EphemeralRoot.Visibility != Visibility.Visible)
                HideHost();
            else
                Relayout();
        });
    }


    public void ShowEphemeral(string message, bool success, string? detail = null, int durationMs = DefaultEphemeralMs) {
        EphemeralText.Text = message;

        if (string.IsNullOrWhiteSpace(detail)) {
            EphemeralDetail.Text = "";
            EphemeralDetail.Visibility = Visibility.Collapsed;
        }
        else {
            var flat = string.Join(' ',
                detail.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
            EphemeralDetail.Text = flat;
            EphemeralDetail.Visibility = Visibility.Visible;
        }

        if (success) {
            EphemeralIcon.Text = "✓";
            EphemeralIcon.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x34, 0xC7, 0x59));
        }
        else {
            EphemeralIcon.Text = "!";
            EphemeralIcon.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFF, 0x3B, 0x30));
        }

        CancelEphemeralVisual();
        var id = ++_ephemeralId;

        StickyRoot.Margin = new Thickness(0, 0, 0, _stickyVisible ? 8 : 0);
        EphemeralRoot.Visibility = Visibility.Visible;
        AnimateIn(EphemeralRoot, EphemeralSlide, keepShowing: () => id == _ephemeralId);
        _ = DismissEphemeralAfterAsync(id, Math.Max(400, durationMs));
    }


    private async Task DismissEphemeralAfterAsync(int showId, int durationMs) {
        try {
            await Task.Delay(durationMs).ConfigureAwait(true);
        }
        catch {
            return;
        }

        if (showId != _ephemeralId)
            return;

        AnimateOut(EphemeralRoot, EphemeralSlide, onHidden: () => {
            if (showId != _ephemeralId)
                return;

            EphemeralRoot.Visibility = Visibility.Collapsed;

            if (!_stickyVisible) {
                StickyRoot.Margin = new Thickness(0);
                HideHost();
                return;
            }

            StickyRoot.Margin = new Thickness(0);
        });
    }


    private void CancelEphemeralVisual() {
        _ephemeralId++;
        EphemeralRoot.BeginAnimation(UIElement.OpacityProperty, null);
        EphemeralSlide.BeginAnimation(TranslateTransform.XProperty, null);
        EphemeralRoot.Opacity = 0;
        EphemeralSlide.X = 48;
    }


    private void StickyRoot_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
        if (!_stickyVisible)
            return;

        AwakeSession.Disable();
        HideSticky();
        ShowEphemeral("Awake off", success: true);
    }


    private void AnimateIn(
        UIElement root,
        TranslateTransform slide,
        Func<bool> keepShowing) {

        root.BeginAnimation(UIElement.OpacityProperty, null);
        slide.BeginAnimation(TranslateTransform.XProperty, null);

        Show();
        Relayout();

        Dispatcher.BeginInvoke(() => {
            if (!keepShowing() || !IsVisible)
                return;
            Relayout();
        }, DispatcherPriority.Loaded);

        var ease = new QuadraticEase { EasingMode = EasingMode.EaseOut };
        root.BeginAnimation(UIElement.OpacityProperty,
            new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(180)) { EasingFunction = ease });
        slide.BeginAnimation(TranslateTransform.XProperty,
            new DoubleAnimation(48, 0, TimeSpan.FromMilliseconds(220)) { EasingFunction = ease });
    }


    private void AnimateOut(UIElement root, TranslateTransform slide, Action onHidden) {
        var ease = new QuadraticEase { EasingMode = EasingMode.EaseIn };
        var fade = new DoubleAnimation(root.Opacity, 0, TimeSpan.FromMilliseconds(180)) {
            EasingFunction = ease
        };
        var move = new DoubleAnimation(slide.X, 48, TimeSpan.FromMilliseconds(200)) {
            EasingFunction = ease
        };

        fade.Completed += (_, _) => onHidden();

        root.BeginAnimation(UIElement.OpacityProperty, fade);
        slide.BeginAnimation(TranslateTransform.XProperty, move);
    }


    private void Relayout() {
        SizeToContent = SizeToContent.Manual;
        ToastStack.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
        Width = Math.Ceiling(ToastStack.DesiredSize.Width);
        Height = Math.Ceiling(ToastStack.DesiredSize.Height);
        UpdateLayout();
        PlaceTopRightOnBarScreen();
    }


    private void PlaceTopRightOnBarScreen() {
        var hwnd = new WindowInteropHelper(this).EnsureHandle();
        UpdateLayout();

        var sizePx = ScreenNative.DipToDevice(this, new System.Windows.Point(ActualWidth, ActualHeight));
        var widthPx = Math.Max(1, (int)Math.Round(sizePx.X));
        var heightPx = Math.Max(1, (int)Math.Round(sizePx.Y));

        if (!_anchorValid) {
            var area = ScreenNative.GetBarScreen().WorkingArea;
            _anchorRightPx = area.Right - MarginPx;
            _anchorTopPx = area.Top + MarginPx;
            _anchorValid = true;
        }

        WindowNative.SetBoundsNoActivate(
            hwnd,
            _anchorRightPx - widthPx,
            _anchorTopPx,
            widthPx,
            heightPx);
    }


    private void HideHost() {
        _anchorValid = false;
        Hide();
    }
}
