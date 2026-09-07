namespace SlashBar.Modules;

public enum ModuleResultKind {
    None,    // nothing to show
    Success, // green toast
    Fail     // red toast
}

public sealed class ModuleResult {

    public ModuleResultKind Kind { get; }
    public string Message { get; }
    public string? Detail { get; }
    public int? DurationMs { get; }

    private ModuleResult(
        ModuleResultKind kind,
        string message,
        string? detail = null,
        int? durationMs = null) {
        Kind = kind;
        Message = message;
        Detail = detail;
        DurationMs = durationMs;
    }

    public static ModuleResult None { get; } = new(ModuleResultKind.None, "");

    public static ModuleResult Ok(string message, string? detail = null, int? durationMs = null) =>
        new(ModuleResultKind.Success, message, detail, durationMs);

    public static ModuleResult Copied(string value) =>
        Ok("Copied", value);

    public static ModuleResult Error(string message) =>
        new(ModuleResultKind.Fail, message);
}
