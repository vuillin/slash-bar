using System.Windows.Media;
using WpfBrush = System.Windows.Media.Brush;
using WpfColor = System.Windows.Media.Color;
namespace SlashBar.Modules.Shortcuts;

internal static class ShortcutTileBackgrounds {
    private static readonly WpfBrush DefaultBorder = CreateSolid(0x1A, 0, 0, 0);
    private static readonly WpfBrush Empty = CreateSolid(0x40, 0x8E, 0x8E, 0x93);
    private static readonly WpfBrush EmptyBorder = CreateSolid(0x33, 0xFF, 0xFF, 0xFF);

    private static readonly WpfBrush Weather = CreateLinear(
        (0x5A, 0xC8, 0xFA),
        (0x0A, 0x84, 0xFF),
        (0x00, 0x47, 0xAB));

    private static readonly WpfBrush Memo = CreateLinear(
        (0x6E, 0xA8, 0xFF),
        (0x3E, 0x8D, 0xFC),
        (0x1E, 0x5C, 0xC8));

    private static readonly WpfBrush Color = CreateLinear(
        (0xFF, 0x69, 0x61),
        (0xFF, 0x3B, 0x30),
        (0xC4, 0x1E, 0x16));

    private static readonly WpfBrush Clip = CreateLinear(
        (0x8E, 0x8E, 0xFF),
        (0x58, 0x56, 0xD6),
        (0x36, 0x34, 0xA3));

    private static WpfBrush CreateLinear(
        (byte r, byte g, byte b) light,
        (byte r, byte g, byte b) mid,
        (byte r, byte g, byte b) dark) {
        var brush = new LinearGradientBrush {
            StartPoint = new System.Windows.Point(0, 0),
            EndPoint = new System.Windows.Point(1, 1),
            GradientStops = {
                new GradientStop(WpfColor.FromRgb(light.r, light.g, light.b), 0),
                new GradientStop(WpfColor.FromRgb(mid.r, mid.g, mid.b), 0.55),
                new GradientStop(WpfColor.FromRgb(dark.r, dark.g, dark.b), 1),
            }
        };
        brush.Freeze();
        return brush;
    }

    public static WpfBrush? ForPrefix(string? prefix) => prefix switch {
        "weather" => Weather,
        "memo" => Memo,
        "color" => Color,
        "clip" => Clip,
        _ => null,
    };

    public static WpfBrush DefaultTileBorder => DefaultBorder;

    public static WpfBrush EmptyTileBackground => Empty;

    public static WpfBrush EmptyTileBorder => EmptyBorder;

    private static SolidColorBrush CreateSolid(byte a, byte r, byte g, byte b) {
        var brush = new SolidColorBrush(WpfColor.FromArgb(a, r, g, b));
        brush.Freeze();
        return brush;
    }
}

/// <summary>
/// Pins, labels, and icons for shortcuts around the bar.
/// </summary>
public static class ShortcutCatalog {

    public const int SlotCount = 5;

    private const double ArcFactor = 0.7;
    private const double AlignNudge = 17;

    // Near the bar → outward. null = empty slot.
    private static readonly string?[] LeftFromBar = ["color", "weather", null, null, null];
    private static readonly string?[] RightFromBar = ["memo", "clip", null, null, null];

    private static readonly Dictionary<string, string> Glyphs = new(StringComparer.OrdinalIgnoreCase);

    private static readonly Dictionary<string, string> IconKeys = new(StringComparer.OrdinalIgnoreCase) {
        ["color"] = "IconColor",
        ["memo"] = "IconMemo",
        ["clip"] = "IconClip",
        ["weather"] = "IconWeather",
    };

    private static readonly Dictionary<string, string> Labels = new(StringComparer.OrdinalIgnoreCase) {
        ["memo"] = "Memo",
        ["clip"] = "Clipboard",
        ["color"] = "Color Picker",
        ["weather"] = "Weather",
    };

    public static IReadOnlyList<ShortcutSlot> CreateLeftRail() =>
        CreateRail(LeftFromBar, mirrored: true);

    public static IReadOnlyList<ShortcutSlot> CreateRightRail() =>
        CreateRail(RightFromBar, mirrored: false);

    private static IReadOnlyList<ShortcutSlot> CreateRail(string?[] pinsFromBar, bool mirrored) {
        return Enumerable.Range(0, SlotCount)
            .Select(i => {
                var distanceFromBar = mirrored ? SlotCount - 1 - i : i;
                var offsetY = distanceFromBar * distanceFromBar * ArcFactor + AlignNudge;

                var prefix = distanceFromBar < pinsFromBar.Length
                    ? pinsFromBar[distanceFromBar]
                    : null;

                System.Windows.Media.ImageSource? icon = null;
                if (prefix != null
                    && IconKeys.TryGetValue(prefix, out var iconKey)
                    && System.Windows.Application.Current?.TryFindResource(iconKey) is System.Windows.Media.ImageSource found) {
                    icon = found;
                }

                var glyph = icon == null
                            && prefix != null
                            && Glyphs.TryGetValue(prefix, out var g)
                    ? g
                    : "";

                var label = prefix != null && Labels.TryGetValue(prefix, out var name)
                    ? name
                    : "";

                var tileBackground = prefix == null
                    ? ShortcutTileBackgrounds.EmptyTileBackground
                    : ShortcutTileBackgrounds.ForPrefix(prefix);
                var tileBorderBrush = prefix == null
                    ? ShortcutTileBackgrounds.EmptyTileBorder
                    : tileBackground != null
                        ? System.Windows.Media.Brushes.Transparent
                        : ShortcutTileBackgrounds.DefaultTileBorder;

                return new ShortcutSlot(
                    offsetY,
                    glyph,
                    prefix,
                    icon,
                    label,
                    tileBackground,
                    tileBorderBrush);
            })
            .ToList();
    }
}
