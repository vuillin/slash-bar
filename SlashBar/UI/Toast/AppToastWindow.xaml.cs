using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using SlashBar.Modules.Native;

namespace SlashBar;

public partial class AppToastWindow : Window {

    private const int DefaultDurationMs = 1600;

    private int _showId;


    public AppToastWindow() {
        InitializeComponent();
    }


    public void ShowToast(string message, bool success, string? detail = null, int durationMs = DefaultDurationMs) {
        ToastText.Text = message;

        if (string.IsNullOrWhiteSpace(detail)) {
            ToastDetail.Text = "";
            ToastDetail.Visibility = Visibility.Collapsed;
        }
        else {
            // single line for ellipsis; spaces / newlines → space
            var flat = string.Join(' ',
                detail.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
            ToastDetail.Text = flat;
            ToastDetail.Visibility = Visibility.Visible;
        }

        if (success) {
            ToastIcon.Text = "✓";
            ToastIcon.FontFamily = new System.Windows.Media.FontFamily("Segoe UI Variable Text, Segoe UI");
            ToastIcon.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x34, 0xC7, 0x59));
        }
        else {
            ToastIcon.Text = "!";
            ToastIcon.FontFamily = new System.Windows.Media.FontFamily("Segoe UI Variable Text, Segoe UI");
            ToastIcon.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFF, 0x3B, 0x30));
        }

        var id = ++_showId;
        AnimateIn();
        _ = DismissAfterAsync(id, Math.Max(400, durationMs));
    }


    private async Task DismissAfterAsync(int showId, int durationMs) {
        try {
            await Task.Delay(durationMs).ConfigureAwait(true);
        }
        catch {
            return;
        }

        if (showId != _showId)
            return;

        AnimateOut(showId);
    }


    private void FitToContent() {
        SizeToContent = SizeToContent.Manual;
        ToastRoot.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
        Width = Math.Ceiling(ToastRoot.DesiredSize.Width);
        Height = Math.Ceiling(ToastRoot.DesiredSize.Height);
    }


    private void PlaceTopRightOnBarScreen() {
        var screen = ScreenNative.GetBarScreen();
        var area = screen.WorkingArea; // absolute pixels on the virtual desktop

        var hwnd = new WindowInteropHelper(this).EnsureHandle();
        UpdateLayout();

        var sizePx = ScreenNative.DipToDevice(this, new System.Windows.Point(ActualWidth, ActualHeight));

        const int margin = 20;
        var x = area.Right - (int)Math.Ceiling(sizePx.X) - margin;
        var y = area.Top + margin;

        WindowNative.MoveNoActivate(hwnd, x, y);
    }


    private void AnimateIn() {
        ToastRoot.BeginAnimation(OpacityProperty, null);
        ToastSlide.BeginAnimation(TranslateTransform.XProperty, null);

        Show();
        FitToContent();
        UpdateLayout();
        PlaceTopRightOnBarScreen();

        Dispatcher.BeginInvoke(() => {
            if (!IsVisible)
                return;
            FitToContent();
            UpdateLayout();
            PlaceTopRightOnBarScreen();
        }, DispatcherPriority.Loaded);

        var ease = new QuadraticEase { EasingMode = EasingMode.EaseOut };
        ToastRoot.BeginAnimation(OpacityProperty,
            new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(180)) { EasingFunction = ease });
        ToastSlide.BeginAnimation(TranslateTransform.XProperty,
            new DoubleAnimation(48, 0, TimeSpan.FromMilliseconds(220)) { EasingFunction = ease });
    }


    private void AnimateOut(int showId) {
        if (showId != _showId)
            return;

        var ease = new QuadraticEase { EasingMode = EasingMode.EaseIn };
        var fade = new DoubleAnimation(ToastRoot.Opacity, 0, TimeSpan.FromMilliseconds(180)) {
            EasingFunction = ease
        };
        var slide = new DoubleAnimation(ToastSlide.X, 48, TimeSpan.FromMilliseconds(200)) {
            EasingFunction = ease
        };

        fade.Completed += (_, _) => {
            if (showId == _showId)
                Hide();
        };

        ToastRoot.BeginAnimation(OpacityProperty, fade);
        ToastSlide.BeginAnimation(TranslateTransform.XProperty, slide);
    }


    public void CancelAndHide() {
        _showId++;
        ToastRoot.BeginAnimation(OpacityProperty, null);
        ToastSlide.BeginAnimation(TranslateTransform.XProperty, null);
        Hide();
    }
}
