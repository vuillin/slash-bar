namespace SlashBar.Modules.Settings;

public sealed class AppSettings {
    public WeatherSettings Weather { get; set; } = new();
    public DisplaySettings Display { get; set; } = new();
    public HotkeySettings Hotkeys { get; set; } = new();
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

public sealed class HotkeyChord {
    public bool Control { get; set; }
    public bool Shift { get; set; }
    public bool Alt { get; set; }
    public string Key { get; set; } = "";
}

public sealed class HotkeySettings {
    public HotkeyChord OpenBar { get; set; } = HotkeyDefaults.OpenBar();
    public HotkeyChord Quit { get; set; } = HotkeyDefaults.Quit();
    public HotkeyChord Pin { get; set; } = HotkeyDefaults.Pin();
}

public static class HotkeyDefaults {
    public static HotkeyChord OpenBar() => new() {
        Control = true,
        Key = "Space"
    };
    
    public static HotkeyChord Quit() => new() {
        Control = true,
        Shift = true,
        Key = "Q"
    };

    public static HotkeyChord Pin() => new() {
        Control = true,
        Shift = true,
        Key = "A"
    };
}