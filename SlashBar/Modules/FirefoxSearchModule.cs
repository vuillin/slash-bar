namespace SlashBar.Modules;

/// <summary>
/// Module permettant d'effectuer des recherches Web ou d'ouvrir des URL dans Firefox,
/// avec support du mode navigation privée.
/// </summary>
public sealed class FirefoxSearchModule : IModule {

    private static readonly ArgCompletion[] Flags = [
        new("private", "Recherche en navigation privée")
    ];

    public string Prefix => "f";
    public string Name => "Recherche Firefox";
    public string Description => "Recherche web dans Firefox";

    public ModuleResult Execute(string argument) {
        argument = argument.Trim();
        if (argument.Length == 0) {
            FirefoxHelper.Start();
            return ModuleResult.None;
        }

        var isPrivate = ModuleArgs.ConsumeFlag(ref argument, "private");

        if (argument.Length == 0) {
            if (isPrivate)
                FirefoxHelper.Start("-private-window");
            return ModuleResult.None;
        }

        if (UrlHelper.TryNormalize(argument, out var url)) {
            FirefoxHelper.OpenUrl(url, isPrivate);
            return ModuleResult.None;
        }

        if (isPrivate)
            FirefoxHelper.SearchPrivate(argument);
        else
            FirefoxHelper.Search(argument);

        return ModuleResult.None;
    }

    public IReadOnlyList<ArgCompletion> SuggestCompletions(string argument) =>
        ModuleArgs.SuggestFlags(argument, Flags);
}
