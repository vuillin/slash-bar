using SlashBar.Modules;

namespace SlashBar.Modules.Clipboard;

public sealed class ClipboardHistoryStore {

    private const int MaxEntries = 50;

    private readonly JsonFileStore<FileModel> _file;

    public event Action? Changed;


    public ClipboardHistoryStore() {
        _file = new JsonFileStore<FileModel>("clipboard-history.json");
        lock (_file.SyncRoot)
            _file.Data.Entries ??= [];
    }


    public IReadOnlyList<ClipboardHistoryEntry> GetAll() {
        lock (_file.SyncRoot)
            return _file.Data.Entries.ToList();
    }


    public void Add(string text) {
        text = text.Trim();
        if (text.Length == 0)
            return;

        lock (_file.SyncRoot) {
            var entries = _file.Data.Entries;

            // same as most recent entry → skip
            if (entries.Count > 0
                && entries[0].Text.Equals(text, StringComparison.Ordinal))
                return;

            entries.Insert(0, new ClipboardHistoryEntry {
                Id = Guid.NewGuid().ToString("N"),
                Text = text,
                CreatedAt = DateTimeOffset.UtcNow
            });

            while (entries.Count > MaxEntries)
                entries.RemoveAt(entries.Count - 1);

            _file.ScheduleSave();
        }

        Changed?.Invoke();
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


    public void ClearAll() {
        lock (_file.SyncRoot) {
            if (_file.Data.Entries.Count == 0)
                return;

            _file.Data.Entries.Clear();
            _file.ScheduleSave();
        }

        Changed?.Invoke();
    }


    public void Flush() => _file.Flush();


    private sealed class FileModel {
        public List<ClipboardHistoryEntry> Entries { get; set; } = [];
    }
}
