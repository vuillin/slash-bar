using System.IO;
using System.Text.Json;

namespace SlashBar.Modules;

/// <summary>
/// Shared JSON persistence: LocalAppData path, corrupt-tolerant load,
/// atomic tmp+copy write, and debounced saves.
/// </summary>
public sealed class JsonFileStore<T> : IDisposable where T : class, new() {

    private static readonly JsonSerializerOptions DefaultJson = new() {
        WriteIndented = true
    };

    private readonly string _path;
    private readonly JsonSerializerOptions _json;
    private readonly DebouncedSaver _saver;
    private readonly object _lock = new();

    private T _data;

    /// <summary>Lock for all reads/mutations of <see cref="Data"/>.</summary>
    public object SyncRoot => _lock;

    /// <summary>In-memory file model. Always access under <see cref="SyncRoot"/>.</summary>
    public T Data => _data;


    public JsonFileStore(string fileName, JsonSerializerOptions? jsonOptions = null) {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SlashBar");

        Directory.CreateDirectory(dir);
        _path = Path.Combine(dir, fileName);
        _json = jsonOptions ?? DefaultJson;

        _data = LoadFromDisk() ?? new T();
        _saver = new DebouncedSaver(WriteToDisk);
    }


    public void ScheduleSave() => _saver.Schedule();

    public void Flush() => _saver.Flush();


    public void Replace(T data) {
        ArgumentNullException.ThrowIfNull(data);
        lock (_lock)
            _data = data;
        _saver.Schedule();
    }


    public void Dispose() => _saver.Dispose();


    private T? LoadFromDisk() {
        if (!File.Exists(_path))
            return null;

        try {
            var json = File.ReadAllText(_path);
            return JsonSerializer.Deserialize<T>(json, _json);
        }
        catch {
            return null;
        }
    }


    private void WriteToDisk() {
        string json;
        lock (_lock)
            json = JsonSerializer.Serialize(_data, _json);

        var tmp = _path + ".tmp";
        File.WriteAllText(tmp, json);
        File.Copy(tmp, _path, overwrite: true);
        File.Delete(tmp);
    }
}