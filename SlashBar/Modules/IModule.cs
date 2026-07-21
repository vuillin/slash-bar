namespace SlashBar.Modules;

// classes IModule du dossier = chargées auto
public interface IModule
{
    string Prefix { get; }
    string Name { get; }
    string Description { get; }

    // "f chatgpt" → argument = "chatgpt"
    void Execute(string argument);

    // tab / ghost text
    IReadOnlyList<string> SuggestCompletions(string argument) => Array.Empty<string>();
}
