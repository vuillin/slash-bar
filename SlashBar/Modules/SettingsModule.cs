namespace SlashBar.Modules;

public sealed class SettingsModule : IModule {

    public string Prefix => "settings";
    public string Name => "Settings";
    public string Description => "Open settings";

    public ModuleResult Execute(string argument) {
        SlashBar.SettingsPanelWindow.Toggle();
        return ModuleResult.None;
    }
}
