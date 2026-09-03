using System.Text.RegularExpressions;

namespace SlashBar.Modules.Clipboard;

public static class ClipboardContentClassifier {

    private const int LongTextMinLength = 180;

    private static readonly Regex HexColor = new(
        @"^#(?:[0-9A-Fa-f]{3,4}|[0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})$",
        RegexOptions.Compiled);

    private static readonly Regex RgbColor = new(
        @"^rgba?\(\s*\d{1,3}%?\s*[, ]\s*\d{1,3}%?\s*[, ]\s*\d{1,3}%?(?:\s*[,/]\s*[\d.]+%?)?\s*\)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex HslColor = new(
        @"^hsla?\(\s*\d+(\.\d+)?(deg|rad|turn)?\s*[, ]\s*[\d.]+%\s*[, ]\s*[\d.]+%(?:\s*[,/]\s*[\d.]+%?)?\s*\)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex Email = new(
        @"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled);

    private static readonly Regex Url = new(
        @"^(https?://[^\s]+|www\.[^\s]+\.[a-zA-Z]{2,}\S*)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex WindowsPath = new(
        @"^(?:[A-Za-z]:\\|\\\\)[^<>:""|?*\r\n]+\\?$",
        RegexOptions.Compiled);

    private static readonly Regex UnixPath = new(
        @"^(?:~|/|\./|\.\./)[^\s<>:""|?*]+/?$",
        RegexOptions.Compiled);

    private static readonly Regex CssDeclaration = new(
        @"^\s*[a-zA-Z][\w-]*\s*:\s*.+;\s*$",
        RegexOptions.Compiled | RegexOptions.Multiline);

    public static ClipboardContentKind Classify(string? text) {
        var value = text?.Trim() ?? "";
        if (value.Length == 0)
            return ClipboardContentKind.Other;

        if (HexColor.IsMatch(value) || RgbColor.IsMatch(value) || HslColor.IsMatch(value))
            return ClipboardContentKind.Color;

        if (Email.IsMatch(value))
            return ClipboardContentKind.Email;

        if (Url.IsMatch(value))
            return ClipboardContentKind.Url;

        if (WindowsPath.IsMatch(value) || UnixPath.IsMatch(value))
            return ClipboardContentKind.FilePath;

        if (LooksLikeCode(value))
            return ClipboardContentKind.Code;

        if (value.Length >= LongTextMinLength || value.Split('\n').Length >= 4)
            return ClipboardContentKind.LongText;

        return ClipboardContentKind.Other;
    }

    private static bool LooksLikeCode(string value) {
        if (CssDeclaration.IsMatch(value))
            return true;

        if (value.Contains('{') && value.Contains('}'))
            return true;

        ReadOnlySpan<string> tokens = [
            "function ", "const ", "let ", "var ", "import ",
            "class ", "def ", "return ", "=>", "</"
        ];

        foreach (var token in tokens) {
            if (value.Contains(token, StringComparison.Ordinal))
                return true;
        }

        return false;
    }
}