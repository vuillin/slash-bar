using System.Windows;
using SlashBar.Modules.Awake;

namespace SlashBar;

public static class AwakeToast {

    private static AwakeToastWindow? _window;
    private static bool _wired;


    public static void Show() {
        EnsureWired();

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


    private static void EnsureWired() {
        if (_wired)
            return;
        _wired = true;

        AwakeSession.Changed += () => {
            if (AwakeSession.IsActive)
                Show();
        };

        AwakeSession.Expired += () => {
            Hide();
            AppToast.ShowSuccess("Awake off");
        };
    }
}