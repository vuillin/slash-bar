namespace SlashBar.Modules;

public enum ModuleResultKind {
    None,    // rien à afficher
    Success, // toast vert
    Fail     // toast rouge
}

public sealed class ModuleResult {

    public ModuleResultKind Kind { get; }
    public string Message { get; }
    public string? Detail { get; }

    private ModuleResult(ModuleResultKind kind, string message, string? detail = null) {
        Kind = kind;
        Message = message;
        Detail = detail;
    }

    public static ModuleResult None { get; } = new(ModuleResultKind.None, "");

    public static ModuleResult Ok(string message, string? detail = null) =>
        new(ModuleResultKind.Success, message, detail);

    public static ModuleResult Copied(string value) =>
        Ok("Copié", value);

    public static ModuleResult Error(string message) =>
        new(ModuleResultKind.Fail, message);
}
