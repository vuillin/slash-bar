using System.Windows;
using System.Windows.Media;

namespace SlashBar.Modules.Shortcuts;

/// <summary>
/// Pins, labels, and icons for shortcuts around the bar.
/// </summary>
public static class ShortcutCatalog {

    public const int SlotCount = 5;

    private const double ArcFactor = 0.7;
    private const double AlignNudge = 17;

    // Near the bar → outward. null = empty slot.
    private static readonly string?[] LeftFromBar = ["memo", null, null, null, null];
    private static readonly string?[] RightFromBar = ["color", "clip", null, null, null];

    private static readonly Dictionary<string, string> Glyphs = new(StringComparer.OrdinalIgnoreCase);

    private static readonly Dictionary<string, string> IconKeys = new(StringComparer.OrdinalIgnoreCase) {
        ["color"] = "IconColor",
        ["memo"] = "IconMemo",
        ["clip"] = "IconClip",
    };

    private static readonly Dictionary<string, string> Labels = new(StringComparer.OrdinalIgnoreCase) {
        ["memo"] = "Memo",
        ["clip"] = "Clipboard",
        ["color"] = "Color Picker",
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

                ImageSource? icon = null;
                if (prefix != null
                    && IconKeys.TryGetValue(prefix, out var iconKey)
                    && System.Windows.Application.Current?.TryFindResource(iconKey) is ImageSource found) {
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

                return new ShortcutSlot(offsetY, glyph, prefix, icon, label);
            })
            .ToList();
    }
}
