using System.IO;
using System.Text.Json;

namespace SlashBar.Modules.Color;

public sealed class ColorHistoryStore {

    private const int MaxEntries = 5;

    private static readonly JsonSerializerOptions JsonOptions = new() {
        WriteIndented = true
    };

    private readonly string _path;
    private readonly List<ColorHistoryEntry> _entries = [];
    private readonly object _lock = new();

    public event Action? Changed;


    public ColorHistoryStore() {
        var dir = Path.Combine (
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SlashBar");

        Directory.CreateDirectory(dir);
        _path = Path.Combine(dir, "color-history.json");
        Load();
    }


    public IReadOnlyList<ColorHistoryEntry> GetAll() {
        lock (_lock)
            return _entries.ToList();
    }


    public void Add(byte r, byte g, byte b) {
        lock (_lock) {
            if (_entries.Count > 0
                && _entries[0].R == r
                && _entries[0].G == g
                && _entries[0].B == b)
                return;

            _entries.Insert(0, new ColorHistoryEntry {
                Id = Guid.NewGuid().ToString("N"),
                R = r,
                G = g,
                B = b,
                CreatedAt = DateTimeOffset.UtcNow
            });

            while (_entries.Count > MaxEntries)
                _entries.RemoveAt(_entries.Count - 1);

            Save();
        }

        Changed?.Invoke();
    }


    public void Add (System.Windows.Media.Color color) => 
        Add(color.R, color.G, color.B);


    private void Load() {
        if (!File.Exists(_path))
            return;

        try {
            var json = File.ReadAllText(_path);
            var data = JsonSerializer.Deserialize<FileModel>(json, JsonOptions);
            if (data?.Entries == null)
                return;

            _entries.Clear();
            _entries.AddRange(data.Entries.Take(MaxEntries));
        } catch {
            // fichier corrompu -> on repart vide 
        }
    }


    private void Save() {
        var json = JsonSerializer.Serialize(new FileModel { Entries = _entries }, JsonOptions);
        var tmp = _path + ".tmp";
        File.WriteAllText(tmp, json);
        File.Copy(tmp, _path, overwrite: true);
        File.Delete(tmp);
    }


    private sealed class FileModel {
        public List<ColorHistoryEntry> Entries { get; set; } = [];
    }
}