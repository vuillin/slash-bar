using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using SlashBar.Modules.Native;

namespace SlashBar.Modules.Clipboard;

public sealed class ClipboardWatcher {

    private readonly ClipboardHistoryStore _store;
    private HwndSource? _source;

    private string _lastSeen = "";
    private string? _suppressText;
    private DateTime _suppressUntilUtc;


    public ClipboardWatcher(ClipboardHistoryStore store) {
        _store = store;
    }


    public void Start() {

        if (_source != null)
            return;

        var parameters = new HwndSourceParameters("SlashBarClipboardWatcher") {
            Width = 0,
            Height = 0,
            WindowStyle = 0,
            ParentWindow = ClipboardNative.HwndMessage
        };

        _source = new HwndSource(parameters);
        _source.AddHook(WndProc);

        ClipboardNative.AddFormatListener(_source.Handle);
    }


    public void Stop() {

        if (_source == null)
            return;

        ClipboardNative.RemoveFormatListener(_source.Handle);
        _source.RemoveHook(WndProc);
        _source.Dispose();
        _source = null;
    }


    public void IgnoreNext(string text) {
        _suppressText = text;
        _suppressUntilUtc = DateTime.UtcNow.AddMilliseconds(500);
    }


    private IntPtr WndProc(
        IntPtr hwnd,
        int msg,
        IntPtr wParam,
        IntPtr lParam,
        ref bool handled) {

        if (msg == ClipboardNative.WmClipboardUpdate) {
            Dispatcher.CurrentDispatcher.BeginInvoke(
                DispatcherPriority.Background,
                Capture);
        }

        return IntPtr.Zero;
    }


    private void Capture() {
        try {

            if (!System.Windows.Clipboard.ContainsText())
                return;

            var text = System.Windows.Clipboard.GetText();
            if (string.IsNullOrWhiteSpace(text))
                return;

            if (_suppressText != null) {
                var stillValid = DateTime.UtcNow <= _suppressUntilUtc;

                if (stillValid && text == _suppressText) {
                    _lastSeen = text;
                    _suppressText = null;
                    return;
                }

                _suppressText = null;
            }

            // same content as last time → skip.
            if (text == _lastSeen)
                return;

            _lastSeen = text;
            _store.Add(text);
        }
        catch {
            // clipboard sometimes still locked despite BeginInvoke.
        }
    }
}
