using System.IO;
using System.Linq;
using System.Text.Json;

namespace SlashBar.Modules.Calendar;

public sealed class CalendarStore {

    private static readonly JsonSerializerOptions JsonOptions = new() {
        WriteIndented = true
    };

    private readonly string _path;
    private readonly List<CalendarEvent> _entries = [];
    private readonly object _lock = new();
    private readonly DebouncedSaver _saver;

    public event Action? Changed;


    public CalendarStore() {

        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SlashBar");

        Directory.CreateDirectory(dir);
        _path = Path.Combine(dir, "calendar-events.json");

        _saver = new DebouncedSaver(WriteToDisk);
        Load();
    }


    public IReadOnlyList<CalendarEvent> GetAll() {
        lock (_lock)
            return _entries.ToList();
    }


    public IReadOnlyList<CalendarEvent> GetOccurringOn(DateTime day) {
        lock (_lock)
            return _entries.Where(e => CalendarRecurrence.OccursOn(e, day)).ToList();
    }


    public bool HasAnyOn(DateTime day) {
        lock (_lock)
            return _entries.Exists(e => CalendarRecurrence.OccursOn(e, day));
    }


    public bool Add(string title, DateTime date, string repeat) {
        title = title.Trim();
        repeat = string.IsNullOrWhiteSpace(repeat) ? "Never" : repeat.Trim();

        if (title.Length == 0)
            return false;

        lock (_lock) {
            _entries.Insert(0, new CalendarEvent {
                Id = Guid.NewGuid().ToString("N"),
                Title = title,
                Date = date.Date,
                Repeat = repeat,
                CreatedAt = DateTimeOffset.UtcNow
            });

            _saver.Schedule();
        }

        Changed?.Invoke();
        return true;
    }


    public void Flush() => _saver.Flush();


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
            // Fichier corrompu
        }
    }


    private void WriteToDisk() {
        string json;
        lock (_lock)
            json = JsonSerializer.Serialize(new FileModel { Entries = _entries }, JsonOptions);

        var tmp = _path + ".tmp";
        File.WriteAllText(tmp, json);
        File.Copy(tmp, _path, overwrite: true);
        File.Delete(tmp);
    }


    private sealed class FileModel {
        public List<CalendarEvent> Entries { get; set; } = [];
    }

}