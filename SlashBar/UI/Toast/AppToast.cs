using System.Windows;

namespace SlashBar;

/// <summary>
/// Global top-right notification (success / error) for launcher commands.
/// </summary>
public static class AppToast {

    private static AppToastWindow? _window;


    public static void ShowSuccess(string message, string? detail = null, int durationMs = 1600) =>
        Show(message, success: true, detail, durationMs);

    public static void ShowError(string message) =>
        Show(message, success: false);


    private static void Show(string message, bool success, string? detail = null, int durationMs = 1600) {
        if (string.IsNullOrWhiteSpace(message))
            return;

        var app = System.Windows.Application.Current;
        if (app == null)
            return;

        void ShowCore() {
            _window ??= new AppToastWindow();
            if (_window.IsVisible)
                _window.CancelAndHide();

            _window.ShowToast(message, success, detail, durationMs);
        }

        if (app.Dispatcher.CheckAccess())
            ShowCore();
        else
            app.Dispatcher.Invoke(ShowCore);
    }
}
