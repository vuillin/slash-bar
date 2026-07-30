namespace SlashBar.Modules;

/// <summary>
/// Ouvre la doc officielle (accueil ou recherche si dispo)
/// </summary>
public sealed class DocModule : IModule {

    public string Prefix => "doc";
    public string Name => "Documentation";
    public string Description => "Ouvre la documentation";


    public ModuleResult Execute(string argument) {
        argument = argument.Trim();
        if (argument.Length == 0)
            return ModuleResult.Error("Langage requis");

        var space = argument.IndexOf(' ');
        var lang = space < 0 ? argument : argument[..space];
        var query = space < 0 ? "" : argument[(space + 1)..].Trim();

        if (!DocSources.ById.TryGetValue(lang, out var source))
            return ModuleResult.Error("Doc introuvable");

        var url = query.Length > 0 && source.SearchUrl != null
            ? string.Format(source.SearchUrl, Uri.EscapeDataString(query))
            : source.HomeUrl;

        FirefoxHelper.OpenNewTab(url);
        return ModuleResult.None;
    }


    public IReadOnlyList<ArgCompletion> SuggestCompletions(string argument) {
        ModuleArgs.SplitCurrentToken(argument, out var before, out var token);

        if (before.Length == 0)
            return ModuleArgs.SuggestFlags(token, DocSources.Flags);

        return Array.Empty<ArgCompletion>();
    }
}
