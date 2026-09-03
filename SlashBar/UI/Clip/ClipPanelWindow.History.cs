using System.Windows;
using System.Windows.Input;
using SlashBar.Modules;
using SlashBar.Modules.Clipboard;
using SlashBar.UI.Shell;

namespace SlashBar;

public partial class ClipPanelWindow {

    private void SubscribeHistory() {
        if (_historySubscribed)
            return;

        ClipboardHistory.Store.Changed += OnHistoryChanged;
        _historySubscribed = true;
    }

    private void RefreshHistory() {
        var items = ClipboardHistory.Store.GetAll()
            .Select(e => new ClipboardHistoryItem(e))
            .ToList();

        HistoryList.ItemsSource = items;
        EntryCountText.Text = items.Count.ToString();
    }

    private void OnHistoryChanged() =>
        Dispatcher.Invoke(RefreshHistory);

    private void HistoryItem_Click(object sender, MouseButtonEventArgs e) {
        if (sender is FrameworkElement { Tag: ClipboardHistoryEntry entry }) {
            ClipboardHistory.Watcher.IgnoreNext();
            ClipboardHelper.SetText(entry.Text);
            CopiedToastAnimator.Show(CopiedToast, CopiedToastSlide);
        }
    }

    private void DeleteHistoryItem_Click(object sender, RoutedEventArgs e) {
        e.Handled = true;

        if (sender is FrameworkElement { Tag: ClipboardHistoryEntry entry })
            ClipboardHistory.Store.Remove(entry.Id);
    }

    private void ClearAllHistory_Click(object sender, RoutedEventArgs e) {
        ClipboardHistory.Store.ClearAll();
    }


    private sealed class ClipboardHistoryItem {
        public ClipboardHistoryEntry Entry { get; }
        public string Text => Entry.Text;
        public System.Windows.Media.ImageSource Icon { get; }

        public ClipboardHistoryItem(ClipboardHistoryEntry entry) {
            Entry = entry;
            Icon = IconFor(ClipboardContentClassifier.Classify(entry.Text));
        }

        private static System.Windows.Media.ImageSource IconFor(ClipboardContentKind kind) {
            var key = kind switch {
                ClipboardContentKind.Color => "IconClipKindColor",
                ClipboardContentKind.Code => "IconClipKindCode",
                ClipboardContentKind.FilePath => "IconClipKindFile",
                ClipboardContentKind.Url => "IconClipKindUrl",
                ClipboardContentKind.Email => "IconClipKindMail",
                ClipboardContentKind.LongText => "IconClipKindLongText",
                _ => "IconClipKindOther"
            };
            return (System.Windows.Media.ImageSource)System.Windows.Application.Current.FindResource(key);
        }
    }
}
