namespace SlashBar.Modules;

// IModule implementations in this folder are discovered automatically.
public interface IModule
{
    string Prefix { get; }
    string Name { get; }
    string Description { get; }

    // "f chatgpt" → argument = "chatgpt"
    Task<ModuleResult> ExecuteAsync(string argument);

    // tab / ghost text
    IReadOnlyList<ArgCompletion> SuggestCompletions(string argument) => Array.Empty<ArgCompletion>();
}
