using System.Windows;
using System.Windows.Input;
using SlashBar.Modules;

namespace SlashBar;

public partial class MainWindow : Window {

    private readonly ModuleRegistry _modules = new();

    public MainWindow() {
        InitializeComponent();

        InitShortcuts();
        PositionAtBottom();

        Loaded += OnLoaded;

        PreviewKeyDown += (_, e) => {
            if (e.Key == Key.Escape) {
                AnimateClose();
                e.Handled = true;
            }
            else if (e.Key == Key.Tab) {
                if (AcceptCompletion())
                    e.Handled = true;
            }
            else if (e.Key == Key.Down) {
                if (CycleCompletion(reverse: false) || NavigateHistory(reverse: false))
                    e.Handled = true;
            }
            else if (e.Key == Key.Up) {
                if (CycleCompletion(reverse: true) || NavigateHistory(reverse: true))
                    e.Handled = true;
            }
        };

        SearchBox.KeyDown += (_, e) => {
            if (e.Key == Key.Enter) {
                SubmitCommand();
                e.Handled = true;
            }
        };

        SearchBox.TextChanged += (_, _) => {
            if (_applyingCompletion)
                return;

            // manual typing → exit history navigation
            if (_historyIndex >= 0)
                ResetHistoryNavigation();

            _completionIndex = 0;
            UpdateSuggestions();
        };
    }

    private void SubmitCommand() =>
        ExecuteShortcut(SearchBox.Text);

    private void ExecuteShortcut(string command) {
        try {
            if (!_modules.TryExecute(command, out var result))
                return;

            _commandHistory.Add(command);

            if (result.Kind == ModuleResultKind.Success)
                AppToast.ShowSuccess(result.Message, result.Detail);
            else if (result.Kind == ModuleResultKind.Fail)
                AppToast.ShowError(result.Message);

            AnimateClose();
        }
        catch (Exception ex) {
            AppToast.ShowError(ex.Message);
            AnimateClose();
        }
    }
}
