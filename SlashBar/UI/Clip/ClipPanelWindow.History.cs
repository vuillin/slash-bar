using System.Windows;
using System.Windows.Input;
using SlashBar.Modules;
using SlashBar.Modules.Clipboard;

namespace SlashBar;

public partial class ClipPanelWindow {

    private void SubscribeHistory() {
        if (_historySubscribed)
            return;

        ClipboardHistory.Store.Changed += OnHistoryChanged;
        _historySubscribed = true;
    }

    private void RefreshHistory() {
        HistoryList.ItemsSource = ClipboardHistory.Store.GetAll();
    }

    private void OnHistoryChanged() =>
        Dispatcher.Invoke(RefreshHistory);

    private void HistoryItem_Click(object sender, MouseButtonEventArgs e) {
        if (sender is FrameworkElement { Tag: ClipboardHistoryEntry entry }) {
            ClipboardHistory.Watcher.IgnoreNext();
            ClipboardHelper.SetText(entry.Text);
        }
    }

    private void DeleteHistoryItem_Click(object sender, RoutedEventArgs e) {
        e.Handled = true;

        if (sender is FrameworkElement { Tag: ClipboardHistoryEntry entry })
            ClipboardHistory.Store.Remove(entry.Id);
    }
}
