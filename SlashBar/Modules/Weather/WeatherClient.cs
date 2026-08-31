using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SlashBar.Modules.Weather;

public static class WeatherClient {

    private static readonly JsonSerializerOptions JsonOptions = new() {
        PropertyNameCaseInsensitive = true
    };


    public static WeatherSnapshot Fetch() {
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        http.DefaultRequestHeaders.UserAgent.ParseAdd("SlashBar/1.0");

        var place = FetchPlace(http);
        var forecast = FetchForecast(http, place.Latitude, place.Longitude);
        var current = forecast.Current!;

        var city = place.City?.Trim() ?? "";
        var country = place.Country?.Trim() ?? "";
        var label = city.Length == 0
            ? "Unknown location"
            : country.Length == 0
                ? city
                : $"{city}, {country}";

        var high = "";
        var low = "";
        if (forecast.Daily?.Temperature2mMax is { Length: > 0 } highs
            && forecast.Daily.Temperature2mMin is { Length: > 0 } lows) {
            high = $"{Math.Round(highs[0])}°";
            low = $"{Math.Round(lows[0])}°";
        }

        return new WeatherSnapshot {
            Place = label,
            Temperature = $"{Math.Round(current.Temperature2m)}°C",
            Condition = WeatherCodes.Label(current.WeatherCode),
            High = high,
            Low = low,
            WeatherCode = current.WeatherCode,
            IsDay = current.IsDay == 1,
            Hours = TakeUpcomingHours(forecast.Hourly)
        };
    }


    private static PlaceDto FetchPlace(HttpClient http) {
        var json = http.GetStringAsync("https://ipwho.is/")
            .GetAwaiter()
            .GetResult();

        var place = JsonSerializer.Deserialize<PlaceDto>(json, JsonOptions);
        if (place is not { Success: true })
            throw new InvalidOperationException("Location unavailable");

        if (place.Latitude == 0 && place.Longitude == 0)
            throw new InvalidOperationException("Location unavailable");

        return place;
    }


    private static ForecastDto FetchForecast(HttpClient http, double lat, double lon) {
        var url =
            "https://api.open-meteo.com/v1/forecast" +
            $"?latitude={lat.ToString(CultureInfo.InvariantCulture)}" +
            $"&longitude={lon.ToString(CultureInfo.InvariantCulture)}" +
            "&current=temperature_2m,weather_code,is_day" +
            "&daily=temperature_2m_max,temperature_2m_min" +
            "&hourly=temperature_2m,weather_code,is_day" +
            "&forecast_days=1" +
            "&forecast_hours=12" +
            "&timezone=auto";

        var json = http.GetStringAsync(url)
            .GetAwaiter()
            .GetResult();

        var forecast = JsonSerializer.Deserialize<ForecastDto>(json, JsonOptions);
        if (forecast?.Current is null)
            throw new InvalidOperationException("Weather unavailable");

        return forecast;
    }


    private static IReadOnlyList<WeatherHour> TakeUpcomingHours(HourlyDto? hourly) {
        if (hourly?.Time is null
            || hourly.Temperature2m is null
            || hourly.WeatherCode is null)
            return [];

        var now = DateTime.Now;
        var length = Math.Min(
            hourly.Time.Length,
            Math.Min(hourly.Temperature2m.Length, hourly.WeatherCode.Length));

        var hours = new List<WeatherHour>(6);

        for (var i = 0; i < length && hours.Count < 6; i++) {
            if (!DateTime.TryParse(
                    hourly.Time[i],
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var at))
                continue;

            if (at <= now)
                continue;

            var isDay = hourly.IsDay is { Length: > 0 }
                        && i < hourly.IsDay.Length
                        && hourly.IsDay[i] == 1;

            hours.Add(new WeatherHour {
                Label = at.ToString("HH") + "h",
                Temperature = $"{Math.Round(hourly.Temperature2m[i])}°",
                Icon = WeatherCodes.IconUri(hourly.WeatherCode[i], isDay)
            });
        }

        return hours;
    }


    private sealed class PlaceDto {
        public bool Success { get; set; }
        public string City { get; set; } = "";
        public string Country { get; set; } = "";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }


    private sealed class ForecastDto {
        public CurrentDto? Current { get; set; }
        public DailyDto? Daily { get; set; }
        public HourlyDto? Hourly { get; set; }
    }


    private sealed class HourlyDto {
        public string[]? Time { get; set; }

        [JsonPropertyName("temperature_2m")]
        public double[]? Temperature2m { get; set; }

        [JsonPropertyName("weather_code")]
        public int[]? WeatherCode { get; set; }

        [JsonPropertyName("is_day")]
        public int[]? IsDay { get; set; }
    }


    private sealed class DailyDto {
        [JsonPropertyName("temperature_2m_max")]
        public double[]? Temperature2mMax { get; set; }

        [JsonPropertyName("temperature_2m_min")]
        public double[]? Temperature2mMin { get; set; }
    }


    private sealed class CurrentDto {
        [JsonPropertyName("temperature_2m")]
        public double Temperature2m { get; set; }

        [JsonPropertyName("weather_code")]
        public int WeatherCode { get; set; }

        [JsonPropertyName("is_day")]
        public int IsDay { get; set; }
    }
}
