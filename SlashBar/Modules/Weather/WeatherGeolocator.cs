using Windows.Devices.Geolocation;

namespace SlashBar.Modules.Weather;

public static class WeatherGeolocator {

    public static GeolocationAccessStatus Access { get; private set; }


    public static async Task RequestAccessAsync() {
        try {
            Access = await Geolocator.RequestAccessAsync();
        }
        catch {
            Access = GeolocationAccessStatus.Denied;
        }
    }


    public static (double Lat, double Lon)? TryGetPosition() {
        if (Access != GeolocationAccessStatus.Allowed)
            return null;

        try {
            var locator = new Geolocator {
                DesiredAccuracy = PositionAccuracy.Default,
                DesiredAccuracyInMeters = 1000
            };

            var pos = locator.GetGeopositionAsync(
                    TimeSpan.FromMinutes(10),
                    TimeSpan.FromSeconds(8))
                .AsTask()
                .GetAwaiter()
                .GetResult();

            var point = pos?.Coordinate?.Point;
            if (point is null)
                return null;

            var lat = point.Position.Latitude;
            var lon = point.Position.Longitude;
            if (lat == 0 && lon == 0)
                return null;

            return (lat, lon);
        }
        catch {
            return null;
        }
    }
}