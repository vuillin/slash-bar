using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using SlashBar.Modules;
using SlashBar.Modules.Clipboard;

namespace SlashBar;

public partial class MainWindow : Window
{
    private sealed record ArgSuggestion(string Value, string Description, bool IsSelected);
    private sealed record ModuleSuggestion(string Prefix, string Name, string Description, bool IsSelected);

    private const int HotkeyId = 9000;
    private const int QuitHotkeyId = 9001;

    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_SHIFT = 0x0004;

    private const uint VK_SPACE = 0x20;
    private const uint VK_Q = 0x51;
    private const int WM_HOTKEY = 0x0312;

    private readonly ModuleRegistry _modules = new();

    private bool _ignoreDeactivate; // sinon l'anim ferme la barre
    private bool _hotkeysRegistered;
    private bool _isAnimating;
    private bool _isOpen;
    private bool _applyingCompletion;

    private IReadOnlyList<ArgCompletion> _argCompletions = Array.Empty<ArgCompletion>();
    private IReadOnlyList<IModule> _moduleCompletions = Array.Empty<IModule>();
    private int _completionIndex;
    private string _ghostSuffix = "";

    private readonly CommandHistoryStore _commandHistory = new();
    private int _historyIndex = -1; // -1 = ligne courante
    private string _historyDraft = "";

    public MainWindow()
    {
        InitializeComponent();
        PositionAtBottom();

        Loaded += OnLoaded;

        PreviewKeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
            {
                AnimateClose();
                e.Handled = true;
            }
            else if (e.Key == Key.Tab)
            {
                if (AcceptCompletion())
                    e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                if (CycleCompletion(reverse: false) || NavigateHistory(reverse: false))
                    e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                if (CycleCompletion(reverse: true) || NavigateHistory(reverse: true))
                    e.Handled = true;
            }
        };

        SearchBox.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter)
            {
                SubmitCommand();
                e.Handled = true;
            }
        };

        SearchBox.TextChanged += (_, _) =>
        {
            if (_applyingCompletion)
                return;

            // frappe manuelle → sort de la nav historique
            if (_historyIndex >= 0)
                ResetHistoryNavigation();

            _completionIndex = 0;
            UpdateSuggestions();
        };
    }

    private void SubmitCommand()
    {
        try
        {
            if (_modules.TryExecute(SearchBox.Text, out var result))
            {
                _commandHistory.Add(SearchBox.Text);

                if (result.Kind == ModuleResultKind.Success)
                    AppToast.ShowSuccess(result.Message, result.Detail);
                else if (result.Kind == ModuleResultKind.Fail)
                    AppToast.ShowError(result.Message);

                AnimateClose();
            }
        }
        catch (Exception ex)
        {
            AppToast.ShowError(ex.Message);
            AnimateClose();
        }
    }

    private bool _suggestionsOpen;
    private int _suggestionsAnimEpoch;

    private void UpdateSuggestions()
    {
        var inArgMode = _modules.IsInArgumentMode(SearchBox.Text);

        if (inArgMode)
        {
            _moduleCompletions = Array.Empty<IModule>();
            _argCompletions = _modules.SuggestArgumentCompletions(SearchBox.Text);

            if (_completionIndex >= _argCompletions.Count)
                _completionIndex = 0;

            ModuleSuggestionsList.Visibility = Visibility.Collapsed;
            ArgSuggestionsList.Visibility = Visibility.Visible;
            RefreshArgSuggestionsList();
            UpdateArgGhost();

            if (_argCompletions.Count == 0)
            {
                HideSuggestionsPanel();
                return;
            }

            ShowSuggestionsPanel();
            return;
        }

        _argCompletions = Array.Empty<ArgCompletion>();

        var raw = SearchBox.Text.TrimStart();
        if (raw.Contains(' '))
        {
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

        if (_moduleCompletions.Count == 0)
        {
            HideSuggestionsPanel();
            return;
        }

        ShowSuggestionsPanel();
    }

    private void RefreshArgSuggestionsList()
    {
        ArgSuggestionsList.ItemsSource = _argCompletions
            .Select((c, i) => new ArgSuggestion(c.Value, c.Description, i == _completionIndex))
            .ToList();
    }

    private void RefreshModuleSuggestionsList()
    {
        ModuleSuggestionsList.ItemsSource = _moduleCompletions
            .Select((m, i) => new ModuleSuggestion(m.Prefix, m.Name, m.Description, i == _completionIndex))
            .ToList();
    }

    private void ShowSuggestionsPanel()
    {
        SuggestionsCard.Width = RootBorder.ActualWidth > 0 ? RootBorder.ActualWidth : Width;

        if (!SuggestionsPopup.IsOpen)
            SuggestionsPopup.IsOpen = true;

        SuggestionsCard.UpdateLayout();

        var target = MeasureSuggestionsHeight();
        if (target <= 0)
        {
            HideSuggestionsPanel();
            return;
        }

        // hauteur en un coup — seule opacity / slide sont animés
        SuggestionsHost.BeginAnimation(HeightProperty, null);
        SuggestionsHost.Height = target;

        if (_suggestionsOpen)
            return;

        _suggestionsOpen = true;
        AnimateSuggestionsIn();
    }

    private void HideSuggestionsPanel()
    {
        ClearGhost();

        if (!_suggestionsOpen)
        {
            ResetSuggestionsInstant();
            return;
        }

        _suggestionsOpen = false;
        AnimateSuggestionsOut();
    }

    private double MeasureSuggestionsHeight()
    {
        var width = SuggestionsCard.Width;
        if (width <= 0)
            width = RootBorder.ActualWidth > 0 ? RootBorder.ActualWidth : Width;

        SuggestionsPanel.InvalidateMeasure();
        SuggestionsPanel.Measure(new System.Windows.Size(width, double.PositiveInfinity));
        return SuggestionsPanel.DesiredSize.Height;
    }

    private void AnimateSuggestionsIn()
    {
        var epoch = ++_suggestionsAnimEpoch;
        var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
        var duration = TimeSpan.FromMilliseconds(180);

        SuggestionsCard.BeginAnimation(OpacityProperty, null);
        SuggestionsSlide.BeginAnimation(TranslateTransform.YProperty, null);

        SuggestionsCard.Opacity = 0;
        SuggestionsSlide.Y = 10;

        var fade = new DoubleAnimation(0, 1, duration) { EasingFunction = ease };
        fade.Completed += (_, _) =>
        {
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

    private void AnimateSuggestionsOut()
    {
        var epoch = ++_suggestionsAnimEpoch;
        var ease = new CubicEase { EasingMode = EasingMode.EaseIn };
        var duration = TimeSpan.FromMilliseconds(140);

        SuggestionsCard.BeginAnimation(OpacityProperty, null);
        SuggestionsSlide.BeginAnimation(TranslateTransform.YProperty, null);

        var fromOpacity = SuggestionsCard.Opacity;
        var fromY = SuggestionsSlide.Y;

        var fade = new DoubleAnimation(fromOpacity, 0, duration) { EasingFunction = ease };
        fade.Completed += (_, _) =>
        {
            if (epoch != _suggestionsAnimEpoch)
                return;
            ResetSuggestionsInstant();
        };

        SuggestionsCard.BeginAnimation(OpacityProperty, fade);
        SuggestionsSlide.BeginAnimation(TranslateTransform.YProperty,
            new DoubleAnimation(fromY, 10, duration) { EasingFunction = ease });
    }

    private void ResetSuggestionsInstant()
    {
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

    private void UpdateArgGhost()
    {
        if (_argCompletions.Count == 0)
        {
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

    private void UpdateModuleGhost()
    {
        if (_moduleCompletions.Count == 0)
        {
            ClearGhost();
            return;
        }

        if (_completionIndex >= _moduleCompletions.Count)
            _completionIndex = 0;

        var chosen = _moduleCompletions[_completionIndex];
        var token = SearchBox.Text.TrimStart();
        if (token.Contains(' '))
        {
            ClearGhost();
            return;
        }

        SetGhostSuffix(chosen.Prefix, token);
    }

    private void SetGhostSuffix(string completion, string token)
    {
        _ghostSuffix = completion.StartsWith(token, StringComparison.OrdinalIgnoreCase)
            ? completion[token.Length..]
            : completion;

        if (_ghostSuffix.Length == 0)
        {
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

    private void ClearGhost()
    {
        _ghostSuffix = "";
        GhostText.Text = "";
        GhostText.Margin = new Thickness(0);
    }

    private bool CycleCompletion(bool reverse)
    {
        if (_modules.IsInArgumentMode(SearchBox.Text))
        {
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

    private bool NavigateHistory(bool reverse)
    {
        var entries = _commandHistory.GetAll();
        if (entries.Count == 0)
            return false;

        // ↑ = plus vieux (index +), ↓ = plus récent (index -)
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
        if (_historyIndex < 0)
        {
            ApplyHistoryEntry(_historyDraft);
            return true;
        }

        ApplyHistoryEntry(entries[_historyIndex]);
        return true;
    }

    private void ApplyHistoryEntry(string text)
    {
        _applyingCompletion = true;
        SearchBox.Text = text;
        SearchBox.CaretIndex = SearchBox.Text.Length;
        _applyingCompletion = false;
        _completionIndex = 0;
        UpdateSuggestions();
    }

    private void ResetHistoryNavigation()
    {
        _historyIndex = -1;
        _historyDraft = "";
    }

    private bool AcceptCompletion()
    {
        if (_modules.IsInArgumentMode(SearchBox.Text))
        {
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

        // "ge" → "gen" (pas d'espace)
        ApplyText(leadingWs + module.Prefix);
        return true;
    }

    private void ApplyText(string text)
    {
        _applyingCompletion = true;
        SearchBox.Text = text;
        SearchBox.CaretIndex = SearchBox.Text.Length;
        _applyingCompletion = false;

        _completionIndex = 0;
        UpdateSuggestions();
    }

    private static string GetCurrentArgument(string input)
    {
        var raw = input.TrimStart();
        var space = raw.IndexOf(' ');
        if (space < 0)
            return "";

        return raw[(space + 1)..];
    }

    private void PositionAtBottom()
    {
        var work = SystemParameters.WorkArea;
        Width = Math.Min(520, work.Width * 0.38);
        Left = work.Left + (work.Width - Width) / 2;

        if (ActualHeight <= 0)
            UpdateLayout();

        const double bottomMargin = 16;
        Top = work.Bottom - Math.Max(ActualHeight, 48) - bottomMargin;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_hotkeysRegistered)
            return;

        try
        {
            PositionAtBottom();
            RegisterGlobalHotkeys();
            _hotkeysRegistered = true;

            ClipboardHistory.Watcher.Start();

            Hide(); // ctrl+espace pour rouvrir
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(this, "Erreur au démarrage :\n" + ex.Message, "SlashBar");
        }
    }

    private void RegisterGlobalHotkeys()
    {
        var helper = new WindowInteropHelper(this);
        helper.EnsureHandle();

        bool okSearch = RegisterHotKey(
            helper.Handle,
            HotkeyId,
            MOD_CONTROL,
            VK_SPACE);

        bool okQuit = RegisterHotKey(
            helper.Handle,
            QuitHotkeyId,
            MOD_CONTROL | MOD_SHIFT,
            VK_Q);

        if (!okSearch || !okQuit)
        {
            System.Windows.MessageBox.Show(
                this,
                "Impossible d'enregistrer Ctrl+Espace ou Ctrl+Shift+Q.\n" +
                "Un autre logiciel utilise peut-être déjà ce raccourci.",
                "SlashBar");
            return;
        }

        var source = HwndSource.FromHwnd(helper.Handle);
        source?.AddHook(HwndHook);
    }

    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg != WM_HOTKEY)
            return IntPtr.Zero;

        int id = wParam.ToInt32();

        if (id == HotkeyId)
        {
            ToggleBar();
            handled = true;
        }
        else if (id == QuitHotkeyId)
        {
            System.Windows.Application.Current.Shutdown();
            handled = true;
        }

        return IntPtr.Zero;
    }

    private void ToggleBar()
    {
        if (_isAnimating)
            return;

        if (_isOpen || IsVisible)
        {
            AnimateClose();
            return;
        }

        AnimateOpen();
    }

    private void AnimateOpen()
    {
        if (_isAnimating)
            return;

        _isAnimating = true;
        PositionAtBottom();
        ResetHistoryNavigation();
        SearchBox.Text = "";
        _completionIndex = 0;
        UpdateSuggestions();

        Opacity = 0;
        SlideTransform.Y = 20;

        _ignoreDeactivate = true;
        Show();
        Activate();
        SearchBox.Focus();

        var ease = new QuadraticEase { EasingMode = EasingMode.EaseOut };

        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(180))
        {
            EasingFunction = ease
        };

        var slideIn = new DoubleAnimation(20, 0, TimeSpan.FromMilliseconds(220))
        {
            EasingFunction = ease
        };

        fadeIn.Completed += (_, _) =>
        {
            _isOpen = true;
            _isAnimating = false;
            _ignoreDeactivate = false;
        };

        BeginAnimation(OpacityProperty, fadeIn);
        SlideTransform.BeginAnimation(TranslateTransform.YProperty, slideIn);
    }

    private void AnimateClose()
    {
        if (_isAnimating || !IsVisible)
            return;

        _isAnimating = true;
        _ignoreDeactivate = true;

        var ease = new QuadraticEase { EasingMode = EasingMode.EaseIn };

        var fadeOut = new DoubleAnimation(Opacity, 0, TimeSpan.FromMilliseconds(140))
        {
            EasingFunction = ease
        };

        var slideOut = new DoubleAnimation(SlideTransform.Y, 16, TimeSpan.FromMilliseconds(160))
        {
            EasingFunction = ease
        };

        fadeOut.Completed += (_, _) =>
        {
            BeginAnimation(OpacityProperty, null);
            SlideTransform.BeginAnimation(TranslateTransform.YProperty, null);
            Opacity = 0;
            SlideTransform.Y = 20;
            ResetSuggestionsInstant();

            Hide();
            _isOpen = false;
            _isAnimating = false;
            _ignoreDeactivate = false;
        };

        BeginAnimation(OpacityProperty, fadeOut);
        SlideTransform.BeginAnimation(TranslateTransform.YProperty, slideOut);
    }

    private void Window_Deactivated(object? sender, EventArgs e)
    {
        if (_ignoreDeactivate || _isAnimating)
            return;

        // Popup = autre HWND : ne pas fermer si le clic reste sur les suggestions
        Dispatcher.BeginInvoke(() =>
        {
            if (_ignoreDeactivate || _isAnimating || IsActive)
                return;

            if (SuggestionsPopup.IsOpen && SuggestionsPopup.IsMouseOver)
                return;

            AnimateClose();
        });
    }

    protected override void OnClosed(EventArgs e)
    {
        var helper = new WindowInteropHelper(this);
        if (helper.Handle != IntPtr.Zero)
        {
            UnregisterHotKey(helper.Handle, HotkeyId);
            UnregisterHotKey(helper.Handle, QuitHotkeyId);
        }

        System.Windows.Application.Current.Shutdown();
        base.OnClosed(e);
    }

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
