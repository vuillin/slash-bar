using System.IO;
using System.Text.Json;

namespace SlashBar.Modules.Setup;

/// <summary>
/// Loads setup profiles from %LocalAppData%/SlashBar/setup-profiles.json.
/// </summary>
public sealed class SetupProfileStore {

    private static readonly JsonSerializerOptions JsonOptions = new() {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private readonly string _path;
    private readonly List<SetupProfile> _profiles = [];
    private readonly object _lock = new();

    public SetupProfileStore() {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SlashBar");

        Directory.CreateDirectory(dir);
        _path = Path.Combine(dir, "setup-profiles.json");

        if (!File.Exists(_path))
            File.WriteAllText(_path, "[]\r\n");

        Load();
    }

    public string ConfigPath => _path;

    public IReadOnlyList<SetupProfile> GetAll() {
        Load();
        lock (_lock)
            return _profiles.ToList();
    }

    public SetupProfile? Find(string name) {
        Load();
        lock (_lock)
            return _profiles.FirstOrDefault(p =>
                p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    private void Load() {
        lock (_lock) {
            try {
                var json = File.ReadAllText(_path);
                var loaded = JsonSerializer.Deserialize<List<SetupProfileDto>>(json, JsonOptions)
                    ?? [];

                _profiles.Clear();
                foreach (var dto in loaded) {
                    if (string.IsNullOrWhiteSpace(dto.Name))
                        continue;

                    var steps = (dto.Steps ?? [])
                        .Where(s => !string.IsNullOrWhiteSpace(s.FileName))
                        .Select(s => new SetupStep(
                            s.FileName!.Trim(),
                            string.IsNullOrWhiteSpace(s.Arguments) ? null : s.Arguments,
                            ParseLayout(s.Layout),
                            string.IsNullOrWhiteSpace(s.WindowProcessName) ? null : s.WindowProcessName))
                        .ToList();

                    _profiles.Add(new SetupProfile(
                        dto.Name.Trim(),
                        dto.Description?.Trim() ?? "",
                        steps));
                }
            }
            catch (Exception) {
                // keep last good in-memory list on parse/IO errors
            }
        }
    }

    private static WindowLayout ParseLayout(string? layout) {
        if (string.IsNullOrWhiteSpace(layout))
            return WindowLayout.Default;

        return Enum.TryParse<WindowLayout>(layout.Trim(), ignoreCase: true, out var value)
            ? value
            : WindowLayout.Default;
    }

    private sealed class SetupProfileDto {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<SetupStepDto>? Steps { get; set; }
    }

    private sealed class SetupStepDto {
        public string? FileName { get; set; }
        public string? Arguments { get; set; }
        public string? Layout { get; set; }
        public string? WindowProcessName { get; set; }
    }
}
