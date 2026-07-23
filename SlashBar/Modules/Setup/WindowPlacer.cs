using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace SlashBar.Modules.Setup;

public static class WindowPlacer {

    private const int SwMaximize = 3;
    private const int SwShowMinimized = 2;
    private const int SwRestore = 9;
    private const int SwpNoZOrder = 0x0004;
    private const int SwpShowWindow = 0x0040;
    private const int SwpFrameChanged = 0x0020;

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(
        IntPtr hWnd, IntPtr hWndInsertAfter,
        int X, int Y, int cx, int cy, uint uFlags);


    public static void Apply(IntPtr hwnd, WindowLayout layout) {

        if (hwnd == IntPtr.Zero)
            return;

        var rightScreen = System.Windows.Forms.Screen.AllScreens
            .OrderBy(s => s.Bounds.X)
            .Last()
            .WorkingArea;

        switch (layout) {

            case WindowLayout.Maximize:
                ShowWindow(hwnd, SwMaximize);
                SetForegroundWindow(hwnd);
                break;

            case WindowLayout.Minimized:
                ShowWindow(hwnd, SwShowMinimized); // 2
                break;

            case WindowLayout.RightMonitor:
                Place(hwnd, rightScreen.Left, rightScreen.Top, rightScreen.Width, rightScreen.Height);
                ShowWindow(hwnd, SwMaximize);
                SetForegroundWindow(hwnd);
                break;

            case WindowLayout.LeftHalf: {
                var w = rightScreen.Width / 2;
                Place(hwnd, rightScreen.Left, rightScreen.Top, w, rightScreen.Height);
                break;
            }

            case WindowLayout.RightHalf: {
                var w = rightScreen.Width / 2;
                Place(hwnd, rightScreen.Left + w, rightScreen.Top, w, rightScreen.Height);
                break;
            }
        }
    }


    private static void Place(IntPtr hwnd, int x, int y, int width, int height) {

        var flags = (uint)(SwpNoZOrder | SwpShowWindow | SwpFrameChanged);
        ShowWindow(hwnd, SwRestore);
        SetWindowPos(hwnd, IntPtr.Zero, x, y, width, height, flags);
        Thread.Sleep(200);
        SetWindowPos(hwnd, IntPtr.Zero, x, y, width, height, flags);
        SetForegroundWindow(hwnd);
    }


    public static HashSet<IntPtr> SnapshotWindows(string processName) {

        var result = new HashSet<IntPtr>();

        EnumWindows((hwnd, _) => {

            if (MatchesProcessWindow(hwnd, processName))
                result.Add(hwnd);

            return true;
        }, IntPtr.Zero);

        return result;
    }


    public static IntPtr WaitForNewWindow(
        string processName,
        HashSet<IntPtr> existing,
        int timeoutMs = 10000) {

        var sw = Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < timeoutMs) {

            foreach (var hwnd in SnapshotWindows(processName)) {
                if (!existing.Contains(hwnd))
                    return hwnd;
            }

            Thread.Sleep(150);
        }

        return IntPtr.Zero;
    }


    public static IntPtr WaitForMainWindow(Process process, string? processName = null, int timeoutMs = 10000) {

        var name = processName ?? process.ProcessName;
        var sw = Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < timeoutMs) {

            process.Refresh();

            if (process.MainWindowHandle != IntPtr.Zero)
                return process.MainWindowHandle;

            var hwnd = FindWindowByProcessName(name);
            if (hwnd != IntPtr.Zero)
                return hwnd;

            Thread.Sleep(150);
        }

        return IntPtr.Zero;
    }


    private static IntPtr FindWindowByProcessName(string processName) {

        IntPtr found = IntPtr.Zero;

        EnumWindows((hwnd, _) => {

            if (!MatchesProcessWindow(hwnd, processName))
                return true;

            found = hwnd;
            return false;
        }, IntPtr.Zero);

        return found;
    }


    private static bool MatchesProcessWindow(IntPtr hwnd, string processName) {

        if (!IsWindowVisible(hwnd))
            return false;

        GetWindowThreadProcessId(hwnd, out var pid);
        try {
            var p = Process.GetProcessById((int)pid);
            if (!p.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase))
                return false;
        }
        catch {
            return false;
        }

        var sb = new StringBuilder(256);
        GetWindowText(hwnd, sb, sb.Capacity);
        return sb.Length > 0;
    }


    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
}
