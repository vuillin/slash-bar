namespace SlashBar.Modules;

public sealed class ClipModule : IModule {

    public string Prefix => "clip";
    public string Name => "Clipboard";
    public string Description => "Clipboard history";

    public ModuleResult Execute(string argument) {
        SlashBar.ClipPanelWindow.Toggle();
        return ModuleResult.None;
    }
}
