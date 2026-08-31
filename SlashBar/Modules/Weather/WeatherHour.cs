namespace SlashBar.Modules.Weather;

public sealed class WeatherHour {

    public string Label { get; init; } = "";
    public string Temperature { get; init; } = "";
    public Uri Icon { get; init; } = null!;
}
