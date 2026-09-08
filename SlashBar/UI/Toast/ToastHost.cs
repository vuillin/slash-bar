using System.Windows;

namespace SlashBar;

/// <summary>
/// Single top-right toast host: sticky (Awake) + ephemeral (success / error).
/// </summary>
public static class ToastHost {

    private static ToastHostWindow? _window;


    public static void ShowEphemeral(
        string message,
        bool success,
        string? detail = null,
        int durationMs = 1600) {

        if (string.IsNullOrWhiteSpace(message))
            return;

        RunOnUi(() => EnsureWindow().ShowEphemeral(message, success, detail, durationMs));
    }


    public static void ShowSticky(string title, string? detail = null) =>
        RunOnUi(() => EnsureWindow().ShowSticky(title, detail));


    public static void HideSticky() =>
        RunOnUi(() => _window?.HideSticky());


    private static ToastHostWindow EnsureWindow() =>
        _window ??= new ToastHostWindow();


    private static void RunOnUi(Action action) {
        var app = System.Windows.Application.Current;
        if (app == null)
            return;

        if (app.Dispatcher.CheckAccess())
            action();
        else
            app.Dispatcher.Invoke(action);
    }
}
