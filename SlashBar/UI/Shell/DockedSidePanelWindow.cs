using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SlashBar.UI.Shell;

/// <summary>
/// Shared shell for docked side panels (slide, traffic lights, detach, minimize).
/// Derived windows must expose SlideTransform and MinimizeButton.
/// </summary>
public abstract class DockedSidePanelWindow : Window {

    protected const double LeftMargin = 14;

    protected abstract double PanelContentWidth { get; }

    /// <summary>Shortcut catalog prefix used for the bottom-left dock icon.</summary>
    public abstract string DockShelfKey { get; }

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


    protected virtual void OnPanelOpening() { }
    protected virtual void OnPanelOpened() { }
    protected virtual void OnPanelClosing() { }


    protected void CloseButton_Click(object sender, RoutedEventArgs e) =>
        AnimateClose();

    protected void MinimizeButton_Click(object sender, RoutedEventArgs e) =>
        MinimizeToShelf();

    protected void HeaderBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
        if (IsAnimating)
            return;

        if (VisualTreeExtensions.FindAncestor<System.Windows.Controls.Primitives.ButtonBase>(e.OriginalSource as DependencyObject) != null)
            return;

        if (!IsDetached)
            EnterDetachedMode();

        DragMove();
    }


    public void ToggleVisibility() {
        if (IsVisible)
            AnimateClose();
        else if (SidePanelDockShelf.Contains(this))
            SidePanelDockShelf.Restore(this);
        else
            AnimateOpen();
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

        SidePanelDockShelf.Remove(this);

        IsAnimating = true;
        IsDetached = false;

        PositionLeft();
        OnPanelOpening();

        Slide.X = HiddenX;
        Opacity = 1;
        Show();
        Activate();

        AnimateSlide(HiddenX, 0, 260, EasingMode.EaseOut, () => {
            IsAnimating = false;
            OnPanelOpened();
        });
    }


    protected void AnimateClose() {
        if (IsAnimating || (!IsVisible && !SidePanelDockShelf.Contains(this)))
            return;

        SidePanelDockShelf.Remove(this);

        if (!IsVisible)
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


    protected void MinimizeToShelf() {
        if (IsAnimating || !IsVisible)
            return;

        if (!SidePanelDockShelf.Minimize(this))
            return;

        IsAnimating = true;
        OnPanelClosing();

        var fade = new DoubleAnimation(Opacity, 0, TimeSpan.FromMilliseconds(160));
        fade.Completed += (_, _) => {
            BeginAnimation(OpacityProperty, null);
            Opacity = 1;
            Hide();
            IsAnimating = false;
        };
        BeginAnimation(OpacityProperty, fade);
    }


    internal void RestoreFromShelf() {
        if (IsAnimating || IsVisible)
            return;

        IsAnimating = true;
        OnPanelOpening();

        Slide.BeginAnimation(TranslateTransform.XProperty, null);
        Slide.X = 0;
        Opacity = 0;
        Show();
        Activate();

        var fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(180));
        fade.Completed += (_, _) => {
            BeginAnimation(OpacityProperty, null);
            Opacity = 1;
            IsAnimating = false;
            OnPanelOpened();
        };
        BeginAnimation(OpacityProperty, fade);
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
    }
}
