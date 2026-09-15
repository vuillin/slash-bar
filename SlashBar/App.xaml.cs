using System.Windows;
using SlashBar.Modules.Awake;
using SlashBar.Modules.Clipboard;
using SlashBar.Modules.Color;
using SlashBar.Modules.Memo;
using SlashBar.Modules.Pin;
using SlashBar.Modules.Calendar;
using SlashBar.Modules.Settings;

namespace SlashBar;

public partial class App : System.Windows.Application {

    private TrayIcon? _tray;

    protected override void OnStartup(StartupEventArgs e) {
        ThemeBook.Apply();
        ThemeBook.StartWatching();
        base.OnStartup(e);
        _tray = new TrayIcon();
    }

    protected override void OnExit(ExitEventArgs e) {
        _tray?.Dispose();
        
        AwakeSession.Disable();
        PinSession.UnpinAll();
        ClipboardHistory.Store.Flush();
        MemoBook.Store.Flush();
        ColorHistory.Store.Flush();
        CalendarBook.Store.Flush();
        SettingsBook.Store.Flush();
        base.OnExit(e);
    }
}
