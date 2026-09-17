namespace SlashBar.Modules;

public sealed class ColorModule : IModule {

    public string Prefix => "color";
    public string Name => "Color Picker";
    public string Description => "Open the color picker";

    public Task<ModuleResult> ExecuteAsync(string argument) {
        SlashBar.ColorPanelWindow.Toggle();
        return Task.FromResult(ModuleResult.None);
    }
}
