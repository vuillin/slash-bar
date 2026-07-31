using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using SlashBar.Modules;

namespace SlashBar;

public partial class MainWindow {

    private sealed record ArgSuggestion(string Value, string Description, bool IsSelected);
    private sealed record ModuleSuggestion(string Prefix, string Name, string Description, bool IsSelected);

    private bool _applyingCompletion;
    private IReadOnlyList<ArgCompletion> _argCompletions = Array.Empty<ArgCompletion>();
    private IReadOnlyList<IModule> _moduleCompletions = Array.Empty<IModule>();
    private int _completionIndex;
    private string _ghostSuffix = "";
    private bool _suggestionsOpen;
    private int _suggestionsAnimEpoch;

    private void UpdateSuggestions() {
        var inArgMode = _modules.IsInArgumentMode(SearchBox.Text);

        if (inArgMode) {
            _moduleCompletions = Array.Empty<IModule>();
            _argCompletions = _modules.SuggestArgumentCompletions(SearchBox.Text);

            if (_completionIndex >= _argCompletions.Count)
                _completionIndex = 0;

            ModuleSuggestionsList.Visibility = Visibility.Collapsed;
            ArgSuggestionsList.Visibility = Visibility.Visible;
            RefreshArgSuggestionsList();
            UpdateArgGhost();

            if (_argCompletions.Count == 0) {
                HideSuggestionsPanel();
                return;
            }

            ShowSuggestionsPanel();
            return;
        }

        _argCompletions = Array.Empty<ArgCompletion>();

        var raw = SearchBox.Text.TrimStart();
        if (raw.Contains(' ')) {
            _moduleCompletions = Array.Empty<IModule>();
            ClearGhost();
            HideSuggestionsPanel();
            return;
        }

        _moduleCompletions = _modules.Suggest(SearchBox.Text, max: 5);

        if (_completionIndex >= _moduleCompletions.Count)
            _completionIndex = 0;

        ArgSuggestionsList.Visibility = Visibility.Collapsed;
        ModuleSuggestionsList.Visibility = Visibility.Visible;
        RefreshModuleSuggestionsList();
        UpdateModuleGhost();

        if (_moduleCompletions.Count == 0) {
            HideSuggestionsPanel();
            return;
        }

        ShowSuggestionsPanel();
    }

    private void RefreshArgSuggestionsList() {
        ArgSuggestionsList.ItemsSource = _argCompletions
            .Select((c, i) => new ArgSuggestion(c.Value, c.Description, i == _completionIndex))
            .ToList();
    }

    private void RefreshModuleSuggestionsList() {
        ModuleSuggestionsList.ItemsSource = _moduleCompletions
            .Select((m, i) => new ModuleSuggestion(m.Prefix, m.Name, m.Description, i == _completionIndex))
            .ToList();
    }

    private void ShowSuggestionsPanel() {
        SuggestionsCard.Width = RootBorder.ActualWidth > 0 ? RootBorder.ActualWidth : RootBorder.Width;

        if (!SuggestionsPopup.IsOpen)
            SuggestionsPopup.IsOpen = true;

        SuggestionsCard.UpdateLayout();

        var target = MeasureSuggestionsHeight();
        if (target <= 0) {
            HideSuggestionsPanel();
            return;
        }

        // set height immediately — only opacity / slide are animated
        SuggestionsHost.BeginAnimation(HeightProperty, null);
        SuggestionsHost.Height = target;

        if (_suggestionsOpen)
            return;

        _suggestionsOpen = true;
        AnimateSuggestionsIn();
    }

    private void HideSuggestionsPanel() {
        ClearGhost();

        if (!_suggestionsOpen) {
            ResetSuggestionsInstant();
            return;
        }

        _suggestionsOpen = false;
        AnimateSuggestionsOut();
    }

    private double MeasureSuggestionsHeight() {
        var width = SuggestionsCard.Width;
        if (width <= 0)
            width = RootBorder.ActualWidth > 0 ? RootBorder.ActualWidth : RootBorder.Width;

        SuggestionsPanel.InvalidateMeasure();
        SuggestionsPanel.Measure(new System.Windows.Size(width, double.PositiveInfinity));
        return SuggestionsPanel.DesiredSize.Height;
    }

    private void AnimateSuggestionsIn() {
        var epoch = ++_suggestionsAnimEpoch;
        var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
        var duration = TimeSpan.FromMilliseconds(180);

        SuggestionsCard.BeginAnimation(OpacityProperty, null);
        SuggestionsSlide.BeginAnimation(TranslateTransform.YProperty, null);

        SuggestionsCard.Opacity = 0;
        SuggestionsSlide.Y = 10;

        var fade = new DoubleAnimation(0, 1, duration) { EasingFunction = ease };
        fade.Completed += (_, _) => {
            if (epoch != _suggestionsAnimEpoch)
                return;
            SuggestionsCard.Opacity = 1;
            SuggestionsCard.BeginAnimation(OpacityProperty, null);
            SuggestionsSlide.Y = 0;
            SuggestionsSlide.BeginAnimation(TranslateTransform.YProperty, null);
        };

        SuggestionsCard.BeginAnimation(OpacityProperty, fade);
        SuggestionsSlide.BeginAnimation(TranslateTransform.YProperty,
            new DoubleAnimation(10, 0, duration) { EasingFunction = ease });
    }

    private void AnimateSuggestionsOut() {
        var epoch = ++_suggestionsAnimEpoch;
        var ease = new CubicEase { EasingMode = EasingMode.EaseIn };
        var duration = TimeSpan.FromMilliseconds(140);

        SuggestionsCard.BeginAnimation(OpacityProperty, null);
        SuggestionsSlide.BeginAnimation(TranslateTransform.YProperty, null);

        var fromOpacity = SuggestionsCard.Opacity;
        var fromY = SuggestionsSlide.Y;

        var fade = new DoubleAnimation(fromOpacity, 0, duration) { EasingFunction = ease };
        fade.Completed += (_, _) => {
            if (epoch != _suggestionsAnimEpoch)
                return;
            ResetSuggestionsInstant();
        };

        SuggestionsCard.BeginAnimation(OpacityProperty, fade);
        SuggestionsSlide.BeginAnimation(TranslateTransform.YProperty,
            new DoubleAnimation(fromY, 10, duration) { EasingFunction = ease });
    }

    private void ResetSuggestionsInstant() {
        _suggestionsAnimEpoch++;
        _suggestionsOpen = false;
        SuggestionsHost.BeginAnimation(HeightProperty, null);
        SuggestionsCard.BeginAnimation(OpacityProperty, null);
        SuggestionsSlide.BeginAnimation(TranslateTransform.YProperty, null);
        SuggestionsHost.Height = 0;
        SuggestionsCard.Opacity = 0;
        SuggestionsSlide.Y = 10;
        SuggestionsPopup.IsOpen = false;
    }

    private void UpdateArgGhost() {
        if (_argCompletions.Count == 0) {
            ClearGhost();
            return;
        }

        if (_completionIndex >= _argCompletions.Count)
            _completionIndex = 0;

        var chosen = _argCompletions[_completionIndex];
        var argument = GetCurrentArgument(SearchBox.Text);
        ModuleArgs.SplitCurrentToken(argument, out _, out var token);

        SetGhostSuffix(chosen.Value, token);
    }

    private void UpdateModuleGhost() {
        if (_moduleCompletions.Count == 0) {
            ClearGhost();
            return;
        }

        if (_completionIndex >= _moduleCompletions.Count)
            _completionIndex = 0;

        var chosen = _moduleCompletions[_completionIndex];
        var token = SearchBox.Text.TrimStart();
        if (token.Contains(' ')) {
            ClearGhost();
            return;
        }

        SetGhostSuffix(chosen.Prefix, token);
    }

    private void SetGhostSuffix(string completion, string token) {
        _ghostSuffix = completion.StartsWith(token, StringComparison.OrdinalIgnoreCase)
            ? completion[token.Length..]
            : completion;

        if (_ghostSuffix.Length == 0) {
            ClearGhost();
            return;
        }

        GhostText.Text = _ghostSuffix;
        SearchBox.UpdateLayout();

        var rect = SearchBox.GetRectFromCharacterIndex(SearchBox.Text.Length);
        GhostText.Margin = rect.IsEmpty
            ? new Thickness(0)
            : new Thickness(rect.X, 0, 0, 0);
    }

    private void ClearGhost() {
        _ghostSuffix = "";
        GhostText.Text = "";
        GhostText.Margin = new Thickness(0);
    }

    private bool CycleCompletion(bool reverse) {
        if (_modules.IsInArgumentMode(SearchBox.Text)) {
            if (_argCompletions.Count <= 1)
                return false;

            _completionIndex = reverse
                ? (_completionIndex - 1 + _argCompletions.Count) % _argCompletions.Count
                : (_completionIndex + 1) % _argCompletions.Count;

            RefreshArgSuggestionsList();
            UpdateArgGhost();
            return true;
        }

        if (_moduleCompletions.Count <= 1)
            return false;

        _completionIndex = reverse
            ? (_completionIndex - 1 + _moduleCompletions.Count) % _moduleCompletions.Count
            : (_completionIndex + 1) % _moduleCompletions.Count;

        RefreshModuleSuggestionsList();
        UpdateModuleGhost();
        return true;
    }

    private bool AcceptCompletion() {
        if (_modules.IsInArgumentMode(SearchBox.Text)) {
            if (_argCompletions.Count == 0)
                return false;

            var chosen = _argCompletions[_completionIndex];
            if (!_modules.TryApplyCompletion(SearchBox.Text, chosen.Value, out var newInput))
                return false;

            ApplyText(newInput);
            return true;
        }

        if (_moduleCompletions.Count == 0)
            return false;

        var module = _moduleCompletions[_completionIndex];
        var raw = SearchBox.Text.TrimStart();
        var leadingWs = SearchBox.Text[..(SearchBox.Text.Length - raw.Length)];

        // "ge" → "gen" (no space)
        ApplyText(leadingWs + module.Prefix);
        return true;
    }

    private void ApplyText(string text) {
        _applyingCompletion = true;
        SearchBox.Text = text;
        SearchBox.CaretIndex = SearchBox.Text.Length;
        _applyingCompletion = false;

        _completionIndex = 0;
        UpdateSuggestions();
    }

    private static string GetCurrentArgument(string input) {
        var raw = input.TrimStart();
        var space = raw.IndexOf(' ');
        if (space < 0)
            return "";

        return raw[(space + 1)..];
    }
}
