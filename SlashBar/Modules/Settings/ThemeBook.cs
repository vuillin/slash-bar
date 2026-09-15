using System.Windows;
using SlashBar.Modules.Settings;

namespace SlashBar.Modules.Settings;

public static class ThemeBook {

    private static ResourceDictionary? _currentTheme;


    public static void Apply() {
        var theme = SettingsBook.Store.Get().Display.Theme;
        var source = theme == DisplayThemes.Dark
            ? "Themes/Dark.xaml"
            : "Themes/Light.xaml";

        var next = new ResourceDictionary {
            Source = new Uri(source, UriKind.Relative)
        };

        var merged = System.Windows.Application.Current.Resources.MergedDictionaries;

        if (_currentTheme is not null)
            merged.Remove(_currentTheme);

        merged.Insert(0, next);
        _currentTheme = next;
    }

    public static void StartWatching() {
        SettingsBook.Store.Changed += () => {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(Apply);
        };
    }
}