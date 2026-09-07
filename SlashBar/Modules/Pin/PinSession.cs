using System.Runtime.InteropServices;
using System.Text;

namespace SlashBar.Modules.Pin;

public static class PinSession {

    private static readonly IntPtr HwndTopmost = new(-1);
    private static readonly IntPtr HwndNotopmost = new(-2);

    private const uint SwpNomove = 0x0002;
    private const uint SwpNosize = 0x0001;
    private const uint SwpNoactivate = 0x0010;
    private const uint Flags = SwpNomove | SwpNosize | SwpNoactivate;

    private static readonly Dictionary<IntPtr, PinBorderWindow> Pinned = [];

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(
        IntPtr hWnd, IntPtr hWndInsertAfter,
        int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern bool IsWindow(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);


    public static int Count {
        get {
            Prune();
            return Pinned.Count;
        }
    }


    /// <summary>Toggle pin on the foreground window. Returns (pinned?, title) or null if nothing to do.</summary>
    public static (bool Pinned, string Title)? ToggleForeground() {
        Prune();

        var hwnd = GetForegroundWindow();
        if (hwnd == IntPtr.Zero || IsOwnProcess(hwnd))
            return null;

        var title = GetTitle(hwnd);

        if (Pinned.Remove(hwnd, out var border)) {
            SetTopmost(hwnd, false);
            CloseBorder(border);
            return (false, title);
        }

        SetTopmost(hwnd, true);
        Pinned[hwnd] = CreateBorder(hwnd);
        return (true, title);
    }


    public static int UnpinAll() {
        Prune();
        var n = Pinned.Count;
        foreach (var (hwnd, border) in Pinned) {
            SetTopmost(hwnd, false);
            CloseBorder(border);
        }
        Pinned.Clear();
        return n;
    }


    private static PinBorderWindow CreateBorder(IntPtr hwnd) {
        var border = new PinBorderWindow();
        border.Attach(hwnd);
        return border;
    }


    private static void CloseBorder(PinBorderWindow? border) {
        if (border is null)
            return;

        void CloseCore() {
            try {
                border.Detach();
            }
            catch {
                // window may already be closing
            }
        }

        var app = System.Windows.Application.Current;
        if (app?.Dispatcher is { } dispatcher && !dispatcher.CheckAccess())
            dispatcher.Invoke(CloseCore);
        else
            CloseCore();
    }


    private static void SetTopmost(IntPtr hwnd, bool topmost) =>
        SetWindowPos(
            hwnd,
            topmost ? HwndTopmost : HwndNotopmost,
            0, 0, 0, 0,
            Flags);


    private static void Prune() {
        List<IntPtr>? dead = null;

        foreach (var (hwnd, border) in Pinned) {
            if (IsWindow(hwnd))
                continue;

            dead ??= [];
            dead.Add(hwnd);
            CloseBorder(border);
        }

        if (dead is null)
            return;

        foreach (var hwnd in dead)
            Pinned.Remove(hwnd);
    }


    private static bool IsOwnProcess(IntPtr hwnd) {
        GetWindowThreadProcessId(hwnd, out var pid);
        return pid == (uint)Environment.ProcessId;
    }


    private static string GetTitle(IntPtr hwnd) {
        var sb = new StringBuilder(256);
        _ = GetWindowText(hwnd, sb, sb.Capacity);
        var title = sb.ToString().Trim();
        return string.IsNullOrEmpty(title) ? "Window" : title;
    }
}
