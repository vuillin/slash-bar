using SlashBar.Modules;

namespace SlashBar.Modules.Calendar;

public sealed class CalendarStore {

    private readonly JsonFileStore<FileModel> _file;

    public event Action? Changed;


    public CalendarStore() {
        _file = new JsonFileStore<FileModel>("calendar-events.json");
        lock (_file.SyncRoot)
            _file.Data.Entries ??= [];
    }


    public IReadOnlyList<CalendarEvent> GetAll() {
        lock (_file.SyncRoot)
            return _file.Data.Entries.ToList();
    }


    public IReadOnlyList<CalendarEvent> GetOccurringOn(DateTime day) {
        lock (_file.SyncRoot)
            return _file.Data.Entries.Where(e => CalendarRecurrence.OccursOn(e, day)).ToList();
    }


    public bool HasAnyOn(DateTime day) {
        lock (_file.SyncRoot)
            return _file.Data.Entries.Exists(e => CalendarRecurrence.OccursOn(e, day));
    }


    public bool Add(string title, DateTime date, string repeat) {
        title = title.Trim();
        repeat = string.IsNullOrWhiteSpace(repeat) ? "Never" : repeat.Trim();

        if (title.Length == 0)
            return false;

        lock (_file.SyncRoot) {
            _file.Data.Entries.Insert(0, new CalendarEvent {
                Id = Guid.NewGuid().ToString("N"),
                Title = title,
                Date = date.Date,
                Repeat = repeat,
                CreatedAt = DateTimeOffset.UtcNow
            });

            _file.ScheduleSave();
        }

        Changed?.Invoke();
        return true;
    }


    public void Remove(string id) {
        if (string.IsNullOrEmpty(id))
            return;

        lock (_file.SyncRoot) {
            var removed = _file.Data.Entries.RemoveAll(e => e.Id == id);
            if (removed == 0)
                return;
            _file.ScheduleSave();
        }

        Changed?.Invoke();
    }


    public void Flush() => _file.Flush();


    private sealed class FileModel {
        public List<CalendarEvent> Entries { get; set; } = [];
    }
}
