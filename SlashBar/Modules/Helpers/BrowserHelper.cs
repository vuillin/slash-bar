using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace SlashBar.Modules;

/// <summary>
/// Opens URLs / searches in the default browser
/// </summary>
public static class BrowserHelper {

    private enum BrowserKind {
        Unknown,
        Firefox,
        Chrome,
        Edge,
        Brave,
        Opera,
        Vivaldi
    }

    public static void Start(bool privateWindow = false) =>
        StartBrowser(privateWindow ? PrivateArgs() : "");

    public static void OpenUrl(string url, bool privateWindow = false) {
        if (!privateWindow) {
            OpenWithShell(url);
            return;
        }

        StartBrowser(PrivateArgs(url));
    }

    public static void OpenNewTab(string url) =>
        OpenWithShell(url);

    public static void Search(string query) {
        var url = "https://www.google.com/search?q=" + Uri.EscapeDataString(query);
        OpenWithShell(url);
    }

    public static void SearchPrivate(string query) {
        var url = "https://www.google.com/search?q=" + Uri.EscapeDataString(query);
        StartBrowser(PrivateArgs(url));
    }

    private static void OpenWithShell(string url) {
        try {
            Process.Start(new ProcessStartInfo {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception ex) when (ex is Win32Exception or FileNotFoundException or InvalidOperationException) {
            throw new InvalidOperationException("Browser not found");
        }
    }

    private static void StartBrowser(string args) {
        var (exe, _) = ResolveBrowser();
        try {
            var process = Process.Start(new ProcessStartInfo {
                FileName = exe,
                Arguments = args,
                UseShellExecute = true
            });

            if (process is null)
                throw new InvalidOperationException("Browser not found");
        }
        catch (Win32Exception) {
            throw new InvalidOperationException("Browser not found");
        }
        catch (FileNotFoundException) {
            throw new InvalidOperationException("Browser not found");
        }
    }

    private static string PrivateArgs(string? url = null) {
        var (_, kind) = ResolveBrowser();

        var flag = kind switch {
            BrowserKind.Firefox => "-private-window",
            BrowserKind.Edge => "--inprivate",
            BrowserKind.Opera => "--private",
            // Chrome / Brave / Vivaldi / Chromium-like / unknown
            _ => "--incognito"
        };

        return string.IsNullOrEmpty(url) ? flag : $"{flag} \"{url}\"";
    }

    private static (string exe, BrowserKind kind) ResolveBrowser() {
        if (TryGetDefaultBrowserExe(out var exe))
            return (exe, DetectKind(exe));

        foreach (var candidate in KnownBrowserPaths()) {
            if (File.Exists(candidate))
                return (candidate, DetectKind(candidate));
        }

        // Edge is almost always present on Win10/11
        var edge = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            "Microsoft", "Edge", "Application", "msedge.exe");

        if (File.Exists(edge))
            return (edge, BrowserKind.Edge);

        throw new InvalidOperationException("Browser not found");
    }

    private static bool TryGetDefaultBrowserExe(out string exe) {
        exe = "";

        try {
            using var userChoice = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\https\UserChoice");

            var progId = userChoice?.GetValue("ProgId") as string;
            if (string.IsNullOrWhiteSpace(progId))
                return false;

            using var commandKey = Registry.ClassesRoot.OpenSubKey($@"{progId}\shell\open\command");
            var command = commandKey?.GetValue(null) as string;
            if (string.IsNullOrWhiteSpace(command))
                return false;

            // "C:\...\chrome.exe" --single-argument %1
            var match = Regex.Match(command, "^\\s*\"([^\"]+)\"");
            if (!match.Success)
                match = Regex.Match(command, "^\\s*(\\S+)");

            if (!match.Success)
                return false;

            var path = match.Groups[1].Value;
            if (!File.Exists(path))
                return false;

            exe = path;
            return true;
        }
        catch {
            return false;
        }
    }

    private static IEnumerable<string> KnownBrowserPaths() {
        var pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var pf86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        yield return Path.Combine(pf, "Google", "Chrome", "Application", "chrome.exe");
        yield return Path.Combine(pf86, "Google", "Chrome", "Application", "chrome.exe");
        yield return Path.Combine(local, "Google", "Chrome", "Application", "chrome.exe");

        yield return Path.Combine(pf86, "Microsoft", "Edge", "Application", "msedge.exe");
        yield return Path.Combine(pf, "Microsoft", "Edge", "Application", "msedge.exe");

        yield return Path.Combine(pf, "Mozilla Firefox", "firefox.exe");
        yield return Path.Combine(pf86, "Mozilla Firefox", "firefox.exe");

        yield return Path.Combine(local, "BraveSoftware", "Brave-Browser", "Application", "brave.exe");
        yield return Path.Combine(pf, "BraveSoftware", "Brave-Browser", "Application", "brave.exe");

        yield return Path.Combine(local, "Programs", "Opera", "opera.exe");
        yield return Path.Combine(pf, "Opera", "opera.exe");
        yield return Path.Combine(pf86, "Opera", "opera.exe");
        yield return Path.Combine(local, "Programs", "Opera GX", "opera.exe");

        yield return Path.Combine(local, "Vivaldi", "Application", "vivaldi.exe");
        yield return Path.Combine(pf, "Vivaldi", "Application", "vivaldi.exe");
    }

    private static BrowserKind DetectKind(string exe) {
        var name = Path.GetFileNameWithoutExtension(exe).ToLowerInvariant();
        return name switch {
            "firefox" => BrowserKind.Firefox,
            "chrome" => BrowserKind.Chrome,
            "msedge" => BrowserKind.Edge,
            "brave" => BrowserKind.Brave,
            "opera" => BrowserKind.Opera,
            "vivaldi" => BrowserKind.Vivaldi,
            _ => BrowserKind.Unknown
        };
    }
}