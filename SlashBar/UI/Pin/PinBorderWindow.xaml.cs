using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace SlashBar;

public partial class PinBorderWindow : Window {

    private const int GwlExstyle = -20;
    private const int WsExTransparent = 0x00000020;
    private const int WsExToolwindow = 0x00000080;
    private const int WsExNoactivate = 0x08000000;
    private const int WsExLayered = 0x00080000;

    private const uint SwpNoactivate = 0x0010;
    private const uint SwpShowWindow = 0x0040;
    private const int DwmwaExtendedFrameBounds = 9;

    private static readonly IntPtr HwndTopmost = new(-1);

    private const uint EventSystemMovesizeStart = 0x000A;
    private const uint EventSystemMovesizeEnd = 0x000B;
    private const uint EventSystemMinimizeStart = 0x0016;
    private const uint EventSystemMinimizeEnd = 0x0017;
    private const uint EventObjectDestroy = 0x8001;
    private const uint EventObjectLocationChange = 0x800B;

    private const int ObjidWindow = 0;
    private const uint WinEventOutOfContext = 0x0000;
    private const uint WinEventSkipOwnProcess = 0x0002;

    private readonly WinEventProc _winEventProc;
    private readonly List<IntPtr> _hooks = [];
    private IntPtr _target;
    private bool _clickThroughApplied;
    private bool _moving;

    public event Action<IntPtr>? TargetLost;


    private delegate void WinEventProc(
        IntPtr hWinEventHook, uint eventType, IntPtr hwnd,
        int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);


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
        GetWindowThreadProcessId(_target, out var pid);

        uint[] events = [
            EventObjectLocationChange,
            EventSystemMovesizeStart,
            EventSystemMovesizeEnd,
            EventSystemMinimizeStart,
            EventSystemMinimizeEnd,
            EventObjectDestroy,
        ];

        foreach (var ev in events) {
            var hook = SetWinEventHook(
                ev, ev,
                IntPtr.Zero,
                _winEventProc,
                pid,
                0,
                WinEventOutOfContext | WinEventSkipOwnProcess);

            if (hook != IntPtr.Zero)
                _hooks.Add(hook);
        }
    }


    private void RemoveHooks() {
        foreach (var hook in _hooks)
            UnhookWinEvent(hook);
        _hooks.Clear();
    }


    private void OnWinEvent(
        IntPtr hWinEventHook, uint eventType, IntPtr hwnd,
        int idObject, int idChild, uint dwEventThread, uint dwmsEventTime) {

        if (_target == IntPtr.Zero || hwnd != _target)
            return;

        // Location spam includes non-window objects; only the window itself
        if (eventType == EventObjectLocationChange && (idObject != ObjidWindow || idChild != 0))
            return;

        Dispatcher.BeginInvoke(() => HandleEvent(eventType), DispatcherPriority.Send);
    }


    private void HandleEvent(uint eventType) {
        if (_target == IntPtr.Zero)
            return;

        switch (eventType) {
            case EventSystemMovesizeStart:
                _moving = true;
                if (IsVisible)
                    Hide();
                break;

            case EventSystemMovesizeEnd:
                _moving = false;
                Sync();
                break;

            case EventSystemMinimizeStart:
                if (IsVisible)
                    Hide();
                break;

            case EventSystemMinimizeEnd:
                Sync();
                break;

            case EventObjectDestroy:
                LostTarget();
                break;

            case EventObjectLocationChange:
                if (!_moving)
                    Sync();
                break;
        }
    }


    private void Sync() {
        if (_target == IntPtr.Zero)
            return;

        if (!IsWindow(_target)) {
            LostTarget();
            return;
        }

        if (_moving || IsIconic(_target) || !IsWindowVisible(_target)) {
            if (IsVisible)
                Hide();
            return;
        }

        if (!TryGetBounds(_target, out var bounds))
            return;

        var width = bounds.Right - bounds.Left;
        var height = bounds.Bottom - bounds.Top;
        if (width <= 0 || height <= 0) {
            if (IsVisible)
                Hide();
            return;
        }

        if (!IsVisible)
            Show();

        var borderHwnd = new WindowInteropHelper(this).EnsureHandle();
        EnsureClickThrough();

        SetWindowPos(
            borderHwnd,
            HwndTopmost,
            bounds.Left,
            bounds.Top,
            width,
            height,
            SwpNoactivate | SwpShowWindow);
    }


    private void EnsureClickThrough() {
        if (_clickThroughApplied)
            return;

        var hwnd = new WindowInteropHelper(this).EnsureHandle();
        var style = GetWindowLongPtr(hwnd, GwlExstyle);
        var next = (nint)style | WsExTransparent | WsExToolwindow | WsExNoactivate | WsExLayered;
        SetWindowLongPtr(hwnd, GwlExstyle, (IntPtr)next);
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


    private static bool TryGetBounds(IntPtr hwnd, out WinRect bounds) {
        if (DwmGetWindowAttribute(hwnd, DwmwaExtendedFrameBounds, out bounds, Marshal.SizeOf<WinRect>()) == 0)
            return true;

        return GetWindowRect(hwnd, out bounds);
    }


    [StructLayout(LayoutKind.Sequential)]
    private struct WinRect {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }


    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(
        IntPtr hWnd, IntPtr hWndInsertAfter,
        int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern bool IsWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out WinRect lpRect);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("dwmapi.dll")]
    private static extern int DwmGetWindowAttribute(
        IntPtr hwnd, int dwAttribute, out WinRect pvAttribute, int cbAttribute);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
    private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll")]
    private static extern IntPtr SetWinEventHook(
        uint eventMin, uint eventMax, IntPtr hmodWinEventProc,
        WinEventProc lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

    [DllImport("user32.dll")]
    private static extern bool UnhookWinEvent(IntPtr hWinEventHook);
}
