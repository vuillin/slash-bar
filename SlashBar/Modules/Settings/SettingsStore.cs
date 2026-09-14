using System.IO;
using System.Text.Json;
using SlashBar.Modules;

namespace SlashBar.Modules.Settings;

public sealed class SettingsStore {

    private static readonly JsonSerializerOptions JsonOptions = new() {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // city, showSunEvents...
        PropertyNameCaseInsensitive = true
    };

    private readonly string _path;
    private readonly object _lock = new();
    private readonly DebouncedSaver _saver;

    private AppSettings _settings;

    public event Action? Changed;


    public SettingsStore() {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SlashBar");

        Directory.CreateDirectory(dir);
        _path = Path.Combine(dir, "settings.json");

        _saver = new DebouncedSaver(WriteToDisk);
        _settings = Load();
    }


    public AppSettings Get() {
        lock (_lock)
            return Clone(_settings);
    }


    public void Update(AppSettings next) {
        ArgumentNullException.ThrowIfNull(next);

        Normalize(next);

        lock (_lock) {
            _settings = Clone(next);
            _saver.Schedule();
        }

        Changed?.Invoke();
    }


    public void Flush() => _saver.Flush();


    private AppSettings Load() {
        if (!File.Exists(_path))
            return new AppSettings();

        try {
            var json = File.ReadAllText(_path);
            var data = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
            if (data is null)
                return new AppSettings();

            Normalize(data);
            return data;
        } catch {
            // corrupt file → fresh defaults
            return new AppSettings();
        }
    }


    private void WriteToDisk() {
        AppSettings snapshot;
        lock (_lock)
            snapshot = Clone(_settings);

        var json = JsonSerializer.Serialize(snapshot, JsonOptions);
        var tmp = _path + ".tmp";
        File.WriteAllText(tmp, json);
        File.Copy(tmp, _path, overwrite: true);
        File.Delete(tmp);
    }


    private static void Normalize(AppSettings s) {
        s.Weather ??= new WeatherSettings();
        s.Display ??= new DisplaySettings();

        s.Weather.City = (s.Weather.City ?? "").Trim();

        if (s.Weather.Unit != WeatherUnits.Celsius
            && s.Weather.Unit != WeatherUnits.Fahrenheit)
            s.Weather.Unit = WeatherUnits.Celsius;

        if (s.Display.Theme != DisplayThemes.Light
            && s.Display.Theme != DisplayThemes.Dark)
            s.Display.Theme = DisplayThemes.Light;
    }


    private static AppSettings Clone(AppSettings source) => new() {
        Weather = new WeatherSettings {
            City = source.Weather.City,
            Unit = source.Weather.Unit,
            ShowSunEvents = source.Weather.ShowSunEvents
        },
        Display = new DisplaySettings {
            Theme = source.Display.Theme
        }
    };
}