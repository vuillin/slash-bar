using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SlashBar.Modules.Weather;

public static class WeatherClient {

    private static readonly HttpClient Http = CreateHttpClient();

    private static readonly JsonSerializerOptions JsonOptions = new() {
        PropertyNameCaseInsensitive = true
    };


    public static string BuildLocationKey() {
        var city = WeatherLocationStore.ReadCity();
        if (city is not null)
            return "city:" + NormalizeLocationToken(city);

        var gps = WeatherGeolocator.TryGetPosition();
        if (gps is not null)
            return $"gps:{gps.Value.Lat:F2},{gps.Value.Lon:F2}";

        return "ip";
    }


    public static WeatherSnapshot FetchFresh(string locationKey) {
        var place = ResolvePlace(locationKey);
        var forecast = FetchForecast(place.Latitude, place.Longitude);
        var snapshot = BuildSnapshot(place, forecast);
        WeatherCacheStore.Write(locationKey, ToResolvedPlace(place), snapshot);
        return snapshot;
    }


    private static HttpClient CreateHttpClient() {
        var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        http.DefaultRequestHeaders.UserAgent.ParseAdd("SlashBar/1.0");
        return http;
    }


    private static string NormalizeLocationToken(string value) =>
        value.Trim().ToLowerInvariant();


    private static PlaceDto ResolvePlace(string locationKey) {
        var city = WeatherLocationStore.ReadCity();
        if (city is not null) {
            var stored = WeatherLocationStore.ReadGeocode(city);
            if (stored is not null)
                return FromStoredGeocode(stored);

            var geocoded = GeocodeCity(city);
            var label = FormatPlaceLabel(geocoded.City, geocoded.Country);
            WeatherLocationStore.SaveGeocode(
                city,
                geocoded.Latitude,
                geocoded.Longitude,
                label);
            return geocoded;
        }

        if (WeatherCacheStore.TryRead(locationKey, out var cached)
            && cached.ResolvedPlace is not null) {
            return FromResolvedPlace(cached.ResolvedPlace);
        }

        var gps = WeatherGeolocator.TryGetPosition();
        if (gps is not null)
            return ReverseGeocode(gps.Value.Lat, gps.Value.Lon);

        return FetchPlaceFromIp();
    }


    private static WeatherSnapshot BuildSnapshot(PlaceDto place, ForecastDto forecast) {
        var current = forecast.Current!;
        var label = FormatPlaceLabel(place.City, place.Country);

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
            Hours = TakeUpcomingHours(forecast.Hourly, forecast.Daily)
        };
    }


    private static string FormatPlaceLabel(string city, string country) {
        city = city.Trim();
        country = country.Trim();
        if (city.Length == 0)
            return "Unknown location";
        return country.Length == 0 ? city : $"{city}, {country}";
    }


    private static PlaceDto FromStoredGeocode(StoredGeocode geocode) {
        var parts = geocode.Label.Split(',', 2, StringSplitOptions.TrimEntries);
        return new PlaceDto {
            Success = true,
            City = parts.Length > 0 ? parts[0] : geocode.CityQuery,
            Country = parts.Length > 1 ? parts[1] : "",
            Latitude = geocode.Latitude,
            Longitude = geocode.Longitude
        };
    }


    private static PlaceDto FromResolvedPlace(ResolvedPlace place) => new() {
        Success = true,
        City = place.City,
        Country = place.Country,
        Latitude = place.Latitude,
        Longitude = place.Longitude
    };


    private static ResolvedPlace ToResolvedPlace(PlaceDto place) => new() {
        City = place.City,
        Country = place.Country,
        Latitude = place.Latitude,
        Longitude = place.Longitude
    };
    private static PlaceDto GeocodeCity(string city) {
        var url =
            "https://geocoding-api.open-meteo.com/v1/search" +
            $"?name={Uri.EscapeDataString(city)}" +
            "&count=1" +
            "&language=en";

        var json = Http.GetStringAsync(url)
            .GetAwaiter()
            .GetResult();

        var result = JsonSerializer.Deserialize<GeocodingDto>(json, JsonOptions);
        var hit = result?.Results is { Count: > 0 } hits ? hits[0] : null;
        if (hit is null || (hit.Latitude == 0 && hit.Longitude == 0))
            throw new InvalidOperationException("City not found");

        return new PlaceDto {
            Success = true,
            City = hit.Name,
            Country = hit.Country,
            Latitude = hit.Latitude,
            Longitude = hit.Longitude
        };
    }


    private static PlaceDto ReverseGeocode(double lat, double lon) {
        var url =
            "https://nominatim.openstreetmap.org/reverse" +
            $"?lat={lat.ToString(CultureInfo.InvariantCulture)}" +
            $"&lon={lon.ToString(CultureInfo.InvariantCulture)}" +
            "&format=json" +
            "&zoom=10" +
            "&addressdetails=1" +
            "&accept-language=en";

        try {
            var json = Http.GetStringAsync(url)
                .GetAwaiter()
                .GetResult();

            var data = JsonSerializer.Deserialize<NominatimDto>(json, JsonOptions);
            var address = data?.Address;
            var name =
                FirstNonEmpty(address?.City, address?.Town, address?.Village, address?.Municipality)
                ?? "";

            return new PlaceDto {
                Success = true,
                City = name.Length == 0 ? "Current location" : name,
                Country = address?.Country ?? "",
                Latitude = lat,
                Longitude = lon
            };
        }
        catch {
            return new PlaceDto {
                Success = true,
                City = "Current location",
                Latitude = lat,
                Longitude = lon
            };
        }
    }


private static string? FirstNonEmpty(params string?[] values) {
    foreach (var value in values) {
        var trimmed = value?.Trim() ?? "";
        if (trimmed.Length > 0)
            return trimmed;
    }
    return null;
}


    private static PlaceDto FetchPlaceFromIp() {
        var json = Http.GetStringAsync("https://ipwho.is/")
            .GetAwaiter()
            .GetResult();

        var place = JsonSerializer.Deserialize<PlaceDto>(json, JsonOptions);
        if (place is not { Success: true })
            throw new InvalidOperationException("Location unavailable");

        if (place.Latitude == 0 && place.Longitude == 0)
            throw new InvalidOperationException("Location unavailable");

        return place;
    }


    private static ForecastDto FetchForecast(double lat, double lon) {
        var url =
            "https://api.open-meteo.com/v1/forecast" +
            $"?latitude={lat.ToString(CultureInfo.InvariantCulture)}" +
            $"&longitude={lon.ToString(CultureInfo.InvariantCulture)}" +
            "&current=temperature_2m,weather_code,is_day" +
            "&daily=sunrise,sunset,temperature_2m_max,temperature_2m_min" +
            "&hourly=temperature_2m,weather_code,is_day" +
            "&forecast_days=1" +
            "&forecast_hours=12" +
            "&timezone=auto";

        var json = Http.GetStringAsync(url)
            .GetAwaiter()
            .GetResult();

        var forecast = JsonSerializer.Deserialize<ForecastDto>(json, JsonOptions);
        if (forecast?.Current is null)
            throw new InvalidOperationException("Weather unavailable");

        return forecast;
    }


    private sealed record HourSlot(DateTime At, WeatherHour Hour);


    private static IReadOnlyList<WeatherHour> TakeUpcomingHours(HourlyDto? hourly, DailyDto? daily) {
        if (hourly?.Time is null
            || hourly.Temperature2m is null
            || hourly.WeatherCode is null)
            return [];

        var now = DateTime.Now;
        var length = Math.Min(
            hourly.Time.Length,
            Math.Min(hourly.Temperature2m.Length, hourly.WeatherCode.Length));

        var slots = new List<HourSlot>(6);

        for (var i = 0; i < length && slots.Count < 6; i++) {
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

            slots.Add(new HourSlot(at, new WeatherHour {
                Label = FormatHourLabel(at),
                Temperature = FormatTemperature(hourly.Temperature2m[i]),
                Icon = WeatherCodes.IconUri(hourly.WeatherCode[i], isDay)
            }));
        }

        if (slots.Count == 0)
            return [];

        var windowEnd = slots[^1].At;
        var sunEvents = CollectSunEvents(hourly, daily, now, windowEnd);
        if (sunEvents.Count == 0)
            return slots.Select(slot => slot.Hour).ToList();

        var kept = slots.Take(6 - sunEvents.Count).ToList();
        kept.AddRange(sunEvents);
        kept.Sort((a, b) => a.At.CompareTo(b.At));

        return kept.Select(slot => slot.Hour).ToList();
    }


    private static List<HourSlot> CollectSunEvents(
        HourlyDto hourly,
        DailyDto? daily,
        DateTime now,
        DateTime windowEnd) {
        var events = new List<HourSlot>(2);

        if (daily?.Sunrise is { Length: > 0 } sunrises
            && TryParseSunEvent(sunrises[0], out var sunrise)
            && sunrise > now
            && sunrise <= windowEnd) {
            events.Add(new HourSlot(sunrise, new WeatherHour {
                Label = FormatHourLabel(sunrise),
                Temperature = FormatTemperature(TemperatureAt(hourly, sunrise)),
                Icon = WeatherCodes.SunriseIconUri()
            }));
        }

        if (daily?.Sunset is { Length: > 0 } sunsets
            && TryParseSunEvent(sunsets[0], out var sunset)
            && sunset > now
            && sunset <= windowEnd) {
            events.Add(new HourSlot(sunset, new WeatherHour {
                Label = FormatHourLabel(sunset),
                Temperature = FormatTemperature(TemperatureAt(hourly, sunset)),
                Icon = WeatherCodes.SunsetIconUri()
            }));
        }

        return events;
    }


    /// <summary>
    /// Hourly API returns on-the-hour samples; linearly interpolate between neighbours.
    /// Open-Meteo also offers minutely_15, but interpolation avoids a second time grid.
    /// </summary>
    private static double? TemperatureAt(HourlyDto hourly, DateTime at) {
        if (hourly.Time is null || hourly.Temperature2m is null)
            return null;

        var length = Math.Min(hourly.Time.Length, hourly.Temperature2m.Length);
        if (length == 0)
            return null;

        DateTime? beforeAt = null;
        double? beforeTemp = null;
        DateTime? afterAt = null;
        double? afterTemp = null;

        for (var i = 0; i < length; i++) {
            if (!DateTime.TryParse(
                    hourly.Time[i],
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var slotAt))
                continue;

            var temp = hourly.Temperature2m[i];

            if (slotAt == at)
                return temp;

            if (slotAt < at) {
                beforeAt = slotAt;
                beforeTemp = temp;
                continue;
            }

            afterAt = slotAt;
            afterTemp = temp;
            break;
        }

        if (beforeTemp is null)
            return afterTemp;

        if (afterTemp is null || beforeAt is null || afterAt is null)
            return beforeTemp;

        var span = (afterAt.Value - beforeAt.Value).TotalMinutes;
        if (span <= 0)
            return beforeTemp;

        var ratio = (at - beforeAt.Value).TotalMinutes / span;
        return beforeTemp.Value + ((afterTemp.Value - beforeTemp.Value) * ratio);
    }


    private static string FormatTemperature(double? celsius) =>
        celsius is null ? "" : $"{Math.Round(celsius.Value)}°";


    private static bool TryParseSunEvent(string raw, out DateTime at) {
        at = default;
        if (!DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out at))
            return false;

        return true;
    }


    private static string FormatHourLabel(DateTime at) =>
        at.Minute == 0
            ? at.ToString("HH") + "h"
            : $"{at:HH}h{at:mm}";


    private sealed class PlaceDto {
        public bool Success { get; set; }
        public string City { get; set; } = "";
        public string Country { get; set; } = "";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }


    private sealed class GeocodingDto {
        public List<GeocodingHitDto>? Results { get; set; }
    }
    

    private sealed class GeocodingHitDto {
        public string Name { get; set; } = "";
        public string Country { get; set; } = "";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }


    private sealed class NominatimDto {
        public NominatimAddressDto? Address { get; set; }
    }

    private sealed class NominatimAddressDto {
        public string? City { get; set; }
        public string? Town { get; set; }
        public string? Village { get; set; }
        public string? Municipality { get; set; }
        public string? Country { get; set; }
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
        public string[]? Sunrise { get; set; }

        public string[]? Sunset { get; set; }

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
