using SlashBar.Modules;

namespace SlashBar;

public partial class MainWindow {

    private readonly CommandHistoryStore _commandHistory = new();
    private int _historyIndex = -1; // -1 = current line
    private string _historyDraft = "";

    private bool NavigateHistory(bool reverse) {
        var entries = _commandHistory.GetAll();
        if (entries.Count == 0)
            return false;

        // ↑ = older (index +), ↓ = newer (index -)
        if (reverse) // Up
        {
            if (_historyIndex < 0)
                _historyDraft = SearchBox.Text;

            if (_historyIndex >= entries.Count - 1)
                return true;

            _historyIndex++;
            ApplyHistoryEntry(entries[_historyIndex]);
            return true;
        }

        // Down
        if (_historyIndex < 0)
            return false;

        _historyIndex--;
        if (_historyIndex < 0) {
            ApplyHistoryEntry(_historyDraft);
            return true;
        }

        ApplyHistoryEntry(entries[_historyIndex]);
        return true;
    }

    private void ApplyHistoryEntry(string text) {
        _applyingCompletion = true;
        SearchBox.Text = text;
        SearchBox.CaretIndex = SearchBox.Text.Length;
        _applyingCompletion = false;
        _completionIndex = 0;
        UpdateSuggestions();
    }

    private void ResetHistoryNavigation() {
        _historyIndex = -1;
        _historyDraft = "";
    }
}
