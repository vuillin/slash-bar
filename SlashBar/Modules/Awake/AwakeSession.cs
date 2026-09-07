using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace SlashBar.Modules.Awake;

public static class AwakeSession {

    private const uint EsContinuous = 0x80000000;
    private const uint EsSystemRequired = 0x00000001;
    private const uint EsDisplayRequired = 0x00000002;

    private static DispatcherTimer? _timer;

    [DllImport("kernel32.dll")]
    private static extern uint SetThreadExecutionState(uint esFlags);


    public static bool IsActive { get; private set; }
    public static AwakeMode Mode { get; private set; }
    public static DateTimeOffset? EndsAt { get; private set; }

    public static event Action? Changed;
    public static event Action? Expired;


    public static void Enable(AwakeMode mode, TimeSpan? duration = null) {
        var flags = mode == AwakeMode.System
            ? EsContinuous | EsSystemRequired
            : EsContinuous | EsSystemRequired | EsDisplayRequired;

        SetThreadExecutionState(flags);
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

        SetThreadExecutionState(EsContinuous);
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