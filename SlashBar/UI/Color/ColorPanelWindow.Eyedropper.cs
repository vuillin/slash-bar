using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using SlashBar.Modules.Color;

namespace SlashBar;

public partial class ColorPanelWindow {

    private const int SampleSize = 11;
    private const int Zoom = 15;
    private const int MagnifierThrottleMs = 16; // ~60 Hz

    private Window? _overlay;
    private Window? _magnifier;
    private System.Windows.Controls.Image? _magnifierImage;
    private WriteableBitmap? _magnifierBitmap;
    private ScreenColorSampler? _sampler;
    private byte[]? _sampleBuffer;

    private System.Windows.Media.Color _hoveredColor;
    private bool _pickModeActive;

    private int _lastSampleX = int.MinValue;
    private int _lastSampleY = int.MinValue;
    private DispatcherTimer? _magnifierThrottle;
    private bool _magnifierUpdatePending;


    private void EnablePickMode() {
        _pickModeActive = true;
        ShowOverlay();
        ShowMagnifier();
        Owner = _overlay;

        // Après Show/Activate du panneau, IsMouseOver peut changer d'un tick
        Dispatcher.BeginInvoke(() => {
            if (_pickModeActive)
                SyncMagnifierVisibility();
        });
    }


    private void DisablePickMode() {
        _pickModeActive = false;
        Owner = null;
        HideOverlay();
        _magnifierUpdatePending = false;
        _lastSampleX = int.MinValue;
        _lastSampleY = int.MinValue;
    }


    /// <summary>Loupe visible hors panneau, cachée dessus.</summary>
    private void SyncMagnifierVisibility() {
        if (!_pickModeActive || _magnifier == null)
            return;

        if (IsMouseOver || IsCollapsed) {
            _magnifier.Hide();
            return;
        }

        UpdateMagnifier(System.Windows.Forms.Cursor.Position, force: true);
        _magnifier.Show();
    }


    private void ShowMagnifier() {
        if (_magnifier != null) {

            _magnifier.Owner = null;
            _magnifier.Owner = _overlay;
            _magnifier.Show();
            return;
        }

        _sampler = new ScreenColorSampler(SampleSize);
        _sampleBuffer = new byte[SampleSize * SampleSize * 4];
        _magnifierBitmap = new WriteableBitmap(
            SampleSize, SampleSize, 96, 96, PixelFormats.Bgra32, null);

        _magnifierImage = new System.Windows.Controls.Image {
            SnapsToDevicePixels = true,
            Stretch = Stretch.Fill,
            Source = _magnifierBitmap,
        };
        RenderOptions.SetBitmapScalingMode(_magnifierImage, BitmapScalingMode.NearestNeighbor);

        int half = SampleSize / 2;

        var grid = new System.Windows.Controls.Grid();
        grid.Children.Add(_magnifierImage);

        grid.Children.Add(new System.Windows.Controls.Border {
            Width = Zoom,
            Height = Zoom,
            BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255)),
            BorderThickness = new Thickness(2),
            Background = System.Windows.Media.Brushes.Transparent,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
            VerticalAlignment = System.Windows.VerticalAlignment.Top,
            Margin = new Thickness(half * Zoom, half * Zoom, 0, 0),
            IsHitTestVisible = false,
        });

        _magnifier = new Window {
            WindowStyle = WindowStyle.None,
            AllowsTransparency = true,
            Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 30, 30)),
            BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255)),
            BorderThickness = new Thickness(1),
            Width = SampleSize * Zoom,
            Height = SampleSize * Zoom,
            Topmost = true,
            ShowInTaskbar = false,
            ShowActivated = false,
            ResizeMode = ResizeMode.NoResize,
            IsHitTestVisible = false,
            Content = grid,
            Owner = _overlay,
            Cursor = System.Windows.Input.Cursors.Cross,
        };

        _magnifierThrottle = new DispatcherTimer {
            Interval = TimeSpan.FromMilliseconds(MagnifierThrottleMs),
        };
        _magnifierThrottle.Tick += (_, _) => {
            _magnifierThrottle.Stop();
            if (!_pickModeActive || !_magnifierUpdatePending)
                return;
            _magnifierUpdatePending = false;
            UpdateMagnifier(System.Windows.Forms.Cursor.Position, force: false);
        };

        _magnifier.Show();
    }


    private void RequestMagnifierUpdate() {
        if (!_pickModeActive)
            return;

        _magnifierUpdatePending = true;
        if (_magnifierThrottle is { IsEnabled: false })
            _magnifierThrottle.Start();
    }


    private void UpdateMagnifier(System.Drawing.Point screenPos, bool force) {
        if (!_pickModeActive || _magnifier == null || _magnifierBitmap == null
            || _sampler == null || _sampleBuffer == null)
            return;

        if (!force && screenPos.X == _lastSampleX && screenPos.Y == _lastSampleY)
            return;

        _lastSampleX = screenPos.X;
        _lastSampleY = screenPos.Y;

        var (r, g, b) = _sampler.Sample(screenPos.X, screenPos.Y, _sampleBuffer);
        _hoveredColor = System.Windows.Media.Color.FromRgb(r, g, b);

        _magnifierBitmap.WritePixels(
            new Int32Rect(0, 0, SampleSize, SampleSize),
            _sampleBuffer,
            SampleSize * 4,
            0);

        if (!_colorLocked)
            ApplyColorPreview(_hoveredColor);

        var dpi = VisualTreeHelper.GetDpi(this);
        _magnifier.Left = (screenPos.X + 24) / dpi.DpiScaleX;
        _magnifier.Top = (screenPos.Y + 24) / dpi.DpiScaleY;
    }


    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);


    private void Overlay_MouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
        if (!_pickModeActive)
            return;

        RequestMagnifierUpdate();

        if (!IsMouseOver)
            _magnifier?.Show();
    }


    private void ShowOverlay() {
        if (_overlay == null) {
            _overlay = new Window {
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(1, 0, 0, 0)),
                Topmost = true,
                ShowInTaskbar = false,
                ResizeMode = ResizeMode.NoResize,
                ShowActivated = false,
                Cursor = System.Windows.Input.Cursors.Cross,
                Left = SystemParameters.VirtualScreenLeft,
                Top = SystemParameters.VirtualScreenTop,
                Width = SystemParameters.VirtualScreenWidth,
                Height = SystemParameters.VirtualScreenHeight,
            };

            _overlay.MouseMove += Overlay_MouseMove;

            _overlay.PreviewKeyDown += (_, e) => {
                switch (e.Key) {
                    case Key.Left:  NudgeCursor(-1, 0); e.Handled = true; break;
                    case Key.Right: NudgeCursor(1, 0); e.Handled = true; break;
                    case Key.Up:    NudgeCursor(0, -1); e.Handled = true; break;
                    case Key.Down:  NudgeCursor(0, 1); e.Handled = true; break;
                    case Key.Escape:
                        AnimateClose();
                        e.Handled = true;
                        break;
                }
            };

            _overlay.MouseLeftButtonDown += (_, e) => {
                if (!_pickModeActive)
                    return;
                _colorLocked = true;
                ApplyColorToUi(_hoveredColor);
                e.Handled = true;
            };
        }

        _overlay.Show();
    }


    private void HideOverlay() {
        _magnifierThrottle?.Stop();
        _overlay?.Hide();
        _magnifier?.Hide();
    }


    private void NudgeCursor(int dx, int dy) {
        if (!_pickModeActive)
            return;

        var p = System.Windows.Forms.Cursor.Position;
        SetCursorPos(p.X + dx, p.Y + dy);

        UpdateMagnifier(System.Windows.Forms.Cursor.Position, force: true);
        SyncMagnifierVisibility();
    }
}
