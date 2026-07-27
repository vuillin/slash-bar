namespace SlashBar.Modules.Color;

public static class ColorFormats {

    public static string ToHex(System.Windows.Media.Color color) =>
        $"#{color.R:X2}{color.G:X2}{color.B:X2}".ToLowerInvariant();

    public static string ToRgbDisplay(System.Windows.Media.Color color) =>
        $"{color.R}, {color.G}, {color.B}";

    public static string ToRgbClipboard(System.Windows.Media.Color color) =>
        $"rgb({color.R}, {color.G}, {color.B})";
}
