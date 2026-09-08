using SlashBar.Modules.Awake;

namespace SlashBar;

public static class AwakeToast {

    private static bool _wired;


    public static void Show() {
        EnsureWired();

        var title = AwakeSession.Mode == AwakeMode.System
            ? "Awake · system"
            : "Awake";

        string? detail = null;
        if (AwakeSession.EndsAt is { } ends) {
            var left = ends - DateTimeOffset.Now;
            detail = $"({AwakeDuration.FormatRemaining(left)})";
        }

        ToastHost.ShowSticky(title, detail);
    }


    public static void Hide() {
        ToastHost.HideSticky();
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
