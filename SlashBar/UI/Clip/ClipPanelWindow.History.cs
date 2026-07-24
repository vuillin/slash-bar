using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
            ShowCopiedToast();
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

    private void ShowCopiedToast() {
        CopiedToast.BeginAnimation(OpacityProperty, null);
        CopiedToastSlide.BeginAnimation(TranslateTransform.YProperty, null);

        var easeOut = new QuadraticEase { EasingMode = EasingMode.EaseOut };
        var easeIn = new QuadraticEase { EasingMode = EasingMode.EaseIn };

        var opacity = new DoubleAnimationUsingKeyFrames();
        opacity.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        opacity.KeyFrames.Add(new EasingDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(160))) {
            EasingFunction = easeOut
        });
        opacity.KeyFrames.Add(new DiscreteDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1000))));
        opacity.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1220))) {
            EasingFunction = easeIn
        });

        var slide = new DoubleAnimationUsingKeyFrames();
        slide.KeyFrames.Add(new EasingDoubleKeyFrame(8, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        slide.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(180))) {
            EasingFunction = easeOut
        });
        slide.KeyFrames.Add(new DiscreteDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1000))));
        slide.KeyFrames.Add(new EasingDoubleKeyFrame(6, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1220))) {
            EasingFunction = easeIn
        });

        CopiedToast.BeginAnimation(OpacityProperty, opacity);
        CopiedToastSlide.BeginAnimation(TranslateTransform.YProperty, slide);
    }
}
