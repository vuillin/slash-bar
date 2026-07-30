using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace SlashBar;

public partial class AppToastWindow : Window {

    private const int SwpNoZOrder = 0x0004;
    private const int SwpNoActivate = 0x0010;
    private const int SwpShowWindow = 0x0040;

    private readonly DispatcherTimer _hideTimer;


    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(
        IntPtr hWnd, IntPtr hWndInsertAfter,
        int X, int Y, int cx, int cy, uint uFlags);


    public AppToastWindow() {
        InitializeComponent();

        _hideTimer = new DispatcherTimer {
            Interval = TimeSpan.FromMilliseconds(1600),
        };
        _hideTimer.Tick += (_, _) => {
            _hideTimer.Stop();
            AnimateOut();
        };
    }


    public void ShowToast(string message, bool success) {
        ToastText.Text = message;

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

        AnimateIn();
        _hideTimer.Stop();
        _hideTimer.Start();
    }


    private void PlaceTopRightOnBarScreen() {
        var screen = GetBarScreen();
        var area = screen.WorkingArea; // pixels absolus du bureau virtuel

        var hwnd = new WindowInteropHelper(this).EnsureHandle();
        UpdateLayout();

        // taille en pixels (DIPs → device)
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


    /// <summary>Écran où se trouve la barre SlashBar</summary>
    private static System.Windows.Forms.Screen GetBarScreen() {
        var bar = System.Windows.Application.Current?.MainWindow
            ?? System.Windows.Application.Current?.Windows.OfType<MainWindow>().FirstOrDefault();

        if (bar != null) {
            var source = PresentationSource.FromVisual(bar);
            var toDevice = source?.CompositionTarget?.TransformToDevice ?? Matrix.Identity;

            // centre de la barre en pixels écran
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

        // Show d'abord pour avoir ActualWidth / CompositionTarget
        Show();
        UpdateLayout();
        PlaceTopRightOnBarScreen();

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


    public void CancelAndHide() {
        _hideTimer.Stop();
        ToastRoot.BeginAnimation(OpacityProperty, null);
        ToastSlide.BeginAnimation(TranslateTransform.XProperty, null);
        Hide();
    }
}
