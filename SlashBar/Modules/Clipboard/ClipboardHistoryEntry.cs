namespace SlashBar.Modules.Clipboard;

public sealed class ClipboardHistoryEntry {
    public string Id { get; set; } = "";
    public string Text { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
}