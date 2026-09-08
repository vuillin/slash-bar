using System.Diagnostics;
using SlashBar.Modules.Native;

namespace SlashBar.Modules.Setup;

public static class WindowPlacer {

    public static async Task ApplyAsync(IntPtr hwnd, WindowLayout layout) {

        if (hwnd == IntPtr.Zero)
            return;

        var rightScreen = System.Windows.Forms.Screen.AllScreens
            .OrderBy(s => s.Bounds.X)
            .Last()
            .WorkingArea;

        switch (layout) {

            case WindowLayout.Maximize:
                WindowNative.Show(hwnd, WindowNative.SwMaximize);
                WindowNative.SetForeground(hwnd);
                break;

            case WindowLayout.Minimized:
                WindowNative.Show(hwnd, WindowNative.SwShowMinimized);
                break;

            case WindowLayout.RightMonitor:
                await PlaceAsync(hwnd, rightScreen.Left, rightScreen.Top, rightScreen.Width, rightScreen.Height);
                WindowNative.Show(hwnd, WindowNative.SwMaximize);
                WindowNative.SetForeground(hwnd);
                break;

            case WindowLayout.LeftHalf: {
                var w = rightScreen.Width / 2;
                await PlaceAsync(hwnd, rightScreen.Left, rightScreen.Top, w, rightScreen.Height);
                break;
            }

            case WindowLayout.RightHalf: {
                var w = rightScreen.Width / 2;
                await PlaceAsync(hwnd, rightScreen.Left + w, rightScreen.Top, w, rightScreen.Height);
                break;
            }
        }
    }


    private static async Task PlaceAsync(IntPtr hwnd, int x, int y, int width, int height) {
        WindowNative.Show(hwnd, WindowNative.SwRestore);
        WindowNative.Place(hwnd, x, y, width, height);
        await Task.Delay(200);
        WindowNative.Place(hwnd, x, y, width, height);
        WindowNative.SetForeground(hwnd);
    }


    public static HashSet<IntPtr> SnapshotWindows(string processName) {
        var result = new HashSet<IntPtr>();

        WindowNative.EnumWindows((hwnd, _) => {
            if (MatchesProcessWindow(hwnd, processName))
                result.Add(hwnd);
            return true;
        });

        return result;
    }


    public static async Task<IntPtr> WaitForNewWindowAsync(
        string processName,
        HashSet<IntPtr> existing,
        int timeoutMs = 10000) {

        var sw = Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < timeoutMs) {

            foreach (var hwnd in SnapshotWindows(processName)) {
                if (!existing.Contains(hwnd))
                    return hwnd;
            }

            await Task.Delay(150);
        }

        return IntPtr.Zero;
    }


    public static async Task<IntPtr> WaitForMainWindowAsync(
        Process process,
        string? processName = null,
        int timeoutMs = 10000) {

        var name = processName ?? process.ProcessName;
        var sw = Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < timeoutMs) {

            process.Refresh();

            if (process.MainWindowHandle != IntPtr.Zero)
                return process.MainWindowHandle;

            var hwnd = FindWindowByProcessName(name);
            if (hwnd != IntPtr.Zero)
                return hwnd;

            await Task.Delay(150);
        }

        return IntPtr.Zero;
    }


    private static IntPtr FindWindowByProcessName(string processName) {
        IntPtr found = IntPtr.Zero;

        WindowNative.EnumWindows((hwnd, _) => {
            if (!MatchesProcessWindow(hwnd, processName))
                return true;

            found = hwnd;
            return false;
        });

        return found;
    }


    private static bool MatchesProcessWindow(IntPtr hwnd, string processName) {
        if (!WindowNative.IsVisible(hwnd))
            return false;

        var pid = WindowNative.GetProcessId(hwnd);
        try {
            var p = Process.GetProcessById((int)pid);
            if (!p.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase))
                return false;
        }
        catch {
            return false;
        }

        return WindowNative.GetTitle(hwnd).Length > 0;
    }
}
