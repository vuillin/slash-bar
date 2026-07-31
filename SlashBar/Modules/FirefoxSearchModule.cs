namespace SlashBar.Modules;

/// <summary>
/// Web search or open URLs in Firefox, with private browsing support.
/// </summary>
public sealed class FirefoxSearchModule : IModule {

    private static readonly ArgCompletion[] Flags = [
        new("private", "Search in a private window")
    ];

    public string Prefix => "f";
    public string Name => "Firefox search";
    public string Description => "Web search in Firefox";

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
