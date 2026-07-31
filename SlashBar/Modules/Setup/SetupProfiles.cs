namespace SlashBar.Modules.Setup;

/// <summary>
/// User setup profiles — loaded from %LocalAppData%/SlashBar/setup-profiles.json
/// </summary>
public static class SetupProfiles {
    public static SetupProfileStore Store { get; } = new();
}
