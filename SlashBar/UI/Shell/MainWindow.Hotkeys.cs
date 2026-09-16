using System.Windows;
using System.Windows.Interop;
using SlashBar.Modules;
using SlashBar.Modules.Clipboard;
using SlashBar.Modules.Native;
using SlashBar.Modules.Pin;
using SlashBar.Modules.Settings;

namespace SlashBar;

public partial class MainWindow {

    private const int HotkeyId = 9000;
    private const int QuitHotkeyId = 9001;
    private const int PinHotkeyId = 9002;

    private bool _hotkeysRegistered;
    private bool _hotkeyHookAdded;

    private void OnLoaded(object sender, RoutedEventArgs e) {
        if (_hotkeysRegistered)
            return;

        try {
            PositionAtBottom();
            RegisterGlobalHotkeys();
            SettingsBook.Store.Changed += OnSettingsChangedForHotkeys;
            _hotkeysRegistered = true;

            ClipboardHistory.Watcher.Start();

            Hide(); // global hotkey to reopen
        }
        catch (Exception ex) {
            System.Windows.MessageBox.Show(this, "Startup error:\n" + ex.Message, "SlashBar");
        }
    }

    private void OnSettingsChangedForHotkeys() {
        Dispatcher.BeginInvoke(ReregisterGlobalHotkeys);
    }

    private void RegisterGlobalHotkeys() {
        var helper = new WindowInteropHelper(this);
        helper.EnsureHandle();

        RegisterHotkeysFromSettings(helper.Handle, showErrors: true);

        if (_hotkeyHookAdded)
            return;

        var source = HwndSource.FromHwnd(helper.Handle);
        source?.AddHook(HwndHook);
        _hotkeyHookAdded = true;
    }

    private void ReregisterGlobalHotkeys() {
        var helper = new WindowInteropHelper(this);
        if (helper.Handle == IntPtr.Zero)
            return;

        UnregisterAllHotkeys(helper.Handle);
        RegisterHotkeysFromSettings(helper.Handle, showErrors: true);
    }

    private void RegisterHotkeysFromSettings(IntPtr hwnd, bool showErrors) {
        var hotkeys = SettingsBook.Store.Get().Hotkeys;
        var failed = new List<string>();

        if (!TryRegisterChord(hwnd, HotkeyId, hotkeys.OpenBar, out var openLabel))
            failed.Add(openLabel);
        if (!TryRegisterChord(hwnd, QuitHotkeyId, hotkeys.Quit, out var quitLabel))
            failed.Add(quitLabel);
        if (!TryRegisterChord(hwnd, PinHotkeyId, hotkeys.Pin, out var pinLabel))
            failed.Add(pinLabel);

        if (!showErrors || failed.Count == 0)
            return;

        System.Windows.MessageBox.Show(
            this,
            "Could not register " + string.Join(", ", failed) + ".\n" +
            "Another app may already use this shortcut.",
            "SlashBar");
    }

    private static bool TryRegisterChord(IntPtr hwnd, int id, HotkeyChord chord, out string label) {
        label = HotkeyChordMapper.ToLabel(chord);
        if (!HotkeyChordMapper.TryToNative(chord, out var modifiers, out var vk))
            return false;

        return HotkeyNative.Register(hwnd, id, modifiers, vk);
    }

    private static void UnregisterAllHotkeys(IntPtr hwnd) {
        HotkeyNative.Unregister(hwnd, HotkeyId);
        HotkeyNative.Unregister(hwnd, QuitHotkeyId);
        HotkeyNative.Unregister(hwnd, PinHotkeyId);
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
        SettingsBook.Store.Changed -= OnSettingsChangedForHotkeys;

        var helper = new WindowInteropHelper(this);
        if (helper.Handle != IntPtr.Zero)
            UnregisterAllHotkeys(helper.Handle);

        System.Windows.Application.Current.Shutdown();
        base.OnClosed(e);
    }
}
