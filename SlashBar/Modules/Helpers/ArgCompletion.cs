namespace SlashBar.Modules;

/// <summary>
/// Représente une proposition d'autocomplétion pour un argument de commande
/// </summary>
public sealed record ArgCompletion(string Value, string Description);
