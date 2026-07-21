using System.Diagnostics;

namespace SlashBar.Modules;

public sealed class FirefoxSearchModule : IModule
{

    private static readonly string[] Flags = ["private"];

    public string Prefix => "f";
    public string Name => "Recherche Firefox";
    public string Description => "Recherche web dans Firefox";

    public void Execute(string argument) {

        argument = argument.Trim();
        if (argument.Length == 0)
            return;
        
        var isPrivate = ConsumeFlag(ref argument, "private");

        if (argument.Length == 0) {

            // "f private" seul -> fenêtre privée
            if (isPrivate)
                StartFirefox("-private-window");
            return;
        }

        if (isPrivate) {

            var url = "https://duckduckgo.com/?q=" + Uri.EscapeDataString(argument);
            StartFirefox($"-private-window \"{url}\"");

        } else {

            var query = argument.Replace("\"", "\\\"");
            StartFirefox($"-search \"{query}\"");
        }
    }

    public IReadOnlyList<string> SuggestCompletions(string argument) {

        // après le 1er mot = requête libre
        if (argument.Contains(' '))
            return Array.Empty<string>();

        return Flags
            .Where(f => f.StartsWith(argument, StringComparison.OrdinalIgnoreCase)
                        && !f.Equals(argument, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }


    private static bool ConsumeFlag(ref string argument, string flag) {

        if (argument.Equals(flag, StringComparison.OrdinalIgnoreCase)) {
            argument = "";
            return true;
        }

        var withSpace = flag + " ";
        if (argument.StartsWith(withSpace, StringComparison.OrdinalIgnoreCase)) {
            argument = argument[withSpace.Length..].Trim();
            return true;
        }

        return false;
    }


    private static void StartFirefox(string args) {
        
        Process.Start(new ProcessStartInfo {
            FileName = "firefox",
            Arguments = args,
            UseShellExecute = true
        });
    }
}
