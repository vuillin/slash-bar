using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using SlashBar.Modules;
using SlashBar.Modules.Color;
using SlashBar.UI.Shell;

namespace SlashBar;

public partial class ColorPanelWindow {

    private const double SatValCornerRadius = 14;

    private double _hue;         // 0–360
    private double _saturation;  // 0–1
    private double _value;       // 0–1 (SV square brightness)

    private System.Windows.Media.Color _selectedColor;
    private readonly SolidColorBrush _satValMarkerFill = new(System.Windows.Media.Color.FromRgb(128, 128, 128));
    private readonly SolidColorBrush _hueThumbFill = new(System.Windows.Media.Color.FromRgb(255, 0, 0));
    private bool _draggingSatVal;
    private bool _draggingHue;


    private void Window_Loaded(object sender, RoutedEventArgs e) {
        SatValMarker.Fill = _satValMarkerFill;
        HueThumb.Fill = _hueThumbFill;
        SatValPicker.SizeChanged += (_, _) => PlaceSatValMarker();
        HueBar.SizeChanged += (_, _) => PlaceHueThumb();
        SetSelectedColor(System.Windows.Media.Color.FromRgb(128, 128, 128));
    }


    /// <summary>Quick preview while eyedropper is hovered (without moving HSV cursors).</summary>
    private void ApplyColorPreview(System.Windows.Media.Color color) {
        _selectedColor = color;
        HsvColorConverter.RgbToHsv(color.R, color.G, color.B, out _hue, out _saturation, out _value);

        PreviewSwatch.Background = new SolidColorBrush(color);
        var hex = ColorFormats.ToHex(color);
        PreviewHexText.Text = hex;
        PreviewRgbText.Text = ColorFormats.ToRgbDisplay(color);
    }


    private void SetSelectedColor(System.Windows.Media.Color color) {
        _selectedColor = color;
        HsvColorConverter.RgbToHsv(color.R, color.G, color.B, out _hue, out _saturation, out _value);
        RefreshPickerUi();
    }


    private void RefreshPickerUi() {
        PreviewSwatch.Background = new SolidColorBrush(_selectedColor);

        var hex = ColorFormats.ToHex(_selectedColor);
        PreviewHexText.Text = hex;
        PreviewRgbText.Text = ColorFormats.ToRgbDisplay(_selectedColor);

        SatValHueLayer.Background = new SolidColorBrush(HsvColorConverter.HsvToRgb(_hue, 1, 1));
        PlaceSatValMarker();
        PlaceHueThumb();
    }


    private void PlaceSatValMarker() {
        var w = SatValPicker.ActualWidth;
        var h = SatValPicker.ActualHeight;
        if (w <= 0 || h <= 0)
            return;

        var cx = _saturation * w;
        var cy = (1 - _value) * h;
        ClampToRoundedRect(ref cx, ref cy, w, h, SatValCornerRadius);

        _satValMarkerFill.Color = _selectedColor;
        SatValMarker.Margin = new Thickness(
            cx - SatValMarker.Width / 2,
            cy - SatValMarker.Height / 2,
            0, 0);
    }


    /// <summary>Projects a point onto the rounded-rect fill so the marker stays on the color.</summary>
    private static void ClampToRoundedRect(ref double x, ref double y, double width, double height, double radius) {
        radius = Math.Min(radius, Math.Min(width, height) / 2.0);
        x = Math.Clamp(x, 0, width);
        y = Math.Clamp(y, 0, height);

        double cornerX, cornerY;
        if (x < radius && y < radius) {
            cornerX = radius;
            cornerY = radius;
        }
        else if (x > width - radius && y < radius) {
            cornerX = width - radius;
            cornerY = radius;
        }
        else if (x < radius && y > height - radius) {
            cornerX = radius;
            cornerY = height - radius;
        }
        else if (x > width - radius && y > height - radius) {
            cornerX = width - radius;
            cornerY = height - radius;
        }
        else {
            return;
        }

        var dx = x - cornerX;
        var dy = y - cornerY;
        var dist = Math.Sqrt(dx * dx + dy * dy);
        if (dist <= radius)
            return;

        x = cornerX + dx / dist * radius;
        y = cornerY + dy / dist * radius;
    }


    private void PlaceHueThumb() {
        const double thumbSize = 16;
        const double trackInset = 8;

        var trackH = HueBar.ActualHeight;
        if (trackH <= 0)
            return;

        var y = trackInset + _hue / 360.0 * trackH - thumbSize / 2;
        y = Math.Clamp(y, 0, trackInset * 2 + trackH - thumbSize);

        _hueThumbFill.Color = HsvColorConverter.HsvToRgb(_hue, 1, 1);
        HueThumb.Margin = new Thickness(0, y, 0, 0);
    }


    private void SatValPicker_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
        _draggingSatVal = true;
        SatValPicker.CaptureMouse();
        UpdateSatValFromMouse(e.GetPosition(SatValPicker));
        e.Handled = true;
    }

    private void SatValPicker_MouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
        if (!_draggingSatVal)
            return;
        UpdateSatValFromMouse(e.GetPosition(SatValPicker));
    }

    private void SatValPicker_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
        _draggingSatVal = false;
        SatValPicker.ReleaseMouseCapture();
    }


    private void UpdateSatValFromMouse(System.Windows.Point pos) {
        var w = SatValPicker.ActualWidth;
        var h = SatValPicker.ActualHeight;
        if (w <= 0 || h <= 0)
            return;

        var x = pos.X;
        var y = pos.Y;
        ClampToRoundedRect(ref x, ref y, w, h, SatValCornerRadius);

        _saturation = x / w;
        _value = 1 - y / h;

        _colorLocked = true;
        CommitHsvToColor();
    }


    private void HueBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
        _draggingHue = true;
        HueBar.CaptureMouse();
        UpdateHueFromMouse(e.GetPosition(HueBar));
        e.Handled = true;
    }

    private void HueBar_MouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
        if (!_draggingHue)
            return;
        UpdateHueFromMouse(e.GetPosition(HueBar));
    }

    private void HueBar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
        _draggingHue = false;
        HueBar.ReleaseMouseCapture();
    }


    private void UpdateHueFromMouse(System.Windows.Point pos) {
        var h = HueBar.ActualHeight;
        if (h <= 0)
            return;

        _hue = Math.Clamp(pos.Y / h * 360.0, 0, 360);
        _colorLocked = true;
        CommitHsvToColor();
    }


    private void CommitHsvToColor() {
        _selectedColor = HsvColorConverter.HsvToRgb(_hue, _saturation, _value);
        RefreshPickerUi();
    }


    private void CopyHex_Click(object sender, MouseButtonEventArgs e) {
        CopySelectedColor(ColorFormats.ToHex(_selectedColor));
        e.Handled = true;
    }


    private void CopyRgb_Click(object sender, MouseButtonEventArgs e) {
        CopySelectedColor(ColorFormats.ToRgbClipboard(_selectedColor));
        e.Handled = true;
    }


    private void CopySelectedColor(string text) {
        ClipboardHelper.SetText(text);
        ColorHistory.Store.Add(_selectedColor);
        CopiedToastAnimator.Show(CopiedToast, CopiedToastSlide);
    }
}
