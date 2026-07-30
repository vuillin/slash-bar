using System.IO;
using System.Text.Json;

namespace SlashBar.Modules.Memo;

public sealed class MemoStore {

    private static readonly JsonSerializerOptions JsonOptions = new() {
        WriteIndented = true
    };

    private readonly string _path;

    private readonly List<MemoEntry> _entries = [];

    private readonly object _lock = new();

    public event Action? Changed;


    public MemoStore() {

        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SlashBar");

        Directory.CreateDirectory(dir);
        _path = Path.Combine(dir, "memos.json");
        Load();
    }


    public IReadOnlyList<MemoEntry> GetAll() {
        lock (_lock)
            return _entries.ToList();
    }


    public void Add(string name, string value) {
        name = name.Trim().ToLowerInvariant();
        value = value.Trim();

        if (name.Length == 0 || value.Length == 0)
            return;

        lock (_lock) {

            // si le nom existe déjà -> on met à jour et on remonte
            var existing = _entries.FindIndex(e =>
                e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (existing >= 0) {

                var entry = _entries[existing];
                entry.Name = name;
                entry.Value = value;
                entry.CreatedAt = DateTimeOffset.UtcNow;
                _entries.RemoveAt(existing);
                _entries.Insert(0, entry);

            } else {

                _entries.Insert(0, new MemoEntry {
                    Id = Guid.NewGuid().ToString("N"),
                    Name = name,
                    Value = value,
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }

            Save();
        }

        Changed?.Invoke();
    }


    public void Update(string id, string name, string value) {
        if (string.IsNullOrEmpty(id))
            return;

        name = name.Trim().ToLowerInvariant();
        value = value.Trim();

        if (name.Length == 0 || value.Length == 0)
            return;

        lock (_lock) {
            var index = _entries.FindIndex(e => e.Id == id);
            if (index < 0)
                return;

            // un autre memo a déjà ce nom → on refuse
            var nameTaken = _entries.Exists(e =>
                e.Id != id && e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (nameTaken)
                return;

            var entry = _entries[index];
            entry.Name = name;
            entry.Value = value;
            entry.CreatedAt = DateTimeOffset.UtcNow;
            _entries.RemoveAt(index);
            _entries.Insert(0, entry);
            Save();
        }

        Changed?.Invoke();
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
            // fichier corrompu
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
        public List<MemoEntry> Entries { get; set; } = [];
    }
}