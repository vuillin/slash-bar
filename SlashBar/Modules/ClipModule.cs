namespace SlashBar.Modules;

public sealed class ClipModule : IModule {

    public string Prefix => "clip";
    public string Name => "Clipboard";
    public string Description => "Clipboard history";

    public Task<ModuleResult> ExecuteAsync(string argument) {
        SlashBar.ClipPanelWindow.Toggle();
        return Task.FromResult(ModuleResult.None);
    }
}
