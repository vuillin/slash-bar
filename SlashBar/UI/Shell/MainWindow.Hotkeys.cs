using System.Windows;
using System.Windows.Interop;
using SlashBar.Modules;
using SlashBar.Modules.Clipboard;
using SlashBar.Modules.Native;
using SlashBar.Modules.Pin;

namespace SlashBar;

public partial class MainWindow {

    private const int HotkeyId = 9000;
    private const int QuitHotkeyId = 9001;
    private const int PinHotkeyId = 9002;

    private bool _hotkeysRegistered;

    private void OnLoaded(object sender, RoutedEventArgs e) {
        if (_hotkeysRegistered)
            return;

        try {
            PositionAtBottom();
            RegisterGlobalHotkeys();
            _hotkeysRegistered = true;

            ClipboardHistory.Watcher.Start();

            Hide(); // Ctrl+Space to reopen
        }
        catch (Exception ex) {
            System.Windows.MessageBox.Show(this, "Startup error:\n" + ex.Message, "SlashBar");
        }
    }

    private void RegisterGlobalHotkeys() {
        var helper = new WindowInteropHelper(this);
        helper.EnsureHandle();

        bool okSearch = HotkeyNative.Register(
            helper.Handle,
            HotkeyId,
            HotkeyNative.ModControl,
            HotkeyNative.VkSpace);

        bool okQuit = HotkeyNative.Register(
            helper.Handle,
            QuitHotkeyId,
            HotkeyNative.ModControl | HotkeyNative.ModShift,
            HotkeyNative.VkQ);

        bool okPin = HotkeyNative.Register(
            helper.Handle,
            PinHotkeyId,
            HotkeyNative.ModControl | HotkeyNative.ModShift,
            HotkeyNative.VkA);

        var failed = new List<string>();
        if (!okSearch)
            failed.Add("Ctrl+Space");
        if (!okQuit)
            failed.Add("Ctrl+Shift+Q");
        if (!okPin)
            failed.Add("Ctrl+Shift+A");

        if (failed.Count > 0) {
            System.Windows.MessageBox.Show(
                this,
                "Could not register " + string.Join(", ", failed) + ".\n" +
                "Another app may already use this shortcut.",
                "SlashBar");
        }

        var source = HwndSource.FromHwnd(helper.Handle);
        source?.AddHook(HwndHook);
    }

    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
        if (msg != HotkeyNative.WmHotkey)
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
        else if (id == PinHotkeyId) {
            TogglePinForeground();
            handled = true;
        }

        return IntPtr.Zero;
    }

    private static void TogglePinForeground() {
        var result = PinSession.ToggleForeground();
        if (result is null)
            return;

        var (pinned, title) = result.Value;
        if (pinned)
            AppToast.ShowSuccess("Pinned", title, PinModule.ToastDurationMs);
        else
            AppToast.ShowSuccess("Unpinned", title, PinModule.ToastDurationMs);
    }

    protected override void OnClosed(EventArgs e) {
        var helper = new WindowInteropHelper(this);
        if (helper.Handle != IntPtr.Zero) {
            HotkeyNative.Unregister(helper.Handle, HotkeyId);
            HotkeyNative.Unregister(helper.Handle, QuitHotkeyId);
            HotkeyNative.Unregister(helper.Handle, PinHotkeyId);
        }

        System.Windows.Application.Current.Shutdown();
        base.OnClosed(e);
    }
}
