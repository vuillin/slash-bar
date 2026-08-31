namespace SlashBar.Modules.Weather;

public sealed class WeatherSnapshot {

    public string Place { get; init; } = "";
    public string Temperature { get; init; } = "";
    public string Condition { get; init; } = "";
    public string High { get; init; } = "";
    public string Low { get; init; } = "";
    public int WeatherCode { get; init; }
    public bool IsDay { get; init; }
    public IReadOnlyList<WeatherHour> Hours { get; init; } = [];
}
