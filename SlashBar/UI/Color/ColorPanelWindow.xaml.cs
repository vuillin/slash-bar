using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DrawingSize = System.Drawing.Size;

namespace SlashBar;

public partial class ColorPanelWindow : Window {

    private const double PanelWidth = 400;
    private const double LeftMargin = 14;
    private const double TabWidth = 32;

    // panneau hors écran, onglet visible au bord
    private static double CollapsedX => -(LeftMargin + PanelWidth);

    private const int SampleSize = 11;
    private const int Zoom = 15;

    private static ColorPanelWindow? _instance;

    private Window? _overlay;
    private Window? _magnifier;
    private System.Windows.Controls.Image? _magnifierImage;

    private bool _colorLocked;
    private System.Windows.Media.Color _hoveredColor;
    private bool _pickModeActive;

    private bool _isCollapsed;
    private bool _isAnimating;
    private bool _isDetached;


    private ColorPanelWindow() {
        InitializeComponent();
        Width = LeftMargin + PanelWidth + TabWidth;

        MouseEnter += (_, _) => {
            if (_pickModeActive)
                _magnifier?.Hide();
        };

        MouseLeave += (_, _) => {
            if (!_pickModeActive || !IsVisible || _isCollapsed)
                return;

            UpdateMagnifier(System.Windows.Forms.Cursor.Position);
            _magnifier?.Show();
        };

        PreviewKeyDown += (_, e) => {
            if (e.Key == Key.Escape) {
                AnimateClose();
                e.Handled = true;
                return;
            }

            if (!_pickModeActive)
                return;

            switch (e.Key) {
                case Key.Left:  NudgeCursor(-1, 0); e.Handled = true; break;
                case Key.Right: NudgeCursor(1,  0); e.Handled = true; break;
                case Key.Up:    NudgeCursor(0, -1); e.Handled = true; break;
                case Key.Down:  NudgeCursor(0,  1); e.Handled = true; break;
            }
        };
    }


    public static void Toggle() {
        _instance ??= new ColorPanelWindow();

        if (_instance.IsVisible)
            _instance.AnimateClose();
        else
            _instance.AnimateOpen();
    }


    private void CloseButton_Click(object sender, RoutedEventArgs e) =>
        AnimateClose();

    private void ResetButton_Click(object sender, RoutedEventArgs e) =>
        ResetToDock();

    private void DockButton_Click(object sender, RoutedEventArgs e) {
        if (_isDetached)
            return;

        if (_isCollapsed)
            AnimateExpand();
        else
            AnimateCollapse();
    }


    private void HeaderBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
        if (_isCollapsed || _isAnimating)
            return;

        if (FindAncestor<System.Windows.Controls.Button>(e.OriginalSource as DependencyObject) != null)
            return;

        if (!_isDetached)
            EnterDetachedMode();

        DragMove();
    }


    private void EnablePickMode() {
        if (_pickModeActive)
            return;

        _pickModeActive = true;
        ShowOverlay();
        ShowMagnifier();
        Owner = _overlay;

        // Clic languette : la souris est déjà sur le panneau → MouseEnter ne se rejoue pas
        if (IsMouseOver)
            _magnifier?.Hide();
    }


    private void DisablePickMode() {
        _pickModeActive = false;
        Owner = null;
        HideOverlay();
    }


    private void ShowMagnifier() {
        if (_magnifier != null) {
            _magnifier.Owner = _overlay;
            _magnifier.Show();
            return;
        }

        _magnifierImage = new System.Windows.Controls.Image {
            SnapsToDevicePixels = true,
            Stretch = Stretch.Fill,
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

        _magnifier.Show();
    }


    private void UpdateMagnifier(System.Drawing.Point screenPos) {
        if (!_pickModeActive || _magnifier == null || _magnifierImage == null)
            return;

        int half = SampleSize / 2;
        int srcX = screenPos.X - half;
        int srcY = screenPos.Y - half;

        using var bmp = new Bitmap(SampleSize, SampleSize);
        using (var g = Graphics.FromImage(bmp)) {
            g.CopyFromScreen(srcX, srcY, 0, 0, new DrawingSize(SampleSize, SampleSize));

            var pixel = bmp.GetPixel(half, half);
            _hoveredColor = System.Windows.Media.Color.FromRgb(pixel.R, pixel.G, pixel.B);

            if (!_colorLocked)
                ApplyColorToUi(_hoveredColor);
        }

        var hBitmap = bmp.GetHbitmap();
        try {
            var source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            source.Freeze();
            _magnifierImage.Source = source;
        }
        finally {
            DeleteObject(hBitmap);
        }

        var dpi = VisualTreeHelper.GetDpi(this);
        _magnifier.Left = (screenPos.X + 24) / dpi.DpiScaleX;
        _magnifier.Top = (screenPos.Y + 24) / dpi.DpiScaleY;
    }


    [System.Runtime.InteropServices.DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);


    private void Overlay_MouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
        if (!_pickModeActive)
            return;
        UpdateMagnifier(System.Windows.Forms.Cursor.Position);
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
        _overlay?.Hide();
        _magnifier?.Hide();
    }


    private void NudgeCursor(int dx, int dy) {
        if (!_pickModeActive)
            return;

        var p = System.Windows.Forms.Cursor.Position;
        SetCursorPos(p.X + dx, p.Y + dy);

        UpdateMagnifier(System.Windows.Forms.Cursor.Position);

        if (!IsMouseOver)
            _magnifier?.Show();
    }


    private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject {
        while (current != null) {
            if (current is T match)
                return match;
            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }
}
