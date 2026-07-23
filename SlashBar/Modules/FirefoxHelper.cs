using System.Diagnostics;

namespace SlashBar.Modules;


/// <summary>
/// Lance Firefox (URL, onglet, recherche, privé)
/// </summary>
public static class FirefoxHelper {

    public static void Start(string args = "") {

        Process.Start(new ProcessStartInfo {
            FileName = "firefox",
            Arguments = args,
            UseShellExecute = true
        });
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
