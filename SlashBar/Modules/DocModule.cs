namespace SlashBar.Modules;

/// <summary>
/// Ouvre la doc officielle (accueil ou recherche si dispo)
/// </summary>
public sealed class DocModule : IModule {

    public string Prefix => "doc";
    public string Name => "Documentation";
    public string Description => "Ouvre la documentation";


    public void Execute(string argument) {

        argument = argument.Trim();
        if (argument.Length == 0)
            return;

        var space = argument.IndexOf(' ');
        var lang = space < 0 ? argument : argument[..space];
        var query = space < 0 ? "" : argument[(space + 1)..].Trim();

        if (!DocSources.ById.TryGetValue(lang, out var source))
            return;

        // recherche seulement si requête + URL de search native
        var url = query.Length > 0 && source.SearchUrl != null
            ? string.Format(source.SearchUrl, Uri.EscapeDataString(query))
            : source.HomeUrl;

        FirefoxHelper.OpenNewTab(url);
    }


    public IReadOnlyList<ArgCompletion> SuggestCompletions(string argument) {

        ModuleArgs.SplitCurrentToken(argument, out var before, out var token);

        if (before.Length == 0)
            return ModuleArgs.SuggestFlags(token, DocSources.Flags);

        return Array.Empty<ArgCompletion>();
    }
}
