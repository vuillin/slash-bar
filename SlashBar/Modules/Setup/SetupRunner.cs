using System.Diagnostics;

namespace SlashBar.Modules.Setup;

public static class SetupRunner {

    public static void Run(SetupProfile profile) {

        foreach (var step in profile.Steps) {

            var processName = step.WindowProcessName;
            var existing = processName is null
                ? []
                : WindowPlacer.SnapshotWindows(processName);

            var process = Process.Start(new ProcessStartInfo {
                FileName = step.FileName,
                Arguments = step.Arguments ?? "",
                UseShellExecute = false
            });

            if (process is null || step.Layout == WindowLayout.Default)
                continue;

            var hwnd = processName is null
                ? WindowPlacer.WaitForMainWindow(process)
                : WindowPlacer.WaitForNewWindow(processName, existing);

            WindowPlacer.Apply(hwnd, step.Layout);
        }
    }
}
