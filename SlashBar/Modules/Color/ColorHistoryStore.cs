using SlashBar.Modules;

namespace SlashBar.Modules.Color;

public sealed class ColorHistoryStore {

    private const int MaxEntries = 5;

    private readonly JsonFileStore<FileModel> _file;

    public event Action? Changed;


    public ColorHistoryStore() {
        _file = new JsonFileStore<FileModel>("color-history.json");
        lock (_file.SyncRoot) {
            _file.Data.Entries ??= [];
            if (_file.Data.Entries.Count > MaxEntries)
                _file.Data.Entries = _file.Data.Entries.Take(MaxEntries).ToList();
        }
    }


    public IReadOnlyList<ColorHistoryEntry> GetAll() {
        lock (_file.SyncRoot)
            return _file.Data.Entries.ToList();
    }


    public void Add(byte r, byte g, byte b) {
        lock (_file.SyncRoot) {
            var entries = _file.Data.Entries;

            if (entries.Count > 0
                && entries[0].R == r
                && entries[0].G == g
                && entries[0].B == b)
                return;

            entries.Insert(0, new ColorHistoryEntry {
                R = r,
                G = g,
                B = b,
            });

            while (entries.Count > MaxEntries)
                entries.RemoveAt(entries.Count - 1);

            _file.ScheduleSave();
        }

        Changed?.Invoke();
    }


    public void Add(System.Windows.Media.Color color) =>
        Add(color.R, color.G, color.B);


    public void Flush() => _file.Flush();


    private sealed class FileModel {
        public List<ColorHistoryEntry> Entries { get; set; } = [];
    }
}
