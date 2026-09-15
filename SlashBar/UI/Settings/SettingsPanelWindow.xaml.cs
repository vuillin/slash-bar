using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SlashBar.Modules.Settings;
using SlashBar.UI.Shell;

namespace SlashBar;

public partial class SettingsPanelWindow : DockedSidePanelWindow {

    protected override double PanelContentWidth => 456;

    public override string DockShelfKey => "settings";

    protected override double DockedHeight(double workAreaHeight) => 546;

    private static SettingsPanelWindow? _instance;

    private bool _loading = true;
    private string _weatherUnit = WeatherUnits.Celsius;
    private string _displayTheme = DisplayThemes.Light;


    private SettingsPanelWindow() {
        InitializeComponent();
        Width = ShellWidth;
        PreviewKeyDown += (_, e) => {
            if (e.Key == Key.Escape)
                AnimateClose();
        };
    }


    public static void Toggle() {
        _instance ??= new SettingsPanelWindow();
        _instance.ToggleVisibility();
    }


    protected override void OnPanelOpening() {
        base.OnPanelOpening();
        LoadSettingsIntoUi();
    }


    protected override void OnPanelOpened() {
        SettingsBook.Store.Changed += OnSettingsChanged;
    }


    protected override void OnPanelClosing() {
        SettingsBook.Store.Changed -= OnSettingsChanged;
    }


    private void OnSettingsChanged() {
        Dispatcher.BeginInvoke(() => {
            ApplyUnitButtons(_weatherUnit);
            ApplyThemeButtons(_displayTheme);
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
        } finally {
            _loading = false;
        }
    }


    private void PersistFromUi() {
        if (_loading)
            return;

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


    private static System.Windows.Media.Brush ThemeBrush(string key) =>
        (System.Windows.Media.Brush)System.Windows.Application.Current.FindResource(key);
}
