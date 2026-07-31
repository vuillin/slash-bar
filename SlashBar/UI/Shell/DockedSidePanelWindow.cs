using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SlashBar.UI.Shell;

/// <summary>
/// Shared shell for docked side panels (slide, collapse, traffic lights, detach).
/// Derived windows must expose the same XAML names: SlideTransform, DockButton,
/// ResetButton, Chevron.
/// </summary>
public abstract class DockedSidePanelWindow : Window {

    protected const double LeftMargin = 14;
    protected const double TabWidth = 32;

    protected abstract double PanelContentWidth { get; }

    protected double CollapsedX => -(LeftMargin + PanelContentWidth);

    protected bool IsCollapsed { get; set; }
    protected bool IsAnimating { get; set; }
    protected bool IsDetached { get; set; }

    private TranslateTransform Slide =>
        (TranslateTransform)FindName("SlideTransform")!;

    private System.Windows.Controls.Button DockBtn =>
        (System.Windows.Controls.Button)FindName("DockButton")!;

    private System.Windows.Controls.Button ResetBtn =>
        (System.Windows.Controls.Button)FindName("ResetButton")!;

    private Path ChevronPath =>
        (Path)FindName("Chevron")!;


    protected virtual void OnPanelOpening() { }
    protected virtual void OnPanelOpened() { }
    protected virtual void OnPanelClosing() { }
    protected virtual void OnPanelCollapsed() { }
    protected virtual void OnPanelExpanding() { }
    protected virtual void OnPanelExpanded() { }
    protected virtual void OnResetToDockCompleted() { }


    protected void CloseButton_Click(object sender, RoutedEventArgs e) =>
        AnimateClose();

    protected void ResetButton_Click(object sender, RoutedEventArgs e) =>
        ResetToDock();

    protected void DockButton_Click(object sender, RoutedEventArgs e) {
        if (IsDetached)
            return;

        if (IsCollapsed)
            AnimateExpand();
        else
            AnimateCollapse();
    }

    protected void HeaderBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
        if (IsCollapsed || IsAnimating)
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

        var height = area.Height * 0.5;
        Left = area.Left;
        Top = area.Top + (area.Height - height) / 2;
        Height = height;
        Width = LeftMargin + PanelContentWidth + TabWidth;
    }


    protected void FadeDockTab(bool show) {
        DockBtn.IsHitTestVisible = show;

        var anim = new DoubleAnimation(
            DockBtn.Opacity,
            show ? 1 : 0,
            TimeSpan.FromMilliseconds(200)) {
            EasingFunction = new QuadraticEase {
                EasingMode = show ? EasingMode.EaseOut : EasingMode.EaseIn
            }
        };

        DockBtn.BeginAnimation(OpacityProperty, anim);
    }


    protected void AnimateOpen() {
        if (IsAnimating)
            return;

        IsAnimating = true;
        IsDetached = false;
        ResetBtn.Visibility = Visibility.Collapsed;
        DockBtn.BeginAnimation(OpacityProperty, null);
        DockBtn.Opacity = 1;
        DockBtn.IsHitTestVisible = true;

        PositionLeft();
        SetChevronCollapsed(false);
        OnPanelOpening();

        Slide.X = CollapsedX;
        Show();
        Activate();

        AnimateSlide(CollapsedX, 0, 260, EasingMode.EaseOut, () => {
            IsCollapsed = false;
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
                IsCollapsed = false;
                IsAnimating = false;
                ResetBtn.Visibility = Visibility.Collapsed;
                DockBtn.BeginAnimation(OpacityProperty, null);
                DockBtn.Opacity = 1;
                DockBtn.IsHitTestVisible = true;
                SetChevronCollapsed(false);
            };
            BeginAnimation(OpacityProperty, fade);
            return;
        }

        var from = Slide.X;
        var to = CollapsedX - TabWidth;

        AnimateSlide(from, to, 200, EasingMode.EaseIn, () => {
            Slide.BeginAnimation(TranslateTransform.XProperty, null);
            Slide.X = CollapsedX;
            Hide();
            IsCollapsed = false;
            IsAnimating = false;
            SetChevronCollapsed(false);
        });
    }


    protected void AnimateCollapse() {
        if (IsAnimating || IsCollapsed || IsDetached || !IsVisible)
            return;

        IsAnimating = true;
        OnPanelCollapsed();

        AnimateSlide(Slide.X, CollapsedX, 220, EasingMode.EaseInOut, () => {
            IsCollapsed = true;
            IsAnimating = false;
            SetChevronCollapsed(true);
        });
    }


    protected void AnimateExpand() {
        if (IsAnimating || !IsCollapsed)
            return;

        IsAnimating = true;
        Activate();
        OnPanelExpanding();

        AnimateSlide(Slide.X, 0, 220, EasingMode.EaseInOut, () => {
            IsCollapsed = false;
            IsAnimating = false;
            SetChevronCollapsed(false);
            OnPanelExpanded();
        });
    }


    protected void AnimateSlide(double from, double to, int ms, EasingMode mode, Action onDone) {
        var anim = new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(ms)) {
            EasingFunction = new QuadraticEase { EasingMode = mode }
        };
        anim.Completed += (_, _) => onDone();
        Slide.BeginAnimation(TranslateTransform.XProperty, anim);
    }


    protected void SetChevronCollapsed(bool collapsed) {
        ChevronPath.Data = Geometry.Parse(collapsed
            ? "M 1,0 L 6,6 L 1,12"
            : "M 6,0 L 1,6 L 6,12");
    }


    protected void EnterDetachedMode() {
        IsDetached = true;
        ResetBtn.Visibility = Visibility.Visible;
        FadeDockTab(show: false);
    }


    protected void ResetToDock() {
        if (IsAnimating)
            return;

        IsAnimating = true;
        IsDetached = false;
        IsCollapsed = false;
        SetChevronCollapsed(false);

        var screen = System.Windows.Forms.Screen.PrimaryScreen
            ?? System.Windows.Forms.Screen.AllScreens[0];
        var area = screen.WorkingArea;
        var height = area.Height * 0.5;
        var targetLeft = (double)area.Left;
        var targetTop = area.Top + (area.Height - height) / 2;

        Slide.BeginAnimation(TranslateTransform.XProperty, null);
        Slide.X = 0;

        var ease = new QuadraticEase { EasingMode = EasingMode.EaseInOut };
        var animL = new DoubleAnimation(Left, targetLeft, TimeSpan.FromMilliseconds(280)) { EasingFunction = ease };
        var animT = new DoubleAnimation(Top, targetTop, TimeSpan.FromMilliseconds(280)) { EasingFunction = ease };
        var animH = new DoubleAnimation(Height, height, TimeSpan.FromMilliseconds(280)) { EasingFunction = ease };

        animL.Completed += (_, _) => {
            Left = targetLeft;
            Top = targetTop;
            Height = height;
            Width = LeftMargin + PanelContentWidth + TabWidth;
            BeginAnimation(LeftProperty, null);
            BeginAnimation(TopProperty, null);
            BeginAnimation(HeightProperty, null);

            ResetBtn.Visibility = Visibility.Collapsed;
            FadeDockTab(show: true);
            IsAnimating = false;
            OnResetToDockCompleted();
        };

        BeginAnimation(LeftProperty, animL);
        BeginAnimation(TopProperty, animT);
        BeginAnimation(HeightProperty, animH);
    }
}
