using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SlashBar.Modules.Settings;
using SlashBar.UI.Shell;

namespace SlashBar;

public partial class SettingsPanelWindow : DockedSidePanelWindow {

    private enum HotkeyCaptureSlot {
        None,
        OpenBar,
        Quit,
        Pin
    }

    protected override double PanelContentWidth => 456;

    public override string DockShelfKey => "settings";

    protected override double DockedHeight(double workAreaHeight) => 546;

    private static SettingsPanelWindow? _instance;

    private bool _loading = true;
    private string _weatherUnit = WeatherUnits.Celsius;
    private string _displayTheme = DisplayThemes.Light;
    private HotkeyCaptureSlot _capture = HotkeyCaptureSlot.None;


    private SettingsPanelWindow() {
        InitializeComponent();
        Width = ShellWidth;
        PreviewKeyDown += SettingsPanel_PreviewKeyDown;
    }


    public static void Toggle() {
        _instance ??= new SettingsPanelWindow();
        _instance.ToggleVisibility();
    }


    public static void ShowPanel() {
        _instance ??= new SettingsPanelWindow();

        if (_instance.IsVisible) {
            _instance.Activate();
            return;
        }

        _instance.ToggleVisibility();
    }


    protected override void OnPanelOpening() {
        base.OnPanelOpening();
        CancelCapture();
        LoadSettingsIntoUi();
    }


    protected override void OnPanelOpened() {
        SettingsBook.Store.Changed += OnSettingsChanged;
    }


    protected override void OnPanelClosing() {
        SettingsBook.Store.Changed -= OnSettingsChanged;
        CancelCapture();
    }


    private void OnSettingsChanged() {
        Dispatcher.BeginInvoke(() => {
            if (_capture != HotkeyCaptureSlot.None)
                return;

            ApplyUnitButtons(_weatherUnit);
            ApplyThemeButtons(_displayTheme);
            RefreshHotkeyChips(SettingsBook.Store.Get().Hotkeys);
        });
    }


    private void LoadSettingsIntoUi() {
        _loading = true;
        try {
            var s = SettingsBook.Store.Get();
            WeatherCityBox.Text = s.Weather.City;
            WeatherSunEventsToggle.IsChecked = s.Weather.ShowSunEvents;
            ApplyUnitButtons(s.Weather.Unit);
            ApplyThemeButtons(s.Display.Theme);
            RefreshHotkeyChips(s.Hotkeys);
        } finally {
            _loading = false;
        }
    }


    private void PersistFromUi() {
        if (_loading)
            return;

        CancelCapture();

        var s = SettingsBook.Store.Get();
        s.Weather.City = WeatherCityBox.Text.Trim();
        s.Weather.Unit = _weatherUnit;
        s.Weather.ShowSunEvents = WeatherSunEventsToggle.IsChecked == true;
        s.Display.Theme = _displayTheme;
        SettingsBook.Store.Update(s);
    }


    private void WeatherCityBox_LostFocus(object sender, RoutedEventArgs e) {
        PersistFromUi();
    }


    private void WeatherSunEventsToggle_Changed(object sender, RoutedEventArgs e) {
        PersistFromUi();
    }


    private void WeatherUnit_Click(object sender, RoutedEventArgs e) {
        if (sender is not System.Windows.Controls.Button { Tag: string unit })
            return;

        ApplyUnitButtons(unit);
        PersistFromUi();
    }


    private void DisplayTheme_Click(object sender, RoutedEventArgs e) {
        if (sender is not System.Windows.Controls.Button { Tag: string theme })
            return;

        ApplyThemeButtons(theme);
        PersistFromUi();
    }


    private void ApplyUnitButtons(string unit) {
        _weatherUnit = unit;
        StyleSegmentButton(WeatherUnitCelsiusButton, unit == WeatherUnits.Celsius);
        StyleSegmentButton(WeatherUnitFahrenheitButton, unit == WeatherUnits.Fahrenheit);
    }


    private void ApplyThemeButtons(string theme) {
        _displayTheme = theme;
        StyleSegmentButton(DisplayThemeLightButton, theme == DisplayThemes.Light);
        StyleSegmentButton(DisplayThemeDarkButton, theme == DisplayThemes.Dark);
    }


    private static void StyleSegmentButton(System.Windows.Controls.Button button, bool selected) {
        button.ApplyTemplate();
        if (button.Template.FindName("Bg", button) is not Border bg)
            return;

        bg.Background = selected
            ? ThemeBrush("Brush.SegmentFill")
            : System.Windows.Media.Brushes.Transparent;

        button.Foreground = selected
            ? ThemeBrush("Brush.TextPrimary")
            : ThemeBrush("Brush.TextSecondary");
    }


    private void SettingsPanel_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
        if (_capture != HotkeyCaptureSlot.None) {
            if (e.Key == Key.Escape) {
                CancelCapture();
                e.Handled = true;
                return;
            }

            TryCommitCapture(e);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Escape)
            AnimateClose();
    }


    private void OpenBarHotkey_Click(object sender, MouseButtonEventArgs e) =>
        BeginCapture(HotkeyCaptureSlot.OpenBar);

    private void QuitHotkey_Click(object sender, MouseButtonEventArgs e) =>
        BeginCapture(HotkeyCaptureSlot.Quit);

    private void PinHotkey_Click(object sender, MouseButtonEventArgs e) =>
        BeginCapture(HotkeyCaptureSlot.Pin);

    private void OpenBarHotkeyReset_Click(object sender, RoutedEventArgs e) =>
        ResetHotkey(h => h.OpenBar = HotkeyDefaults.OpenBar());

    private void QuitHotkeyReset_Click(object sender, RoutedEventArgs e) =>
        ResetHotkey(h => h.Quit = HotkeyDefaults.Quit());

    private void PinHotkeyReset_Click(object sender, RoutedEventArgs e) =>
        ResetHotkey(h => h.Pin = HotkeyDefaults.Pin());


    private void BeginCapture(HotkeyCaptureSlot slot) {
        if (_loading)
            return;

        _capture = slot;
        RefreshHotkeyChips(SettingsBook.Store.Get().Hotkeys);
        Activate();
        Focus();
    }


    private void CancelCapture() {
        if (_capture == HotkeyCaptureSlot.None)
            return;

        _capture = HotkeyCaptureSlot.None;
        RefreshHotkeyChips(SettingsBook.Store.Get().Hotkeys);
    }


    private void TryCommitCapture(System.Windows.Input.KeyEventArgs e) {
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (HotkeyChordMapper.IsModifierKey(key) || key == Key.Escape)
            return;

        if (!HotkeyChordMapper.TryFromKey(key, Keyboard.Modifiers, out var chord)) {
            AppToast.ShowError("Use Ctrl, Shift, or Alt + a key");
            return;
        }

        var s = SettingsBook.Store.Get();
        if (IsDuplicate(_capture, s.Hotkeys, chord)) {
            AppToast.ShowError("Shortcut already used");
            return;
        }

        ApplyCapturedChord(s.Hotkeys, _capture, chord);
        _capture = HotkeyCaptureSlot.None;
        SettingsBook.Store.Update(s);
        RefreshHotkeyChips(s.Hotkeys);
    }


    private void ResetHotkey(Action<HotkeySettings> apply) {
        if (_loading)
            return;

        _capture = HotkeyCaptureSlot.None;

        var s = SettingsBook.Store.Get();
        apply(s.Hotkeys);
        SettingsBook.Store.Update(s);
        RefreshHotkeyChips(s.Hotkeys);
    }


    private static void ApplyCapturedChord(HotkeySettings hotkeys, HotkeyCaptureSlot slot, HotkeyChord chord) {
        switch (slot) {
            case HotkeyCaptureSlot.OpenBar:
                hotkeys.OpenBar = chord;
                break;
            case HotkeyCaptureSlot.Quit:
                hotkeys.Quit = chord;
                break;
            case HotkeyCaptureSlot.Pin:
                hotkeys.Pin = chord;
                break;
        }
    }


    private static bool IsDuplicate(HotkeyCaptureSlot slot, HotkeySettings hotkeys, HotkeyChord chord) {
        if (slot != HotkeyCaptureSlot.OpenBar && HotkeyChordMapper.Same(chord, hotkeys.OpenBar))
            return true;
        if (slot != HotkeyCaptureSlot.Quit && HotkeyChordMapper.Same(chord, hotkeys.Quit))
            return true;
        if (slot != HotkeyCaptureSlot.Pin && HotkeyChordMapper.Same(chord, hotkeys.Pin))
            return true;
        return false;
    }


    private void RefreshHotkeyChips(HotkeySettings hotkeys) {
        SetHotkeyRow(OpenBarHotkeyChips, OpenBarHotkeyPrompt, hotkeys.OpenBar, _capture == HotkeyCaptureSlot.OpenBar);
        SetHotkeyRow(QuitHotkeyChips, QuitHotkeyPrompt, hotkeys.Quit, _capture == HotkeyCaptureSlot.Quit);
        SetHotkeyRow(PinHotkeyChips, PinHotkeyPrompt, hotkeys.Pin, _capture == HotkeyCaptureSlot.Pin);
    }


    private static void SetHotkeyRow(ItemsControl chips, TextBlock prompt, HotkeyChord chord, bool capturing) {
        if (capturing) {
            chips.ItemsSource = null;
            chips.Visibility = Visibility.Collapsed;
            prompt.Visibility = Visibility.Visible;
            return;
        }

        chips.ItemsSource = HotkeyChordMapper.ToChips(chord);
        chips.Visibility = Visibility.Visible;
        prompt.Visibility = Visibility.Collapsed;
    }


    private static System.Windows.Media.Brush ThemeBrush(string key) =>
        (System.Windows.Media.Brush)System.Windows.Application.Current.FindResource(key);
}
