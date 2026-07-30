using SlashBar.Modules.Memo;

namespace SlashBar.Modules;

public sealed class MemoModule : IModule {

    public string Prefix => "memo";
    public string Name => "Memo";
    public string Description => "Raccourcis de texte à copier";


    public ModuleResult Execute(string argument) {
        argument = argument.Trim().ToLowerInvariant();

        if (argument.Length == 0) {
            SlashBar.MemoPanelWindow.Toggle();
            return ModuleResult.None;
        }

        var entry = MemoBook.Store.FindByName(argument);
        if (entry == null)
            return ModuleResult.Error("Memo introuvable");

        ClipboardHelper.SetText(entry.Value);
        return ModuleResult.Copied(entry.Value);
    }


    public IReadOnlyList<ArgCompletion> SuggestCompletions(string argument) {
        argument = argument.Trim().ToLowerInvariant();

        return MemoBook.Store.GetAll()
            .Where(m => m.Name.StartsWith(argument, StringComparison.OrdinalIgnoreCase)
                    && !m.Name.Equals(argument, StringComparison.OrdinalIgnoreCase))
            .Take(5)
            .Select(m => new ArgCompletion(m.Name, m.Value))
            .ToList();
    }
}
