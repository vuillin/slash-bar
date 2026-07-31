using System.IO;
using System.Text.Json;

namespace SlashBar.Modules;

public sealed class CommandHistoryStore {

    private const int MaxEntries = 50;

    private static readonly JsonSerializerOptions JsonOptions = new() {
        WriteIndented = true
    };

    private readonly string _path;
    private readonly List<string> _entries = [];
    private readonly object _lock = new();


    public CommandHistoryStore() {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SlashBar");

        Directory.CreateDirectory(dir);
        _path = Path.Combine(dir, "command-history.json");
        Load();
    }


    public IReadOnlyList<string> GetAll() {
        lock (_lock)
            return _entries.ToList();
    }


    public void Add(string command) {
        command = command.Trim();
        if (command.Length == 0)
            return;

        lock (_lock) {
            _entries.RemoveAll(c => c.Equals(command, StringComparison.OrdinalIgnoreCase));
            _entries.Insert(0, command);

            while (_entries.Count > MaxEntries)
                _entries.RemoveAt(_entries.Count - 1);

            Save();
        }
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
            _entries.AddRange(
                data.Entries
                    .Where(e => !string.IsNullOrWhiteSpace(e))
                    .Select(e => e.Trim())
                    .Take(MaxEntries));
        }
        catch {
            // corrompu → vide
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
        public List<string> Entries { get; set; } = [];
    }
}
