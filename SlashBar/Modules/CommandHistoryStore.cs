namespace SlashBar.Modules;

public sealed class CommandHistoryStore {

    private const int MaxEntries = 50;

    private readonly JsonFileStore<FileModel> _file;


    public CommandHistoryStore() {
        _file = new JsonFileStore<FileModel>("command-history.json");
        lock (_file.SyncRoot) {
            _file.Data.Entries ??= [];
            _file.Data.Entries = _file.Data.Entries
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(e => e.Trim())
                .Take(MaxEntries)
                .ToList();
        }
    }


    public IReadOnlyList<string> GetAll() {
        lock (_file.SyncRoot)
            return _file.Data.Entries.ToList();
    }


    public void Add(string command) {
        command = command.Trim();
        if (command.Length == 0)
            return;

        lock (_file.SyncRoot) {
            var entries = _file.Data.Entries;
            entries.RemoveAll(c => c.Equals(command, StringComparison.OrdinalIgnoreCase));
            entries.Insert(0, command);

            while (entries.Count > MaxEntries)
                entries.RemoveAt(entries.Count - 1);

            _file.ScheduleSave();
        }
    }


    public void Flush() => _file.Flush();


    private sealed class FileModel {
        public List<string> Entries { get; set; } = [];
    }
}
