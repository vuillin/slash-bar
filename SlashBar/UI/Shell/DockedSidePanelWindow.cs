using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SlashBar.UI.Shell;

/// <summary>
/// Shared shell for docked side panels (slide, traffic lights, detach).
/// Derived windows must expose SlideTransform and ResetButton.
/// </summary>
public abstract class DockedSidePanelWindow : Window {

    protected const double LeftMargin = 14;

    protected abstract double PanelContentWidth { get; }

    protected virtual double DockedHeight(double workAreaHeight) => workAreaHeight * 0.5;

    protected virtual double DockedTop(System.Drawing.Rectangle workArea, double height) =>
        workArea.Top + (workArea.Height - height) / 2;

    protected double ChromeWidth => LeftMargin + PanelContentWidth;

    private static Thickness ShadowMargin =>
        (Thickness)System.Windows.Application.Current.FindResource("SidePanelShadowMargin");

    protected double ShellWidth => ChromeWidth + ShadowMargin.Right;

    protected double HiddenX => -(LeftMargin + PanelContentWidth);

    protected bool IsAnimating { get; set; }
    protected bool IsDetached { get; set; }

    private TranslateTransform Slide =>
        (TranslateTransform)FindName("SlideTransform")!;

    private System.Windows.Controls.Button ResetBtn =>
        (System.Windows.Controls.Button)FindName("ResetButton")!;


    protected virtual void OnPanelOpening() { }
    protected virtual void OnPanelOpened() { }
    protected virtual void OnPanelClosing() { }
    protected virtual void OnResetToDockCompleted() { }


    protected void CloseButton_Click(object sender, RoutedEventArgs e) =>
        AnimateClose();

    protected void ResetButton_Click(object sender, RoutedEventArgs e) =>
        ResetToDock();

    protected void HeaderBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
        if (IsAnimating)
            return;

        if (VisualTreeExtensions.FindAncestor<System.Windows.Controls.Button>(e.OriginalSource as DependencyObject) != null)
            return;

        if (!IsDetached)
            EnterDetachedMode();

        DragMove();
    }


    protected void PositionLeft() {
        var screen = System.Windows.Forms.Screen.PrimaryScreen
            ?? System.Windows.Forms.Screen.AllScreens[0];
        var area = screen.WorkingArea;

        var height = DockedHeight(area.Height);
        var pad = ShadowMargin;
        Left = area.Left - pad.Left;
        Top = DockedTop(area, height) - pad.Top;
        Height = height + pad.Top + pad.Bottom;
        Width = ShellWidth;
    }


    protected void AnimateOpen() {
        if (IsAnimating)
            return;

        IsAnimating = true;
        IsDetached = false;
        ResetBtn.Visibility = Visibility.Collapsed;

        PositionLeft();
        OnPanelOpening();

        Slide.X = HiddenX;
        Show();
        Activate();

        AnimateSlide(HiddenX, 0, 260, EasingMode.EaseOut, () => {
            IsAnimating = false;
            OnPanelOpened();
        });
    }


    protected void AnimateClose() {
        if (IsAnimating || !IsVisible)
            return;

        IsAnimating = true;
        OnPanelClosing();

        if (IsDetached) {
            var fade = new DoubleAnimation(Opacity, 0, TimeSpan.FromMilliseconds(160));
            fade.Completed += (_, _) => {
                BeginAnimation(OpacityProperty, null);
                Opacity = 1;
                Hide();
                IsDetached = false;
                IsAnimating = false;
                ResetBtn.Visibility = Visibility.Collapsed;
            };
            BeginAnimation(OpacityProperty, fade);
            return;
        }

        var from = Slide.X;

        AnimateSlide(from, HiddenX, 200, EasingMode.EaseIn, () => {
            Slide.BeginAnimation(TranslateTransform.XProperty, null);
            Slide.X = HiddenX;
            Hide();
            IsAnimating = false;
        });
    }


    protected void AnimateSlide(double from, double to, int ms, EasingMode mode, Action onDone) {
        var anim = new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(ms)) {
            EasingFunction = new QuadraticEase { EasingMode = mode }
        };
        anim.Completed += (_, _) => onDone();
        Slide.BeginAnimation(TranslateTransform.XProperty, anim);
    }


    protected void EnterDetachedMode() {
        IsDetached = true;
        ResetBtn.Visibility = Visibility.Visible;
    }


    protected void ResetToDock() {
        if (IsAnimating)
            return;

        IsAnimating = true;
        IsDetached = false;

        var screen = System.Windows.Forms.Screen.PrimaryScreen
            ?? System.Windows.Forms.Screen.AllScreens[0];
        var area = screen.WorkingArea;
        var height = DockedHeight(area.Height);
        var pad = ShadowMargin;
        var targetLeft = area.Left - pad.Left;
        var targetTop = DockedTop(area, height) - pad.Top;
        var targetHeight = height + pad.Top + pad.Bottom;

        Slide.BeginAnimation(TranslateTransform.XProperty, null);
        Slide.X = 0;

        var ease = new QuadraticEase { EasingMode = EasingMode.EaseInOut };
        var animL = new DoubleAnimation(Left, targetLeft, TimeSpan.FromMilliseconds(280)) { EasingFunction = ease };
        var animT = new DoubleAnimation(Top, targetTop, TimeSpan.FromMilliseconds(280)) { EasingFunction = ease };
        var animH = new DoubleAnimation(Height, targetHeight, TimeSpan.FromMilliseconds(280)) { EasingFunction = ease };

        animL.Completed += (_, _) => {
            Left = targetLeft;
            Top = targetTop;
            Height = targetHeight;
            Width = ShellWidth;
            BeginAnimation(LeftProperty, null);
            BeginAnimation(TopProperty, null);
            BeginAnimation(HeightProperty, null);

            ResetBtn.Visibility = Visibility.Collapsed;
            IsAnimating = false;
            OnResetToDockCompleted();
        };

        BeginAnimation(LeftProperty, animL);
        BeginAnimation(TopProperty, animT);
        BeginAnimation(HeightProperty, animH);
    }
}
