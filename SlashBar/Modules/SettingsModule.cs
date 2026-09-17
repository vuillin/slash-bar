namespace SlashBar.Modules;

public sealed class SettingsModule : IModule {

    public string Prefix => "settings";
    public string Name => "Settings";
    public string Description => "Open settings";

    public Task<ModuleResult> ExecuteAsync(string argument) {
        SlashBar.SettingsPanelWindow.Toggle();
        return Task.FromResult(ModuleResult.None);
    }
}
