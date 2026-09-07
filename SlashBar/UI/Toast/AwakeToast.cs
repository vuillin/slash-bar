using System.Windows;

namespace SlashBar;

/// <summary>
/// Persistent top-right indicator while awake mode is active.
/// </summary>
public static class AwakeToast {

    private static AwakeToastWindow? _window;


    public static void Show() {
        var app = System.Windows.Application.Current;
        if (app == null)
            return;

        void ShowCore() {
            _window ??= new AwakeToastWindow();
            _window.ShowIndicator();
        }

        if (app.Dispatcher.CheckAccess())
            ShowCore();
        else
            app.Dispatcher.Invoke(ShowCore);
    }


    public static void Hide() {
        var app = System.Windows.Application.Current;
        if (app == null)
            return;

        void HideCore() => _window?.HideIndicator();

        if (app.Dispatcher.CheckAccess())
            HideCore();
        else
            app.Dispatcher.Invoke(HideCore);
    }
}