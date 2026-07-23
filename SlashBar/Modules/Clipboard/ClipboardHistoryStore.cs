using System.IO;
using System.Text.Json;

namespace SlashBar.Modules.Clipboard;

public sealed class ClipboardHistoryStore {

    private const int MaxEntries = 50;

    private static readonly JsonSerializerOptions JsonOptions = new() {
        WriteIndented = true
    };

    private readonly string _path;
    private readonly List<ClipboardHistoryEntry> _entries = [];
    private readonly object _lock = new();

    public event Action? Changed;


    public ClipboardHistoryStore() {
        
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SlashBar");
        
        Directory.CreateDirectory(dir);
        _path = Path.Combine(dir, "clipboard-history.json");
        Load();
    }


    public IReadOnlyList<ClipboardHistoryEntry> GetAll() {
        lock (_lock)
            return _entries.ToList();
    }


    public void Add(string text) {

        text = text.Trim();
        if (text.Length == 0)
            return;

        lock (_lock) {

            // si identique au plus récent -> ignore 
            if (_entries.Count > 0
                && _entries[0].Text.Equals(text, StringComparison.Ordinal))
                return;

            _entries.Insert(0, new ClipboardHistoryEntry {
                Id = Guid.NewGuid().ToString("N"),
                Text = text,
                CreatedAt = DateTimeOffset.UtcNow
            });

            while (_entries.Count > MaxEntries)
                _entries.RemoveAt(_entries.Count - 1);

            Save();
        }

        Changed?.Invoke();
    }


    private void Load() {

        if (!File.Exists(_path))
            return;

        try {
            var json = File.ReadAllText(_path);
            var data = JsonSerializer.Deserialize<FileModel>(json, JsonOptions);
            if (data?.Entries == null)
                return;

            _entries.Clear();
            _entries.AddRange(data.Entries);
        
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


    public void Remove(string id) {

        if (string.IsNullOrEmpty(id))
            return;

        lock (_lock) {
            var removed = _entries.RemoveAll(e => e.Id == id);
            if (removed == 0)
                return;
            Save();
        }

        Changed?.Invoke();
    }


    private sealed class FileModel {
        public List<ClipboardHistoryEntry> Entries { get; set; } = [];
    }
}