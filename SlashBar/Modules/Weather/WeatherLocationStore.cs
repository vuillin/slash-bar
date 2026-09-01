using System.IO;
using System.Text.Json;

namespace SlashBar.Modules.Weather;

public static class WeatherLocationStore {

    private static readonly JsonSerializerOptions JsonOptions = new() {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private static readonly string Path = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SlashBar",
        "weather.json");


    public static string? ReadCity() {
        var file = ReadFile();
        var city = file?.City?.Trim() ?? "";
        return city.Length == 0 ? null : city;
    }


    public static StoredGeocode? ReadGeocode(string cityQuery) {
        var file = ReadFile();
        var geocode = file?.Geocode;
        if (geocode is null)
            return null;

        if (!string.Equals(geocode.CityQuery?.Trim(), cityQuery.Trim(), StringComparison.OrdinalIgnoreCase))
            return null;

        if (geocode.Latitude == 0 && geocode.Longitude == 0)
            return null;

        return new StoredGeocode {
            CityQuery = geocode.CityQuery.Trim(),
            Latitude = geocode.Latitude,
            Longitude = geocode.Longitude,
            Label = geocode.Label?.Trim() ?? ""
        };
    }


    public static void SaveGeocode(string cityQuery, double latitude, double longitude, string label) {
        var file = ReadFile() ?? new FileModel { City = cityQuery.Trim() };
        file.City = cityQuery.Trim();
        file.Geocode = new GeocodeModel {
            CityQuery = cityQuery.Trim(),
            Latitude = latitude,
            Longitude = longitude,
            Label = label
        };

        WriteFile(file);
    }


    private static FileModel? ReadFile() {
        EnsureFile();
        try {
            var json = File.ReadAllText(Path);
            return JsonSerializer.Deserialize<FileModel>(json, JsonOptions);
        }
        catch {
            return null;
        }
    }


    private static void WriteFile(FileModel file) {
        EnsureDirectory();
        var json = JsonSerializer.Serialize(file, JsonOptions);
        File.WriteAllText(Path, json + "\r\n");
    }


    private static void EnsureFile() {
        EnsureDirectory();
        if (File.Exists(Path))
            return;

        var json = JsonSerializer.Serialize(new FileModel { City = "" }, JsonOptions);
        File.WriteAllText(Path, json + "\r\n");
    }


    private static void EnsureDirectory() {
        var dir = System.IO.Path.GetDirectoryName(Path)!;
        Directory.CreateDirectory(dir);
    }


    private sealed class FileModel {
        public string City { get; set; } = "";
        public GeocodeModel? Geocode { get; set; }
    }


    private sealed class GeocodeModel {
        public string CityQuery { get; set; } = "";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Label { get; set; } = "";
    }
}


public sealed class StoredGeocode {
    public string CityQuery { get; init; } = "";
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public string Label { get; init; } = "";
}
