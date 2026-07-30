namespace SlashBar.Modules;

public sealed class MemoModule : IModule {

    public string Prefix => "memo";
    public string Name => "Memo";
    public string Description => "Raccourcis de texte à copier";

    public void Execute(string argument) {
        SlashBar.MemoPanelWindow.Toggle();
    }
}
