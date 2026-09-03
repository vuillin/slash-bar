using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using SlashBar.Modules.Color;

namespace SlashBar;

public partial class ColorPanelWindow {


    private bool _historySubscribed;


    private void SubscribeHistory() {
        if (_historySubscribed)
            return;

        ColorHistory.Store.Changed += OnHistoryChanged;
        _historySubscribed = true;
    }


    private void RefreshHistory() {
        var items = ColorHistory.Store.GetAll()
            .Select(e => new ColorHistoryItem(e))
            .ToList();

        HistoryList.ItemsSource = items;
        HistorySection.Visibility = items.Count > 0
            ? Visibility.Visible
            : Visibility.Collapsed;
        SyncPanelHeight();
    }


    private void SyncPanelHeight() {
        if (!IsVisible || IsAnimating)
            return;

        var pad = (Thickness)System.Windows.Application.Current.FindResource("SidePanelShadowMargin");
        Height = DockedHeight(0) + pad.Top + pad.Bottom;
    }


    private void OnHistoryChanged() =>
        Dispatcher.Invoke(RefreshHistory);


    private void HistorySwatch_Click(object sender, MouseButtonEventArgs e) {
        if (sender is not FrameworkElement { Tag: ColorHistoryEntry entry })
            return;

        var color = System.Windows.Media.Color.FromRgb(entry.R, entry.G, entry.B);
        _colorLocked = true;
        SetSelectedColor(color);
        e.Handled = true;
    }


    private sealed class ColorHistoryItem {
        private static readonly SolidColorBrush HexOnDark = Freeze(Colors.White);
        private static readonly SolidColorBrush HexOnLight = Freeze(Colors.Black);

        public ColorHistoryEntry Entry { get; }
        public SolidColorBrush Brush { get; }
        public SolidColorBrush HexForeground { get; }
        public string Hex { get; }

        public ColorHistoryItem(ColorHistoryEntry entry) {
            Entry = entry;
            var color = System.Windows.Media.Color.FromRgb(entry.R, entry.G, entry.B);
            Brush = new SolidColorBrush(color);
            Brush.Freeze();
            Hex = ColorFormats.ToHex(color);
            HexForeground = PerceivedLuminance(color) >= 160 ? HexOnLight : HexOnDark;
        }

        private static int PerceivedLuminance(System.Windows.Media.Color color) =>
            (color.R * 299 + color.G * 587 + color.B * 114) / 1000;

        private static SolidColorBrush Freeze(System.Windows.Media.Color color) {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }
    }
}