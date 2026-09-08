using SlashBar.Modules.Native;

namespace SlashBar.Modules.Pin;

public static class PinSession {

    private static readonly Dictionary<IntPtr, PinBorderWindow> Pinned = [];


    public static int Count {
        get {
            Prune();
            return Pinned.Count;
        }
    }


    /// <summary>Toggle pin on the foreground window. Returns (pinned?, title) or null if nothing to do.</summary>
    public static (bool Pinned, string Title)? ToggleForeground() {
        Prune();

        var hwnd = WindowNative.GetForeground();
        if (hwnd == IntPtr.Zero || IsOwnProcess(hwnd))
            return null;

        var title = GetTitle(hwnd);

        if (Pinned.Remove(hwnd, out var border)) {
            WindowNative.SetTopmost(hwnd, false);
            CloseBorder(border);
            return (false, title);
        }

        WindowNative.SetTopmost(hwnd, true);
        Pinned[hwnd] = CreateBorder(hwnd);
        return (true, title);
    }


    public static int UnpinAll() {
        Prune();
        var n = Pinned.Count;
        foreach (var (hwnd, border) in Pinned) {
            WindowNative.SetTopmost(hwnd, false);
            CloseBorder(border);
        }
        Pinned.Clear();
        return n;
    }


    private static PinBorderWindow CreateBorder(IntPtr hwnd) {
        var border = new PinBorderWindow();
        border.TargetLost += OnBorderTargetLost;
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


    private static void Prune() {
        List<IntPtr>? dead = null;

        foreach (var (hwnd, border) in Pinned) {
            if (WindowNative.IsWindow(hwnd))
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


    private static bool IsOwnProcess(IntPtr hwnd) =>
        WindowNative.GetProcessId(hwnd) == (uint)Environment.ProcessId;


    private static string GetTitle(IntPtr hwnd) {
        var title = WindowNative.GetTitle(hwnd).Trim();
        return string.IsNullOrEmpty(title) ? "Window" : title;
    }


    private static void OnBorderTargetLost(IntPtr hwnd) {
        if (!Pinned.Remove(hwnd, out var border))
            return;

        CloseBorder(border);
    }
}
