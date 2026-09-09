namespace SlashBar.Modules;

/// <summary>
/// Coalesces rapid save requests into one write after a quiet period.
/// </summary>
public sealed class DebouncedSaver : IDisposable {

    private readonly Action _save;
    private readonly int _delayMs;
    private readonly object _gate = new();
    private System.Threading.Timer? _timer;
    private bool _pending;
    private bool _disposed;


    public DebouncedSaver(Action save, int delayMs = 400) {
        _save = save;
        _delayMs = delayMs;
    }


    public void Schedule() {
        lock (_gate) {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _pending = true;
            _timer ??= new System.Threading.Timer(OnTimer, null, Timeout.Infinite, Timeout.Infinite);
            _timer.Change(_delayMs, Timeout.Infinite);
        }
    }


    public void Flush() {
        bool run;
        lock (_gate) {
            if (_disposed)
                return;

            run = _pending;
            _pending = false;
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
        }

        if (run)
            _save();
    }


    private void OnTimer(object? _) => Flush();


    public void Dispose() {
        Flush();
        lock (_gate) {
            if (_disposed)
                return;

            _disposed = true;
            _timer?.Dispose();
            _timer = null;
        }
    }
}
