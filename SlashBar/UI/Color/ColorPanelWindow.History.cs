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
    }


    private void OnHistoryChanged() =>
        Dispatcher.Invoke(RefreshHistory);


    private void HistorySwatch_Click(object sender, MouseButtonEventArgs e) {
        if (sender is not FrameworkElement { Tag: ColorHistoryEntry entry })
            return;

        var color = System.Windows.Media.Color.FromRgb(entry.R, entry.G, entry.B);
        _colorLocked = true;
        ApplyColorToUi(color);
        e.Handled = true;
    }


    private sealed class ColorHistoryItem {
        public ColorHistoryEntry Entry { get; }
        public SolidColorBrush Brush { get; }
        public string Hex { get; }

        public ColorHistoryItem(ColorHistoryEntry entry) {
            Entry = entry;
            var color = System.Windows.Media.Color.FromRgb(entry.R, entry.G, entry.B);
            Brush = new SolidColorBrush(color);
            Brush.Freeze();
            Hex = ColorFormats.ToHex(color);
        }
    }
}