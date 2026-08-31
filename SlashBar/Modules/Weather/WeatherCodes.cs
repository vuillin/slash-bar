namespace SlashBar.Modules.Weather;

/// <summary>
/// Open-Meteo WMO codes → English label + PNG in Assets/Icons/Weather.
/// Sunrise / sunset icons are not mapped here.
/// </summary>
public static class WeatherCodes {

    public static string Label(int code) => code switch {
        0 => "Clear",
        1 => "Mainly clear",
        2 => "Partly cloudy",
        3 => "Overcast",
        45 => "Mist",
        48 => "Fog",
        51 or 53 or 55 => "Drizzle",
        56 or 57 => "Freezing drizzle",
        61 or 63 or 65 => "Rain",
        66 or 67 => "Freezing rain",
        71 or 73 or 75 => "Snow",
        77 => "Snow grains",
        80 or 81 or 82 => "Showers",
        85 or 86 => "Snow showers",
        95 => "Thunderstorm",
        96 or 99 => "Thunderstorm with hail",
        _ => "Cloudy"
    };


    public static string IconFile(int code, bool isDay) => code switch {
        0 => isDay ? "ciel_degage" : "ciel_degage_nuit",
        1 or 2 => isDay ? "quelques_nuages" : "quelques_nuages_nuit",
        3 => "ciel_nuageux",
        45 => "brume",
        48 => "brouillard",
        51 or 53 or 55 => "bruine_nuit",
        56 or 57 or 66 or 67 => "pluie_verglacante",
        61 or 63 or 65 or 80 or 81 or 82 => "pluie",
        71 or 73 or 75 or 77 or 85 or 86 => "neige",
        95 or 96 or 99 => "orage",
        _ => "ciel_nuageux"
    };


    public static Uri IconUri(int code, bool isDay) =>
        new($"pack://application:,,,/Assets/Icons/Weather/{IconFile(code, isDay)}.png");
}
