using SlashBar.Modules.Pin;

namespace SlashBar.Modules;

public sealed class PinModule : IModule {

    public const int ToastDurationMs = 3200;

    private static readonly ArgCompletion[] Flags = [
        new("off", "Unpin all pinned windows"),
    ];


    public string Prefix => "pin";
    public string Name => "Pin";
    public string Description => "Keep windows always on top (Ctrl+Shift+A)";


    public ModuleResult Execute(string argument) {
        var arg = argument.Trim();

        if (arg.Equals("off", StringComparison.OrdinalIgnoreCase)) {
            var n = PinSession.UnpinAll();
            return n == 0
                ? ModuleResult.Ok("Nothing pinned", durationMs: ToastDurationMs)
                : ModuleResult.Ok(
                    n == 1 ? "Unpinned 1 window" : $"Unpinned {n} windows",
                    durationMs: ToastDurationMs);
        }

        if (arg.Length > 0)
            return ModuleResult.Error("Unknown option");

        var count = PinSession.Count;
        return count == 0
            ? ModuleResult.Ok("Nothing pinned", "Ctrl+Shift+A to pin", ToastDurationMs)
            : ModuleResult.Ok(
                count == 1 ? "1 window pinned" : $"{count} windows pinned",
                "Ctrl+Shift+A to pin off",
                ToastDurationMs);
    }


    public IReadOnlyList<ArgCompletion> SuggestCompletions(string argument) =>
        ModuleArgs.SuggestFlags(argument, Flags);
}
