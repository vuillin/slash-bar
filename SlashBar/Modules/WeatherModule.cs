namespace SlashBar.Modules;

public sealed class WeatherModule : IModule {

    public string Prefix => "weather";
    public string Name => "Weather";
    public string Description => "Current weather";

    public ModuleResult Execute(string argument) {
        SlashBar.WeatherPanelWindow.Toggle();
        return ModuleResult.None;
    }
}
