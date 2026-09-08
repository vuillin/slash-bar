using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using SlashBar.Modules.Native;

namespace SlashBar;

public partial class PinBorderWindow : Window {

    private readonly NativeMethods.WinEventProc _winEventProc;
    private readonly List<IntPtr> _hooks = [];
    private IntPtr _target;
    private bool _clickThroughApplied;
    private bool _moving;

    public event Action<IntPtr>? TargetLost;


    public PinBorderWindow() {
        InitializeComponent();
        // Keep delegate alive for native hooks
        _winEventProc = OnWinEvent;
    }


    public void Attach(IntPtr target) {
        _target = target;
        Show();
        EnsureClickThrough();
        InstallHooks();
        Sync();
    }


    public void Detach() {
        TargetLost = null;
        RemoveHooks();
        _target = IntPtr.Zero;
        Close();
    }


    private void InstallHooks() {
        var pid = WindowNative.GetProcessId(_target);

        uint[] events = [
            WinEventNative.EventObjectLocationChange,
            WinEventNative.EventSystemMovesizeStart,
            WinEventNative.EventSystemMovesizeEnd,
            WinEventNative.EventSystemMinimizeStart,
            WinEventNative.EventSystemMinimizeEnd,
            WinEventNative.EventObjectDestroy,
        ];

        foreach (var ev in events) {
            var hook = WinEventNative.Hook(ev, _winEventProc, pid);
            if (hook != IntPtr.Zero)
                _hooks.Add(hook);
        }
    }


    private void RemoveHooks() {
        foreach (var hook in _hooks)
            WinEventNative.Unhook(hook);
        _hooks.Clear();
    }


    private void OnWinEvent(
        IntPtr hWinEventHook, uint eventType, IntPtr hwnd,
        int idObject, int idChild, uint dwEventThread, uint dwmsEventTime) {

        if (_target == IntPtr.Zero || hwnd != _target)
            return;

        // Location spam includes non-window objects; only the window itself
        if (eventType == WinEventNative.EventObjectLocationChange
            && (idObject != WinEventNative.ObjidWindow || idChild != 0))
            return;

        Dispatcher.BeginInvoke(() => HandleEvent(eventType), DispatcherPriority.Send);
    }


    private void HandleEvent(uint eventType) {
        if (_target == IntPtr.Zero)
            return;

        switch (eventType) {
            case WinEventNative.EventSystemMovesizeStart:
                _moving = true;
                if (IsVisible)
                    Hide();
                break;

            case WinEventNative.EventSystemMovesizeEnd:
                _moving = false;
                Sync();
                break;

            case WinEventNative.EventSystemMinimizeStart:
                if (IsVisible)
                    Hide();
                break;

            case WinEventNative.EventSystemMinimizeEnd:
                Sync();
                break;

            case WinEventNative.EventObjectDestroy:
                LostTarget();
                break;

            case WinEventNative.EventObjectLocationChange:
                if (!_moving)
                    Sync();
                break;
        }
    }


    private void Sync() {
        if (_target == IntPtr.Zero)
            return;

        if (!WindowNative.IsWindow(_target)) {
            LostTarget();
            return;
        }

        if (_moving || WindowNative.IsMinimized(_target) || !WindowNative.IsVisible(_target)) {
            if (IsVisible)
                Hide();
            return;
        }

        if (!WindowNative.TryGetBounds(_target, out var bounds))
            return;

        var width = bounds.Width;
        var height = bounds.Height;
        if (width <= 0 || height <= 0) {
            if (IsVisible)
                Hide();
            return;
        }

        if (!IsVisible)
            Show();

        var borderHwnd = new WindowInteropHelper(this).EnsureHandle();
        EnsureClickThrough();

        WindowNative.SyncTopmostBounds(
            borderHwnd,
            bounds.Left,
            bounds.Top,
            width,
            height);
    }


    private void EnsureClickThrough() {
        if (_clickThroughApplied)
            return;

        var hwnd = new WindowInteropHelper(this).EnsureHandle();
        WindowNative.EnableClickThrough(hwnd);
        _clickThroughApplied = true;
    }


    private void LostTarget() {
        if (_target == IntPtr.Zero)
            return;

        var hwnd = _target;
        RemoveHooks();
        _target = IntPtr.Zero;
        if (IsVisible)
            Hide();

        TargetLost?.Invoke(hwnd);
    }
}
