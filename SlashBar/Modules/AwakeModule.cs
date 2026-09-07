using SlashBar.Modules.Awake;

namespace SlashBar.Modules;

public sealed class AwakeModule : IModule {

    private static readonly ArgCompletion[] Flags = [
        new("system", "Keep system awake (display may sleep)"),
        new("30m", "Stay awake for 30 minutes"),
        new("2h", "Stay awake for 2 hours"),
    ];


    public string Prefix => "awake";
    public string Name => "Awake";
    public string Description => "Keep the computer awake";


    public ModuleResult Execute(string argument) {
        if (!TryParseArgs(argument, out var mode, out var duration, out var error))
            return ModuleResult.Error(error);

        // Duration → always start / restart
        if (duration is not null) {
            AwakeSession.Enable(mode, duration);
            AwakeToast.Show();
            return ModuleResult.None;
        }

        // No duration → toggle
        if (AwakeSession.IsActive) {
            AwakeSession.Disable();
            AwakeToast.Hide();
            return ModuleResult.Ok("Awake off");
        }

        AwakeSession.Enable(mode);
        AwakeToast.Show();
        return ModuleResult.None;
    }


    public IReadOnlyList<ArgCompletion> SuggestCompletions(string argument) =>
        ModuleArgs.SuggestFlags(argument, Flags);


    private static bool TryParseArgs(
        string argument,
        out AwakeMode mode,
        out TimeSpan? duration,
        out string error) {

        mode = AwakeMode.Default;
        duration = null;
        error = "";

        var tokens = argument.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var token in tokens) {
            if (token.Equals("system", StringComparison.OrdinalIgnoreCase)) {
                mode = AwakeMode.System;
                continue;
            }

            if (AwakeDuration.TryParse(token, out var d)) {
                if (duration is not null) {
                    error = "Only one duration";
                    return false;
                }
                duration = d;
                continue;
            }

            if (LooksLikeDuration(token)) {
                error = "Max 24h";
                return false;
            }

            error = "Unknown option";
            return false;
        }

        return true;
    }


    private static bool LooksLikeDuration(string token) {
        if (token.Length == 0)
            return false;

        var last = char.ToLowerInvariant(token[^1]);
        var body = last is 'm' or 'h' ? token[..^1] : token;
        return body.Length > 0 && body.All(char.IsDigit);
    }
}