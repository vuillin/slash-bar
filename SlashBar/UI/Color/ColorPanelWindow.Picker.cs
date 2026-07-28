using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using SlashBar.Modules;
using SlashBar.Modules.Color;
using SlashBar.UI.Shell;

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


    /// <summary>Preview rapide pendant le hover eyedropper (sans repositionner les curseurs HSV).</summary>
    private void ApplyColorPreview(System.Windows.Media.Color color) {
        _selectedColor = color;
        HsvColorConverter.RgbToHsv(color.R, color.G, color.B, out _hue, out _saturation, out _value);

        PreviewSwatch.Background = new SolidColorBrush(color);
        var hex = ColorFormats.ToHex(color);
        PreviewHexText.Text = hex;
        HexCardValue.Text = hex;
        RgbCardValue.Text = ColorFormats.ToRgbDisplay(color);
        RgbCardValue.Tag = ColorFormats.ToRgbClipboard(color);
    }


    private void SetSelectedColor(System.Windows.Media.Color color, bool fromPicker) {
        _selectedColor = color;

        if (!fromPicker)
            HsvColorConverter.RgbToHsv(color.R, color.G, color.B, out _hue, out _saturation, out _value);

        RefreshPickerUi();
    }


    private void RefreshPickerUi() {
        PreviewSwatch.Background = new SolidColorBrush(_selectedColor);

        var hex = ColorFormats.ToHex(_selectedColor);
        PreviewHexText.Text = hex;
        HexCardValue.Text = hex;
        RgbCardValue.Text = ColorFormats.ToRgbDisplay(_selectedColor);
        RgbCardValue.Tag = ColorFormats.ToRgbClipboard(_selectedColor);

        SatValHueLayer.Background = new SolidColorBrush(HsvColorConverter.HsvToRgb(_hue, 1, 1));
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

        // Le curseur est frère de HueBar (pas clipé) → positionné sur la même largeur
        var x = _hue / 360.0 * w - HueThumb.Width / 2;
        HueThumb.Margin = new Thickness(x, 0, 0, 0);
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
        var w = HueBar.ActualWidth;
        if (w <= 0)
            return;

        _hue = Math.Clamp(pos.X / w * 360.0, 0, 360);
        _colorLocked = true;
        CommitHsvToColor();
    }


    private void CommitHsvToColor() {
        _selectedColor = HsvColorConverter.HsvToRgb(_hue, _saturation, _value);
        RefreshPickerUi();
    }


    private void CopyHex_Click(object sender, RoutedEventArgs e) {
        ClipboardHelper.SetText(ColorFormats.ToHex(_selectedColor));
        ColorHistory.Store.Add(_selectedColor);
        CopiedToastAnimator.Show(CopiedToast, CopiedToastSlide);
    }


    private void CopyRgb_Click(object sender, RoutedEventArgs e) {
        var rgb = RgbCardValue.Tag as string
            ?? ColorFormats.ToRgbClipboard(_selectedColor);
        ClipboardHelper.SetText(rgb);
        ColorHistory.Store.Add(_selectedColor);
        CopiedToastAnimator.Show(CopiedToast, CopiedToastSlide);
    }
}
