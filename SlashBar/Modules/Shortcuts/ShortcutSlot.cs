using System.Windows.Media;

namespace SlashBar.Modules.Shortcuts;

public sealed record ShortcutSlot(
    double OffsetY,
    string Glyph,
    string? Prefix,
    ImageSource? Icon,
    string Label);
