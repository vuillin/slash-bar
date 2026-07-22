using System.Diagnostics;

namespace SlashBar.Modules;

/// <summary>
/// Module permettant d'effectuer des recherches Web ou d'ouvrir des URL dans Firefox,
/// avec support du mode navigation privée.
/// </summary>
public sealed class FirefoxSearchModule : IModule
{

    private static readonly ArgCompletion[] Flags = [
        new("private", "Recherche en navigation privée")
    ];

    public string Prefix => "f";
    public string Name => "Recherche Firefox";
    public string Description => "Recherche web dans Firefox";

    public void Execute(string argument) {

        argument = argument.Trim();
        if (argument.Length == 0)
            return;
        
        var isPrivate = ModuleArgs.ConsumeFlag(ref argument, "private");

        if (argument.Length == 0) {

            // "f private" seul -> fenêtre privée
            if (isPrivate)
                StartFirefox("-private-window");
            return;
        }

        if (UrlHelper.TryNormalize(argument, out var url)) {
            OpenUrl(url, isPrivate);
            return;
        }

        if (isPrivate) {

            var searchUrl = "https://duckduckgo.com/?q=" + Uri.EscapeDataString(argument);
            StartFirefox($"-private-window \"{searchUrl}\"");

        } else {

            var query = argument.Replace("\"", "\\\"");
            StartFirefox($"-search \"{query}\"");
        }
    }

    public IReadOnlyList<ArgCompletion> SuggestCompletions(string argument) =>
        ModuleArgs.SuggestFlags(argument, Flags);


    private static void StartFirefox(string args) {
        
        Process.Start(new ProcessStartInfo {
            FileName = "firefox",
            Arguments = args,
            UseShellExecute = true
        });
    }


    private static void OpenUrl(string url, bool isPrivate) {

        var args = isPrivate
            ? $"-private-window \"{url}\""
            : $"\"{url}\"";

        StartFirefox(args);
    }
}
