namespace SlashBar.Modules.Setup;

public sealed record SetupStep (
    string FileName,
    string? Arguments = null,
    WindowLayout Layout = WindowLayout.Default,
    string? WindowProcessName = null
);

public sealed record SetupProfile (
    string Name,
    string Description,
    IReadOnlyList<SetupStep> Steps
);