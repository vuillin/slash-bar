using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SlashBar;

public partial class ColorPanelWindow {

    private double _hue;         // 0–360
    private double _saturation;  // 0–1
    private double _value;       // 0–1 (luminosité du carré SV)

    private System.Windows.Media.Color _selectedColor;
    private bool _draggingSatVal;
    private bool _draggingHue;


    private void Window_Loaded(object sender, RoutedEventArgs e) {
        SatValPicker.SizeChanged += (_, _) => PlaceSatValMarker();
        HueBar.SizeChanged += (_, _) => PlaceHueThumb();
        SetSelectedColor(System.Windows.Media.Color.FromRgb(128, 128, 128), fromPicker: false);
    }


    private void ApplyColorToUi(System.Windows.Media.Color color) =>
        SetSelectedColor(color, fromPicker: false);


    private void SetSelectedColor(System.Windows.Media.Color color, bool fromPicker) {
        _selectedColor = color;

        if (!fromPicker)
            RgbToHsv(color.R, color.G, color.B, out _hue, out _saturation, out _value);

        RefreshPickerUi();
    }


    private void RefreshPickerUi() {
        var brush = new SolidColorBrush(_selectedColor);
        PreviewSwatch.Background = brush;

        var hex = $"#{_selectedColor.R:X2}{_selectedColor.G:X2}{_selectedColor.B:X2}".ToLowerInvariant();
        var rgb = $"rgb({_selectedColor.R}, {_selectedColor.G}, {_selectedColor.B})";

        PreviewHexText.Text = hex;
        PreviewRgbText.Text = rgb;
        HexCardValue.Text = hex;
        RgbCardValue.Text = rgb;

        SatValHueLayer.Background = new SolidColorBrush(HsvToRgb(_hue, 1, 1));
        PlaceSatValMarker();
        PlaceHueThumb();
    }


    private void PlaceSatValMarker() {
        var w = SatValPicker.ActualWidth;
        var h = SatValPicker.ActualHeight;
        if (w <= 0 || h <= 0)
            return;

        var x = _saturation * w - SatValMarker.Width / 2;
        var y = (1 - _value) * h - SatValMarker.Height / 2;

        x = Math.Clamp(x, -SatValMarker.Width / 2, w - SatValMarker.Width / 2);
        y = Math.Clamp(y, -SatValMarker.Height / 2, h - SatValMarker.Height / 2);

        SatValMarker.Margin = new Thickness(x, y, 0, 0);
    }


    private void PlaceHueThumb() {
        var w = HueBar.ActualWidth;
        if (w <= 0)
            return;

        var x = _hue / 360.0 * w;
        HueThumb.Margin = new Thickness(x - HueThumb.Width / 2, 0, 0, 0);
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

        _saturation = Math.Clamp(pos.X / w, 0, 1);
        _value = Math.Clamp(1 - pos.Y / h, 0, 1);

        _colorLocked = true;
        CommitHsvToColor(fromPicker: true);
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
        var w = HueBar.ActualWidth;
        if (w <= 0)
            return;

        _hue = Math.Clamp(pos.X / w * 360.0, 0, 360);
        _colorLocked = true;
        CommitHsvToColor(fromPicker: true);
    }


    private void CommitHsvToColor(bool fromPicker) {
        _selectedColor = HsvToRgb(_hue, _saturation, _value);
        RefreshPickerUi();
    }


    private void CopyHex_Click(object sender, RoutedEventArgs e) {
        var hex = $"#{_selectedColor.R:X2}{_selectedColor.G:X2}{_selectedColor.B:X2}".ToLowerInvariant();
        System.Windows.Clipboard.SetText(hex);
    }


    private void CopyRgb_Click(object sender, RoutedEventArgs e) {
        var rgb = $"rgb({_selectedColor.R}, {_selectedColor.G}, {_selectedColor.B})";
        System.Windows.Clipboard.SetText(rgb);
    }


    private static System.Windows.Media.Color HsvToRgb(double h, double s, double v) {
        var c = v * s;
        var x = c * (1 - Math.Abs(h / 60.0 % 2 - 1));
        var m = v - c;

        double r, g, b;
        if (h < 60) { r = c; g = x; b = 0; }
        else if (h < 120) { r = x; g = c; b = 0; }
        else if (h < 180) { r = 0; g = c; b = x; }
        else if (h < 240) { r = 0; g = x; b = c; }
        else if (h < 300) { r = x; g = 0; b = c; }
        else { r = c; g = 0; b = x; }

        return System.Windows.Media.Color.FromRgb(
            (byte)Math.Round((r + m) * 255),
            (byte)Math.Round((g + m) * 255),
            (byte)Math.Round((b + m) * 255));
    }


    private static void RgbToHsv(byte r, byte g, byte b, out double h, out double s, out double v) {
        var rf = r / 255.0;
        var gf = g / 255.0;
        var bf = b / 255.0;

        var max = Math.Max(rf, Math.Max(gf, bf));
        var min = Math.Min(rf, Math.Min(gf, bf));
        var delta = max - min;

        v = max;
        s = max == 0 ? 0 : delta / max;

        if (delta == 0)
            h = 0;
        else if (max == rf)
            h = 60 * (((gf - bf) / delta) % 6);
        else if (max == gf)
            h = 60 * (((bf - rf) / delta) + 2);
        else
            h = 60 * (((rf - gf) / delta) + 4);

        if (h < 0)
            h += 360;
    }
}
