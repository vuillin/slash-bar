using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using SlashBar.Modules.Awake;

namespace SlashBar;

public partial class AwakeToastWindow : Window {

    private const int SwpNoZOrder = 0x0004;
    private const int SwpNoActivate = 0x0010;
    private const int SwpShowWindow = 0x0040;

    private bool _showing;


    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(
        IntPtr hWnd, IntPtr hWndInsertAfter,
        int X, int Y, int cx, int cy, uint uFlags);


    public AwakeToastWindow() {
        InitializeComponent();
    }


    public void ShowIndicator() {
        ToastText.Text = AwakeSession.Mode == AwakeMode.System
            ? "Awake · system"
            : "Awake";

        if (AwakeSession.EndsAt is { } ends) {
            var left = ends - DateTimeOffset.Now;
            ToastDuration.Text = $"({AwakeDuration.FormatRemaining(left)})";
            ToastDuration.Visibility = Visibility.Visible;
        }
        else {
            ToastDuration.Text = "";
            ToastDuration.Visibility = Visibility.Collapsed;
        }

        if (_showing) {
            FitToContent();
            UpdateLayout();
            PlaceTopRightOnBarScreen();
            return;
        }

        _showing = true;
        AnimateIn();
    }


    public void HideIndicator() {
        if (!_showing)
            return;

        _showing = false;
        AnimateOut();
    }


    private void ToastRoot_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
        if (!_showing)
            return;

        AwakeSession.Disable();
        HideIndicator();
        AppToast.ShowSuccess("Awake off");
    }


    private void FitToContent() {
        SizeToContent = SizeToContent.Manual;
        ToastRoot.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
        Width = Math.Ceiling(ToastRoot.DesiredSize.Width);
        Height = Math.Ceiling(ToastRoot.DesiredSize.Height);
    }


    private void PlaceTopRightOnBarScreen() {
        var screen = GetBarScreen();
        var area = screen.WorkingArea;

        var hwnd = new WindowInteropHelper(this).EnsureHandle();
        UpdateLayout();

        var source = PresentationSource.FromVisual(this);
        var toDevice = source?.CompositionTarget?.TransformToDevice ?? Matrix.Identity;
        var sizePx = toDevice.Transform(new System.Windows.Point(ActualWidth, ActualHeight));

        const int margin = 20;
        var x = area.Right - (int)Math.Ceiling(sizePx.X) - margin;
        var y = area.Top + margin;

        SetWindowPos(
            hwnd,
            IntPtr.Zero,
            x, y,
            0, 0,
            SwpNoZOrder | SwpNoActivate | SwpShowWindow | 0x0001 /* SWP_NOSIZE */);
    }


    private static System.Windows.Forms.Screen GetBarScreen() {
        var bar = System.Windows.Application.Current?.MainWindow
            ?? System.Windows.Application.Current?.Windows.OfType<MainWindow>().FirstOrDefault();

        if (bar != null) {
            var source = PresentationSource.FromVisual(bar);
            var toDevice = source?.CompositionTarget?.TransformToDevice ?? Matrix.Identity;

            var centerDip = new System.Windows.Point(
                bar.Left + bar.ActualWidth / 2,
                bar.Top + Math.Max(bar.ActualHeight / 2, 1));
            var centerPx = toDevice.Transform(centerDip);

            return System.Windows.Forms.Screen.FromPoint(
                new System.Drawing.Point(
                    (int)Math.Round(centerPx.X),
                    (int)Math.Round(centerPx.Y)));
        }

        return System.Windows.Forms.Screen.PrimaryScreen
            ?? System.Windows.Forms.Screen.AllScreens[0];
    }


    private void AnimateIn() {
        ToastRoot.BeginAnimation(OpacityProperty, null);
        ToastSlide.BeginAnimation(TranslateTransform.XProperty, null);

        Show();
        FitToContent();
        UpdateLayout();
        PlaceTopRightOnBarScreen();

        Dispatcher.BeginInvoke(() => {
            if (!_showing || !IsVisible)
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


    private void AnimateOut() {
        var ease = new QuadraticEase { EasingMode = EasingMode.EaseIn };
        var fade = new DoubleAnimation(ToastRoot.Opacity, 0, TimeSpan.FromMilliseconds(180)) {
            EasingFunction = ease
        };
        var slide = new DoubleAnimation(ToastSlide.X, 48, TimeSpan.FromMilliseconds(200)) {
            EasingFunction = ease
        };

        fade.Completed += (_, _) => Hide();

        ToastRoot.BeginAnimation(OpacityProperty, fade);
        ToastSlide.BeginAnimation(TranslateTransform.XProperty, slide);
    }
}
