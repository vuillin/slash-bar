using System.Text.Json;
using SlashBar.Modules;

namespace SlashBar.Modules.Weather;

public static class WeatherLocationStore {

    private static readonly JsonSerializerOptions JsonOptions = new() {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private static readonly JsonFileStore<FileModel> Store =
        new("weather.json", JsonOptions);


    public static StoredGeocode? ReadGeocode(string cityQuery) {
        lock (Store.SyncRoot) {
            var geocode = Store.Data.Geocode;
            if (geocode is null)
                return null;

            if (!string.Equals(geocode.CityQuery?.Trim(), cityQuery.Trim(), StringComparison.OrdinalIgnoreCase))
                return null;

            if (geocode.Latitude == 0 && geocode.Longitude == 0)
                return null;

            return new StoredGeocode {
                CityQuery = geocode.CityQuery?.Trim() ?? "",
                Latitude = geocode.Latitude,
                Longitude = geocode.Longitude,
                Label = geocode.Label?.Trim() ?? ""
            };
        }
    }


    public static void SaveGeocode(string cityQuery, double latitude, double longitude, string label) {
        lock (Store.SyncRoot) {
            Store.Data.Geocode = new GeocodeModel {
                CityQuery = cityQuery.Trim(),
                Latitude = latitude,
                Longitude = longitude,
                Label = label
            };
            Store.ScheduleSave();
        }
    }


    public static void Flush() => Store.Flush();


    private sealed class FileModel {
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
