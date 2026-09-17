using SlashBar.Modules;

namespace SlashBar.Modules.Memo;

public sealed class MemoStore {

    private readonly JsonFileStore<FileModel> _file;

    public event Action? Changed;


    public MemoStore() {
        _file = new JsonFileStore<FileModel>("memos.json");
        lock (_file.SyncRoot)
            _file.Data.Entries ??= [];
    }


    public IReadOnlyList<MemoEntry> GetAll() {
        lock (_file.SyncRoot)
            return _file.Data.Entries.ToList();
    }


    public bool Add(string name, string value) {
        name = name.Trim().ToLowerInvariant();
        value = value.Trim();

        if (name.Length == 0 || value.Length == 0)
            return false;

        lock (_file.SyncRoot) {
            var entries = _file.Data.Entries;

            // name already exists → update and move to top
            var existing = entries.FindIndex(e =>
                e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (existing >= 0) {
                var entry = entries[existing];
                entry.Name = name;
                entry.Value = value;
                entry.CreatedAt = DateTimeOffset.UtcNow;
                entries.RemoveAt(existing);
                entries.Insert(0, entry);
            }
            else {
                entries.Insert(0, new MemoEntry {
                    Id = Guid.NewGuid().ToString("N"),
                    Name = name,
                    Value = value,
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }

            _file.ScheduleSave();
        }

        Changed?.Invoke();
        return true;
    }


    public bool Update(string id, string name, string value) {
        if (string.IsNullOrEmpty(id))
            return false;

        name = name.Trim().ToLowerInvariant();
        value = value.Trim();

        if (name.Length == 0 || value.Length == 0)
            return false;

        lock (_file.SyncRoot) {
            var entries = _file.Data.Entries;
            var index = entries.FindIndex(e => e.Id == id);
            if (index < 0)
                return false;

            // another memo already has this name → reject
            var nameTaken = entries.Exists(e =>
                e.Id != id && e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (nameTaken)
                return false;

            var entry = entries[index];
            entry.Name = name;
            entry.Value = value;
            entry.CreatedAt = DateTimeOffset.UtcNow;
            entries.RemoveAt(index);
            entries.Insert(0, entry);
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


    public MemoEntry? FindByName(string name) {
        name = name.Trim().ToLowerInvariant();
        if (name.Length == 0)
            return null;

        lock (_file.SyncRoot) {
            return _file.Data.Entries.FirstOrDefault(e =>
                e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }


    public void Flush() => _file.Flush();


    private sealed class FileModel {
        public List<MemoEntry> Entries { get; set; } = [];
    }
}
