using System.Text.Json;
using SlashBar.Modules;

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

    private static readonly JsonFileStore<FileModel> Store =
        new("weather-cache.json", JsonOptions);


    public static bool TryRead(string locationKey, out WeatherCacheEntry entry) {
        entry = null!;

        lock (Store.SyncRoot) {
            var data = Store.Data;
            if (data.LocationKey != locationKey || data.Snapshot is null)
                return false;

            entry = new WeatherCacheEntry {
                LocationKey = data.LocationKey,
                FetchedAt = data.FetchedAt,
                ResolvedPlace = data.ResolvedPlace,
                Snapshot = ToSnapshot(data.Snapshot)
            };
            return true;
        }
    }


    public static bool IsFresh(WeatherCacheEntry entry) =>
        DateTime.Now - entry.FetchedAt <= ForecastTtl;


    public static void Write(
        string locationKey,
        ResolvedPlace place,
        WeatherSnapshot snapshot) {
        lock (Store.SyncRoot) {
            Store.Data.LocationKey = locationKey;
            Store.Data.FetchedAt = DateTime.Now;
            Store.Data.ResolvedPlace = place;
            Store.Data.Snapshot = FromSnapshot(snapshot);
            Store.ScheduleSave();
        }
    }


    public static void Flush() => Store.Flush();


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
