namespace SlashBar.Modules.Settings;

public sealed class AppSettings {
    public WeatherSettings Weather { get; set; } = new();
    public DisplaySettings Display { get; set; } = new();
}

public sealed class WeatherSettings {
    public string City { get; set; } = "";

    public string Unit { get; set; } = WeatherUnits.Celsius;

    public bool ShowSunEvents { get; set; } = true;
}

public sealed class DisplaySettings {
    public string Theme { get; set; } = DisplayThemes.Light;
}

public static class WeatherUnits {
    public const string Celsius = "celsius";
    public const string Fahrenheit = "fahrenheit";
}

public static class DisplayThemes {
    public const string Light = "light";
    public const string Dark = "dark";
}