namespace SlashBar.Modules.Shortcuts;

public sealed record ShortcutSlot(
    double OffsetY,
    string Glyph,
    string? Prefix,
    System.Windows.Media.ImageSource? Icon,
    string Label,
    System.Windows.Media.Brush? TileBackground = null,
    System.Windows.Media.Brush? TileBorderBrush = null);
