namespace SlashBar.Modules.Color;

public sealed class ColorHistoryEntry {
    public string Id { get; set; } = "";
    public byte R { get; set; }
    public byte G { get; set; }
    public byte B { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}