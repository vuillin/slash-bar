using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace SlashBar.Modules;


/// <summary>
/// Launches Firefox (URL, tab, search, private window).
/// </summary>
public static class FirefoxHelper {

    public static void Start(string args = "") {

        try {
            var process = Process.Start(new ProcessStartInfo {
                FileName = "firefox",
                Arguments = args,
                UseShellExecute = true
            });

            if (process is null)
                throw new InvalidOperationException("Firefox not found");
        }
        catch (Win32Exception) {
            throw new InvalidOperationException("Firefox not found");
        }
        catch (FileNotFoundException) {
            throw new InvalidOperationException("Firefox not found");
        }
    }

    public static void OpenUrl(string url, bool privateWindow = false) {

        var args = privateWindow
            ? $"-private-window \"{url}\""
            : $"\"{url}\"";

        Start(args);
    }

    public static void OpenNewTab(string url) =>
        Start($"-new-tab \"{url}\"");

    public static void Search(string query) {

        var escaped = query.Replace("\"", "\\\"");
        Start($"-search \"{escaped}\"");
    }

    public static void SearchPrivate(string query) {

        var searchUrl = "https://duckduckgo.com/?q=" + Uri.EscapeDataString(query);
        Start($"-private-window \"{searchUrl}\"");
    }
}
