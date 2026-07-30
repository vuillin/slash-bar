namespace SlashBar.Modules;

public enum ModuleResultKind {
    None,    // rien à afficher
    Success, // toast vert
    Fail     // toast rouge
}

public sealed class ModuleResult {

    public ModuleResultKind Kind { get; }
    public string Message { get; }

    private ModuleResult(ModuleResultKind kind, string message) {
        Kind = kind;
        Message = message;
    }

    public static ModuleResult None { get; } = new(ModuleResultKind.None, "");

    public static ModuleResult Ok(string message) =>
        new(ModuleResultKind.Success, message);

    public static ModuleResult Error(string message) =>
        new(ModuleResultKind.Fail, message);
}