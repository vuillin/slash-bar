using System.Windows;
using System.Windows.Media;

namespace SlashBar.Modules.Native;

/// <summary>DPI-aware screen helpers for overlay placement.</summary>
public static class ScreenNative {

    public static System.Windows.Point DipToDevice(Visual visual, System.Windows.Point dip) {
        var source = PresentationSource.FromVisual(visual);
        var toDevice = source?.CompositionTarget?.TransformToDevice ?? Matrix.Identity;
        return toDevice.Transform(dip);
    }


    public static System.Windows.Forms.Screen GetCursorScreen() =>
        System.Windows.Forms.Screen.FromPoint(System.Windows.Forms.Cursor.Position);


    /// <summary>Monitor working area in WPF DIP units.</summary>
    public static System.Windows.Rect GetWorkingAreaDip(Visual? visual, System.Windows.Forms.Screen screen) {
        var area = screen.WorkingArea;

        if (visual is not null) {
            var source = PresentationSource.FromVisual(visual);
            if (source?.CompositionTarget is { } target) {
                var fromDevice = target.TransformFromDevice;
                var topLeft = fromDevice.Transform(new System.Windows.Point(area.Left, area.Top));
                var bottomRight = fromDevice.Transform(new System.Windows.Point(area.Right, area.Bottom));
                return new System.Windows.Rect(topLeft, bottomRight);
            }

            var dpi = VisualTreeHelper.GetDpi(visual);
            return new System.Windows.Rect(
                area.Left / dpi.DpiScaleX,
                area.Top / dpi.DpiScaleY,
                area.Width / dpi.DpiScaleX,
                area.Height / dpi.DpiScaleY);
        }

        return new System.Windows.Rect(area.Left, area.Top, area.Width, area.Height);
    }


    /// <summary>Screen that contains the SlashBar main window (device pixels).</summary>
    public static System.Windows.Forms.Screen GetBarScreen() {
        var app = System.Windows.Application.Current;
        Window? bar = app?.MainWindow;
        if (bar is null && app is not null) {
            foreach (Window window in app.Windows) {
                if (window.GetType().Name == "MainWindow") {
                    bar = window;
                    break;
                }
            }
        }

        if (bar != null) {
            var centerDip = new System.Windows.Point(
                bar.Left + bar.ActualWidth / 2,
                bar.Top + Math.Max(bar.ActualHeight / 2, 1));
            var centerPx = DipToDevice(bar, centerDip);

            return System.Windows.Forms.Screen.FromPoint(
                new System.Drawing.Point(
                    (int)Math.Round(centerPx.X),
                    (int)Math.Round(centerPx.Y)));
        }

        return System.Windows.Forms.Screen.PrimaryScreen
            ?? System.Windows.Forms.Screen.AllScreens[0];
    }
}
