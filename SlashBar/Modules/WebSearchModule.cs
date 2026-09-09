namespace SlashBar.Modules;

/// <summary>
/// Web search or open URLs in the default browser, with private browsing support.
/// </summary>
public sealed class WebSearchModule : IModule {

    private static readonly ArgCompletion[] Flags = [
        new("private", "Search in a private window")
    ];

    public string Prefix => "web";
    public string Name => "Web search";
    public string Description => "Web search in the default browser";

    public ModuleResult Execute(string argument) {
        argument = argument.Trim();
        if (argument.Length == 0) {
            BrowserHelper.Start();
            return ModuleResult.None;
        }

        var isPrivate = ModuleArgs.ConsumeFlag(ref argument, "private");

        if (argument.Length == 0) {
            if (isPrivate)
                BrowserHelper.Start(privateWindow: true);
            return ModuleResult.None;
        }

        if (UrlHelper.TryNormalize(argument, out var url)) {
            BrowserHelper.OpenUrl(url, isPrivate);
            return ModuleResult.None;
        }

        if (isPrivate)
            BrowserHelper.SearchPrivate(argument);
        else
            BrowserHelper.Search(argument);

        return ModuleResult.None;
    }

    public IReadOnlyList<ArgCompletion> SuggestCompletions(string argument) =>
        ModuleArgs.SuggestFlags(argument, Flags);
}