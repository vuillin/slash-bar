namespace SlashBar.Modules.Clipboard;

public static class ClipboardHistory {
    public static ClipboardHistoryStore Store { get; } = new();
    public static ClipboardWatcher Watcher { get; } = new(Store);
}