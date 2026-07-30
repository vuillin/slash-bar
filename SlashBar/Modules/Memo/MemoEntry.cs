namespace SlashBar.Modules.Memo;

public sealed class MemoEntry {

    public string Id { get; set; } = "";

    public string Name { get; set; } = "";

    public string Value { get; set; } = "";

    public DateTimeOffset CreatedAt { get; set; }
}