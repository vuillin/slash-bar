using System.Windows;

namespace SlashBar;

/// <summary>
/// Notif globale haut-droite (succès / erreur) pour les commandes lanceur.
/// </summary>
public static class AppToast {

    private static AppToastWindow? _window;


    public static void ShowSuccess(string message) =>
        Show(message, success: true);

    public static void ShowError(string message) =>
        Show(message, success: false);


    private static void Show(string message, bool success) {
        if (string.IsNullOrWhiteSpace(message))
            return;

        var app = System.Windows.Application.Current;
        if (app == null)
            return;

        void ShowCore() {
            _window ??= new AppToastWindow();
            if (_window.IsVisible)
                _window.CancelAndHide();

            _window.ShowToast(message, success);
        }

        if (app.Dispatcher.CheckAccess())
            ShowCore();
        else
            app.Dispatcher.Invoke(ShowCore);
    }
}
