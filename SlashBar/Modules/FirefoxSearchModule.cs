using System.Diagnostics;

namespace SlashBar.Modules;

public sealed class FirefoxSearchModule : IModule
{

    public string Prefix => "f";
    public string Name => "Recherche Firefox";
    public string Description => "Recherche web dans Firefox";

    public void Execute(string argument) {
        
        if (string.IsNullOrWhiteSpace(argument))
            return;

        // -search = moteur par défaut de Firefox (pas un URL hardcodé)
        var query = argument.Replace("\"", "\\\"");

        Process.Start(new ProcessStartInfo
        {
            FileName = "firefox",
            Arguments = $"-search \"{query}\"",
            UseShellExecute = true
        });
    }
}