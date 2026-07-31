namespace SlashBar.Modules;

public sealed class ColorModule : IModule {

    public string Prefix => "color";
    public string Name => "Color Picker";
    public string Description => "Open the color picker";

    public ModuleResult Execute(string argument) {
        SlashBar.ColorPanelWindow.Toggle();
        return ModuleResult.None;
    }
}
