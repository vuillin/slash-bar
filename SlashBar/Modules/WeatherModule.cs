namespace SlashBar.Modules;

public sealed class WeatherModule : IModule {

    public string Prefix => "weather";
    public string Name => "Weather";
    public string Description => "Current weather";

    public Task<ModuleResult> ExecuteAsync(string argument) {
        SlashBar.WeatherPanelWindow.Toggle();
        return Task.FromResult(ModuleResult.None);
    }
}
