namespace SlashBar;

/// <summary>
/// Global ephemeral notification (success / error) for launcher commands.
/// </summary>
public static class AppToast {

    public static void ShowSuccess(string message, string? detail = null, int durationMs = 1600) =>
        ToastHost.ShowEphemeral(message, success: true, detail, durationMs);

    public static void ShowError(string message) =>
        ToastHost.ShowEphemeral(message, success: false);
}
