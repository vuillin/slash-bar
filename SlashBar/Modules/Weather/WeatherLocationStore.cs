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
        EnsureFile();
        try {
            var json = File.ReadAllText(Path);
            var data = JsonSerializer.Deserialize<FileModel>(json, JsonOptions);
            var city = data?.City?.Trim() ?? "";
            return city.Length == 0 ? null : city;
        }
        catch {
            return null;
        }
    }


    private static void EnsureFile() {
        var dir = System.IO.Path.GetDirectoryName(Path)!;
        Directory.CreateDirectory(dir);
        
        if (File.Exists(Path))
            return;

        var json = JsonSerializer.Serialize(new FileModel { City = "" }, JsonOptions);
        File.WriteAllText(Path, json + "\r\n");
    }


    private sealed class FileModel {
        public string City { get; set; } = "";
    }
}
