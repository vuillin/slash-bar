using System.Drawing;
using System.IO;
using WinForms = System.Windows.Forms;

namespace SlashBar;

public sealed class TrayIcon : IDisposable {

    private readonly WinForms.NotifyIcon _notifyIcon;
    private readonly WinForms.ContextMenuStrip _menu;
    private readonly Icon _icon;

    public TrayIcon() {
        _icon = LoadIcon();

        _menu = new WinForms.ContextMenuStrip();
        _menu.Items.Add("Show bar", null, (_, _) => RunOnUi(OnShowBar));
        _menu.Items.Add("Calendar", null, (_, _) => RunOnUi(OnCalendar));
        _menu.Items.Add("Settings", null, (_, _) => RunOnUi(OnSettings));
        _menu.Items.Add(new WinForms.ToolStripSeparator());
        _menu.Items.Add("Quit", null, (_, _) => RunOnUi(OnQuit));

        _notifyIcon = new WinForms.NotifyIcon {
            Icon = _icon,
            Text = "SlashBar",
            ContextMenuStrip = _menu,
            Visible = true
        };

        _notifyIcon.MouseClick += (_, e) => {
            if (e.Button == WinForms.MouseButtons.Left)
                RunOnUi(OnShowBar);
        };
    }


    private static Icon LoadIcon() {
        var uri = new Uri("pack://application:,,,/Assets/App/slashbar.ico");
        var resource = System.Windows.Application.GetResourceStream(uri)
            ?? throw new FileNotFoundException("Missing tray icon: Assets/App/slashbar.ico");

        using (resource.Stream)
            return new Icon(resource.Stream);
    }


    private static void RunOnUi(Action action) {
        var app = System.Windows.Application.Current;
        if (app is null)
            return;

        if (app.Dispatcher.CheckAccess())
            action();
        else
            app.Dispatcher.Invoke(action);
    }


    private static void OnShowBar() {
        if (System.Windows.Application.Current.MainWindow is MainWindow bar)
            bar.ShowBar();
    }

    private static void OnCalendar() =>
        CalendarPanelWindow.ShowPanel();

    private static void OnSettings() =>
        SettingsPanelWindow.ShowPanel();

    private static void OnQuit() =>
        System.Windows.Application.Current.Shutdown();


    public void Dispose() {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _menu.Dispose();
        _icon.Dispose();
    }
}
