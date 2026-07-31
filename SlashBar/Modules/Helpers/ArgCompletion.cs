namespace SlashBar.Modules;

/// <summary>
/// Represents an autocomplete suggestion for a command argument.
/// </summary>
public sealed record ArgCompletion(string Value, string Description);
