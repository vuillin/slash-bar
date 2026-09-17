using System.Text.Json;
using SlashBar.Modules;

namespace SlashBar.Modules.Settings;

public sealed class SettingsStore {

    private static readonly JsonSerializerOptions JsonOptions = new() {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly JsonFileStore<AppSettings> _file;

    public event Action? Changed;


    public SettingsStore() {
        _file = new JsonFileStore<AppSettings>("settings.json", JsonOptions);
        lock (_file.SyncRoot)
            Normalize(_file.Data);
    }


    public AppSettings Get() {
        lock (_file.SyncRoot)
            return Clone(_file.Data);
    }


    public void Update(AppSettings next) {
        ArgumentNullException.ThrowIfNull(next);

        Normalize(next);
        _file.Replace(Clone(next));
        Changed?.Invoke();
    }


    public void Flush() => _file.Flush();


    private static void Normalize(AppSettings s) {
        s.Weather ??= new WeatherSettings();
        s.Display ??= new DisplaySettings();
        s.Hotkeys ??= new HotkeySettings();

        s.Hotkeys.OpenBar = NormalizeChord(s.Hotkeys.OpenBar, HotkeyDefaults.OpenBar());
        s.Hotkeys.Quit = NormalizeChord(s.Hotkeys.Quit, HotkeyDefaults.Quit());
        s.Hotkeys.Pin = NormalizeChord(s.Hotkeys.Pin, HotkeyDefaults.Pin());

        s.Weather.City = (s.Weather.City ?? "").Trim();

        if (s.Weather.Unit != WeatherUnits.Celsius
            && s.Weather.Unit != WeatherUnits.Fahrenheit)
            s.Weather.Unit = WeatherUnits.Celsius;

        if (s.Display.Theme != DisplayThemes.Light
            && s.Display.Theme != DisplayThemes.Dark)
            s.Display.Theme = DisplayThemes.Light;
    }


    private static HotkeyChord NormalizeChord(HotkeyChord? chord, HotkeyChord fallback) {
        if (chord is null || string.IsNullOrWhiteSpace(chord.Key))
            return CloneChord(fallback);

        chord.Key = chord.Key.Trim();
        return chord;
    }

    private static HotkeyChord CloneChord(HotkeyChord c) => new() {
        Control = c.Control,
        Shift = c.Shift,
        Alt = c.Alt,
        Key = c.Key
    };


    private static AppSettings Clone(AppSettings source) {
        var weather = source.Weather ?? new WeatherSettings();
        var display = source.Display ?? new DisplaySettings();
        var hotkeys = source.Hotkeys ?? new HotkeySettings();

        return new AppSettings {
            Weather = new WeatherSettings {
                City = weather.City,
                Unit = weather.Unit,
                ShowSunEvents = weather.ShowSunEvents
            },
            Display = new DisplaySettings {
                Theme = display.Theme
            },
            Hotkeys = new HotkeySettings {
                OpenBar = CloneChord(hotkeys.OpenBar ?? HotkeyDefaults.OpenBar()),
                Quit = CloneChord(hotkeys.Quit ?? HotkeyDefaults.Quit()),
                Pin = CloneChord(hotkeys.Pin ?? HotkeyDefaults.Pin())
            }
        };
    }
}
