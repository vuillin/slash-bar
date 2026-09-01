using System.IO;
using System.Text.Json;

namespace SlashBar.Modules.Weather;

public static class WeatherCacheStore {

    public static readonly TimeSpan ForecastTtl = TimeSpan.FromMinutes(15);

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
        "weather-cache.json");


    public static bool TryRead(string locationKey, out WeatherCacheEntry entry) {
        entry = null!;
        EnsureDirectory();

        if (!File.Exists(Path))
            return false;

        try {
            var json = File.ReadAllText(Path);
            var file = JsonSerializer.Deserialize<FileModel>(json, JsonOptions);
            if (file?.LocationKey != locationKey || file.Snapshot is null)
                return false;

            entry = new WeatherCacheEntry {
                LocationKey = file.LocationKey,
                FetchedAt = file.FetchedAt,
                ResolvedPlace = file.ResolvedPlace,
                Snapshot = ToSnapshot(file.Snapshot)
            };
            return true;
        }
        catch {
            return false;
        }
    }


    public static bool IsFresh(WeatherCacheEntry entry) =>
        DateTime.Now - entry.FetchedAt <= ForecastTtl;


    public static void Write(
        string locationKey,
        ResolvedPlace place,
        WeatherSnapshot snapshot) {
        EnsureDirectory();

        var file = new FileModel {
            LocationKey = locationKey,
            FetchedAt = DateTime.Now,
            ResolvedPlace = place,
            Snapshot = FromSnapshot(snapshot)
        };

        var json = JsonSerializer.Serialize(file, JsonOptions);
        File.WriteAllText(Path, json + "\r\n");
    }


    private static void EnsureDirectory() {
        var dir = System.IO.Path.GetDirectoryName(Path)!;
        Directory.CreateDirectory(dir);
    }


    private static WeatherSnapshot ToSnapshot(SnapshotModel model) => new() {
        Place = model.Place ?? "",
        Temperature = model.Temperature ?? "",
        Condition = model.Condition ?? "",
        High = model.High ?? "",
        Low = model.Low ?? "",
        WeatherCode = model.WeatherCode,
        IsDay = model.IsDay,
        Hours = model.Hours?.Select(hour => new WeatherHour {
            Label = hour.Label ?? "",
            Temperature = hour.Temperature ?? "",
            Icon = new Uri(hour.Icon ?? WeatherCodes.IconUri(0, true).ToString())
        }).ToList() ?? []
    };


    private static SnapshotModel FromSnapshot(WeatherSnapshot snapshot) => new() {
        Place = snapshot.Place,
        Temperature = snapshot.Temperature,
        Condition = snapshot.Condition,
        High = snapshot.High,
        Low = snapshot.Low,
        WeatherCode = snapshot.WeatherCode,
        IsDay = snapshot.IsDay,
        Hours = snapshot.Hours.Select(hour => new HourModel {
            Label = hour.Label,
            Temperature = hour.Temperature,
            Icon = hour.Icon.ToString()
        }).ToList()
    };


    private sealed class FileModel {
        public string LocationKey { get; set; } = "";
        public DateTime FetchedAt { get; set; }
        public ResolvedPlace? ResolvedPlace { get; set; }
        public SnapshotModel? Snapshot { get; set; }
    }


    private sealed class SnapshotModel {
        public string? Place { get; set; }
        public string? Temperature { get; set; }
        public string? Condition { get; set; }
        public string? High { get; set; }
        public string? Low { get; set; }
        public int WeatherCode { get; set; }
        public bool IsDay { get; set; }
        public List<HourModel>? Hours { get; set; }
    }


    private sealed class HourModel {
        public string? Label { get; set; }
        public string? Temperature { get; set; }
        public string? Icon { get; set; }
    }
}


public sealed class WeatherCacheEntry {
    public string LocationKey { get; init; } = "";
    public DateTime FetchedAt { get; init; }
    public ResolvedPlace? ResolvedPlace { get; init; }
    public WeatherSnapshot Snapshot { get; init; } = null!;
}


public sealed class ResolvedPlace {
    public string City { get; set; } = "";
    public string Country { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
