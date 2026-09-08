using System.Windows.Threading;
using SlashBar.Modules.Native;

namespace SlashBar.Modules.Awake;

public static class AwakeSession {

    private static DispatcherTimer? _timer;


    public static bool IsActive { get; private set; }
    public static AwakeMode Mode { get; private set; }
    public static DateTimeOffset? EndsAt { get; private set; }

    public static event Action? Changed;
    public static event Action? Expired;


    public static void Enable(AwakeMode mode, TimeSpan? duration = null) {
        var flags = mode == AwakeMode.System
            ? ExecutionStateNative.Continuous | ExecutionStateNative.SystemRequired
            : ExecutionStateNative.Continuous | ExecutionStateNative.SystemRequired | ExecutionStateNative.DisplayRequired;

        ExecutionStateNative.Set(flags);
        Mode = mode;
        IsActive = true;

        StopTimer();

        if (duration is { } d) {
            EndsAt = DateTimeOffset.Now + d;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += OnTick;
            _timer.Start();
        }
        else {
            EndsAt = null;
        }

        Changed?.Invoke();
    }


    public static void Disable() {
        if (!IsActive && _timer is null)
            return;

        ExecutionStateNative.Set(ExecutionStateNative.Continuous);
        IsActive = false;
        EndsAt = null;
        StopTimer();
        Changed?.Invoke();
    }


    private static void OnTick(object? sender, EventArgs e) {
        if (EndsAt is null)
            return;

        if (DateTimeOffset.Now >= EndsAt.Value) {
            Disable();
            Expired?.Invoke();
            return;
        }

        Changed?.Invoke();
    }


    private static void StopTimer() {
        if (_timer is null)
            return;
        _timer.Stop();
        _timer.Tick -= OnTick;
        _timer = null;
    }
}
