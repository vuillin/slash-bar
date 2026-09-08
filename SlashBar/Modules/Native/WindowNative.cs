using System.Runtime.InteropServices;
using System.Text;

namespace SlashBar.Modules.Native;

/// <summary>Semantic window helpers on top of <see cref="NativeMethods"/>.</summary>
public static class WindowNative {

    public static readonly IntPtr HwndTopmost = new(-1);
    public static readonly IntPtr HwndNotopmost = new(-2);

    public const uint SwpNosize = 0x0001;
    public const uint SwpNomove = 0x0002;
    public const uint SwpNoZOrder = 0x0004;
    public const uint SwpNoActivate = 0x0010;
    public const uint SwpFrameChanged = 0x0020;
    public const uint SwpShowWindow = 0x0040;

    public const int SwHide = 0;
    public const int SwShowMinimized = 2;
    public const int SwMaximize = 3;
    public const int SwShow = 5;
    public const int SwRestore = 9;

    public const int GwlExstyle = -20;
    public const int WsExTransparent = 0x00000020;
    public const int WsExToolwindow = 0x00000080;
    public const int WsExNoactivate = 0x08000000;
    public const int WsExLayered = 0x00080000;

    private const int DwmwaExtendedFrameBounds = 9;


    public static bool SetPos(
        IntPtr hwnd,
        IntPtr insertAfter,
        int x,
        int y,
        int cx,
        int cy,
        uint flags) =>
        NativeMethods.SetWindowPos(hwnd, insertAfter, x, y, cx, cy, flags);


    /// <summary>Move without resizing or activating (toasts).</summary>
    public static bool MoveNoActivate(IntPtr hwnd, int x, int y) =>
        SetPos(
            hwnd,
            IntPtr.Zero,
            x, y,
            0, 0,
            SwpNoZOrder | SwpNoActivate | SwpShowWindow | SwpNosize);


    /// <summary>Move + resize without activating (toast host layout).</summary>
    public static bool SetBoundsNoActivate(IntPtr hwnd, int x, int y, int width, int height) =>
        SetPos(
            hwnd,
            IntPtr.Zero,
            x, y, width, height,
            SwpNoZOrder | SwpNoActivate | SwpShowWindow);


    /// <summary>Toggle always-on-top without moving/resizing/activating.</summary>
    public static bool SetTopmost(IntPtr hwnd, bool topmost) =>
        SetPos(
            hwnd,
            topmost ? HwndTopmost : HwndNotopmost,
            0, 0, 0, 0,
            SwpNomove | SwpNosize | SwpNoActivate);


    /// <summary>Position + size, keep z-order, show, notify frame change (setup placer).</summary>
    public static bool Place(IntPtr hwnd, int x, int y, int width, int height) =>
        SetPos(
            hwnd,
            IntPtr.Zero,
            x, y, width, height,
            SwpNoZOrder | SwpShowWindow | SwpFrameChanged);


    /// <summary>Match another window's rect and stay topmost without activating (pin border).</summary>
    public static bool SyncTopmostBounds(IntPtr hwnd, int x, int y, int width, int height) =>
        SetPos(
            hwnd,
            HwndTopmost,
            x, y, width, height,
            SwpNoActivate | SwpShowWindow);


    public static bool Show(IntPtr hwnd, int cmdShow) =>
        NativeMethods.ShowWindow(hwnd, cmdShow);

    public static bool SetForeground(IntPtr hwnd) =>
        NativeMethods.SetForegroundWindow(hwnd);

    public static bool IsWindow(IntPtr hwnd) =>
        NativeMethods.IsWindow(hwnd);

    public static bool IsVisible(IntPtr hwnd) =>
        NativeMethods.IsWindowVisible(hwnd);

    public static bool IsMinimized(IntPtr hwnd) =>
        NativeMethods.IsIconic(hwnd);

    public static IntPtr GetForeground() =>
        NativeMethods.GetForegroundWindow();

    public static uint GetProcessId(IntPtr hwnd) {
        NativeMethods.GetWindowThreadProcessId(hwnd, out var pid);
        return pid;
    }

    public static string GetTitle(IntPtr hwnd, int capacity = 256) {
        var sb = new StringBuilder(capacity);
        _ = NativeMethods.GetWindowText(hwnd, sb, sb.Capacity);
        return sb.ToString();
    }

    public static bool EnumWindows(NativeMethods.EnumWindowsProc callback) =>
        NativeMethods.EnumWindows(callback, IntPtr.Zero);

    public static bool TryGetBounds(IntPtr hwnd, out NativeMethods.Rect bounds) {
        if (NativeMethods.DwmGetWindowAttribute(
                hwnd,
                DwmwaExtendedFrameBounds,
                out bounds,
                Marshal.SizeOf<NativeMethods.Rect>()) == 0)
            return true;

        return NativeMethods.GetWindowRect(hwnd, out bounds);
    }

    public static void EnableClickThrough(IntPtr hwnd) {
        var style = NativeMethods.GetWindowLongPtr(hwnd, GwlExstyle);
        var next = (nint)style | WsExTransparent | WsExToolwindow | WsExNoactivate | WsExLayered;
        NativeMethods.SetWindowLongPtr(hwnd, GwlExstyle, (IntPtr)next);
    }

    public static bool SetCursorPos(int x, int y) =>
        NativeMethods.SetCursorPos(x, y);
}
