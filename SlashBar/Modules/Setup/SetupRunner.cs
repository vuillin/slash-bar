using System.Diagnostics;

namespace SlashBar.Modules.Setup;

public static class SetupRunner {

    public static async Task RunAsync(SetupProfile profile) {

        foreach (var step in profile.Steps) {

            var processName = step.WindowProcessName;
            var existing = processName is null
                ? []
                : WindowPlacer.SnapshotWindows(processName);

            var fileName = Environment.ExpandEnvironmentVariables(step.FileName);

            var process = Process.Start(new ProcessStartInfo {
                FileName = fileName,
                Arguments = step.Arguments ?? "",
                UseShellExecute = false
            });

            if (process is null || step.Layout == WindowLayout.Default)
                continue;

            var hwnd = processName is null
                ? await WindowPlacer.WaitForMainWindowAsync(process)
                : await WindowPlacer.WaitForNewWindowAsync(processName, existing);

            await WindowPlacer.ApplyAsync(hwnd, step.Layout);
        }
    }
}
