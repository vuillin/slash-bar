using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using SlashBar.Modules.Clipboard;

namespace SlashBar;

public partial class MainWindow {

    private const int HotkeyId = 9000;
    private const int QuitHotkeyId = 9001;

    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_SHIFT = 0x0004;

    private const uint VK_SPACE = 0x20;
    private const uint VK_Q = 0x51;
    private const int WM_HOTKEY = 0x0312;

    private bool _hotkeysRegistered;

    private void OnLoaded(object sender, RoutedEventArgs e) {
        if (_hotkeysRegistered)
            return;

        try {
            PositionAtBottom();
            RegisterGlobalHotkeys();
            _hotkeysRegistered = true;

            ClipboardHistory.Watcher.Start();

            Hide(); // ctrl+espace pour rouvrir
        }
        catch (Exception ex) {
            System.Windows.MessageBox.Show(this, "Erreur au démarrage :\n" + ex.Message, "SlashBar");
        }
    }

    private void RegisterGlobalHotkeys() {
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

        if (!okSearch || !okQuit) {
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

    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
        if (msg != WM_HOTKEY)
            return IntPtr.Zero;

        int id = wParam.ToInt32();

        if (id == HotkeyId) {
            ToggleBar();
            handled = true;
        }
        else if (id == QuitHotkeyId) {
            System.Windows.Application.Current.Shutdown();
            handled = true;
        }

        return IntPtr.Zero;
    }

    protected override void OnClosed(EventArgs e) {
        var helper = new WindowInteropHelper(this);
        if (helper.Handle != IntPtr.Zero) {
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
